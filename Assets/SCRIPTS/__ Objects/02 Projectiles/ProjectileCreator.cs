﻿using SS.Diplomacy;
using SS.Levels.SaveStates;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SS.Projectiles
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
			projectile.defId = def.id;
			projectile.hitSound = def.hitSoundEffect;
			projectile.missSound = def.missSoundEffect;
			

			// Set the projectile's lifetime and reset the lifetime timer.
			TimerHandler t = gameObject.GetComponent<TimerHandler>();
			t.duration = def.lifetime;
			t.RestartTimer(); // DON'T just StartTimer(), RestartTimer() in case the timer has been started before.
			
			if( data.isStuck )
			{
				MakeStuck( gameObject );
				gameObject.transform.rotation = data.stuckRotation;
			}
			else
			{
				Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
				rigidbody.velocity = data.velocity;
			}
			
			// Set the faction id.
			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			factionMember.factionId = data.factionId;

			// Set the damage information.
			projectile.damageSource = new DamageSource( data.damageTypeOverride, data.damageOverride, data.armorPenetrationOverride );
			
			// Make the projectile do something when it hits objects.
			TriggerOverlapHandler triggerOverlapHandler = gameObject.AddComponent<TriggerOverlapHandler>();
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
					if( def.getsStuckInGround )
					{
						if( !projectile.isStuck )
						{
							MakeStuck( gameObject );
							AudioManager.PlaySound( projectile.missSound );
						}
					}
					else
					{
						Object.Destroy( gameObject );
						AudioManager.PlaySound( projectile.missSound );
					}
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
				AudioManager.PlaySound( projectile.hitSound );
				Object.Destroy( gameObject );
			} );

			//
			//    MODULES
			//

			SSObjectCreator.AssignModules( gameObject, def, data );
		}

		private static void MakeStuck( GameObject unstuckProjectile )
		{
			unstuckProjectile.GetComponent<TimerHandler>().RestartTimer(); // reset the timer to count again from after being stuck.

			unstuckProjectile.GetComponent<Projectile>().isStuck = true;

			Object.Destroy( unstuckProjectile.GetComponent<RotateAlongVelocity>() );
			Object.Destroy( unstuckProjectile.GetComponent<Rigidbody>() );
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

			FactionMember factionMember = container.AddComponent<FactionMember>();
			
			// Make the projectile rotate to face the direction of flight.
			container.AddComponent<RotateAlongVelocity>();

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

			Projectile projectile = gameObject.GetComponent<Projectile>();
			if( projectile.guid == null )
			{
				throw new Exception( "Guid was not assigned." );
			}
			data.guid = projectile.guid.Value;

			data.position = gameObject.transform.position;

			Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
			if( projectile.isStuck )
			{
				data.isStuck = true;
				data.stuckRotation = gameObject.transform.rotation;
			}
			else
			{
				data.isStuck = false;
				data.velocity = rigidbody.velocity;
			}

			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			data.factionId = factionMember.factionId;

			DamageSource damageSource = gameObject.GetComponent<Projectile>().damageSource;
			data.damageTypeOverride = damageSource.damageType;
			data.damageOverride = damageSource.damage;
			data.armorPenetrationOverride = damageSource.armorPenetration;

			//
			// MODULES
			//

			SSObjectCreator.ExtractModules( gameObject, data );


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