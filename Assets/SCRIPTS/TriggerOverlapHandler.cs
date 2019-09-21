using System;
using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	/// <summary>
	/// Handles 'trigger' collider collisions, raising events on start, stay, and exit.
	/// </summary>
	[RequireComponent( typeof( Collider ) )]
	public sealed class TriggerOverlapHandler : MonoBehaviour
	{
		[Serializable]
		public class _UnityEvent_Collider : UnityEvent<Collider> { }

		/// <summary>
		/// Called, when the 'trigger' collider starts colliding with another.
		/// </summary>
		public _UnityEvent_Collider onTriggerEnter = new _UnityEvent_Collider();

		/// <summary>
		/// Called, when the 'trigger' collider is colliding with another.
		/// </summary>
		public _UnityEvent_Collider onTriggerStay = new _UnityEvent_Collider();

		/// <summary>
		/// Called, when the 'trigger' collider stops colliding with another.
		/// </summary>
		public _UnityEvent_Collider onTriggerExit = new _UnityEvent_Collider();


		void OnTriggerEnter( Collider other )
		{
			onTriggerEnter?.Invoke( other );
		}

		void OnTriggerStay( Collider other )
		{
			onTriggerStay?.Invoke( other );
		}

		void OnTriggerExit( Collider other )
		{
			onTriggerExit?.Invoke( other );
		}
	}
}