using UnityEngine;

namespace SS
{
	/// <summary>
	/// Controls the daylight cycle - time of day, and rotation of the sun/moon lights.
	/// </summary>
	[DisallowMultipleComponent]
	public class DaylightCycleController : MonoBehaviour
	{
		private const float DAY_NIGHT_EASE_MARGIN = 0.2f;

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

		public Color ambientLightColor = Color.black;

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

		private float GetSunIntensity( float time )
		{
			if( this.isDay )
			{
				float margin = this.dayLength * DAY_NIGHT_EASE_MARGIN;

				if( time > margin && time < this.dayLength - margin )
				{
					return this.sunIntensity;
				}
				if( time < this.dayLength / 2f )
				{
					return Mathf.Lerp( 0f, this.sunIntensity, (time) / margin );
				}
				else
				{
					return Mathf.Lerp( this.sunIntensity, 0f, (time - this.dayLength + margin) / margin );
				}
			}
			return 0f;
		}

		private float GetMoonIntensity( float time )
		{
			if( this.isNight )
			{
				float margin = this.nightLength * DAY_NIGHT_EASE_MARGIN;

				if( time > this.dayLength + margin && time < this.dayLength + this.nightLength - margin )
				{
					return this.moonIntensity;
				}
				if( time < this.dayLength + (this.nightLength / 2f) )
				{
					return Mathf.Lerp( 0f, this.moonIntensity, (time - this.dayLength) / margin );
				}
				else
				{
					return Mathf.Lerp( this.moonIntensity, 0f, (time - this.dayLength - this.nightLength + margin) / margin );
				}
			}
			return 0f;
		}

		private Color GetAmbientColor( float time )
		{
			if( this.isDay )
			{
				float margin = this.dayLength * (DAY_NIGHT_EASE_MARGIN / 2.0f);

				float multiplier = 1.0f;
				if( time > margin && time < this.dayLength - margin )
				{
					return this.ambientLightColor;
				}
				if( time < this.dayLength / 2f )
				{
					multiplier = Mathf.Lerp( 2.5f, 1.0f, (time) / margin );
				}
				else
				{
					multiplier = Mathf.Lerp( 1.0f, 2.5f, (time - this.dayLength + margin) / margin );
				}
				return new Color( ambientLightColor.r / multiplier, ambientLightColor.g / multiplier, ambientLightColor.b / multiplier );
			}
			else
			{
				float margin = this.nightLength * (DAY_NIGHT_EASE_MARGIN / 2.0f);

				float multiplier = 4.0f;
				if( time > this.dayLength + margin && time < this.dayLength + this.nightLength - margin )
				{
					return new Color( ambientLightColor.r / multiplier, ambientLightColor.g / multiplier, ambientLightColor.b / multiplier );
				}
				if( time < this.dayLength + (this.nightLength / 2f) )
				{
					multiplier = Mathf.Lerp( 2.5f, 4.0f, (time - this.dayLength) / margin );
				}
				else
				{
					multiplier = Mathf.Lerp( 4.0f, 2.5f, (time - this.dayLength - this.nightLength + margin) / margin );
				}
				return new Color( ambientLightColor.r / multiplier, ambientLightColor.g / multiplier, ambientLightColor.b / multiplier );
			}
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
			

			this.sun.intensity = GetSunIntensity( time );
			this.moon.intensity = GetMoonIntensity( time );

			RenderSettings.ambientLight = this.GetAmbientColor( this.time );
		}
	}
}