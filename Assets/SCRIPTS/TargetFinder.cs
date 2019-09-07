﻿using System;
using UnityEngine;

namespace SS
{
	public class TargetFinder : MonoBehaviour, ITargetFinder
	{
		private Damageable __target;
		
		
		public float searchRange { get; set; }

		// Recalculate the target, when the target needs to be accessed.
		// Two types, melee and ranged. ranged takes into account the projectile trajectory, and doesn't target behind walls/etc.

		public Func<FactionMember, FactionMember, bool> canTarget { get; set; }

		public Damageable GetTarget()
		{
			if( this.__target == null )
			{
				this.__target = FindTarget( this.searchRange );
			}
			return this.__target ?? null;
		}

		private Damageable FindTarget( float searchRange )
		{
			Collider[] col = Physics.OverlapSphere( this.transform.position, searchRange );
			if( col.Length == 0 )
			{
				return null;
			}

			FactionMember selfFactionMember = this.GetComponent<FactionMember>();

			for( int i = 0; i < col.Length; i++ )
			{
				// If the overlapped object can't be damaged.
				Damageable potentialTarget = col[i].GetComponent<Damageable>();
				if( potentialTarget == null )
				{
					continue;
				}
				
				FactionMember targetFactionMember = col[i].GetComponent<FactionMember>();
				// Check if the overlapped object can be targeted by this finder.
				if( !this.canTarget.Invoke( selfFactionMember, targetFactionMember ) )
				{
					continue;
				}
				
				return potentialTarget;
			}
			return null;
		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if( this.__target != null )
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine( this.transform.position, this.__target.transform.position );
				Gizmos.DrawSphere( this.__target.transform.position, 0.125f );
			}
		}
#endif
	}
}