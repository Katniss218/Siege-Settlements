using System;
using UnityEngine;
using UnityEngine.Events;

namespace SS
{

	[RequireComponent( typeof( Collider ) )]
	public sealed class TriggerOverlapHandler : MonoBehaviour
	{
		[Serializable]
		public class _UnityEventCollider : UnityEvent<GameObject, Collider> { }

		public _UnityEventCollider onTriggerEnter = new _UnityEventCollider();
		public _UnityEventCollider onTriggerStay = new _UnityEventCollider();
		public _UnityEventCollider onTriggerExit = new _UnityEventCollider();

		void OnTriggerEnter( Collider other )
		{
			onTriggerEnter?.Invoke( this.gameObject, other );
		}

		void OnTriggerStay( Collider other )
		{
			onTriggerStay?.Invoke( this.gameObject, other );
		}

		void OnTriggerExit( Collider other )
		{
			onTriggerExit?.Invoke( this.gameObject, other );
		}
	}
}