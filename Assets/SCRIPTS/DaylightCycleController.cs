using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public class DaylightCycleController : MonoBehaviour
	{
		[SerializeField] private int dayLength = 400;
		[SerializeField] private int nightLength = 200;
		private int totalDayLen;
		[SerializeField] private float startTime = 80;

		public float time { get; private set; }

		public bool isDay { get { return this.time < dayLength; } } // day is <0, dayLength)
		public bool isNight { get { return this.time >= dayLength; } } // night is <dayLength, totalDayLen)

		[SerializeField] Light sun = null;
		[SerializeField] Light moon = null;
		[SerializeField] Transform sunPivot = null;
		[SerializeField] Transform moonPivot = null;

		Transform sunTransform = null;
		Transform moonTransform = null;

		[SerializeField] private float sunIntensity = 0.8f;
		[SerializeField] private float moonIntensity = 0.1f;

		[SerializeField] private float sunElevationAngle = 60;
		[SerializeField] private float moonElevationAngle = 60;

		void Awake()
		{
			totalDayLen = dayLength + nightLength;

			this.time = startTime % totalDayLen;

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
			if( this.time > totalDayLen )
			{
				this.time %= totalDayLen;
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