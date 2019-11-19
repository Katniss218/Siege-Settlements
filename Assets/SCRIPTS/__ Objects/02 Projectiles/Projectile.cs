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
				AudioManager.PlaySound( this.missSound );
			}
			else
			{
				Object.Destroy( gameObject );
				AudioManager.PlaySound( this.missSound );
			}
		}

		public void DamageAndStuckLogic( Damageable damageableOther, FactionMember factionMemberOther, bool canGetStuck )
		{
			if( this.blastRadius == 0.0f )
			{
				if( damageableOther == null )
				{
					this.__MakeStuckOrDestroy( canGetStuck );
				}
				else
				{
					if( this.factionMember.CanTargetAnother( factionMemberOther ) )
					{
						damageableOther.TakeDamage( this.damageSource.damageType, this.damageSource.GetRandomizedDamage(), this.damageSource.armorPenetration );

						AudioManager.PlaySound( this.hitSound );
						Object.Destroy( this.gameObject );
					}
				}
			}
			else
			{
				Collider[] overlap = Physics.OverlapSphere( this.transform.position, this.blastRadius );

				for( int i = 0; i < overlap.Length; i++ )
				{
					Damageable dam = overlap[i].GetComponent<Damageable>();
					if( dam == null )
					{
						continue;
					}
					FactionMember facM = overlap[i].GetComponent<FactionMember>();
					if( this.factionMember.CanTargetAnother( factionMemberOther ) )
					{
						float distance = Vector3.Distance( this.transform.position, overlap[i].transform.position );
						if( distance >= this.blastRadius )
						{
							continue;
						}

						float damageScale = (distance / this.blastRadius);

						float damageScaledToDist = this.damageSource.GetRandomizedDamage() * damageScale;
						if( damageScaledToDist <= 0 )
						{
							continue;
						}
						dam.TakeDamage( this.damageSource.damageType, damageScaledToDist, this.damageSource.armorPenetration );
					}
				}

				if( damageableOther == null )
				{
					this.__MakeStuckOrDestroy( canGetStuck );
				}
				else
				{
					if( this.factionMember.CanTargetAnother( factionMemberOther ) )
					{
						AudioManager.PlaySound( this.hitSound );
						Object.Destroy( this.gameObject );
					}
				}
			}
		}
	}
}