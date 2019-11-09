using System;
using UnityEngine;

namespace SS
{
	[DisallowMultipleComponent]
	public class SSObject : MonoBehaviour
	{
		private Guid? __guid = null;
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

		private string __defId = null;
		/// <summary>
		/// Gets or sets the definition ID of the object (CAN"T be re-assigned after setting it once).
		/// </summary>
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
					throw new Exception( "Tried to re-assign definition to '" + gameObject.name + "'. A definition is already assigned." );
				}
				this.__defId = value;
			}
		}

		/// <summary>
		/// Contains the display name of the object.
		/// </summary>
		public string displayName = "<missing>";
	}
}