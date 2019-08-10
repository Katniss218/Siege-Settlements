﻿using KFF;
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
		private string id;
		public int factionId { get; private set; }

		public void SetFaction( int id )
		{
			this.factionId = id;
		}

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


		public GenericUnitInventory inventory { get; private set; }

		new private BoxCollider collider;
		private Transform gfxTransform;
		private MeshFilter meshFilter;
		private MeshRenderer meshRenderer;
		private NavMeshAgent navMeshAgent;


		void Awake()
		{
			this.collider = this.GetComponent<BoxCollider>();
			gfxTransform = this.transform.GetChild( 0 );
			this.meshFilter = gfxTransform.GetComponent<MeshFilter>();
			this.meshRenderer = gfxTransform.GetComponent<MeshRenderer>();
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
			this.slashArmor = def.slashArmor;
			this.pierceArmor = def.pierceArmor;
			this.concussionArmor = def.concussionArmor;
			this.inventory = new GenericUnitInventory( def.inventorySize );
			this.collider.size = new Vector3( 1f, 0.1875f, 0.5f );
			this.collider.center = new Vector3( 0f, 0.1875f / 2f, 0f );

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
			if( type == DamageType.Slash )
			{
				amount *= 1 - (this.slashArmor - armorPenetration);
			}
			if( type == DamageType.Pierce )
			{
				amount *= 1 - (this.pierceArmor - armorPenetration);
			}
			if( type == DamageType.Concussion )
			{
				amount *= 1 - (this.concussionArmor - armorPenetration);
			}
			AudioManager.PlayNew( Main.instance.hit, 1.0f, 1.0f );
			this.health -= amount;
		}

		public override void Die()
		{
			Destroy( this.gameObject );
		}

		public static GameObject Create( UnitDefinition def, Vector3 pos, int factionId )
		{
			// this should add respective classes for generic & custom units.

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

			RangedComponent ranged = container.AddComponent<RangedComponent>();

			ranged.projectile = DataManager.FindDefinition<ProjectileDefinition>( "projectile.arrow" );
			ranged.armorPenetration = 0.0f;
			ranged.damage = 0.1f;
			ranged.localOffsetMin = new Vector3( -0.5f, 0.1875f, -0.25f );
			ranged.localOffsetMax = new Vector3( 0.5f, 0.1875f, 0.25f );
			ranged.attackRange = 6;
			ranged.attackCooldown = 3;
			ranged.projectileCount = 5;
			ranged.velocity = 2.5f;



			return container;
		}
	}
}