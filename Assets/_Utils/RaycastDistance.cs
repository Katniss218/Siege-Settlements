using UnityEngine;

namespace Katniss.Utils
{
	public static class PhysicsDistance
	{
		public static bool OverlapInRange( Transform source, Transform target, float range )
		{
			int layer = target.gameObject.layer;
			

			Collider[] cols = Physics.OverlapSphere( source.position, range, 1 << layer );

			if( cols != null && cols.Length > 0 )
			{
				for( int i = 0; i < cols.Length; i++ )
				{
					if( cols[i].transform == target )
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}