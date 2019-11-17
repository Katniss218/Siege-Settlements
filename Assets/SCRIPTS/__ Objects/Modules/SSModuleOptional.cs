using System;
using UnityEngine;

namespace SS.Modules
{
	/// <summary>
	/// Represents any module, that can be added to an object to expand it's functionality. Modules encapsulate the functionality in a single MonoMehaviour.
	/// </summary>
	public abstract class SSModuleOptional : MonoBehaviour
	{
		private SSObject __ssObject = null;
		public SSObject ssObject
		{
			get
			{
				if( this.__ssObject == null )
				{
					this.__ssObject = this.GetComponent<SSObject>() ?? throw new Exception( "" + this.gameObject.name + ": This module was added to a non-SSObject." );
				}
				return this.__ssObject;
			}
		}
		
		/// <summary>
		/// A unique identifier that identifies a specific module on an object. Must be unique on per-object basis.
		/// </summary>
		public Guid moduleId { get; set; }

		public abstract ModuleData GetData();

		public abstract void SetDefData( ModuleDefinition def, ModuleData data );
	}
}