using SS.Modules;
using SS.Objects.Buildings;
using SS.Objects.Extras;
using SS.Objects.Heroes;
using SS.Objects.Projectiles;
using SS.Objects.SubObjects;
using SS.Objects.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects
{
	[DisallowMultipleComponent]
	public class SSObject : MonoBehaviour
	{
		public const float HUD_DAMAGE_DISPLAY_DURATION = 1.0f;


		private static List<SSObject> allSSObjects = new List<SSObject>();

		private static List<SSObjectSelectable> allSelectables = new List<SSObjectSelectable>();
		
		private static List<Unit> allUnits = new List<Unit>();
		private static List<Building> allBuildings = new List<Building>();
		private static List<Projectile> allProjectiles = new List<Projectile>();
		private static List<Hero> allHeroes = new List<Hero>();
		private static List<Extra> allExtras = new List<Extra>();

		public static SSObject[] GetAllSSObjects()
		{
			return allSSObjects.ToArray();
		}

		public static SSObjectSelectable[] GetAllSelectables()
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


		private Guid? __guid = null;
		private string __definitionId = null;
		private string __displayName = "<missing>";

		/// <summary>
		/// Gets or sets the unique identifier (Guid) of the object (CAN'T be re-assigned after setting it once).
		/// </summary>
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
			return null;
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


		protected virtual void OnEnable()
		{
			allSSObjects.Add( this );
			if( this is SSObjectSelectable )
			{
				allSelectables.Add( this as SSObjectSelectable );
				return;
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
			if( this is SSObjectSelectable )
			{
				allSelectables.Remove( this as SSObjectSelectable );
				return;
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
	}
}