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
	public class Unit : Damageable, IFactionMember, IDefinableBy<UnitDefinition>, ISelectable
	{
		public string id { get; private set; }

		public int factionId { get; private set; }

		public void SetFaction( int id )
		{
			this.factionId = id;
		}
		// TODO! ----- New "data" struct that holds variables saved with the level.
		public override float health { get; protected set; }

		public override float healthMax { get; protected set; }

		/// <summary>
		/// Percentage reduction of the slash-type damage.
		/// </summary>
		public float slashArmor { get; private set; }
		/// <summary>
		/// Percentage reduction of the pierce-type damage.
		/// </summary>
		public float pierceArmor { get; private set; }
		/// <summary>
		/// Percentage reduction of the concussion-type damage.
		/// </summary>
		public float concussionArmor { get; private set; }

		/// <summary>
		/// Contains the items that the unit is currently holding (Read Only).
		/// </summary>
		public GenericUnitInventory inventory { get; private set; }

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
			this.inventory = new GenericUnitInventory( def.inventorySize );
			this.collider.size = new Vector3( def.radius * 2, def.height, def.radius * 2 );
			this.collider.center = new Vector3( 0f, def.height / 2f, 0f );

			this.meshFilter.mesh = def.mesh.Item2;
			this.meshRenderer.material = UnitUtils.CreateMaterial( Color.red, def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.5f );
		}

		public void SerializeData( KFFSerializer serializer )
		{
			serializer.WriteInt( "", "FactionId", this.factionId );
			serializer.WriteFloat( "", "Health", this.health );
			if( this.inventory.amtMax > 0 )
			{
				serializer.WriteString( "", "CarriedResourceId", this.inventory.resId );
				serializer.WriteInt( "", "CarriedResourceAmount", this.inventory.amt );
			}
		}

		public void DeserializeData( KFFSerializer serializer )
		{
			this.factionId = serializer.ReadInt( "FactionId" );
			this.health = serializer.ReadFloat( "Health" );
			if( this.inventory.amtMax > 0 )
			{
				string resId = serializer.ReadString( "CarriedResourceId" );
				int resAmt = serializer.ReadUShort( "CarriedResourceAmount" );
				this.inventory.Clear();
				this.inventory.Add( resId, (ushort)resAmt );
			}
		}

		/// <summary>
		/// Makes the unit move to the specified position.
		/// </summary>
		public void SetDestination( Vector3 position )
		{
			this.navMeshAgent.SetDestination( position );
		}

		/// <summary>
		/// Makes the Unit take damage.
		/// </summary>
		/// <param name="type">The type of damage taken.</param>
		/// <param name="amount">The amount of damage.</param>
		/// <param name="armorPenetration">The percentage of armor penetration (formula for reduction is 'src_damage *= 1 - (armor - penetr)').</param>
		public override void TakeDamage( DamageType type, float amount, float armorPenetration )
		{
			if( amount < 0 )
			{
				throw new System.Exception( "Can't take less than 0 damage" );
			}
			float mult = 0;
			if( type == DamageType.Slash )
			{
				mult = 1 - (this.slashArmor - armorPenetration);
			}
			if( type == DamageType.Pierce )
			{
				mult = 1 - (this.pierceArmor - armorPenetration);
			}
			if( type == DamageType.Concussion )
			{
				mult = 1 - (this.concussionArmor - armorPenetration);
			}
			if( mult > 1 )
			{
				mult = 1;
			}
			this.health -= amount * mult;
			if( this.health <= 0 )
			{
				this.Die();
			}
		}

		/// <summary>
		/// Makes the unit die.
		/// </summary>
		public override void Die()
		{
			Destroy( this.gameObject );
		}

		public static GameObject Create( UnitDefinition def, Vector3 pos, int factionId )
		{
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
			navMeshAgent.radius = def.radius;
			navMeshAgent.height = def.height;
			navMeshAgent.speed = def.movementSpeed;
			navMeshAgent.angularSpeed = def.rotationSpeed;
			navMeshAgent.stoppingDistance = 0.125f;

			Unit unitComponent = container.AddComponent<Unit>();
			unitComponent.AssignDefinition( def );
			unitComponent.SetFaction( factionId );

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