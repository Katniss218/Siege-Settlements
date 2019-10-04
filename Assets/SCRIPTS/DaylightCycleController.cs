using UnityEngine;

namespace SS
{
	/// <summary>
	/// Controls the daylight cycle - time of day, and rotation of the sun/moon lights.
	/// </summary>
	[DisallowMultipleComponent]
	public class DaylightCycleController : MonoBehaviour
	{
		public static DaylightCycleController instance { get; private set; }

		[SerializeField] Light sun = null;
		[SerializeField] Light moon = null;
		[SerializeField] Transform sunPivot = null;
		[SerializeField] Transform moonPivot = null;

		public int dayLength = 400;
		public int nightLength = 200;

		public float sunIntensity = 0.8f;
		public float moonIntensity = 0.1f;

		public float sunElevationAngle = 60;
		public float moonElevationAngle = 60;

		/// <summary>
		/// The total length of the day and night combined.
		/// </summary>
		public int totalDayLength
		{
			get
			{
				return this.dayLength + this.nightLength;
			}
		}

		[SerializeField] private float __time;
		/// <summary>
		/// Contains the current time, in range between 0-totalDaylength (Read only).
		/// </summary>
		public float time
		{
			get
			{
				return this.__time;
			}
			set
			{
				this.__time = value % this.totalDayLength;
			}
		}

		/// <summary>
		/// Returns true, it the current time is day, false otherwise (Read Only).
		/// </summary>
		// day is <0, dayLength)
		public bool isDay { get { return this.time < dayLength; } }

		/// <summary>
		/// Returns true, it the current time is night, false otherwise (Read Only).
		/// </summary>
		// night is <dayLength, totalDayLen)
		public bool isNight { get { return this.time >= dayLength; } }

		private Transform sunTransform = null;
		private Transform moonTransform = null;

		void Awake()
		{
			this.sunTransform = this.sun.transform;
			this.moonTransform = this.moon.transform;

			if( instance != null )
			{
				throw new System.Exception( "There was more than 1 daylight cycle controller." );
			}
			instance = this;
		}

		void Start()
		{
			this.sunPivot.rotation = Quaternion.Euler( 0, 0, this.sunElevationAngle );
			this.moonPivot.rotation = Quaternion.Euler( 0, 0, this.moonElevationAngle );
		}

		private float GetSunAngle( float time )
		{
			if( this.isNight )
			{
				return 180 + (((this.time - this.dayLength) / this.nightLength) * 180);
			}
			return (this.time / this.dayLength) * 180; // percentage of the full circle around.
		}
		
		void Update()
		{
			this.time += Time.deltaTime;
			if( this.time > this.totalDayLength )
			{
				this.time %= this.totalDayLength;
			}

			float sAngle = GetSunAngle( this.time );
			float mAngle = sAngle + 180;
			this.sunTransform.localRotation = Quaternion.Euler( 0, -sAngle, 0 );
			this.moonTransform.localRotation = Quaternion.Euler( 0, -mAngle, 0 );

			if( this.isDay )
			{
				this.sun.intensity = this.sunIntensity;
				this.moon.intensity = 0;
			}
			if( this.isNight )
			{
				this.sun.intensity = 0;
				this.moon.intensity = this.moonIntensity;
			}
		}
	}
}