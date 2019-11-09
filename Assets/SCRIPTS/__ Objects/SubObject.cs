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
	}
}