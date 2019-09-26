﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Buildings
{
	public class Building : MonoBehaviour
	{
		public static bool IsValid( GameObject gameObject )
		{
			if( gameObject.layer != ObjectLayer.BUILDINGS )
			{
				return false;
			}
			if( gameObject.GetComponent<Building>() == null )
			{
				return false;
			}
			return true;
		}

		private static List<GameObject> _allBuildings = new List<GameObject>();

		public static GameObject[] GetAllBuildings()
		{
			return _allBuildings.ToArray();
		}

		// The amount of health that the building marked as being constructed is going to start with.
		public const float STARTING_HEALTH_PERCENT = 0.1f;

		public Guid guid { get; set; }

		public string defId { get; set; }
		
		public Vector3[] placementNodes { get; set; }

		public Vector3 entrance { get; set; }

		public Dictionary<string, int> StartToEndConstructionCost { get; set; }

		public AudioClip buildSoundEffect { get; set; }

		public string displayName { get; set; }

		public AudioClip deathSound { get; set; }

		/// <summary>
		/// Checks if the building is in a 'usable' state (not under construction/repair and not below 50% health).
		/// </summary>
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
		
		/// <summary>
		/// Checks if the building can be repaired (repair hasn't started already).
		/// </summary>
		public static bool IsRepairable( Damageable building )
		{
			// If the construction/repair is currently being done.
			if( building.GetComponent<ConstructionSite>() != null )
			{
				return false;
			}
			// If the construction/repair is NOT being done.
			return building.health < building.healthMax;
		}

		void OnEnable()
		{
			_allBuildings.Add( this.gameObject );
		}

		void OnDisable()
		{
			_allBuildings.Remove( this.gameObject );
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