using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Projectiles
{
	public class Projectile : MonoBehaviour
	{
		public static bool IsValid( GameObject gameObject )
		{
			if( gameObject.layer != ObjectLayer.PROJECTILES )
			{
				return false;
			} 
			if( gameObject.GetComponent<Projectile>() == null )
			{
				return false;
			}
			return true;
		}

		private static List<GameObject> _allProjectiles = new List<GameObject>();
		
		public Guid guid { get; set; }

		public DamageSource damageSource { get; set; }

		public AudioClip hitSound { get; set; }
		public AudioClip missSound { get; set; }


		public static GameObject[] GetAllProjectiles()
		{
			return _allProjectiles.ToArray();
		}

		public string defId { get; set; }


		void OnEnable()
		{
			_allProjectiles.Add( this.gameObject );
		}

		void OnDisable()
		{
			_allProjectiles.Remove( this.gameObject );
		}
	}
}