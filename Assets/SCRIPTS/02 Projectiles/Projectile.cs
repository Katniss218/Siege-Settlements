using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Projectiles
{
	public class Projectile : SSObject
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


		private Guid? __guid = null;
		public Guid? guid
		{
			get
			{
				return this.__guid;
			}
			set
			{
				if( this.guid != null )
				{
					throw new Exception( "Tried to re-assign guid to '" + gameObject.name + "'. A guid is already assigned." );
				}
				this.__guid = value;
			}
		}


		private string __defId = null;
		public string defId
		{
			get
			{
				return this.__defId;
			}
			set
			{
				if( this.__defId != null )
				{
					throw new Exception( "Tried to assign definition to '" + gameObject.name + "' more than once." );
				}
				this.__defId = value;
			}
		}


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