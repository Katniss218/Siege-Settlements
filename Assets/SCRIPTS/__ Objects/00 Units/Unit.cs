using SS.Content;
using SS.Objects.Modules;
using SS.Objects.SubObjects;
using SS.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SS.Objects.Units
{
	public class Unit : SSObjectDFS, IHUDHolder, IMovable, IMouseOverHandlerListener, IPopulationScaler, IInteriorUser
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

				float populationRatio = (float)value / (float)populationBefore;

				// override.
				// Calculate health first, assign later. Prevents weird behaviour due to clamping, which prevents invalid values from being assigned.
				float newHealthMax = this.healthMax * populationRatio;
				float newHealth = this.health * populationRatio;

				this.healthMax = newHealthMax;
				this.health = newHealth;
				
				this.SetSize( value );

				InventoryModule[] inventories = this.GetModules<InventoryModule>();
				for( int i = 0; i < inventories.Length; i++ )
				{
					InventoryModule.SlotGroup[] slotGroups = inventories[i].GetSlots();

					for( int j = 0; j < slotGroups.Length; j++ )
					{
						slotGroups[j].capacityOverride = (int)(slotGroups[j].capacity * (float)value);
					}

					inventories[i].SetSlots( slotGroups );
				}
				RangedModule[] ranged = this.GetModules<RangedModule>();
				for( int i = 0; i < ranged.Length; i++ )
				{
					ranged[i].projectileCountOverride = (int)(ranged[i].projectileCount * (float)value);
				}
				MeleeModule[] melee = this.GetModules<MeleeModule>();
				for( int i = 0; i < melee.Length; i++ )
				{
					melee[i].damageOverride = melee[i].damage * (float)value;
				}
				
#warning attack speed instead of damage/arrow count? BUT I want it to not be perfectly rythmic - instead with randomization.
				// maybe make the attack modules have a pool of available attacks (like bows that are reloaded currently). and the bigger pop, the more of them are.


				MeshPredicatedSubObject[] meshPopulationSubObjects = this.GetSubObjects<MeshPredicatedSubObject>();
				for( int i = 0; i < meshPopulationSubObjects.Length; i++ )
				{
					meshPopulationSubObjects[i].lookupKey = (int)value;
				}

				this.__population = value;
			}
		}

		/// <summary>
		/// Returns the hud that's attached to this object.
		/// </summary>
		public HUD hud { get; set; }


		public bool hasBeenHiddenSinceLastDamage { get; set; }


		private static int avoidancePriorityIncremented = 10;
		/// <summary>
		/// Returns a value for the avoidance priority (helps prevent units blocking each other in tight spaces - they'll just push the troublesome unit aside).
		/// </summary>
		public static int GetNextAvPriority( bool employed )
		{
			// 10 - 49, 50 - 89
			avoidancePriorityIncremented++;
			if( avoidancePriorityIncremented == 50 )
			{
				avoidancePriorityIncremented = 10;
			}
			return employed ? avoidancePriorityIncremented + 40 : avoidancePriorityIncremented;
		}

		public const int AVOIDANCE_PRORITY_GENERAL = 1;



