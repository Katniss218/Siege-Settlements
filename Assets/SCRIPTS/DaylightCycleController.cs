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

		public int __dayLength = 400;
		public int dayLength
		{
			get
			{
				return this.__dayLength;
			}
			set
			{
				if( value <= 0 )
				{
					throw new System.Exception( "Day Length must be greater than 0." );
				}
				this.__dayLength = value;
			}
		}
		public int __nightLength = 200;
		public int nightLength
		{
			get
			{
				return this.__nightLength;
			}
			set
			{
				if( value <= 0 )
				{
					throw new System.Exception( "Night Length must be greater than 0." );
				}
				this.__nightLength = value;
			}
		}

		private float __sunIntensity = 0.8f;
		public float sunIntensity
		{
			get
			{
				return this.__sunIntensity;
			}
			set
			{
				if( value <= 0 )
				{
					throw new System.Exception( "Sun Intensity must be greater then 0." );
				}
				this.__sunIntensity = value;
			}
		}

		private float __moonIntensity = 0.1f;
		public float moonIntensity
		{
			get
			{
				return this.__moonIntensity;
			}
			set
			{
				if( value <= 0 )
				{
					throw new System.Exception( "Moon Intensity must be greater then 0." );
				}
				this.__moonIntensity = value;
			}
		}

		private float __sunElevationAngle = 60;
		public float sunElevationAngle
		{
			get
			{
				return this.__sunElevationAngle;
			}
			set
			{
				if( value < 0 || value > 180 )
				{
					throw new System.Exception( "Sun Elevation Angle must be between 0 and 180." );
				}
				this.__sunElevationAngle = value;
			}
		}

		private float __moonElevationAngle = 60;
		public float moonElevationAngle
		{
			get
			{
				return this.__moonElevationAngle;
			}
			set
			{
				if( value < 0 || value > 180 )
				{
					throw new System.Exception( "Moon Elevation Angle must be between 0 and 180." );
				}
				this.__moonElevationAngle = value;
			}
		}

		public Color ambientDayColor = Color.black;
		public Color ambientNightColor = Color.black;

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
				if( value < 0 )
				{
					throw new System.Exception( "Time must be greater or equal to 0." );
				}
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

		public bool IsWorkTime()
		{
			return this.time > this.dayLength * 0.05f && this.time < this.dayLength * 0.95f;
		}

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

		static float Remap( float value, float start1, float end1, float start2, float end2 )
		{
			return start2 + (end2 - start2) * ((value - start1) / (end1 - start1));
		}


		private Color GetAmbientColor( float time )
		{
			// day:

			// 0 <==> dayLength * (margin/2) <==> dayLength - (dayLength * (margin/2)) <==> dayLength
			// half - 1 - 1 - half

			// night:

			// 0 <==> dayLength + nightLength * (margin/2) <==> dayLength + nightLength - (nightLength * (margin/2)) <==> dayLength + nightLength

			// half - 0 - 0 - half

			float t = 0.5f;
			if( this.isDay )
			{
				float margin = this.dayLength * DAY_NIGHT_EASE_MARGIN;

				t = 0;
				if( this.time < margin )
				{
					t = Remap( this.time, 0, margin, 0.5f, 0.0f );
				}
				if( this.time > dayLength - margin )
				{
					t = Remap( this.time, dayLength - margin, dayLength, 0.0f, 0.5f );
				}
			}
			else
			{
				float margin = this.nightLength * DAY_NIGHT_EASE_MARGIN;

				t = 1;
				if( (this.time - dayLength) < margin )
				{
					t = Remap( (this.time - dayLength), 0, margin, 0.5f, 1.0f );
				}
				if( (this.time - dayLength) > nightLength - margin )
				{
					t = Remap( (this.time - dayLength), nightLength - margin, nightLength, 1.0f, 0.5f );
				}
			}

			return Color.Lerp( this.ambientDayColor, this.ambientNightColor, t );
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