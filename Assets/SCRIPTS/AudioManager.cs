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

		private static Transform audioSourcesParent => Main.main_transform;

		private static AudioSource CreateSourceAndPlay( AudioClip clip, float volume, float pitch )
		{
			// Create a new source GameObject to hold the new AudioSource.
			GameObject gameObject = new GameObject( "AudioSource" );
			gameObject.transform.SetParent( audioSourcesParent );

			// Add the necessary components.
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			TimerHandler timerHandler = gameObject.AddComponent<TimerHandler>();

			// Setup the timer.
			timerHandler.onTimerEnd.AddListener( () =>
			{
				audioSource.Stop();
			} );

			// Setup the clip, volume, and pitch.
			SetClipAndPlay( audioSource, timerHandler, clip, volume, pitch );
			return audioSource;
		}

		/// <summary>
		/// Plays a new sound. Can specify the sound, volume and pitch.
		/// </summary>
		/// <param name="clip">The sound to play.</param>
		/// <param name="volume">The volume (0-1)</param>
		/// <param name="pitch">The pitch.</param>
		public static void PlayNew( AudioClip clip, float volume = 1.0f, float pitch = 1.0f )
		{
			foreach( AudioSource audioSource in sources )
			{
				// If the source is currently playing a sound, don't interrupt that, skip it.
				if( audioSource.isPlaying )
				{
					continue;
				}
				SetClipAndPlay( audioSource, audioSource.GetComponent<TimerHandler>(), clip, volume, pitch );
				return;
			}
			// If no source GameObject can be reused (every single one is playing at the moment):
			AudioSource newAudioSource = CreateSourceAndPlay( clip, volume, pitch );
			
			sources.Add( newAudioSource );
		}

		private static void SetClipAndPlay( AudioSource source, TimerHandler timer, AudioClip clip, float volume, float pitch )
		{
			// Sets the source's clip, volume, and pitch.
			// Also sets the timer's duration to the clip's length, as it should never be different.

			source.volume = volume;
			source.pitch = pitch;
			source.clip = clip;

			timer.duration = clip.length;

			source.Play();
		}
	}
}