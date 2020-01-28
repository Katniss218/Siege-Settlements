using UnityEngine;

namespace Katniss.Utils
{
	public static class DistanceUtils
	{
		public static bool IsInRange( Vector3 source, Vector3 target, float range )
		{
			return (target - source).sqrMagnitude <= range * range;
		}

		/// <summary>
		/// Returns whether or not the objects are within range. Returns the distance between them (ONLY IF IN RANGE).
		/// </summary>
		public static bool IsInRange( Vector3 source, Vector3 target, float range, out float distance )
		{
			float sqMag = (target - source).sqrMagnitude;
			if( sqMag <= range )
			{
				distance = Mathf.Sqrt( sqMag );
			}
			else
			{
				distance = float.MaxValue;
			}
			return sqMag <= range * range;
		}

		public static bool IsInRange( Transform source, Transform target, float range )
		{
			return (target.position - source.position).sqrMagnitude <= range * range;
		}

		/// <summary>
		/// Returns whether or not the objects are within range. Returns the distance between them (ONLY IF IN RANGE).
		/// </summary>
		public static bool IsInRange( Transform source, Transform target, float range, out float distance )
		{
			float sqMag = (target.position - source.position).sqrMagnitude;
			if( sqMag <= range )
			{
				distance = Mathf.Sqrt( sqMag );
			}
			else
			{
				distance = float.MaxValue;
			}
			return sqMag <= range * range;
		}

		public static bool IsInRangePhysical( Transform source, Transform target, float range )
		{
			Collider[] cols = Physics.OverlapSphere( source.position, range, (1 << target.gameObject.layer) );

			if( cols.Length == 0 )
			{
				return false;
			}

			for( int i = 0; i < cols.Length; i++ )
			{
				if( cols[i].transform == target )
				{
					return true;
				}
			}
			return false;
		}
	}
}