#warning This should be handled by a separate class "CivilianUnitExtension" - move everything civilian-related to there, and call it's ondisplay, etc. when the unit is displayed, etc.
#warning IsCivilian is simply an indication that cue is present.
		private bool __isCivilian;
		public bool isCivilian
		{
			get
			{
				return this.__isCivilian;
			}
			set
			{
				this.__isCivilian = value;
				// if the unit is civilian, make sure to set it's avoidance priority to 10-99, otherwise set them to 1. Do this to reduce civilians getting stuck on each other.
				if( value )
				{
					CivilianUnitExtension cue = this.gameObject.AddComponent<CivilianUnitExtension>();

					if( cue.workplace == null )
					{
						this.navMeshAgent.avoidancePriority = GetNextAvPriority( false );
					}
					else
					{
						this.navMeshAgent.avoidancePriority = GetNextAvPriority( true );
					}

					cue.onAutomaticDutyToggle.AddListener( () =>
					{
						if( !Selection.IsDisplayed( this ) )
						{
							return;
						}

						Transform t = ActionPanel.instance.GetActionButton( "civilian.autoduty" );
						if( cue.isOnAutomaticDuty )
						{
							t.GetComponent<Image>().sprite = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/autodutyoff" );
						}
						else
						{
							t.GetComponent<Image>().sprite = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/autoduty" );
						}
					} );

					cue.onEmploy.AddListener( () =>
					{
						if( !Selection.IsDisplayed( this ) )
						{
							return;
						}

						ActionPanel.instance.Clear( "civilian.autoduty" );
						ActionPanel.instance.Clear( "civilian.employ" );
						this.CreateUnemployButton( cue );
						ActionPanel.instance.Clear( "unit.ap.pickup" );
						ActionPanel.instance.Clear( "unit.ap.dropoff" );
					} );

					cue.onUnemploy.AddListener( () =>
					{
						if( !Selection.IsDisplayed( this ) )
						{
							return;
						}

						ActionPanel.instance.Clear( "civilian.unemploy" );
						this.CreateAutodutyButton( cue );
						this.CreateEmployButton( cue );
						this.CreateQueryButtons();
					} );
				}
				else
				{
					Object.Destroy( this.GetComponent<CivilianUnitExtension>() );

					navMeshAgent.avoidancePriority = AVOIDANCE_PRORITY_GENERAL;
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
				if( this.movementSpeedOverride == null ) // if an override is not present - set the speed.
				{
					this.navMeshAgent.speed = value;
				}
			}
		}

		float? __movementSpeedOverride;
		public float? movementSpeedOverride
		{
			get
			{
				return this.__movementSpeedOverride;
			}
			set
			{
				this.__movementSpeedOverride = value;
				if( value != null ) // if the new override is not null - set the speed.
				{
					this.navMeshAgent.speed = value.Value;
				}
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
				if( this.rotationSpeedOverride == null ) // if an override is not present - set the speed.
				{
					this.navMeshAgent.angularSpeed = value;
				}
			}
		}
		float? __rotationSpeedOverride;
		public float? rotationSpeedOverride
		{
			get
			{
				return this.__rotationSpeedOverride;
			}
			set
			{
				this.__rotationSpeedOverride = value;
				if( value != null ) // if the new override is not null - set the speed.
				{
					this.navMeshAgent.angularSpeed = value.Value;
				}
			}
		}

		private Vector3 __sizePerPopulation;
		public Vector3 sizePerPopulation
		{
			get
			{
				return this.__sizePerPopulation;
			}
			set
			{
				this.__sizePerPopulation = value;
				this.SetSize( this.population );
			}
		}

		private void SetSize( PopulationSize population )
		{
			float x = this.sizePerPopulation.x;
			float z = this.sizePerPopulation.z;

			if( population == PopulationSize.x1 )
			{
				x *= 1;
				z *= 1;
			}
			else if( population == PopulationSize.x2 )
			{
				x *= 2;
				z *= 1;
			}
			else if( population == PopulationSize.x4 )
			{
				x *= 2;
				z *= 2;
			}
			else if( population == PopulationSize.x8 )
			{
				x *= 4;
				z *= 2;
			}
			this.navMeshAgent.radius = ((x + z) / 2f) / 2;
			this.navMeshAgent.height = this.sizePerPopulation.y;
			this.collider.size = new Vector3( x, this.sizePerPopulation.y, z );
			this.collider.center = new Vector3( 0.0f, this.sizePerPopulation.y * 0.5f, 0.0f );
		}
		
		//
		//
		//
		
		void Update()
		{
			if( this.hud.isVisible )
			{
				this.hud.transform.position = Main.camera.WorldToScreenPoint( this.transform.position );
			}
			
			if( !this.hasBeenHiddenSinceLastDamage )
			{
				return;
			}
			if( Main.isHudForcedVisible )
			{
				return;
			}
			if( Selection.IsSelected( this ) )
			{
				return;
			}
			if( Time.time > this.lastDamageTakenTimestamp + SSObject.HUD_DAMAGE_DISPLAY_DURATION )
			{
				if( MouseOverHandler.currentObjectMouseOver == this.gameObject )
				{
					return;
				}
				this.hud.isVisible = false;
				this.hasBeenHiddenSinceLastDamage = false;
			}
		}


		// = = = = = = = = = = =
		// = = = = = = = = = = =
		// = = = = = = = = = = =


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

			InteriorModule.Slot slot = null;
			HUDInterior.Element slotHud = null;
			if( slotType == InteriorModule.SlotType.Generic )
			{
				slot = interior.slots[slotIndex];
				slotHud = interior.hudInterior.slots[slotIndex];
			}
			else if( slotType == InteriorModule.SlotType.Worker )
			{
				slot = interior.workerSlots[slotIndex];
				slotHud = interior.hudInterior.workerSlots[slotIndex];
			}
			slot.objInside = this;
			slotHud.SetSprite( this.icon );

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

			InteriorModule.Slot slot = null;
			HUDInterior.Element slotHud = null;
			if( this.slotType == InteriorModule.SlotType.Generic )
			{
				slot = interior.slots[slotIndex];
				slotHud = interior.hudInterior.slots[slotIndex];
			}
			else if( this.slotType == InteriorModule.SlotType.Worker )
			{
				slot = interior.workerSlots[slotIndex];
				slotHud = interior.hudInterior.workerSlots[slotIndex];
			}
			slot.objInside = null;
			slotHud.ClearSprite();

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


		public void OnMouseEnterListener()
		{
			if( Main.isHudForcedVisible ) { return; }

			if( Selection.IsSelected( this ) )
			{
				return;
			}
			this.hud.isVisible = true;
		}

		public void OnMouseStayListener()
		{

		}

		public void OnMouseExitListener()
		{
			if( Main.isHudForcedVisible ) { return; }

			if( this.hasBeenHiddenSinceLastDamage )
			{
				return;
			}
			if( Selection.IsSelected( this ) )
			{
				return;
			}
			this.hud.isVisible = false;
		}
		
		private void CreateAutodutyButton( CivilianUnitExtension cue )
		{
			Sprite s = cue.isOnAutomaticDuty ?
						AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/autodutyoff" ) :
						AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/autoduty" );

			ActionPanel.instance.CreateButton( "civilian.autoduty", s, "Toggle automatic duty", "Makes the civilian deliver resources to where they are needed.", () =>
			{
				cue.isOnAutomaticDuty = !cue.isOnAutomaticDuty;
			} );
		}

		private void CreateEmployButton( CivilianUnitExtension cue )
		{
			ActionPanel.instance.CreateButton( "civilian.employ", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/employ" )
				, "Employ civilian", "Makes the civilian employed at a specified workplace.", () =>
			{
				InputOverrideEmployment.EnableEmploymentInput( cue );
			} );
		}

		private void CreateUnemployButton( CivilianUnitExtension cue )
		{
			ActionPanel.instance.CreateButton( "civilian.unemploy", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/unemploy" )
				, "Fire civilian", "Makes the civilian unemployed.", () =>
			{
				cue.workplace.UnEmploy( cue );
			} );
		}

		protected override void OnObjDestroyed()
		{
			// prevent killed units making the employ input stuck in 'on' position.
			if( InputOverrideEmployment.cueTracker == this )
			{
				InputOverrideEmployment.DisableEmploymentInput();
			}
			base.OnObjDestroyed();
		}

		private void CreateQueryButtons()
		{
#warning it's possible to enable both inputs, don't do that.
			ActionPanel.instance.CreateButton( "unit.ap.dropoff", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/dropoff" )
			, "Drop off resources", "Select storage to drop off the resources at. OR, deliver resources to construction sites, barracks, etc.", () =>
			{
				InputOverrideDropOffQuery.EnableInput();
			} );

			ActionPanel.instance.CreateButton( "unit.ap.pickup", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/pickup" )
			, "Pick up resources", "Select storage or resource deposit to pick the resources up from.", () =>
			{
				InputOverridePickUpQuery.EnableInput();
			} );
		}

		public override void OnDisplay()
		{
			SelectionPanel.instance.obj.SetIcon( this.icon );

			SelectionPanel.instance.obj.displayNameText.text = this.displayName;

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 25.0f, -25.0f ), new Vector2( 200.0f, 25.0f ), new Vector2( 0.0f, 1.0f ), new Vector2( 0.0f, 1.0f ), new Vector2( 0.0f, 1.0f ) ), SSObjectDFS.GetHealthDisplay( this.health, this.healthMax ) );
			SelectionPanel.instance.obj.RegisterElement( "unit.health", healthUI.transform );

			if( !this.IsDisplaySafe() )
			{
				return;
			}

			bool blockManual = false;
			if( this.isCivilian )
			{
				CivilianUnitExtension cue = this.GetComponent<CivilianUnitExtension>();

				if( cue.workplace == null )
				{
					this.CreateAutodutyButton( cue );
					this.CreateEmployButton( cue );
				}
				else
				{
					this.CreateUnemployButton( cue );
					blockManual = true;
				}
			}

			if( this.hasInventoryModule && !blockManual )
			{
				CreateQueryButtons();
			}

		}

		public override void OnHide()
		{
			
		}
		
		public bool CanChangePopulation()
		{
			if( this.isPopulationLocked )
			{
#warning figure out something for the pop locking.
				return false;
			}
			SSModule[] modules = this.GetModules();
			for( int i = 0; i < modules.Length; i++ )
			{
				if( modules[i] is IPopulationChangeBlockerModule )
				{
					if( !((IPopulationChangeBlockerModule)modules[i]).CanChangePopulation() )
					{
						return false;
					}
				}
			}
			return true;
		}
		

		// // // =-    -    -  -       -  -  -   -    -   -  -  -      -      -    -  -  -  -  -        -  -  -     -   -    -


		// // //
		// // //			STATIC METHODS
		// // //


		// // // =-    -      - -       -  -  -   -     -  -   - -      -      -    -  -  -  -  -        -  -  -     -  -    -


		/// <summary>
		/// Tries to join specified units with additional units to make a bigger unit.
		/// </summary>
		/// <param name="beacon">The unit to enlarge (increase population).</param>
		/// <param name="additional">Units to take the additional population from.</param>
		public static void Join( Unit beacon, List<Unit> additional )
		{
			byte selfPop = (byte)beacon.population;
			byte popTotal = selfPop; // total population of the new unit
			byte targetPop = 0; // total population of the new unit (if popTotal is a valid PopulationSize).

			float healthTotal = beacon.health;

			bool isSelectedAdditionalAny = false;

			for( int i = 0; i < additional.Count; i++ )
			{
				if( !isSelectedAdditionalAny )
				{
					if( Selection.IsSelected( additional[i] ) )
					{
						isSelectedAdditionalAny = true;
					}
				}

				popTotal += (byte)additional[i].population;

				// Find the population of the new, joined unit (total population of all units, clamped to highest valid population size).
				if( popTotal > selfPop )
				{
					healthTotal += additional[i].health;
					if( popTotal == 8 )
					{
						targetPop = 8;
						break;
					}
					if( popTotal == 4 )
					{
						targetPop = 4;
						break;
					}
					if( popTotal == 2 )
					{
						targetPop = 2;
						break;
					}
					if( popTotal == 1 )
					{
						targetPop = 1;
						break;
					}
				}
			}

			if( targetPop > selfPop )
			{
				int i = 0;
				do
				{
					selfPop += (byte)additional[i].population;
					
					additional[i].Die();

					i++;
					// Join as long as selfPop is less than targetPop AND as long as the selfPop is not a valid pop number.
				} while( selfPop < targetPop && (selfPop != 1 || selfPop != 2 || selfPop != 4 || selfPop != 8) );

				if( isSelectedAdditionalAny )
				{
					Selection.TrySelect( beacon );
				}
				// assign the new, "joined" population.
				beacon.population = (PopulationSize)selfPop;
				beacon.health = healthTotal;
			}
		}

		/// <summary>
		/// Splits the unit so that one of the results has population size of specified value.
		/// </summary>
		/// <param name="beacon">The unit to split.</param>
		/// <param name="desiredPopulation">Beacon unit after splitting will be this size.</param>
		public static List<Unit> Split( Unit beacon, PopulationSize? desiredPopulation )
		{
			// If target pop is equal to this pop - don't split.
			// else
			// - beacon becomes the specified size, and additional units are spawned.
			// - only additional units are added to return list.
			
			byte populationPool = (byte)beacon.population;

			byte populationTarget = 1;
			if( desiredPopulation == null )
			{
				populationTarget = beacon.population == PopulationSize.x1 ? (byte)1 : (byte)((int)beacon.population / 2);
			}
			else
			{
				populationTarget = (byte)desiredPopulation;
			}

			if( populationTarget >= populationPool )
			{
				throw new Exception( "Tried to split into bigger or equally-sized unit." );
			}

			populationPool -= populationTarget;
			if( populationPool != 0 ) // if the population changed
			{
				beacon.population = (PopulationSize)populationTarget;
			}

			float healthPercentSrc = beacon.healthPercent;

			List<Unit> ret = new List<Unit>();
			ret.Add( beacon );
			UnitDefinition beaconDef = DefinitionManager.GetUnit( beacon.definitionId );

			bool isSelected = Selection.IsSelected( beacon );

			// Split into new units (largest-possible) as long as there is enough population in the pool.
			while( populationPool > 0 )
			{
				PopulationSize newSize = PopulationSize.x1;
				// Find the largest unit possible to make from the available population pool.
				if( populationPool >= 8 )
				{
					populationPool -= 8;
					newSize = PopulationSize.x8;
				}
				else if( populationPool >= 4 )
				{
					populationPool -= 4;
					newSize = PopulationSize.x4;
				}
				else if( populationPool >= 2 )
				{
					populationPool -= 2;
					newSize = PopulationSize.x2;
				}
				else if( populationPool >= 1 )
				{
					populationPool -= 1;
					newSize = PopulationSize.x1;
				}

				
				Vector3 pos = beacon.transform.position + new Vector3( UnityEngine.Random.Range( -0.01f, 0.01f ), UnityEngine.Random.Range( -0.01f, 0.01f ), UnityEngine.Random.Range( -0.01f, 0.01f ) );
				Quaternion rot = beacon.transform.rotation;

				Unit u = UnitCreator.Create( beaconDef, Guid.NewGuid(), pos, rot, beacon.factionId ).GetComponent<Unit>();
				u.population = newSize;
				u.healthPercent = healthPercentSrc;
				
				ret.Add( u );

				if( isSelected )
				{
					Selection.TrySelect( u );
				}
			}
			return ret;
		}
	}
}
 