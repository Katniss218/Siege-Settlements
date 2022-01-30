using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	/// <summary>
	/// Manages audio clips. Use it to play sounds.
	/// </summary>
	public class AudioManager : MonoBehaviour
	{
		private static List<AudioSource> sources = new List<AudioSource>();

		private static Transform __audioSourceContainer;
		private static Transform audioSourceContainer
		{
			get
			{
				if( __audioSourceContainer == null )
				{
					__audioSourceContainer = Object.FindObjectOfType<AudioManager>().transform;
				}
				return __audioSourceContainer;
			}
		}
		
		private static AudioSource CreateSourceAndPlay( AudioClip clip, float volume, float pitch, Vector3 position )
		{
			if( audioSourceContainer == null )
			{
				throw new System.Exception( "Can't play sound. There are no AudioManagers added to any GameObject." );
			}

			// Create a new source GameObject to hold the new AudioSource.
			GameObject gameObject = new GameObject( "AudioSource" );
			gameObject.transform.SetParent( audioSourceContainer );
			gameObject.transform.position = position;

			// Add the necessary components.
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.spatialBlend = 1.0f;
			audioSource.rolloffMode = AudioRolloffMode.Linear;
			audioSource.minDistance = 1.5f;
			audioSource.maxDistance = 15.0f;
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
		public static void PlaySound( AudioClip clip, Vector3 position, float volume = 1.0f, float pitch = 1.0f )
		{
			foreach( AudioSource audioSource in sources )
			{
				if( audioSource == null )
				{
					throw new System.Exception( "Null audio source found." );
				}
				// If the source is currently playing a sound, don't interrupt that, skip it.
				if( audioSource.isPlaying )
				{
					continue;
				}

				audioSource.transform.position = position;
				SetClipAndPlay( audioSource, audioSource.GetComponent<TimerHandler>(), clip, volume, pitch );
				return;
			}

			// If no source GameObject can be reused (every single one is playing at the moment):
			AudioSource newAudioSource = CreateSourceAndPlay( clip, volume, pitch, position );
			
			sources.Add( newAudioSource );
		}

		public static void StopSounds( AudioClip matchClip = null )
		{
			foreach( AudioSource audioSource in sources )
			{
				if( audioSource == null )
				{
					throw new System.Exception( "Null audio source found." );
				}

				if( audioSource.isPlaying )
				{
					if( matchClip == null || audioSource.clip == matchClip )
					{
						audioSource.Stop();
					}
				}
			}
		}

		private static void SetClipAndPlay( AudioSource source, TimerHandler timerHandler, AudioClip clip, float volume, float pitch )
		{
			// Sets the source's clip, volume, and pitch.
			// Also sets the timer's duration to the clip's length, as it should never be different.

			source.volume = volume;
			source.pitch = pitch;
			source.clip = clip;

			timerHandler.duration = clip.length;

			source.Play();
		}
	}
}