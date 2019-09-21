using System;
using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	/// <summary>
	/// Handles collider collisions, raising events on start, stay, and exit.
	/// </summary>
	[RequireComponent( typeof( Collider ) )]
	public sealed class CollisionOverlapHandler : MonoBehaviour
	{
		[Serializable]
		public class _UnityEvent_Collision : UnityEvent<Collision> { }

		/// <summary>
		/// Called, when the collider starts colliding with another.
		/// </summary>
		public _UnityEvent_Collision onCollisionEnter = new _UnityEvent_Collision();

		/// <summary>
		/// Called, when the collider is colliding with another.
		/// </summary>
		public _UnityEvent_Collision onCollisionStay = new _UnityEvent_Collision();

		/// <summary>
		/// Called, when the collider stops colliding with another.
		/// </summary>
		public _UnityEvent_Collision onCollisionExit = new _UnityEvent_Collision();


		void OnCollisionEnter( Collision collision )
		{
			onCollisionEnter?.Invoke( collision );
		}

		void OnCollisionStay( Collision collision )
		{
			onCollisionStay?.Invoke( collision );
		}

		void OnCollisionExit( Collision collision )
		{
			onCollisionExit?.Invoke( collision );
		}
	}
}