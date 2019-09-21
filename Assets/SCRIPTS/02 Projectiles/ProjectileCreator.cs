using Katniss.Utils;
using UnityEngine;

namespace SS.Projectiles
{
	public static class ProjectileCreator
	{
		public static GameObject Create( ProjectileDefinition def, Vector3 position, Vector3 velocity, int factionId, DamageType damageTypeOverride, float damageOverride, float armorPenetrationOverride, Transform owner )
		{
			const float LIFETIME = 15.0f;
			const float HITBOX_RADIUS = 0.0625f;

			if( def == null )
			{
				throw new System.ArgumentNullException( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Projectile (\"" + def.id + "\"), (f: " + factionId + ")" );
			container.layer = ObjectLayer.PROJECTILES;

			GameObject gfx = new GameObject( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME );
			gfx.transform.SetParent( container.transform );

			container.transform.SetPositionAndRotation( position, Quaternion.identity );

			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateOpaque( def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f );

			if( def.hasTrail )
			{
				gfx.AddParticleSystem( def.trailAmt, def.trailTexture.Item2, Color.white, def.trailStartSize, def.trailEndSize, 0.02f, def.trailLifetime );
			}

			Rigidbody rigidbody = container.AddComponent<Rigidbody>();
			rigidbody.velocity = velocity;

			SphereCollider col = container.AddComponent<SphereCollider>();
			col.radius = HITBOX_RADIUS; // hitbox size
			col.isTrigger = true;


			TimerHandler t = container.AddComponent<TimerHandler>();
			t.duration = LIFETIME; // lifetime
			t.onTimerEnd.AddListener( () => Object.Destroy( container ) );

			FactionMember f = container.AddComponent<FactionMember>();
			f.factionId = factionId;

			DamageSource damageSource = new DamageSource( damageTypeOverride, damageOverride, armorPenetrationOverride );

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