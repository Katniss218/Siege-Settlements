using SS.Levels;
using SS.Objects.Modules;
using SS.Objects.SubObjects;
using SS.UI;
using SS.UI.HUDs;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace SS.Objects.Units
{
	[UseHud(typeof(UnitHUD), "hud")]
	public class Unit : SSObjectDFSC, IMovable, IMouseOverHandlerListener, IInteriorUser
	{
		private NavMeshAgent __navMeshAgent = null;
		public NavMeshAgent navMeshAgent
		{
			get
			{
				if( this.__navMeshAgent == null )
				{
					this.__navMeshAgent = this.GetComponent<NavMeshAgent>();
				}
				return this.__navMeshAgent;
			}
		}

		private BoxCollider __collider = null;
		public new BoxCollider collider
		{
			get
			{
				if( this.__collider == null )
				{
					this.__collider = this.GetComponent<BoxCollider>();
				}
				return this.__collider;
			}
		}
		
		public bool isPopulationLocked { get; set; }
		public byte? populationSizeLimit { get; set; }
		private PopulationSize __population = PopulationSize.x1;
		public PopulationSize population
		{
			get
			{
				return this.__population;
			}
			set
			{
				if( !this.CanChangePopulation() )
				{
					return;
				}
				if( this.populationSizeLimit != null && (byte)value > this.populationSizeLimit )
				{
					return;
				}

				PopulationSize populationBefore = this.__population;


				if( this.factionId != SSObjectDFSC.FACTIONID_INVALID && populationBefore != value )
				{
					if( populationBefore != PopulationSize.x1 )
					{
						LevelDataManager.factionData[this.factionId].populationCache -= (int)populationBefore;
					}
					LevelDataManager.factionData[this.factionId].populationCache += (int)value;
					if( this.factionId == LevelDataManager.PLAYER_FAC )
					{
						ResourcePanel.instance.UpdatePopulationDisplay( LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].populationCache );
					}
				}

				
				this.__population = value;
				PopulationUnitUtils.ScaleStats( this, populationBefore, value );
			}
		}

		/// <summary>
		/// Returns the hud that's attached to this object.
		/// </summary>
		public UnitHUD hud { get; set; }
		public override HUDDFSC hudDFSC { get { return this.hud; } }






		public CivilianUnitExtension civilian { get; private set; }
		
		public bool isCivilian
		{
			get
			{
				return this.civilian != null;
			}
			set
			{
				if( this.isCivilian == value )
				{
					return;
				}
				// if the unit is civilian, make sure to set it's avoidance priority to 10-99, otherwise set them to 1. Do this to reduce civilians getting stuck on each other.
				if( value )
				{
					this.civilian = this.gameObject.AddComponent<CivilianUnitExtension>();
					this.navMeshAgent.avoidancePriority = CivilianUnitExtension.NextAvoidancePriority( false ); // no need for employed, since there's no time for the civilian to get employed.
				}
				else
				{
					Object.Destroy( this.civilian );
					navMeshAgent.avoidancePriority = CivilianUnitExtension.AVOIDANCE_PRORITY_GENERAL;
				}
			}
		}


		//
		//
		//


		float __movementSpeed;
		public float movementSpeed
		{
			get
			{
				return this.__movementSpeed;
			}
			set
			{
				this.__movementSpeed = value;
				this.navMeshAgent.speed = value;
			}
		}
		
		float __rotationSpeed;
		public float rotationSpeed
		{
			get
			{
				return this.__rotationSpeed;
			}
			set
			{
				this.__rotationSpeed = value;
				this.navMeshAgent.angularSpeed = value;
			}
		}

		private Vector3 __sizePerPopulation;
		public Vector3 sizePerPopulation
		{
			get => this.__sizePerPopulation;
			set
			{
				this.__sizePerPopulation = value;
				PopulationUnitUtils.ScaleSize( this, this.population );
			}
		}


		//
		//
		//
		
		/// <summary>
		/// The interior module the unit is currently in. Null if not is any interior.
		/// </summary>
		public InteriorModule interior { get; private set; }
		public InteriorModule.SlotType slotType { get; private set; }
		/// <summary>
		/// The slot of the interior module the unit is currently in.
		/// </summary>
		public int slotIndex { get; private set; }

		public bool isInside
		{
			get { return this.interior != null; }
		}
		public bool isInsideHidden { get; private set; } // if true, the unit is not visible - graphics (sub-objects) are disabled.



		/// <summary>
		/// Marks the unit as being inside.
		/// </summary>
		public void SetInside( InteriorModule interior, InteriorModule.SlotType slotType, int slotIndex )
		{
			if( this.isInside )
			{
				return;
			}

			// - Interior fields
			
			InteriorModule.GetSlot( interior, slotType, slotIndex, out InteriorModule.Slot slot, out HUDInteriorSlot slotHud );
			
			slot.objInside = this;

			slotHud.SetHealth( this.healthPercent );
			if( slotType == InteriorModule.SlotType.Worker )
			{
				slotHud.SetVisible( true );
			}
			else
			{
				slotHud.SetSprite( this.icon );
			}

			this.navMeshAgent.enabled = false;

			// -
			
			this.transform.position = interior.SlotWorldPosition( slot );
			this.transform.rotation = interior.SlotWorldRotation( slot );
			
			if( slot.isHidden )
			{
				SubObject[] subObjects = this.GetSubObjects();

				for( int j = 0; j < subObjects.Length; j++ )
				{
					subObjects[j].gameObject.SetActive( false );
				}

				this.isInsideHidden = true;
			}

			this.interior = interior;
			this.slotIndex = slotIndex;
			this.slotType = slotType;
		}

		/// <summary>
		/// Marks the unit as being outside.
		/// </summary>
		public void SetOutside()
		{
			if( !this.isInside )
			{
				return;
			}


			// - Interior fields.

			InteriorModule.GetSlot( interior, this.slotType, this.slotIndex, out InteriorModule.Slot slot, out HUDInteriorSlot slotHud );
			
			slot.objInside = null;

			slotHud.SetHealth( null );
			if( this.slotType == InteriorModule.SlotType.Worker )
			{
				slotHud.SetVisible( false );
			}
			else
			{
				slotHud.ClearSprite();
			}

			// -

			this.transform.position = this.interior.EntranceWorldPosition();
			this.transform.rotation = Quaternion.identity;

			this.navMeshAgent.enabled = true;
			
			if( this.isInsideHidden )
			{
				SubObject[] subObjects = this.GetSubObjects();

				for( int i = 0; i < subObjects.Length; i++ )
				{
					subObjects[i].gameObject.SetActive( true );
				}
			}

			this.interior = null;
			this.slotIndex = -1;
			this.isInsideHidden = false;
		}


		//
		//
		//


		protected override void OnObjSpawn()
		{
			if( this.population == PopulationSize.x1 )
			{
				LevelDataManager.factionData[this.factionId].populationCache += (int)this.population;
				if( this.factionId == LevelDataManager.PLAYER_FAC )
				{
					ResourcePanel.instance.UpdatePopulationDisplay( LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].populationCache );
				}
			}
		
			base.OnObjSpawn();
		}

		protected override void OnObjDestroyed()
		{
			// prevent killed units making the 'employ' input stuck in 'on' position.
			if( InputOverrideEmployment.affectedCivilian == this )
			{
				InputOverrideEmployment.DisableEmploymentInput();
			}

			LevelDataManager.factionData[this.factionId].populationCache -= (int)this.population;
			if( this.factionId == LevelDataManager.PLAYER_FAC )
			{
				ResourcePanel.instance.UpdatePopulationDisplay( LevelDataManager.factionData[this.factionId].populationCache );
			}

			base.OnObjDestroyed();
		}





		public void OnMouseEnterListener() => this.hud.ConditionalShow();
		public void OnMouseStayListener() { }
		public void OnMouseExitListener() => this.hud.ConditionalHide();

		public override void OnDisplay() => UnitDisplayManager.Display( this );
		public override void OnHide() => UnitDisplayManager.Hide( this );


		public bool CanChangePopulation()
		{
#warning figure out something for the pop locking.
			return !this.isPopulationLocked;
		}
	}
}
 