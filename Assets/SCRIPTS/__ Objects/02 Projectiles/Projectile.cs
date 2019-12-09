using SS.Diplomacy;
using SS.Objects.Modules;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SS.Objects.Projectiles
{
	public class Projectile : SSObject
	{
		public bool isStuck { get; set; }

		public float blastRadius { get; set; }

		public DamageSource damageSource { get; set; }

		public AudioClip hitSound { get; set; }
		public AudioClip missSound { get; set; }

		public int ownerFactionIdCache = -1; // Osed when the owner is dead, but the projectile needs to keep knowing it's supposed faction Id.
		private RangedModule __owner;
		public RangedModule owner
		{
			get
			{
				return this.__owner;
			}
			set
			{
				if( value == null )
				{
					this.ownerFactionIdCache = -1;
				}
				else
				{
					this.ownerFactionIdCache = (value.ssObject as IFactionMember).factionMember.factionId;
				}
				this.__owner = value;
			}
		}

		

		public void MakeStuck()
		{
			if( this.isStuck )
			{
				return;
			}
			this.GetComponent<TimerHandler>().RestartTimer(); // reset the timer to count again from after being stuck.

			this.GetComponent<Projectile>().isStuck = true;

			Object.Destroy( this.GetComponent<RotateAlongVelocity>() );
			Object.Destroy( this.GetComponent<Rigidbody>() );
		}

		private void __MakeStuckOrDestroy( bool canGetStuck )
		{
			if( canGetStuck )
			{
				this.MakeStuck();
			}
			else
			{
				Object.Destroy( gameObject );
			}
			AudioManager.PlaySound( this.missSound );
		}

		public void DamageAndStuckLogic( Damageable hitDamageable, FactionMember hitFactionMember, bool canGetStuck )
		{
			if( this.blastRadius == 0.0f )
			{
				if( hitDamageable == null )
				{
					this.__MakeStuckOrDestroy( canGetStuck );
				}
				else
				{
					if( FactionMember.CanTarget( this.ownerFactionIdCache, hitFactionMember ) )
					{
						hitDamageable.TakeDamage( this.damageSource.damageType, this.damageSource.GetRandomizedDamage(), this.damageSource.armorPenetration );

						AudioManager.PlaySound( this.hitSound );
						Object.Destroy( this.gameObject );
					}
				}
			}
			else
			{
				// only masking the damageable ones.
				Collider[] col = Physics.OverlapSphere( this.transform.position, this.blastRadius, ObjectLayer.UNITS_MASK | ObjectLayer.BUILDINGS_MASK | ObjectLayer.HEROES_MASK );

				for( int i = 0; i < col.Length; i++ )
				{
					SSObject potentialDamagee = col[i].GetComponent<SSObject>();
					if( !FactionMember.CanTarget( this.ownerFactionIdCache, (potentialDamagee as IFactionMember).factionMember ) )
					{
						continue;
					}

					float distance = Vector3.Distance( this.transform.position, col[i].transform.position );
					if( distance >= this.blastRadius )
					{
						continue;
					}

					float damageScale = (distance / this.blastRadius);

					float damageScaledToDist = this.damageSource.GetRandomizedDamage() * damageScale;
					if( damageScaledToDist <= 0 )
					{
						Debug.LogWarning( "Damage scaled to distance was less than or equal to 0 (" + damageScaledToDist + ")." );
						continue;
					}
					(potentialDamagee as IDamageable).damageable.TakeDamage( this.damageSource.damageType, damageScaledToDist, this.damageSource.armorPenetration );
				}

				if( hitDamageable == null )
				{
					this.__MakeStuckOrDestroy( canGetStuck );
				}
				else
				{
					if( FactionMember.CanTarget( this.ownerFactionIdCache, hitFactionMember ) )
					{
						AudioManager.PlaySound( this.hitSound );
						Object.Destroy( this.gameObject );
					}
				}
			}
		}
	}
}