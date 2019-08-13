using UnityEngine;

namespace SS
{
	public class DaylightCycleController : MonoBehaviour
	{
		[SerializeField] int dayLength = 400;
		[SerializeField] int nightLength = 200;
		[SerializeField] float startTime = 80;

		[SerializeField] Light sun = null;
		[SerializeField] Light moon = null;
		[SerializeField] Transform sunPivot = null;
		[SerializeField] Transform moonPivot = null;

		[SerializeField] float sunIntensity = 0.8f;
		[SerializeField] float moonIntensity = 0.1f;

		[SerializeField] float sunElevationAngle = 60;
		[SerializeField] float moonElevationAngle = 60;

		/// <summary>
		/// Current time, in range between 0-totalDaylength
		/// </summary>
		public float time { get; private set; }

		public bool isDay { get { return this.time < dayLength; } } // day is <0, dayLength)
		public bool isNight { get { return this.time >= dayLength; } } // night is <dayLength, totalDayLen)

		/// <summary>
		/// The total length of the day and night combined.
		/// </summary>
		public int totalDayLength { get; private set; }
		private Transform sunTransform = null;
		private Transform moonTransform = null;

		void Awake()
		{
			totalDayLength = dayLength + nightLength;

			this.time = startTime % totalDayLength;

			this.sunTransform = this.sun.transform;
			this.moonTransform = this.moon.transform;
		}
		
		void Start()
		{
			this.sunPivot.rotation = Quaternion.Euler( 0, 0, sunElevationAngle );
			this.moonPivot.rotation = Quaternion.Euler( 0, 0, moonElevationAngle );
		}

		private float getSunAngle( float time )
		{
			if( isNight )
			{
				return 180 + (((this.time - dayLength) / nightLength) * 180);
			}
			return (this.time / dayLength) * 180; // percentage of the full circle around.
		}
		
		void Update()
		{
			this.time += Time.deltaTime;
			if( this.time > totalDayLength )
			{
				this.time %= totalDayLength;
			}

			float sAngle = getSunAngle( this.time );
			float mAngle = sAngle + 180;
			this.sunTransform.localRotation = Quaternion.Euler( 0, -sAngle, 0 );
			this.moonTransform.localRotation = Quaternion.Euler( 0, -mAngle, 0 );

			if( isDay )
			{
				sun.intensity = sunIntensity;
				moon.intensity = 0;
			}
			if( isNight )
			{
				sun.intensity = 0;
				moon.intensity = moonIntensity;
			}
		}
	}
}