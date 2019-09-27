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

		private static List<Projectile> _allProjectiles = new List<Projectile>();

		public static Projectile[] GetAllProjectiles()
		{
			return _allProjectiles.ToArray();
		}


		public Guid guid { get; set; }

		public string defId { get; set; }

		public DamageSource damageSource { get; set; }

		public AudioClip hitSound { get; set; }
		public AudioClip missSound { get; set; }




		void OnEnable()
		{
			_allProjectiles.Add( this );
		}

		void OnDisable()
		{
			_allProjectiles.Remove( this );
		}
	}
}