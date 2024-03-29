﻿using Katniss.Utils;
using SS.Objects.Extras;
using SS.Objects.Modules;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SS.Objects.Projectiles
{
    public class Projectile : SSObject, IFactionMember
    {
        public bool isStuck { get; set; }

        public bool canGetStuck { get; set; }

        public float blastRadius { get; set; }

        public float damage;
        public float armorPenetration;
        public DamageType damageType;

        public float? originY { get; set; }

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
                    this.ownerFactionIdCache = (value.ssObject as IFactionMember).factionId;
                }
                this.__owner = value;
            }
        }



        public int factionId { get; set; }

        public UnityEvent_int_int onFactionChange { get; set; } = new UnityEvent_int_int();



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
                this.Destroy();
            }

            AudioManager.PlaySound( this.missSound, this.transform.position );
        }

        private void OnTriggerEnter( Collider other )
        {
            if( this.isStuck )
            {
                return;
            }

            SSObject otherSSObject = other.GetComponent<SSObject>();
            if( otherSSObject == null )
            {
                if( other.gameObject.layer == ObjectLayer.TERRAIN )
                {
                    this.OnHitSomething( null, null, this.canGetStuck );
                }
                return;
            }

            // If the object that was hit is non-SSObject or another projectile, do nothing.
            if( otherSSObject is Projectile )
            {
                return;
            }

            // If the object that was hit is not solid, do nothing.
            if( otherSSObject is Extra )
            {
                Extra extra = (Extra)otherSSObject;
                if( extra.obstacle == null )
                {
                    return;
                }
            }

            IDamageable damageableOther = other.GetComponent<IDamageable>();
            IFactionMember factionMemberOther = otherSSObject as IFactionMember;

            this.OnHitSomething( damageableOther, factionMemberOther, this.canGetStuck );
        }

        public void OnHitSomething( IDamageable hitDamageable, IFactionMember hitFactionMember, bool canGetStuck )
        {
            if( this.blastRadius == 0.0f )
            {
                if( hitDamageable == null )
                {
                    this.__MakeStuckOrDestroy( canGetStuck );
                    return;
                }

                if( SSObjectDFC.CanTarget( this.ownerFactionIdCache, hitFactionMember ) )
                {
                    SSObject targetObj = ((SSObject)hitDamageable);
                    float hitChance = Main.CalculateHitChance( targetObj, this.originY ?? this.transform.position.y );

                    if( Main.IsHit( hitChance ) )
                    {
                        hitDamageable.TakeDamage( this.damageType, DamageUtils.GetRandomized( this.damage, DamageUtils.RANDOM_DEVIATION ), this.armorPenetration );
                        AudioManager.PlaySound( this.hitSound, this.transform.position );
                    }
                    else
                    {
                        AudioManager.PlaySound( this.hitSound, this.transform.position, 0.5f, 1.5f );
                    }

                    this.Destroy();
                }
            }
            else
            {
                if( hitFactionMember != null && hitFactionMember.factionId == this.factionId ) // pass through whatever shot this projectile (and through any allies).
                {
                    return;
                }

                // regardless of what was hit, go boom.
                //          ---------------------------------------------------------------------     making the damageable ones.
                Collider[] col = Physics.OverlapSphere( this.transform.position, this.blastRadius, ObjectLayer.UNITS_MASK | ObjectLayer.BUILDINGS_MASK | ObjectLayer.HEROES_MASK );

                foreach( var hitCollider in col )
                {
                    SSObject potentialDamagee = hitCollider.GetComponent<SSObject>();

                    if( !SSObjectDFC.CanTarget( this.ownerFactionIdCache, (potentialDamagee as IFactionMember) ) )
                    {
                        continue;
                    }

                    if( potentialDamagee is IDamageable )
                    {
                        IDamageable damageable = (IDamageable)potentialDamagee;

                        if( DistanceUtils.IsInRange( this.transform, hitCollider.transform, this.blastRadius, out float distance ) )
                        {
                            float damageScale = (distance / this.blastRadius);

                            float damageScaledToDist = DamageUtils.GetRandomized( this.damage, DamageUtils.RANDOM_DEVIATION ) * damageScale;
                            if( damageScaledToDist <= 0 )
                            {
                                Debug.LogWarning( "Damage scaled to distance was less than or equal to 0 (" + damageScaledToDist + ")." );
                                continue;
                            }

                            SSObject hitObj = ((SSObject)damageable);
                            float hitChance = Main.CalculateHitChance( hitObj, this.originY ?? this.transform.position.y );

                            if( Main.IsHit( hitChance ) )
                            {
                                (potentialDamagee as IDamageable).TakeDamage( this.damageType, damageScaledToDist, this.armorPenetration );
                            }
                        }
                    }
                }

                // destroy after exploding.
                this.__MakeStuckOrDestroy( false );
            }
        }
    }
}