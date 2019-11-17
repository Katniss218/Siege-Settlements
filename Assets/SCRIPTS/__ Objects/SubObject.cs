using System;
using UnityEngine;

namespace SS.Objects.SubObjects
{
	[DisallowMultipleComponent]
	public class SubObject : MonoBehaviour
	{
		private SSObject __ssObject;
		
		/// <summary>
		/// Returns the SSObject that this SubObject is attached to. Throws an exception if N/A.
		/// </summary>
		public SSObject ssObject
		{
			get
			{
				if( this.__ssObject == null )
				{
					Transform parent = this.transform.parent;
					if( parent == null )
					{
						throw new Exception( "This SubObject doesn't have proper hierarchy (SSObject >> SubObject)." );
					}
					this.__ssObject = parent.GetComponent<SSObject>() ?? throw new Exception( "This SubObject doesn't have proper hierarchy (SSObject >> SubObject)." );
				}
				return this.__ssObject;
			}
		}

		Guid __subObjectId = Guid.Empty;

		/// <summary>
		/// A unique identifier that identifies this specific SubObject (Must be unique on an per-SSObject basis).
		/// </summary>
		public Guid subObjectId
		{
			get
			{
				return this.__subObjectId;
			}
			set
			{
				if( this.ssObject.GetSubObject( value ) != null )
				{
					throw new Exception( "There's a SubObject with id '" + value.ToString( "D" ) + "' already attached to this SSObject." );
				}
				this.__subObjectId = value;
			}
		}

		/// <summary>
		/// The default position (in local-space) of this SubObject.
		/// </summary>
		public Vector3 defaultPosition { get; set; }

		/// <summary>
		/// The default position (in local-space) of this SubObject.
		/// </summary>
		public Quaternion defaultRotation { get; set; }
	}
}