using KFF;
using SS.DataStructures;
using SS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Projectiles
{
	public class Projectile : MonoBehaviour, IFactionMember, IDefinableBy<ProjectileDefinition>
	{
		public string id { get; private set; }

		public int factionId { get; private set; }

		public DamageType damageType { get; private set; }
		public float damage { get; private set; }
		public float armorPenetration { get; private set; }

		private Transform graphicsTransform;
		new private Rigidbody rigidbody;
		private MeshFilter meshFilter;
		private MeshRenderer meshRenderer;

		private Transform owner = null;

		public void SetFaction( int id )
		{
			this.factionId = id;
		}

		public void AssignDefinition( ProjectileDefinition def )
		{
			this.id = def.id;

			this.meshFilter.mesh = def.mesh.Item2;
			this.meshRenderer.material = UnitUtils.CreateMaterial( Color.black, def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.5f );
		}

		public void SerializeData( KFFSerializer serializer )
		{
			serializer.WriteInt( "", "FactionId", this.factionId );
			serializer.WriteFloat( "", "Damage", this.damage );
		}

		public void DeserializeData( KFFSerializer serializer )
		{
			this.factionId = serializer.ReadInt( "FactionId" );
			this.damage = serializer.ReadFloat( "Damage" );
		}

		void Awake()
		{
			this.graphicsTransform = this.transform.GetChild( 0 );
			this.rigidbody = this.GetComponent<Rigidbody>();
			this.meshFilter = this.graphicsTransform.GetComponent<MeshFilter>();
			this.meshRenderer = this.graphicsTransform.GetComponent<MeshRenderer>();
		}
		
		void Start()
		{

		}
		
		void Update()
		{
			this.transform.forward = this.rigidbody.velocity.normalized;
		}

		private void OnTriggerEnter( Collider other )
		{
			if(this.owner != null && other.transform == this.owner )
			{
				return;
			}
			if( other.GetComponent<Projectile>() != null )
			{
				return;
			}
			Damageable od = other.GetComponent<Damageable>();
			if( od == null )
			{
				Destroy( this.gameObject );
				return;
			}
			if( od is IFactionMember )
			{
				IFactionMember f = (IFactionMember)od;
				if( f.factionId == this.factionId )//|| Main.currentRelations[f.factionId, this.factionMember.factionId] != FactionRelation.Enemy )
				{
					return;
				}
			}
			od.TakeDamage( this.damageType, this.damage, this.armorPenetration );
			AudioManager.PlayNew( Main.hit, 1.0f, 1.0f );
			Destroy( this.gameObject );
		}

		public static GameObject Create( ProjectileDefinition def, Vector3 position, Vector3 velocity, int factionId, float damage, Transform owner )
		{
			if( def == null )
			{
				throw new System.Exception( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Projectile (\"" + def.id + "\"), (f: " + factionId + ")" );

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );

			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();

			if( def.hasTrail )
			{
				gfx.AddParticleSystem( def.trailAmt, def.trailTexture.Item2, Color.white, def.trailStartSize, def.trailEndSize, 0.02f, def.trailLifetime );
			}

			Rigidbody rb = container.AddComponent<Rigidbody>();
			rb.velocity = velocity;

			SphereCollider col = container.AddComponent<SphereCollider>();
			col.radius = 0.0625f;
			col.isTrigger = true;

			container.transform.position = position;

			Projectile projectileComponent = container.AddComponent<Projectile>();
			projectileComponent.AssignDefinition( def );
			projectileComponent.SetFaction( factionId );

			return container;
		}
	}
}