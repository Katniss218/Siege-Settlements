using SS.Modules;
using SS.Objects.SubObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	[DisallowMultipleComponent]
	public class SSObject : MonoBehaviour
	{
		public const float HUD_DAMAGE_DISPLAY_DURATION = 1.0f;

		private Guid? __guid = null;
		private string __definitionId = null;

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
		/// Contains the display name of the object.
		/// </summary>
		public string displayName = "<missing>";

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
	}
}