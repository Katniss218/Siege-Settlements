using UnityEngine;

namespace SS.Objects.SubObjects
{
	[RequireComponent(typeof(Light))]
	public class LightSubObject : SubObject
	{
		private Light __light;
		private new Light light
		{
			get
			{
				if( this.__light == null )
				{
					this.__light = this.GetComponent<Light>();
				}
				return this.__light;
			}
		}

		public float minIntensity { get; set; } = 1.0f;
		public float maxIntensity { get; set; } = 2.0f;

		public float flickerSpeed { get; set; } = 8.0f;

		public Color color
		{
			get
			{
				return this.light.color;
			}
			set
			{
				this.light.color = value;
			}
		}

		public float range
		{
			get
			{
				return this.light.range;
			}
			set
			{
				this.light.range = value;
			}
		}

		private int randOffset = 0;

		void Start()
		{
			randOffset = this.GetInstanceID();
		}

		void Update()
		{
			light.intensity = Mathf.Lerp( this.minIntensity, this.maxIntensity, Mathf.PerlinNoise( Time.time * flickerSpeed, this.randOffset ) );
		}
	}
}