using UnityEngine;

namespace SS
{
	public class LightFlickerer : MonoBehaviour
	{
		public new Light light;

		public float minIntensity;
		public float maxIntensity;

		public float speedMultiplier = 13.0f;

		private int randOffset = 0;
		
		void Start()
		{
			randOffset = this.GetInstanceID();
		}
		
		void Update()
		{
			light.intensity = Mathf.Lerp( this.minIntensity, this.maxIntensity, Mathf.PerlinNoise( Time.time * speedMultiplier, this.randOffset ) );
		}
	}
}