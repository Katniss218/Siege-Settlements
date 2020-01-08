using SS.Content;
using SS.Objects.Modules;
using SS.Objects.SubObjects;
using SS.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
					/*for( int j = 0; j < inventories[i].slotCount; j++ )
					{
						inventories[i].slotGroups[j].capacityOverride = (int)(inventories[i].slotGroups[j].capacity * (float)value);
					}*/
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

		private static int avoidancePriorityIncrement = 10;

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
					this.navMeshAgent.avoidancePriority = avoidancePriorityIncrement++;
					if( avoidancePriorityIncrement > 99 )
					{
						avoidancePriorityIncrement = 10;
					}
				}
				else
				{
					navMeshAgent.avoidancePriority = 1;
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
			
			this.navMeshAgent.enabled = false;

			this.interior = interior;
			this.slotType = slotType;
			this.slotIndex = slotIndex;

			slot.objInside = this;

			this.transform.position = interior.SlotWorldPosition( slot );
			this.transform.rotation = interior.SlotWorldRotation( slot );
			
			slotHud.SetSprite( this.icon );

			if( slot.isHidden )
			{
				SubObject[] subObjects = this.GetSubObjects();

				for( int j = 0; j < subObjects.Length; j++ )
				{
					subObjects[j].gameObject.SetActive( false );
				}

				this.isInsideHidden = true;
			}
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

			this.transform.position = this.interior.EntranceWorldPosition();
			this.transform.rotation = Quaternion.identity;

			this.navMeshAgent.enabled = true;

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


		public override void OnDisplay()
		{
			SelectionPanel.instance.obj.SetIcon( this.icon );

			SelectionPanel.instance.obj.displayNameText.text = this.displayName;

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 25.0f, -25.0f ), new Vector2( 200.0f, 25.0f ), new Vector2( 0.0f, 1.0f ), new Vector2( 0.0f, 1.0f ), new Vector2( 0.0f, 1.0f ) ), SSObjectDFS.GetHealthDisplay( this.health, this.healthMax ) );
			SelectionPanel.instance.obj.RegisterElement( "unit.health", healthUI.transform );
		}

		public override void OnHide()
		{

		}

		public bool CanChangePopulation()
		{
			if( this.isPopulationLocked )
			{
#warning figure out something for the pop locking.
				//Debug.Log( "PL: " + this.guid );
				return false;
			}
			SSModule[] modules = this.GetModules();
			for( int i = 0; i < modules.Length; i++ )
			{
				if( modules[i] is IPopulationChangeBlockerModule )
				{
					if( !((IPopulationChangeBlockerModule)modules[i]).CanChangePopulation() )
					{
						//Debug.Log( "PL: " + this.guid + " -    " + modules[i].moduleId );
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
		/// <param name="targetPopulation">Beacon unit after splitting will be this size.</param>
		public static List<Unit> Split( Unit beacon, PopulationSize? targetPopulation )
		{
			// If target pop is equal to this pop - don't split.
			// else
			// - beacon becomes the specified size, and additional units are spawned.
			// - only additional units are added to return list.

			byte populationPool = (byte)beacon.population;

			byte populationTarget = (byte)targetPopulation;

			if( populationTarget > populationPool )
			{
				throw new Exception( "Tried to split unit into bigger unit. That doesn't make sense. Don't do that." );
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

			while( populationPool > 0 )
			{
				PopulationSize newSize = PopulationSize.x1;
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
 