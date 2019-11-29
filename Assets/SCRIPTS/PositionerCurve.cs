using UnityEngine;

namespace SS
{
	public class PositionerCurve : MonoBehaviour
	{
		
		public Vector3 localStart = Vector3.zero;
		public Vector3 localEnd = Vector3.zero;

		public float duration = 2.5f;

		public AnimationCurve interpolationCurve = null;

		public bool destroyOnEnd = false;



		private Vector3 cachedStart;
		private Vector3 cachedEnd;

		private float activationTimeStamp;
		private bool isActive;



		private Vector3 EvalCurrentGlobalPosition()
		{
			float timeNormalized = (Time.time - this.activationTimeStamp) / this.duration;

			float timeLerp = this.interpolationCurve.Evaluate( timeNormalized );

			float x = Mathf.Lerp( cachedStart.x, cachedEnd.x, timeLerp );
			float y = Mathf.Lerp( cachedStart.y, cachedEnd.y, timeLerp );
			float z = Mathf.Lerp( cachedStart.z, cachedEnd.z, timeLerp );

			return new Vector3( x, y, z );
		}


		private void CacheCurrentPosition()
		{
			Matrix4x4 toWorld = this.transform.localToWorldMatrix;

			this.cachedStart = toWorld.MultiplyPoint( this.localStart );
			this.cachedEnd = toWorld.MultiplyPoint( this.localEnd );
		}



		public void Activate()
		{
			this.CacheCurrentPosition();
			this.isActive = true;
			this.activationTimeStamp = Time.time;
		}

		public void Deactivate()
		{
			this.isActive = false;
		}
		


		void FixedUpdate()
		{
			if( this.isActive )
			{
				this.transform.position = EvalCurrentGlobalPosition();

				if( this.destroyOnEnd )
				{
					if( (Time.time - this.activationTimeStamp) >= this.duration )
					{
						Object.Destroy( this );
					}
				}
			}
		}
	}
}