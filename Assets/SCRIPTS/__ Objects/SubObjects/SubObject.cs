using System;
using UnityEngine;

namespace SS.Objects.SubObjects
{
	[DisallowMultipleComponent]
	public abstract class SubObject : MonoBehaviour
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
					Transform containerTransform = this.transform.parent;
					if( containerTransform == null )
					{
						throw new Exception( "This SubObject doesn't have proper hierarchy (SSObject >> SubObject)." );
					}
					this.__ssObject = containerTransform.GetComponent<SSObject>() ?? throw new Exception( "This SubObject doesn't have proper hierarchy (SSObject >> SubObject)." );
				}
				return this.__ssObject;
			}
		}

		/// <summary>
		/// A unique identifier that identifies this specific SubObject (Must be unique on an per-SSObject basis).
		/// </summary>
		public Guid subObjectId { get; internal set; } = Guid.Empty;


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