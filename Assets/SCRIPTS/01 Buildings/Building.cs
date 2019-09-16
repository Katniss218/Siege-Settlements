using System.Collections.Generic;
using UnityEngine;

namespace SS.Buildings
{
	public class Building : MonoBehaviour
	{
		// The amount of health that the building marked as being constructed is going to start with.
		public const float STARTING_HEALTH_PERCENT = 0.1f;

		public Vector3[] placementNodes { get; set; }

		public Vector3 entrance { get; set; }

		public Dictionary<string, int> StartToEndConstructionCost { get; set; }

		public AudioClip buildSoundEffect { get; set; }

		/// <summary>
		/// Checks if the building is in a 'usable' state (not under construction/repair and not below 50% health).
		/// </summary>
		/// <param name="building">The building whoose health to check.</param>
		public static bool IsUsable( Damageable building )
		{
			// If not under construction/repair.
			if( building.GetComponent<ConstructionSite>() == null )
			{
				return building.healthPercent >= 0.5f;
			}
			// If under construction/repair.
			return false;
		}
		
#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.white;

			Matrix4x4 toWorld = this.transform.localToWorldMatrix;
			for( int i = 0; i < this.placementNodes.Length; i++ )
			{
				Gizmos.DrawSphere( toWorld.MultiplyVector( this.placementNodes[i] ) + this.transform.position, 0.05f );
			}
		}
#endif
	}
}