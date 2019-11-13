﻿using SS.Diplomacy;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SS.Projectiles
{
	public class Projectile : SSObject
	{
		public static bool IsValid( GameObject gameObject )
		{
			if( gameObject.layer != ObjectLayer.PROJECTILES )
			{
				return false;
			} 
			if( gameObject.GetComponent<Projectile>() == null )
			{
				return false;
			}
			return true;
		}

		private static List<Projectile> _allProjectiles = new List<Projectile>();

		public static Projectile[] GetAllProjectiles()
		{
			return _allProjectiles.ToArray();
		}
		
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

		public void DamageAndStuckLogic( Damageable damageableOther, FactionMember factionMemberOther, bool canGetStuck )
		{			
			if( FactionMember.CanTargetAnother( factionMemberOther, this.factionMember ) )
			{
				if( this.blastRadius == 0.0f )
				{
					damageableOther.TakeDamage( this.damageSource.damageType, this.damageSource.GetRandomizedDamage(), this.damageSource.armorPenetration );
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
						if( FactionMember.CanTargetAnother( factionMemberOther, this.factionMember ) )
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
				}
				AudioManager.PlaySound( this.hitSound );
				Object.Destroy( gameObject );
				return;
			}

			if( damageableOther == null )
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
		}


		void OnEnable()
		{
			_allProjectiles.Add( this );
		}

		void OnDisable()
		{
			_allProjectiles.Remove( this );
		}
	}
}