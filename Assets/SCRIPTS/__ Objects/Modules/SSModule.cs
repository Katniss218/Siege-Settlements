﻿using System;
using UnityEngine;

namespace SS.Objects.Modules
{
	public abstract class SSModule : MonoBehaviour
	{
		private SSObject __ssObject = null;
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

		private Guid __moduleId = Guid.Empty;
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

		public string displayName { get; set; }
		
		public Sprite icon { get; set; }

		//
		//
		//


		public virtual void OnObjDestroyed() { }

		/// <summary>
		/// makes sure that the data is not null & of the specified type. Returns the data cast as the specified type. Throws exception if data is null or not the specified type.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when the data is null.</exception>
		/// <exception cref="Exception">Thrown when the data is not of the specified type.</exception>
		protected static T ValidateDataType<T>( ModuleData data ) where T : ModuleData
		{
			if( data == null )
			{
				throw new ArgumentNullException( "Provided data is null." );
			}
			if( !(data is T) )
			{
				throw new Exception( "Provided data is not of the correct type (needs to be '" + typeof( T ).Name + "')." );
			}

			return (T)data;
		}


		//
		//
		//


		public abstract ModuleData GetData();
		public abstract void SetData( ModuleData _data );
	}
}