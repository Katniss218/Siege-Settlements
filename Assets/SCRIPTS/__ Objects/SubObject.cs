using System;
using UnityEngine;

namespace SS
{
	[DisallowMultipleComponent]
	public class SubObject : MonoBehaviour
	{
		/// <summary>
		/// The unique id that can be used to specify this exact sub-object.
		/// </summary>
		public Guid subObjectId { get; set; }

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