using Katniss.Utils;
using SS.Levels.SaveStates;
using UnityEngine;

namespace SS.Projectiles
{
	public static class ProjectileCreator
	{
		private const string GAMEOBJECT_NAME = "Projectile";



		public static string GetDefinitionId( GameObject gameObject )
		{
			if( gameObject.layer != ObjectLayer.PROJECTILES )
			{
				throw new System.Exception( "The specified GameObject is not a projectile." );
			}

			Projectile projectile = gameObject.GetComponent<Projectile>();
			return projectile.defId;
		}

		/// <summary>
		/// Creates a new ProjectileData from a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to extract the save state from. Must be a projectile.</param>
		public static ProjectileData GetSaveState( GameObject gameObject )
		{
			if( gameObject.layer != ObjectLayer.PROJECTILES )
			{
				throw new System.Exception( "The specified GameObject is not a projectile." );
			}
			ProjectileData data = new ProjectileData();
			
			data.position = gameObject.transform.position;

			Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
			data.velocity = rigidbody.velocity;


			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			data.factionId = factionMember.factionId;

			DamageSource damageSource = gameObject.GetComponent<DamageSource>();
			data.damageTypeOverride = damageSource.damageType;
			data.damageOverride = damageSource.damage;
			data.armorPenetrationOverride = damageSource.armorPenetration;
			
			return data;
		}

		public static GameObject Create( ProjectileDefinition def, ProjectileData data )
		{
			const float LIFETIME = 15.0f;
			const float HITBOX_RADIUS = 0.0625f;

			if( def == null )
			{
				throw new System.ArgumentNullException( "Definition can't be null." );
			}
			GameObject container = new GameObject( GAMEOBJECT_NAME + " (\"" + def.id + "\"), (f: " + data.factionId + ")" );
			container.layer = ObjectLayer.PROJECTILES;

			GameObject gfx = new GameObject( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME );
			gfx.transform.SetParent( container.transform );

			container.transform.SetPositionAndRotation( data.position, Quaternion.identity );

			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateOpaque( def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f );

			if( def.hasTrail )
			{
				gfx.AddParticleSystem( def.trailAmt, def.trailTexture.Item2, Color.white, def.trailStartSize, def.trailEndSize, 0.02f, def.trailLifetime );
			}

			Projectile projectile = container.AddComponent<Projectile>();
			projectile.defId = def.id;

			Rigidbody rigidbody = container.AddComponent<Rigidbody>();
			rigidbody.velocity = data.velocity;

			SphereCollider col = container.AddComponent<SphereCollider>();
			col.radius = HITBOX_RADIUS; // hitbox size
			col.isTrigger = true;


			TimerHandler t = container.AddComponent<TimerHandler>();
			t.duration = LIFETIME; // lifetime
			t.onTimerEnd.AddListener( () => Object.Destroy( container ) );

			FactionMember f = container.AddComponent<FactionMember>();
			f.factionId = data.factionId;

			DamageSource damageSource = new DamageSource( data.damageTypeOverride, data.damageOverride, data.armorPenetrationOverride );

			container.AddComponent<RotateAlongVelocity>();

			TriggerOverlapHandler triggerOverlapHandler = container.AddComponent<TriggerOverlapHandler>();
			triggerOverlapHandler.onTriggerEnter.AddListener( ( Collider other ) =>
			{
				// If it hit other projectile, do nothing.
				if( other.GetComponent<TriggerOverlapHandler>() != null ) // this can later be switched to a script editable by the player.
				{
					return;
				}
				Damageable hitDamageable = other.GetComponent<Damageable>();
				if( hitDamageable == null )
				{
					// when the projectile hits non-damageable object, it sticks into it (like an arrow).
					Object.Destroy( container.GetComponent<RotateAlongVelocity>() );
					Object.Destroy( container.GetComponent<Rigidbody>() );
					Object.Destroy( container.transform.GetChild( 0 ).GetComponent<ParticleSystem>() );
					AudioManager.PlayNew( def.missSoundEffect.Item2 );
					return;
				}

				// If it has factionMember, check if the faction is enemy, otherwise, just deal damage.
				FactionMember hitFactionMember = other.GetComponent<FactionMember>();
				if( hitFactionMember != null )
				{
					if( hitFactionMember.factionId == container.GetComponent<FactionMember>().factionId )
					{
						return;
					}
				}
				hitDamageable.TakeDamage( damageSource.damageType, damageSource.GetRandomizedDamage(), damageSource.armorPenetration );
				AudioManager.PlayNew( def.hitSoundEffect.Item2 );
				Object.Destroy( container );
			} );

			return container;
		}
	}
}