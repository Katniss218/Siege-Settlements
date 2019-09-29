using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public class ScaleOscillator : MonoBehaviour
	{
		public AnimationCurve oscillationCurve;

		public float timeLength = 5;

		public float minScale = 0.75f;
		public float maxScale = 1.25f;


		private float timer = 0;

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			timer += Time.deltaTime;

			if( timer > timeLength )
			{
				timer %= timeLength;
			}

			float eval = oscillationCurve.Evaluate( timer / timeLength );
			float scale = Mathf.LerpUnclamped( minScale, maxScale, eval );
			this.transform.localScale = new Vector3( scale, scale, scale );
		}
	}
}