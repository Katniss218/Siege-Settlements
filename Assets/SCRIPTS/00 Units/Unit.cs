using KFF;
using SS.DataStructures;
using SS.Projectiles;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Units
{
	/// <summary>
	/// A class that represents a basic Unit.
	/// </summary>
	public class Unit : Damageable, IFactionMember, ISelectable, IDefinableBy<UnitDefinition>
	{
		public string id { get; private set; }

		public int factionId { get; private set; }

		public void SetFaction( int id )
		{
			this.factionId = id;
			this.meshRenderer.material.SetColor( "_FactionColor", FactionManager.factions[id].color );
		}

		// TODO! ----- New "data" struct that holds variables saved with the level.
		
		
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

			this.meshFilter.mesh = def.mesh.Item2;
			this.meshRenderer.material = UnitUtils.CreateMaterial( FactionManager.factions[this.factionId].color, def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.5f );
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
		}

		public override void Heal( float amount )
		{
			base.Heal( amount );
			
			this.meshRenderer.material.SetFloat( "_Dest", 1 - this.healthPercent );
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

			Unit unitComponent = container.AddComponent<Unit>();
			unitComponent.SetFaction( factionId );
			unitComponent.AssignDefinition( def ); // FIXME! - melee & ranged are a part of the definition, yet are not changed by this method.

			if( def.isMelee )
			{
				MeleeComponent melee = container.AddComponent<MeleeComponent>();

				melee.armorPenetration = def.meleeArmorPenetration;
				melee.damage = def.meleeDamage;
				melee.damageType = def.meleeDamageType;
				melee.attackCooldown = def.meleeAttackCooldown;
				melee.attackRange = def.meleeAttackRange;
			}

			if( def.isRanged )
			{
				RangedComponent ranged = container.AddComponent<RangedComponent>();

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


			return container;
		}
	}
}