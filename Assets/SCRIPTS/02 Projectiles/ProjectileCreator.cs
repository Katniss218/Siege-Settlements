using Katniss.Utils;
using UnityEngine;

namespace SS.Projectiles
{
	public static class ProjectileCreator
	{
		public static GameObject Create( ProjectileDefinition def, Vector3 position, Vector3 velocity, int factionId, DamageType damageType, float damageOverride, float armorPenetration, Transform owner )
		{
			if( def == null )
			{
				throw new System.ArgumentNullException( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Projectile (\"" + def.id + "\"), (f: " + factionId + ")" );
			container.layer = ObjectLayer.PROJECTILES;

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );

			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateOpaque( def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f );

			if( def.hasTrail )
			{
				gfx.AddParticleSystem( def.trailAmt, def.trailTexture.Item2, Color.white, def.trailStartSize, def.trailEndSize, 0.02f, def.trailLifetime );
			}

			Rigidbody rb = container.AddComponent<Rigidbody>();
			rb.velocity = velocity;

			SphereCollider col = container.AddComponent<SphereCollider>();
			col.radius = 0.0625f; // hitbox size
			col.isTrigger = true;

			container.transform.position = position;

			TimerHandler t = container.AddComponent<TimerHandler>();
			t.duration = 15f; // lifetime
			t.onTimerEnd.AddListener( () => Object.Destroy( container ) );

			FactionMember f = container.AddComponent<FactionMember>();
			f.factionId = factionId;

			DamageSource damageSource = new DamageSource( damageType, damageOverride, armorPenetration );

			container.AddComponent<RotateAlongVelocity>();

			TriggerOverlapHandler toh = container.AddComponent<TriggerOverlapHandler>();
			toh.onTriggerEnter.AddListener(
			( GameObject proj, Collider other ) =>
			{
				// If it hit other projectile, do nothing.
				if( other.GetComponent<TriggerOverlapHandler>() != null ) // this can later be switched to a script editable by the player.
				{
					return;
				}
				Damageable od = other.GetComponent<Damageable>();
				if( od == null )
				{
					// when the projectile hits non-damageable object, it sticks into it (like an arrow).
					proj.isStatic = true;
					Object.Destroy( proj.GetComponent<RotateAlongVelocity>() );
					Object.Destroy( proj.GetComponent<Rigidbody>() );
					Object.Destroy( proj.transform.GetChild( 0 ).GetComponent<ParticleSystem>() );
					AudioManager.PlayNew( def.missSoundEffect.Item2, 1f, 1.0f );
					return;
				}

				// If it has factionMember, check if the faction is enemy, otherwise, just deal damage.
				FactionMember fac = other.GetComponent<FactionMember>();
				if( fac != null )
				{
					if( fac.factionId == proj.GetComponent<FactionMember>().factionId )
					{
						return;
					}
				}
				od.TakeDamage( damageSource.damageType, damageSource.GetRandomizedDamage(), damageSource.armorPenetration );
				AudioManager.PlayNew( def.hitSoundEffect.Item2, 1f, 1.0f );
				Object.Destroy( proj );
			} );

			return container;
		}
	}
}