using SS.DataStructures;
using SS.Projectiles;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Units
{
	[RequireComponent( typeof( BoxCollider ) )]
	[RequireComponent( typeof( NavMeshAgent ) )]
	[RequireComponent( typeof( Rigidbody ) )]
	/// <summary>
	/// A class that represents a basic Unit (units can move, can be interacted with, and have a faction).
	/// </summary>
	public class Unit : Damageable, IFactionMember, ISelectable, IDefinableBy<UnitDefinition>
	{
		/// <summary>
		/// The definition's Id.
		/// </summary>
		public string id { get; private set; }

		public int factionId { get; private set; }

		public void SetFaction( int id )
		{
			this.factionId = id;
			Color color = FactionManager.factions[id].color;
			this.ui.SetFactionColor( color );
			this.meshRenderer.material.SetColor( "_FactionColor", color );
		}
		
		private UnitUI ui;
		
		private Transform graphicsTransform;
		new private Rigidbody rigidbody;
		new private BoxCollider collider;
		private MeshFilter meshFilter;
		private MeshRenderer meshRenderer;
		private NavMeshAgent navMeshAgent;


		void Awake()
		{
			this.collider = this.GetComponent<BoxCollider>();
			this.graphicsTransform = this.transform.GetChild( 0 );
			this.rigidbody = this.GetComponent<Rigidbody>();
			this.meshFilter = this.graphicsTransform.GetComponent<MeshFilter>();
			this.meshRenderer = this.graphicsTransform.GetComponent<MeshRenderer>();
			this.navMeshAgent = this.GetComponent<NavMeshAgent>();
		}

		void Start()
		{

		}

		void Update()
		{
			if( transform.hasChanged )
			{
				ui.transform.position = Main.camera.WorldToScreenPoint( this.transform.position );
			}
		}

		public void AssignDefinition( UnitDefinition def )
		{
			this.id = def.id;
			this.healthMax = def.healthMax;
			this.Heal();
			this.slashArmor = def.slashArmor;
			this.pierceArmor = def.pierceArmor;
			this.concussionArmor = def.concussionArmor;
			this.collider.size = new Vector3( def.radius * 2, def.height, def.radius * 2 );
			this.collider.center = new Vector3( 0f, def.height / 2f, 0f );
			this.navMeshAgent.radius = def.radius;
			this.navMeshAgent.height = def.height;
			this.navMeshAgent.speed = def.movementSpeed;
			this.navMeshAgent.angularSpeed = def.rotationSpeed;

			// If the new unit is melee, setup the melee module.
			if( def.isMelee )
			{
				// If the unit already has melee module (was melee before definition change).
				MeleeModule melee = this.GetComponent<MeleeModule>();
				if( melee == null )
					melee = this.gameObject.AddComponent<MeleeModule>();

				melee.armorPenetration = def.meleeArmorPenetration;
				melee.damage = def.meleeDamage;
				melee.damageType = def.meleeDamageType;
				melee.attackCooldown = def.meleeAttackCooldown;
				melee.attackRange = def.meleeAttackRange;
			}

			// If the new unit is ranged, setup the ranged module.
			if( def.isRanged )
			{
				// If the unit already has ranged module (was ranged before definition change).
				RangedModule ranged = this.GetComponent<RangedModule>();
				if( ranged == null )
					ranged = this.gameObject.AddComponent<RangedModule>();

				ranged.projectile = DataManager.FindDefinition<ProjectileDefinition>( def.rangedProjectileId );
				ranged.armorPenetration = def.rangedArmorPenetration;
				ranged.damage = def.rangedDamage;
				ranged.localOffsetMin = def.rangedLocalOffsetMin;
				ranged.localOffsetMax = def.rangedLocalOffsetMax;
				ranged.attackRange = def.rangedAttackRange;
				ranged.attackCooldown = def.rangedAttackCooldown;
				ranged.projectileCount = def.rangedProjectileCount;
				ranged.velocity = def.rangedVelocity;
			}

			// Apply the mesh.
			this.meshFilter.mesh = def.mesh.Item2;

			// Apply the material's properties.
			this.meshRenderer.material = Main.materialFactionColoredDestroyable;
			this.meshRenderer.material.SetColor( "_FactionColor", FactionManager.factions[this.factionId].color );
			this.meshRenderer.material.SetTexture( "_Albedo", def.albedo.Item2 );

			this.meshRenderer.material.SetTexture( "_Normal", def.normal.Item2 );
			this.meshRenderer.material.SetTexture( "_Emission", null );
			this.meshRenderer.material.SetFloat( "_Metallic", 0.0f );
			this.meshRenderer.material.SetFloat( "_Smoothness", 0.5f );
		}
		
		/// <summary>
		/// Makes the unit move to the specified position.
		/// </summary>
		public void SetDestination( Vector3 position )
		{
			this.navMeshAgent.SetDestination( position );
		}

		public override void Heal()
		{
			base.Heal();

			this.meshRenderer.material.SetFloat( "_Dest", 1 - this.healthPercent );
			this.ui.SetHealthFill( this.healthPercent );
		}

		public override void Heal( float amount )
		{
			base.Heal( amount );
			
			this.meshRenderer.material.SetFloat( "_Dest", 1 - this.healthPercent );
			this.ui.SetHealthFill( this.healthPercent );
		}

		/// <summary>
		/// Makes the Unit take damage.
		/// </summary>
		/// <param name="type">The type of damage taken.</param>
		/// <param name="amount">The amount of damage.</param>
		/// <param name="armorPenetration">The percentage of armor penetration (formula for reduction is 'src_damage *= 1 - (armor - penetr)').</param>
		public override void TakeDamage( DamageType type, float amount, float armorPenetration )
		{
			base.TakeDamage( type, amount, armorPenetration );

			this.meshRenderer.material.SetFloat( "_Dest", 1 - this.healthPercent );
			this.ui.SetHealthFill( this.healthPercent );
		}

		public override void Die()
		{
			base.Die();

			Destroy( this.ui.gameObject );
			// for breakup make several meshes that are made up of the original one, attach physics to them.
			// let the physics play for a few seconds (randomize durations for each piece), then disable rigidbodies, and pull them downwards, reducing their scale at the same time.
			// when the scale reaches 0.x, remove the piece.

			// also, play a poof from some particle system for smoke or something at the moment of death.

			SelectionManager.Deselect( this );
		}

		public static GameObject Create( UnitDefinition def, Vector3 pos, int factionId )
		{
			if( def == null )
			{
				throw new System.Exception( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Unit (\"" + def.id + "\"), (f: " + factionId + ")" );

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );

			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();


			BoxCollider collider = container.AddComponent<BoxCollider>();

			container.transform.position = pos;

			Rigidbody rigidbody = container.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;

			NavMeshAgent navMeshAgent = container.AddComponent<NavMeshAgent>();
			navMeshAgent.acceleration = 8;
			navMeshAgent.stoppingDistance = 0.125f;

			Unit unitComponent = container.AddComponent<Unit>();
			unitComponent.ui = Instantiate( Main.unitUI, Main.camera.WorldToScreenPoint( pos ), Quaternion.identity, Main.worldUIs ).GetComponent<UnitUI>();
			unitComponent.SetFaction( factionId );
			unitComponent.AssignDefinition( def );

			

			


			return container;
		}
	}
}