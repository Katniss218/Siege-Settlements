using System;
using UnityEngine;

namespace SS
{
	public class TargetFinder : MonoBehaviour, ITargetFinder
	{
		public Func<FactionMember, FactionMember, bool> canTarget { get; set; }

		public Damageable FindTarget( float searchRange )
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
				if( !canTarget.Invoke( selfFactionMember, targetFactionMember ) )
				{
					continue;
				}
				
				return potentialTarget;
			}
			return null;
		}
	}
}