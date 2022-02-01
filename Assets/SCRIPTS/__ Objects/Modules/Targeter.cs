using Katniss.Utils;
using System.Linq;
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

		const int LAYERS = ObjectLayer.UNITS_MASK | ObjectLayer.BUILDINGS_MASK | ObjectLayer.HEROES_MASK;

		public static bool CanTarget( Vector3 positionSelf, float searchRange, SSObjectDFC target, SSObjectDFC self )
		{
			if( target == null )
			{
				return false;
			}

			if( !self.CanTargetAnother( target ) )
			{
				return false;
			}

			if( !DistanceUtils.IsInRange( target.transform.position, positionSelf, searchRange ) )
			{
				return false;
			}

			return true;
		}



		public static SSObjectDFC TrySetTarget( Vector3 positionSelf, float searchRange, SSObjectDFC self, SSObjectDFC target, bool requireExactDistance )
		{
			if( target == null )
			{
				return null;
			}

			SSObjectDFC targetRet = null;

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
				Collider[] colliders = Physics.OverlapSphere( positionSelf, searchRange, LAYERS );
				if( !colliders.Any() )
				{
					return null;
				}

				foreach( var col in colliders )
				{
					SSObjectDFC facOther = col.GetComponent<SSObjectDFC>();

					if( facOther == target )
					{
						targetRet = target;
					}
				}
			}

			return targetRet;
		}

		public static SSObjectDFC FindTargetClosest( Vector3 positionSelf, float searchRange, SSObjectDFC self, bool needPivotInRange )
		{
			if( needPivotInRange ) // the pivot of the object must be in range
			{
				SSObjectDFC[] dfcObjects = SSObject.GetAllDFC();

				SSObjectDFC closest = null;
				float closestSqDistSoFar = searchRange * searchRange;

				foreach( var dfc in dfcObjects )
				{
					// Check if the overlapped object can be targeted by this finder.
					if( !self.CanTargetAnother( dfc ) )
					{
						continue;
					}

					float distSq = (dfc.transform.position - positionSelf).sqrMagnitude;
					if( distSq > closestSqDistSoFar )
					{
						continue;
					}


					closestSqDistSoFar = distSq;
					closest = dfc;
				}
				return closest;
			}
			else // any part of the object needs to be in range.
			{
				Collider[] colliders = Physics.OverlapSphere( positionSelf, searchRange, LAYERS );
				if( !colliders.Any() )
				{
					return null;
				}

				SSObjectDFC closest = null;
				float closestSqDistSoFar = float.MaxValue;

				foreach( var col in colliders )
				{
					SSObjectDFC facOther = col.GetComponent<SSObjectDFC>();

					// Check if the overlapped object can be targeted by this finder.
					if( !self.CanTargetAnother( facOther ) )
					{
						continue;
					}

					float distSq = (col.transform.position - positionSelf).sqrMagnitude;
					if( distSq > closestSqDistSoFar )
					{
						continue;
					}

					closestSqDistSoFar = distSq;
					closest = facOther;
				}
				return closest;
			}
		}
	}
}