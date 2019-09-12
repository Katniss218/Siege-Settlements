using UnityEngine;

namespace Katniss.Utils
{
	public static class RaycastDistance
	{
		public static bool IsInRange( GameObject target, Vector3 targetCenter, Vector3 source, float range )
		{
			int layer = target.layer;

			Vector3 direction = targetCenter - source;

			RaycastHit[] hitInfos = Physics.RaycastAll( source, direction, range, 1 << layer );

			if( hitInfos != null && hitInfos.Length > 0 )
			{
				for( int i = 0; i < hitInfos.Length; i++ )
				{
					if( hitInfos[i].collider.gameObject == target )
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}