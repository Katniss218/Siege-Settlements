using Katniss.Utils;
using SS.Levels.SaveStates;
using SS.Objects.Modules;
using System;
using UnityEngine;

namespace SS.Objects.Projectiles
{
	public static class ProjectileCreator
	{
		const float DEFAULT_HITBOX_RADIUS = 0.0625f;

		private const string GAMEOBJECT_NAME = "Projectile";

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static void SetData( Projectile projectile, ProjectileData data )
		{
			//
			//    CONTAINER GAMEOBJECT
			//

			// Set the position/movement information.
			projectile.transform.SetPositionAndRotation( data.position, Quaternion.identity );

			// Set the projectile's native parameters.
			if( projectile.guid != data.guid )
			{
				throw new Exception( "Mismatched guid." );
			}
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
			TimerHandler timerHandler = projectile.GetComponent<TimerHandler>();
			timerHandler.StartTimer();

			if( data.isStuck )
			{
				projectile.MakeStuck();
				projectile.transform.rotation = data.stuckRotation;
			}
			else
			{
				Rigidbody rigidbody = projectile.GetComponent<Rigidbody>();
				rigidbody.velocity = data.velocity;
			}

			// Set the damage information.
			projectile.damageType = data.damageTypeOverride;
			projectile.damage = data.damageOverride;
			projectile.armorPenetration = data.armorPenetrationOverride;

			//
			//    MODULES
			//

			SSObjectCreator.AssignModuleData( projectile, data );
		}

		private static Projectile CreateProjectile( ProjectileDefinition def, Guid guid )
		{
			GameObject gameObject = new GameObject( GAMEOBJECT_NAME + " - '" + def.id + "'" );
			gameObject.layer = ObjectLayer.PROJECTILES;

			//
			//    CONTAINER GAMEOBJECT
			//

			Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();

			Projectile projectile = gameObject.AddComponent<Projectile>();
			projectile.guid = guid;
			projectile.definitionId = def.id;
			projectile.displayName = def.displayName;
			projectile.hitSound = def.hitSoundEffect;
			projectile.missSound = def.missSoundEffect;
			projectile.blastRadius = def.blastRadius;
			projectile.canGetStuck = def.canGetStuck;


			SphereCollider collider = gameObject.AddComponent<SphereCollider>();
			collider.radius = DEFAULT_HITBOX_RADIUS;
			collider.isTrigger = true;

			// Make the projectile destroy itself after certain time.
			TimerHandler timerHandler = gameObject.AddComponent<TimerHandler>();
			timerHandler.duration = def.lifetime;
			timerHandler.onTimerEnd.AddListener( () => projectile.Destroy() );

			// Make the projectile rotate to face the direction of flight.
			RotateAlongVelocity rotateAlongVelocity = gameObject.AddComponent<RotateAlongVelocity>();
			rotateAlongVelocity.direction = RotateAlongVelocity.Direction.Forward;

			//
			//    SUB-OBJECTS
			//

			SSObjectCreator.AssignSubObjects( projectile, def );

			//
			//    MODULES
			//

			SSObjectCreator.AssignModules( projectile, def );

			return projectile;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		

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

			data.damageTypeOverride = projectile.damageType;
			data.damageOverride = projectile.damage;
			data.armorPenetrationOverride = projectile.armorPenetration;

			if( projectile.owner == null )
			{
				data.owner = null;
			}
			else
			{
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

		public static Projectile Create( ProjectileDefinition def, Guid guid )
		{
			return CreateProjectile( def, guid );
		}
	}
}