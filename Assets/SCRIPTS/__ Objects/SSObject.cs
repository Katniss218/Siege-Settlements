﻿using SS.Objects.Modules;
using SS.Objects.Buildings;
using SS.Objects.Extras;
using SS.Objects.Heroes;
using SS.Objects.Projectiles;
using SS.Objects.SubObjects;
using SS.Objects.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SS.Objects
{
	[DisallowMultipleComponent]
	public class SSObject : MonoBehaviour
	{
		public const float HUD_DAMAGE_DISPLAY_DURATION = 1.0f;


		private static List<SSObject> allSSObjects = new List<SSObject>();

		private static List<SSObjectDFS> allSelectables = new List<SSObjectDFS>();
		
		private static List<Unit> allUnits = new List<Unit>();
		private static List<Building> allBuildings = new List<Building>();
		private static List<Projectile> allProjectiles = new List<Projectile>();
		private static List<Hero> allHeroes = new List<Hero>();
		private static List<Extra> allExtras = new List<Extra>();

		public static SSObject[] GetAll()
		{
			return allSSObjects.ToArray();
		}

		public static SSObjectDFS[] GetAllDFS()
		{
			return allSelectables.ToArray();
		}

		public static Unit[] GetAllUnits()
		{
			return allUnits.ToArray();
		}
		public static Building[] GetAllBuildings()
		{
			return allBuildings.ToArray();
		}
		public static Projectile[] GetAllProjectiles()
		{
			return allProjectiles.ToArray();
		}
		public static Hero[] GetAllHeroes()
		{
			return allHeroes.ToArray();
		}
		public static Extra[] GetAllExtras()
		{
			return allExtras.ToArray();
		}

		/// <summary>
		/// Returns true if the object should display it's parameters on the Selection Panel. By default always true for non-faction objects.
		/// </summary>
		public virtual bool IsDisplaySafe()
		{
			return true;
		}

		private Guid? __guid = null;
		private string __definitionId = null;
		private string __displayName = "<missing>";

		/// <summary>
		/// Gets or sets the unique identifier (Guid) of the object (CAN'T be re-assigned after setting it once).
		/// </summary>
		public Guid guid
		{
			get
			{
				if( this.__guid == null )
				{
					throw new Exception( "Guid hasn't been assigned yet." );
				}
				return this.__guid.Value;
			}
			set
			{
				if( this.__guid != null )
				{
					throw new Exception( "Tried to re-assign guid to '" + gameObject.name + "'. A guid is already assigned." );
				}
				this.__guid = value;
			}
		}

		/// <summary>
		/// Gets or sets the definition ID of the object (CAN"T be re-assigned after setting it once).
		/// </summary>
		public string definitionId
		{
			get
			{
				return this.__definitionId;
			}
			set
			{
				if( this.__definitionId != null )
				{
					throw new Exception( "Tried to re-assign definition to '" + gameObject.name + "'. A definition is already assigned." );
				}
				this.__definitionId = value;
			}
		}

		/// <summary>
		/// Gets or sets the display name of the object.
		/// </summary>
		public virtual string displayName
		{
			get
			{
				return this.__displayName;
			}
			set
			{
				this.__displayName = value;
			}
		}

		/// <summary>
		/// Gets a SubObject with specified Id. Returns null if none are present.
		/// </summary>
		public SubObject GetSubObject( Guid subObjectId )
		{
			for( int i = 0; i < this.transform.childCount; i++ )
			{
				SubObject subObject = this.transform.GetChild( i ).GetComponent<SubObject>();

				if( subObject == null )
				{
					throw new Exception( "A non-SubObject has been assigned to this SSObject." );
				}

				if( subObject.subObjectId == subObjectId )
				{
					return subObject;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets all SubObjects assigned to this SSObject.
		/// </summary>
		public SubObject[] GetSubObjects()
		{
			List<SubObject> ret = new List<SubObject>();
			for( int i = 0; i < this.transform.childCount; i++ )
			{
				SubObject subObject = this.transform.GetChild( i ).GetComponent<SubObject>();

				if( subObject == null )
				{
					throw new Exception( "A non-SubObject has been assigned to this SSObject." );
				}

				ret.Add( subObject );
			}
			return ret.ToArray();
		}

		/// <summary>
		/// Gets all SubObjects assigned to this SSObject.
		/// </summary>
		public T[] GetSubObjects<T>() where T : SubObject
		{
			List<T> ret = new List<T>();
			for( int i = 0; i < this.transform.childCount; i++ )
			{
				T subObject = this.transform.GetChild( i ).GetComponent<T>();

				if( subObject == null )
				{
					continue;
				}

				ret.Add( subObject );
			}
			return ret.ToArray();
		}

		/// <summary>
		/// Gets a specified module.
		/// </summary>
		public SSModule GetModule( Guid moduleId )
		{
			SSModule[] modules = this.GetComponents<SSModule>();
			for( int i = 0; i < modules.Length; i++ )
			{
				if( modules[i].moduleId == moduleId )
				{
					return modules[i];
				}
			}
			return null;
		}

		/// <summary>
		/// Gets all modules assigned to this SSobject.
		/// </summary>
		public SSModule[] GetModules()
		{
			SSModule[] modules = this.GetComponents<SSModule>();
			return modules;
		}

		/// <summary>
		/// Gets a specified module of specified type T.
		/// </summary>
		public T GetModule<T>( Guid moduleId ) where T : SSModule
		{
			T[] modules = this.GetComponents<T>();
			for( int i = 0; i < modules.Length; i++ )
			{
				if( modules[i].moduleId == moduleId )
				{
					return modules[i];
				}
			}
			return null;
		}

		/// <summary>
		/// Gets all modules of specified type T assigned to this SSobject.
		/// </summary>
		public T[] GetModules<T>() where T : SSModule
		{
			T[] modules = this.GetComponents<T>();
			return modules;
		}

		protected virtual void OnObjDestroyed()
		{

		}

		public void Destroy()
		{
			SSModule[] modules = this.GetModules();
			for( int i = 0; i < modules.Length; i++ )
			{
				modules[i].OnObjDestroyed();
			}

			this.OnObjDestroyed();
			
			Object.Destroy( this.gameObject );
		}


		protected virtual void OnEnable()
		{
			allSSObjects.Add( this );
			if( this is SSObjectDFS )
			{
				allSelectables.Add( this as SSObjectDFS );
			}

			if( this is Unit )
			{
				allUnits.Add( this as Unit );
				return;
			}
			if( this is Building )
			{
				allBuildings.Add( this as Building );
				return;
			}
			if( this is Projectile )
			{
				allProjectiles.Add( this as Projectile );
				return;
			}
			if( this is Hero )
			{
				allHeroes.Add( this as Hero );
				return;
			}
			if( this is Extra )
			{
				allExtras.Add( this as Extra );
				return;
			}
		}

		protected virtual void OnDisable()
		{
			allSSObjects.Remove( this );
			if( this is SSObjectDFS )
			{
				allSelectables.Remove( this as SSObjectDFS );
			}

			if( this is Unit )
			{
				allUnits.Remove( this as Unit );
				return;
			}
			if( this is Building )
			{
				allBuildings.Remove( this as Building );
				return;
			}
			if( this is Projectile )
			{
				allProjectiles.Remove( this as Projectile );
				return;
			}
			if( this is Hero )
			{
				allHeroes.Remove( this as Hero );
				return;
			}
			if( this is Extra )
			{
				allExtras.Remove( this as Extra );
				return;
			}
		}

		public static SSObject Find( Guid guid )
		{
			SSObject[] ssObjectArray = GetAll();

			for( int i = 0; i < ssObjectArray.Length; i++ )
			{
				if( ssObjectArray[i].guid == guid )
				{
					return ssObjectArray[i];
				}
			}
			return null;
		}
	}
}