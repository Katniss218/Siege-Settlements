using Katniss.ModifierAffectedValues;
using SS.Objects.Modules;
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

				//float healthPercBefore = this.healthPercent;
				this.healthMax = this.healthMax * populationRatio;
				//this.healthMax["pop"] = (byte)value;
				this.health = this.health * populationRatio;

#warning TODO! - this scales linearly. we want square/rect.
#warning TODO! - unit definitions specify how long it's per each population. It's some sort of scaling factor. It shows how big a single unit is.
#warning TODO! - unit definitions can limit how big can the units get. E.g. Elephants (which are pretty big in scale) can only be 1x or 2x (to prevent overly HUGE units).
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
				this.size = new Vector3( x/2, this.size.y, z/2 );
				this.transform.GetChild( 0 ).localScale = new Vector3( x, 1, z ); // INSTEAD OF THIS - switch the meshes & materials depending on population.

				InventoryModule[] inventories = this.GetModules<InventoryModule>();
				for( int i = 0; i < inventories.Length; i++ )
				{
#warning each object & each module keep a reference to their default unchangable values. Data can modify the other value to whatever it wants to be.
				}
				RangedModule[] ranged = this.GetModules<RangedModule>();
				for( int i = 0; i < ranged.Length; i++ )
				{
					ranged[i].projectileCountOverride = ranged[i].projectileCount * (int)value;
				}
				MeleeModule[] melee = this.GetModules<MeleeModule>();
				for( int i = 0; i < melee.Length; i++ )
				{
#warning each object & each module keep a reference to their default unchangable values. Data can modify the other value to whatever it wants to be.
					melee[i].damage = (int)value;
				}
				// ===SET size
				// ===SET health
				// ===SET maxHealth
				// SETM inventory size
#warning inventory drops items when size can no longer contain them (make sure to remove them first to prevent duplicates).
				// SETM melee damage
				// SETM ranged projectileCount
				// SETM inside slotCount

				this.__population = value;
			}
		}

		public HUD hud { get; set; }

		public bool hasBeenHiddenSinceLastDamage { get; set; }


		//
		//
		//
		
		public FloatM movementSpeed { get; private set; }
		public FloatM rotationSpeed { get; private set; }

		private Vector3 __size;
		public Vector3 size { get
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

		private void InitModifierAffectedValues()
		{
			this.movementSpeed = new FloatM();
			this.movementSpeed.onAnyChangeCallback = () =>
			{
				this.navMeshAgent.speed = this.movementSpeed.value;
			};

			this.rotationSpeed = new FloatM();
			this.rotationSpeed.onAnyChangeCallback = () =>
			{
				this.navMeshAgent.angularSpeed = this.rotationSpeed.value;
			};
		}
		
		protected override void Awake()
		{
			base.Awake();

			this.InitModifierAffectedValues();
		}

		void Update()
		{
			if( this.hud.isVisible )
			{
#warning TODO! - only if the camera or transform has moved or rotated or scaled (cam).
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

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), "Health: " + (int)this.health + "/" + (int)this.healthMax );
			SelectionPanel.instance.obj.RegisterElement( "unit.health", healthUI.transform );
		}

		public override void OnHide()
		{

		}
	}
}