using SS.Diplomacy;
using SS.Levels.SaveStates;
using SS.Objects.Modules;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SS.Objects.Projectiles
{
	public static class ProjectileCreator
	{
		const float DEFAULT_HITBOX_RADIUS = 0.0625f;

		private const string GAMEOBJECT_NAME = "Projectile";
		
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static void SetDefData( GameObject gameObject, ProjectileDefinition def, ProjectileData data )
		{

			//
			//    SUB-OBJECTS
			//

			SSObjectCreator.AssignSubObjects( gameObject, def );
			
			//
			//    CONTAINER GAMEOBJECT
			//

			// Set the position/movement information.
			gameObject.transform.SetPositionAndRotation( data.position, Quaternion.identity );

			// Set the projectile's size.
			SphereCollider col = gameObject.GetComponent<SphereCollider>();
			col.radius = DEFAULT_HITBOX_RADIUS;
			
			// Set the projectile's native parameters.
			Projectile projectile = gameObject.GetComponent<Projectile>();
			projectile.definitionId = def.id;
			projectile.displayName = def.displayName;
			projectile.hitSound = def.hitSoundEffect;
			projectile.missSound = def.missSoundEffect;
			projectile.blastRadius = def.blastRadius;
			projectile.canGetStuck = def.canGetStuck;
			if( data.owner == null )
			{
				projectile.owner = null;
				projectile.ownerFactionIdCache = data.ownerFactionIdCache;
			}
			else
			{
				projectile.owner = SSObject.Find( data.owner.Item1 ).GetModule<RangedModule>( data.owner.Item2 );
			}
			

			// Set the projectile's lifetime and reset the lifetime timer.
			TimerHandler t = gameObject.GetComponent<TimerHandler>();
			t.duration = def.lifetime;
			t.StartTimer();
			
			if( data.isStuck )
			{
				projectile.MakeStuck();
				gameObject.transform.rotation = data.stuckRotation;
			}
			else
			{
				Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
				rigidbody.velocity = data.velocity;
			}
			
			// Set the damage information.
			projectile.damageSource = new DamageSource( data.damageTypeOverride, data.damageOverride, data.armorPenetrationOverride );
			
			//
			//    MODULES
			//

			SSObjectCreator.AssignModules( gameObject, def, data );
		}
		
		private static GameObject CreateProjectile( Guid guid )
		{
			GameObject container = new GameObject( GAMEOBJECT_NAME );
			container.layer = ObjectLayer.PROJECTILES;

			//
			//    CONTAINER GAMEOBJECT
			//

			Projectile projectile = container.AddComponent<Projectile>();
			projectile.guid = guid;

			container.AddComponent<Rigidbody>();

			SphereCollider collider = container.AddComponent<SphereCollider>();
			collider.isTrigger = true;

			// Make the projectile destroy itself after certain time.
			TimerHandler timerHandler = container.AddComponent<TimerHandler>();
			timerHandler.onTimerEnd.AddListener( () => Object.Destroy( container ) );
						
			// Make the projectile rotate to face the direction of flight.
			container.AddComponent<RotateAlongVelocity>();

			return container;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		/// <summary>
		/// Creates a new ProjectileData from a GameObject.
		/// </summary>
		/// <param name="projectile">The GameObject to extract the save state from. Must be a projectile.</param>
		public static ProjectileData GetData( Projectile projectile )
		{
			if( projectile.guid == null )
			{
				throw new Exception( "Guid was not assigned." );
			}

			ProjectileData data = new ProjectileData();
			data.guid = projectile.guid;

			data.position = projectile.transform.position;

			Rigidbody rigidbody = projectile.GetComponent<Rigidbody>();
			if( projectile.isStuck )
			{
				data.isStuck = true;
				data.stuckRotation = projectile.transform.rotation;
			}
			else
			{
				data.isStuck = false;
				data.velocity = rigidbody.velocity;
			}
			
			data.ownerFactionIdCache = projectile.factionId;

			DamageSource damageSource = projectile.GetComponent<Projectile>().damageSource;
			data.damageTypeOverride = damageSource.damageType;
			data.damageOverride = damageSource.damage;
			data.armorPenetrationOverride = damageSource.armorPenetration;

			if( projectile.owner == null )
			{
				data.owner = null;
			}
			else {
				data.owner = new Tuple<Guid, Guid>(
				projectile.owner.ssObject.guid,
				projectile.owner.moduleId
				);
			}

			//
			// MODULES
			//

			SSObjectCreator.ExtractModulesToData( projectile, data );


			return data;
		}
		

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		public static GameObject CreateEmpty( Guid guid )
		{
			GameObject gameObject = CreateProjectile( guid );
			
			return gameObject;
		}

		public static GameObject Create( ProjectileDefinition def, ProjectileData data )
		{
			GameObject gameObject = CreateProjectile( data.guid );

			SetDefData( gameObject, def, data );
			
			return gameObject;
		}
	}
}