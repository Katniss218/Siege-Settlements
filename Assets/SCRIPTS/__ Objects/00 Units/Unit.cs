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
	[UseHud( typeof( UnitHUD ), "hud" )]
	public class Unit : SSObjectDFC, IMovable, IMouseOverHandlerListener, IInteriorUser
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


		//
		//
		//


		/// <summary>
		/// If true, the population can't be changed without forcing.
		/// </summary>
		public bool isPopulationLocked { get; set; }

		/// <summary>
		/// Contains the maximum population allowed for this unit.
		/// </summary>
		public byte? populationSizeLimit { get; set; } = null;

		/// <summary>
		/// Contains the current population size of the unit (Read Only) - Use SetPopulation( ... ) to set.
		/// </summary>
		public PopulationSize population { get; private set; } = PopulationSize.x1;

		/// <summary>
		/// Sets the population, with additional functionality.
		/// </summary>
		/// <param name="newPopulation">The new population.</param>
		/// <param name="scaleStats">If true, the stats will be rescaled to fit the new population size.</param>
		/// <param name="force">If true, it bypasses all additional checks, such as is the population locked, population limits, etc.</param>
		public void SetPopulation( PopulationSize newPopulation, bool scaleStats, bool force )
		{
			// If not forced assignment - check if the population can be changed, throw exception if not.
			if( !force )
			{
				if( !this.CanChangePopulation() )
				{
					throw new System.Exception( "Can't set non-forced population: Population can't be changed currently." );
				}
				if( this.populationSizeLimit != null && (byte)newPopulation > this.populationSizeLimit )
				{
					throw new System.Exception( "Can't set non-forced population: " + (byte)newPopulation + " is larger than population limit (" + this.populationSizeLimit + ")." );
				}
			}

			PopulationSize oldPopulation = this.population;

			this.population = newPopulation;

			if( this.factionId != SSObjectDFC.FACTIONID_INVALID && oldPopulation != newPopulation )
			{
				if( oldPopulation != PopulationSize.x1 )
				{
					LevelDataManager.factionData[this.factionId].populationCache -= (int)oldPopulation;
				}
				LevelDataManager.factionData[this.factionId].populationCache += (int)newPopulation;
				if( this.factionId == LevelDataManager.PLAYER_FAC )
				{
					ResourcePanel.instance.UpdatePopulationDisplay(
						LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].populationCache,
						LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].maxPopulationCache );
				}
			}

			if( scaleStats )
			{
				PopulationUnitUtils.ScaleStats( this, oldPopulation, newPopulation );
			}
		}

		/// <summary>
		/// Checks if the unit can change population. Returns true if nothing is blocking the population change.
		/// </summary>
		public bool CanChangePopulation()
		{
			if( this.isPopulationLocked )
			{
				return false;
			}

			SSModule[] modules = this.GetModules();
			for( int i = 0; i < modules.Length; i++ )
			{
				if( modules[i] is IPopulationBlocker )
				{
					if( !((IPopulationBlocker)modules[i]).CanChangePopulation() )
					{
						return false;
					}
				}
			}

			return true;
		}


		//
		//
		//


		/// <summary>
		/// Returns the hud that's attached to this object.
		/// </summary>
		public UnitHUD hud { get; set; }
		public override HUDDFC hudDFC { get { return this.hud; } }






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


		private float __movementSpeed;
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

		private float __rotationSpeed;
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
		/// The interior module the unit is currently in. Null if not is any interior (Read Only).
		/// </summary>
		public InteriorModule interior { get; private set; }
		/// <summary>
		/// The slot type that the unit is currently inside (Read Only).
		/// </summary>
		public InteriorModule.SlotType slotType { get; private set; }
		/// <summary>
		/// The slot of the interior module the unit is currently in (Read Only).
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
		public void MakeInside( InteriorModule interior, InteriorModule.SlotType slotType, int slotIndex )
		{
			if( this.isInside )
			{
				return;
			}

			// - Interior fields

			(InteriorModule.Slot slot, HUDInteriorSlot slotHud) = interior.GetSlot( slotType, slotIndex );

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
		public void MakeOutside()
		{
			if( !this.isInside )
			{
				return;
			}


			// - Interior fields.

			(InteriorModule.Slot slot, HUDInteriorSlot slotHud) = interior.GetSlot( this.slotType, this.slotIndex );

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
					ResourcePanel.instance.UpdatePopulationDisplay(
						LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].populationCache,
						LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].maxPopulationCache );
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

			// Update population cache and display.
			LevelDataManager.factionData[this.factionId].populationCache -= (int)this.population;

			if( this.factionId == LevelDataManager.PLAYER_FAC )
			{
				ResourcePanel.instance.UpdatePopulationDisplay(
					LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].populationCache,
					LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].maxPopulationCache );
			}

			base.OnObjDestroyed();
		}





		public void OnMouseEnterListener() => this.hud.ConditionalShow();
		public void OnMouseStayListener() { }
		public void OnMouseExitListener() => this.hud.ConditionalHide();

		public override void OnDisplay() => UnitDisplayManager.Display( this );
		public override void OnHide() => UnitDisplayManager.Hide( this );
	}
}