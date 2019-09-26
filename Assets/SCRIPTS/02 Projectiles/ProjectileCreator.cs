using Katniss.Utils;
using SS.Levels.SaveStates;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SS.Projectiles
{
	public static class ProjectileCreator
	{
		const float DEFAULT_LIFETIME = 15.0f;
		const float DEFAULT_HITBOX_RADIUS = 0.0625f;

		private const string GAMEOBJECT_NAME = "Projectile";



		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		private static void SetProjectileDefinition( GameObject gameObject, ProjectileDefinition def )
		{

			//
			//    GRAPHICS GAMEOBJECT
			//

			GameObject gfx = gameObject.transform.Find( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME ).gameObject;
			
			MeshFilter meshFilter = gfx.GetComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			MeshRenderer meshRenderer = gfx.GetComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateOpaque( def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f );


			//
			//    CONTAINER GAMEOBJECT
			//

			// Set the projectile's size.
			SphereCollider col = gameObject.GetComponent<SphereCollider>();
			col.radius = DEFAULT_HITBOX_RADIUS;
			
			// Set the projectile's native parameters.
			Projectile projectile = gameObject.GetComponent<Projectile>();
			projectile.defId = def.id;
			projectile.hitSound = def.hitSoundEffect.Item2;
			projectile.missSound = def.missSoundEffect.Item2;
			

			// Set the projectile's lifetime and reset the lifetime timer.
			TimerHandler t = gameObject.GetComponent<TimerHandler>();
			t.duration = DEFAULT_LIFETIME;
			t.ResetTimer();


			// Remove old trail (if present).
			ParticleSystem particleSystem = gfx.GetComponent<ParticleSystem>();
			if( particleSystem != null )
			{
				Object.Destroy( particleSystem );
			}
			// If projectile has trail, add it.
			if( def.hasTrail )
			{
				gfx.AddParticleSystem( def.trailAmt, def.trailTexture.Item2, Color.white, def.trailStartSize, def.trailEndSize, 0.02f, def.trailLifetime );
			}
		}

		private static void SetProjectileData( GameObject gameObject, ProjectileData data )
		{

			//
			//    CONTAINER GAMEOBJECT
			//

			// Set the position/movement information.
			gameObject.transform.SetPositionAndRotation( data.position, Quaternion.identity );

			Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
			rigidbody.velocity = data.velocity;
			
			// Set the globally unique identifier.
			Projectile projectile = gameObject.GetComponent<Projectile>();
			projectile.guid = data.guid;

			// Set the faction id.
			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			factionMember.factionId = data.factionId;

			// Set the damage information.
			projectile.damageSource = new DamageSource( data.damageTypeOverride, data.damageOverride, data.armorPenetrationOverride );
		}

		private static GameObject CreateProjectile()
		{
			GameObject container = new GameObject( GAMEOBJECT_NAME );
			container.layer = ObjectLayer.PROJECTILES;


			//
			//    GRAPHICS GAMEOBJECT
			//

			GameObject gfx = new GameObject( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME );
			gfx.transform.SetParent( container.transform );
			
			gfx.AddComponent<MeshFilter>();

			gfx.AddComponent<MeshRenderer>();


			//
			//    CONTAINER GAMEOBJECT
			//

			Projectile projectile = container.AddComponent<Projectile>();

			container.AddComponent<Rigidbody>();

			SphereCollider collider = container.AddComponent<SphereCollider>();
			collider.isTrigger = true;

			// Make the projectile destroy itself after certain time.
			TimerHandler timerHandler = container.AddComponent<TimerHandler>();
			timerHandler.onTimerEnd.AddListener( () => Object.Destroy( container ) );

			FactionMember factionMember = container.AddComponent<FactionMember>();
			
			// Make the projectile rotate to face the direction of flight.
			container.AddComponent<RotateAlongVelocity>();

			// Make the projectile do something when it hits objects.
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
					AudioManager.PlayNew( projectile.missSound );
					return;
				}

				// If it has factionMember, check if the faction is enemy, otherwise, just deal damage.
				FactionMember hitFactionMember = other.GetComponent<FactionMember>();
				if( hitFactionMember != null )
				{
					if( hitFactionMember.factionId == factionMember.factionId )
					{
						return;
					}
				}
				hitDamageable.TakeDamage( projectile.damageSource.damageType, projectile.damageSource.GetRandomizedDamage(), projectile.damageSource.armorPenetration );
				AudioManager.PlayNew( projectile.hitSound );
				Object.Destroy( container );
			} );

			return container;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static string GetDefinitionId( GameObject gameObject )
		{
			if( !Projectile.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid projectile." );
			}

			Projectile projectile = gameObject.GetComponent<Projectile>();
			return projectile.defId;
		}

		/// <summary>
		/// Creates a new ProjectileData from a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to extract the save state from. Must be a projectile.</param>
		public static ProjectileData GetData( GameObject gameObject )
		{
			if( !Projectile.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid projectile." );
			}

			ProjectileData data = new ProjectileData();

			data.guid = gameObject.GetComponent<Projectile>().guid;

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


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		public static void SetData( GameObject gameObject, ProjectileData data )
		{
			if( !Projectile.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid projectile." );
			}
			SetProjectileData( gameObject, data );
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

#warning use the deserialized data's guid value when creating objects. then after all objs are created, use that same data instance to assign other data values.
		
		public static GameObject CreateEmpty( Guid guid, ProjectileDefinition def )
		{
			GameObject gameObject = CreateProjectile();

			SetProjectileDefinition( gameObject, def );

			Projectile projectile = gameObject.GetComponent<Projectile>();
			projectile.guid = guid;

			return gameObject;
		}

		public static GameObject Create( ProjectileDefinition def, ProjectileData data )
		{
			GameObject gameObject = CreateProjectile();

			SetProjectileDefinition( gameObject, def );
			SetProjectileData( gameObject, data );
			
			return gameObject;
		}
	}
}