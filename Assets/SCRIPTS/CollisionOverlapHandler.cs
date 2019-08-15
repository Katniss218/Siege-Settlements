using System;
using UnityEngine;
using UnityEngine.Events;

namespace SS
{

	[RequireComponent( typeof( Collider ) )]
	public sealed class CollisionOverlapHandler : MonoBehaviour
	{
		[Serializable]
		public class _UnityEvent_GameObject_Collision : UnityEvent<GameObject, Collision> { }

		public _UnityEvent_GameObject_Collision onCollisionEnter = new _UnityEvent_GameObject_Collision();
		public _UnityEvent_GameObject_Collision onCollisionStay = new _UnityEvent_GameObject_Collision();
		public _UnityEvent_GameObject_Collision onCollisionExit = new _UnityEvent_GameObject_Collision();

		void OnCollisionEnter( Collision collision )
		{
			onCollisionEnter?.Invoke( this.gameObject, collision );
		}

		void OnCollisionStay( Collision collision )
		{
			onCollisionStay?.Invoke( this.gameObject, collision );
		}

		void OnCollisionExit( Collision collision )
		{
			onCollisionExit?.Invoke( this.gameObject, collision );
		}
	}
}