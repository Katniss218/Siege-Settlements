using UnityEngine;

namespace SS
{
	public class TargetFinderModule : MonoBehaviour, ITargetFinder
	{
		/// <summary>
		/// Forces the MeleeComponent to seek for targets.
		/// </summary>
		public Damageable FindTarget( float searchRange )
		{
			Collider[] col = Physics.OverlapSphere( this.transform.position, searchRange );
			if( col.Length == 0 )
			{
				return null;
			}

			FactionMember factionMember = this.GetComponent<FactionMember>();

			for( int i = 0; i < col.Length; i++ )
			{
				Damageable potentialTarget = col[i].GetComponent<Damageable>();
				if( potentialTarget == null )
				{
					continue;
				}

				FactionMember potentialFactionMember = potentialTarget.GetComponent<FactionMember>();
				if( potentialFactionMember != null )
				{
					if( potentialFactionMember.factionId == factionMember.factionId )//|| Main.currentRelations[f.factionId, this.factionMember.factionId] != FactionRelation.Enemy )
					{
						continue;
					}
				}
				
				return potentialTarget;
			}
			return null;
		}
	}
}