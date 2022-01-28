using Katniss.Utils;
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
						
		public static bool CanTarget( Vector3 positionSelf, float searchRange, SSObjectDFC target, SSObjectDFC factionMemberSelf )
		{
			if( target == null )
			{
				return false;
			}

			if( !factionMemberSelf.CanTargetAnother( target ) )
			{
				return false;
			}

			if( !DistanceUtils.IsInRange( target.transform.position, positionSelf, searchRange ) )
			{
				return false;
			}

			return true;
		}

		public const int LAYERS = ObjectLayer.UNITS_MASK | ObjectLayer.BUILDINGS_MASK | ObjectLayer.HEROES_MASK;


		public static SSObjectDFC TrySetTarget( Vector3 positionSelf, float searchRange, SSObjectDFC self, SSObjectDFC target, bool requireExactDistance )
		{
			SSObjectDFC targetRet = null;
			if( target == null )
			{
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
					SSObjectDFC facOther = col[i].GetComponent<SSObjectDFC>();

					if( facOther == target )
					{
						targetRet = target;
					}
				}
			}

			return targetRet;
		}

		public static SSObjectDFC FindTargetClosest( Vector3 positionSelf, float searchRange, SSObjectDFC factionMemberSelf, bool requireExactDistance )
		{
			if( requireExactDistance )
			{
				SSObjectDFC[] dfs = SSObject.GetAllDFC();

				SSObjectDFC ret = null;
				float needThisCloseSq = searchRange * searchRange;


				for( int i = 0; i < dfs.Length; i++ )
				{
					// Check if the overlapped object can be targeted by this finder.
					if( !factionMemberSelf.CanTargetAnother( dfs[i] ) )
					{
						continue;
					}

					float distSq = (dfs[i].transform.position - positionSelf).sqrMagnitude;
					if( distSq > needThisCloseSq )
					{
						continue;
					}


					needThisCloseSq = distSq;
					ret = dfs[i];
				}
				return ret;
			}
			else
			{
				Collider[] col = Physics.OverlapSphere( positionSelf, searchRange, LAYERS );
				if( col.Length == 0 )
				{
					return null;
				}
				SSObjectDFC ret = null;
				float needThisCloseSq = float.MaxValue;

				for( int i = 0; i < col.Length; i++ )
				{
					SSObjectDFC facOther = col[i].GetComponent<SSObjectDFC>();

					// Check if the overlapped object can be targeted by this finder.
					if( !factionMemberSelf.CanTargetAnother( facOther ) )
					{
						continue;
					}

					float distSq = (col[i].transform.position - positionSelf).sqrMagnitude;
					if( distSq > needThisCloseSq )
					{
						continue;
					}

					needThisCloseSq = distSq;
					ret = facOther;
				}
				return ret;
			}
		}
	}
}