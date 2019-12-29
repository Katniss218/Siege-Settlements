using System;
using UnityEngine;

namespace SS.Objects.Modules
{
	public static class Targeter
	{
		public enum TargetingMode : byte
		{
			CLOSEST,
			TARGET
		}
						
		public static bool CanTarget( Vector3 positionSelf, float searchRange, SSObjectDFS target, SSObjectDFS factionMemberSelf )
		{
			if( target == null )
			{
				return false;
			}

			if( !factionMemberSelf.CanTargetAnother( target.GetComponent<IFactionMember>() ) )
			{
				return false;
			}

			if( Vector3.Distance( target.transform.position, positionSelf ) > searchRange )
			{
				return false;
			}

			return true;
		}

		public const int LAYERS = ObjectLayer.UNITS_MASK | ObjectLayer.BUILDINGS_MASK | ObjectLayer.HEROES_MASK;


		public static SSObjectDFS TrySetTarget( Vector3 positionSelf, float searchRange, SSObjectDFS self, SSObjectDFS target, bool requireExactDistance )
		{
			SSObjectDFS targetRet = null;
			if( target == null )
			{
				targetRet = null;
				return null;
			}
			// Check if the overlapped object can be targeted by this finder.
			if( !self.CanTargetAnother( target ) )
			{
				return targetRet;
			}

			if( requireExactDistance )
			{
				if( !Main.IsInRange( target.transform.position, positionSelf, searchRange ) )
				{
					return targetRet;
				}
				targetRet = target;
			}
			else
			{
				Collider[] col = Physics.OverlapSphere( positionSelf, searchRange, LAYERS );
				if( col.Length == 0 )
				{
					return null;
				}
				for( int i = 0; i < col.Length; i++ )
				{
					SSObjectDFS facOther = col[i].GetComponent<SSObjectDFS>();

					if( facOther == target )
					{
						targetRet = target;
					}
				}
			}

			return targetRet;
		}

		public static SSObjectDFS FindTargetClosest( Vector3 positionSelf, float searchRange, SSObjectDFS factionMemberSelf, bool requireExactDistance )
		{
			Collider[] col = Physics.OverlapSphere( positionSelf, searchRange, LAYERS );
			if( col.Length == 0 )
			{
				return null;
			}
			SSObjectDFS ret = null;
			float needThisCloseSq = float.MaxValue;
			if( !requireExactDistance )
			{
				needThisCloseSq = searchRange * searchRange;
			}

			for( int i = 0; i < col.Length; i++ )
			{
				SSObjectDFS facOther = col[i].GetComponent<SSObjectDFS>();

				// Check if the overlapped object can be targeted by this finder.
				if( !factionMemberSelf.CanTargetAnother( facOther ) )
				{
					continue;
				}

				float distSq = (col[i].transform.position - positionSelf).sqrMagnitude;
				if( requireExactDistance )
				{
					if( distSq >= needThisCloseSq )
					{
						continue;
					}
				}

				needThisCloseSq = distSq;
				ret = facOther;
			}
			return ret;
		}
	}
}