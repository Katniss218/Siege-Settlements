using SS.Diplomacy;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SS.Objects.Projectiles
{
	public class Projectile : SSObject, IFactionMember
	{
		public bool isStuck { get; set; }

		public float blastRadius { get; set; }

		public DamageSource damageSource { get; set; }

		public AudioClip hitSound { get; set; }
		public AudioClip missSound { get; set; }


		private FactionMember _factionMember = null;
		public FactionMember factionMember
		{
			get
			{
				if( this._factionMember == null )
				{
					this._factionMember = this.GetComponent<FactionMember>();
				}
				return this._factionMember;
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
					if( this.factionMember.CanTargetAnother( hitFactionMember ) )
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
					if( !this.factionMember.CanTargetAnother( (potentialDamagee as IFactionMember).factionMember ) )
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
					if( this.factionMember.CanTargetAnother( hitFactionMember ) )
					{
						AudioManager.PlaySound( this.hitSound );
						Object.Destroy( this.gameObject );
					}
				}
			}
		}
	}
}