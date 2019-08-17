using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	/// <summary>
	/// Manages audio clips. Use it to play sounds.
	/// </summary>
	public static class AudioManager
	{
		private static List<AudioSource> sources = new List<AudioSource>();

		private static void AddNewAndPlay( AudioClip clip, float volume, float pitch )
		{
			GameObject gameObject = new GameObject( "AudioSource" );
			gameObject.transform.SetParent( Main.main_transform );

			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.volume = volume;
			audioSource.pitch = pitch;
			audioSource.clip = clip;
			audioSource.Play();

			TimerHandler timer = gameObject.AddComponent<TimerHandler>();
			timer.duration = clip.length;
			timer.onTimerEnd.AddListener( () =>
			{
				audioSource.Stop();
			} );
			sources.Add( audioSource );
		}

		/// <summary>
		/// Plays a new sound.
		/// </summary>
		/// <param name="clip">The sound to play.</param>
		/// <param name="volume">The volume (0-1)</param>
		/// <param name="pitch">The pitch.</param>
		public static void PlayNew( AudioClip clip, float volume, float pitch )
		{
			foreach( AudioSource source in sources )
			{
				if( source.isPlaying )
				{
					continue;
				}
				source.clip = clip;
				source.volume = volume;
				source.pitch = pitch;
				source.Play();
				TimerHandler timer = source.GetComponent<TimerHandler>();
				timer.duration = clip.length;
				timer.onTimerEnd.AddListener( () =>
				{
					source.Stop();
				} );
				return;
			}
			AddNewAndPlay( clip, volume, pitch );
		}
	}
}