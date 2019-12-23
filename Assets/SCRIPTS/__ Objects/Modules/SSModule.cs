using System;
using UnityEngine;

namespace SS.Objects.Modules
{
	public abstract class SSModule : MonoBehaviour
	{
		private SSObject __ssObject = null;
		private Guid __moduleId = Guid.Empty;

		public Sprite icon { get; set; }

		/// <summary>
		/// Returns the SSObject that this module is added to. Throws an exception if N/A.
		/// </summary>
		public SSObject ssObject
		{
			get
			{
				if( this.__ssObject == null )
				{
					this.__ssObject = this.GetComponent<SSObject>() ?? throw new Exception( this.gameObject.name + ": This module was added to a non-SSObject." );
				}
				return this.__ssObject;
			}
		}

		/// <summary>
		/// A unique identifier that identifies this specific module (Must be unique on an per-SSObject basis).
		/// </summary>
		public Guid moduleId
		{
			get
			{
				return this.__moduleId;
			}
			set
			{
				if( this.ssObject.GetModule( value ) != null )
				{
					throw new Exception( "There's a module with id '" + value.ToString( "D" ) + "' already attached to this SSObject." );
				}
				this.__moduleId = value;
			}
		}


		public abstract ModuleData GetData();
#warning definitions now handled by ModuleDefinition
		public abstract void SetData( ModuleData data );
		//public abstract void SetDefData( ModuleDefinition def, ModuleData data );
	}
}