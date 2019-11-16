using SS.Levels;
using TMPro;
using UnityEngine;

namespace SS
{
	public class GameTimeCounter : MonoBehaviour
	{
		[SerializeField] private TMP_Text textField = null;

		public string format = "{0}";
		public float gameTimeOffset = 0.0f;


		private float startTime = 0.0f;
		
		public float GetElapsed()
		{
			return Time.time - this.startTime + gameTimeOffset;
		}

		void Start()
		{
			this.startTime = LevelManager.lastLoadTime;
		}

		private string FormatTime()
		{
			int timeSinceStart = (int)this.GetElapsed(); // time since the level load + time elapsed in previous sessions.

			int hours = timeSinceStart / 3600;
			int mins = timeSinceStart / 60 % 60;
			int secs = timeSinceStart % 60;

			string sHours = hours.ToString();
			string sMins;
			string sSecs;

			if( hours > 0 && mins < 10 )
			{
				sMins = "0" + mins.ToString();
			}
			else
			{
				sMins = mins.ToString();
			}

			if( secs < 10 )
			{
				sSecs = "0" + secs.ToString();
			}
			else
			{
				sSecs = secs.ToString();
			}

			if( hours == 0 )
			{
				return sMins + ":" + sSecs;
			}
			return sHours + ":" + sMins +  ":" + sSecs;
		}
		
		void Update()
		{
			this.textField.text = string.Format( this.format, this.FormatTime() );
		}
	}
}