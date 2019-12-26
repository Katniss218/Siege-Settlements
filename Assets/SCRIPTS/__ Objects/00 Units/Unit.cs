using SS.Objects.Modules;
using SS.Objects.SubObjects;
using SS.UI;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects.Units
{
	public class Unit : SSObjectDFS, IHUDHolder, IDamageable, INavMeshAgent, IFactionMember, IMouseOverHandlerListener, IPopulationScaler
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

		private PopulationSize __population = PopulationSize.x1;
		public PopulationSize population
		{
			get
			{
				return this.__population;
			}
			set
			{
				// Recalculate here, before setting '__population'. (we need the from size - '__population', and the to size - 'value')
#warning Damageable needs to play damage sound when the health gets down due to damage, but not when it goes down due to unit split.

				PopulationSize populationBefore = this.__population;

				float populationRatio = (float)value / (float)populationBefore;

				// override.
				// Calculate health first, assign later. Prevents weird behaviour due to clamping, which prevents invalid values from being assigned.
				float newHealthMax = this.healthMax * populationRatio;
				float newHealth = this.health * populationRatio;

				this.healthMax = newHealthMax;
				this.health = newHealth;

#warning TODO! - unit definitions specify how big it's hitbox is per each population. It's some sort of scaling factor. It shows how big a single unit is.
#warning TODO! - unit definitions can limit how big can the units get. E.g. Elephants (which are pretty big in scale) can only be 1x or 2x.
				// We can make sure that e.g. Elephants 1x can't go on top of tower by just blocking elephants from going inside of tower.

				float x = 0.5f;
				float z = 0.5f;

				if( value == PopulationSize.x1 )
				{
					x *= 1;
					z *= 1;
				}
				else if( value == PopulationSize.x2 )
				{
					x *= 2;
					z *= 1;
				}
				else if( value == PopulationSize.x4 )
				{
					x *= 2;
					z *= 2;
				}
				else if( value == PopulationSize.x8 )
				{
					x *= 4;
					z *= 2;
				}
				this.size = new Vector3( x / 2, this.size.y, z / 2 );

				InventoryModule[] inventories = this.GetModules<InventoryModule>();
				for( int i = 0; i < inventories.Length; i++ )
				{
					for( int j = 0; j < inventories[i].slotCount; j++ )
					{
						inventories[i].SetCapacityOverride( j, (int)(inventories[i].GetCapacity( j ) * (float)value) );
					}
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
#warning TODO! - ranged module spread set to auto from hitbox size. (emits randomly from object's hitbox's top surface) - or position, if n/a.

#warning What if 8x unit goes inside a tower, and creates 4x, 2x, 1x? That's a lot of waste and small units.
				// Maybe make the population formation-independent. Slots will specify formation. This allows to make arbitrarily-sized formations.
				// Iteratively create the next biggest-most unit possible from the leftover pop.

				// ===SET size
				// ===SET health
				// ===SET maxHealth
				// ===SETM inventory size
#warning inventory drops items when size can no longer contain them (make sure to remove them first to prevent duplicates).
				// ===SETM melee damage
				// ===SETM ranged projectileCount
#warning inside slots can only be put on a "non-formation" unit.
#warning - We can actually make that, because we know what unit we are spawning at the time of spawn, co we can assign different Unit subclasses, depending on the unit sub-type.
				// ===SETS mesh
				// ===SETS material

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


		//
		//
		//

		
		public InteriorModule interior { get; private set; }

		public bool isInside
		{
			get { return this.interior != null; }
		}
		public bool isInsideHidden { get; private set; }

		/// <summary>
		/// Marks the unit as being inside.
		/// </summary>
		public void SetInside( InteriorModule interior )
		{
			if( this.isInside )
			{
				return;
			}

			if( (byte)this.population > (byte)interior.slots[0].maxPopulation )
			{
				throw new System.Exception( "Can't enter." );
			}

			if( !interior.slots[0].isEmpty )
			{
				throw new System.Exception( "Slot not empty." );
			}

			this.navMeshAgent.enabled = false;

			this.interior = interior;

			interior.slots[0].unitInside = this;

			this.transform.position = interior.SlotWorldPosition( interior.slots[0] );
			this.transform.rotation = interior.SlotWorldRotation( interior.slots[0] );

			if( interior.slots[0].isHidden )
			{
				SubObject[] subObjects = this.GetSubObjects();

				for( int i = 0; i < subObjects.Length; i++ )
				{
					subObjects[i].gameObject.SetActive( false );
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

#warning Unit needs to have it's current inside slot cached.
			this.transform.position = this.interior.EntranceWorldPosition();
			this.transform.rotation = Quaternion.identity;

			this.navMeshAgent.enabled = true;

			interior.slots[0].unitInside = null;

			if( this.isInsideHidden )
			{
				SubObject[] subObjects = this.GetSubObjects();

				for( int i = 0; i < subObjects.Length; i++ )
				{
					subObjects[i].gameObject.SetActive( true );
				}
			}

			this.interior = null;
			this.isInsideHidden = false;
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

		private Vector3 __size;
		public Vector3 size
		{
			get
			{
				return this.__size;
			}
			set
			{
				this.__size = value;
				this.navMeshAgent.radius = value.x < value.z ? value.x * 0.5f : value.z * 0.5f;
				this.navMeshAgent.height = value.y;
				this.collider.size = value;
				this.collider.center = new Vector3( 0.0f, value.y * 0.5f, 0.0f );
			}
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

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), SSObjectDFS.GetHealthDisplay( this.health, this.healthMax ) );
			SelectionPanel.instance.obj.RegisterElement( "unit.health", healthUI.transform );
		}

		public override void OnHide()
		{

		}
	}
}