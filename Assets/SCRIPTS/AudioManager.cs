using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public class AudioManager : MonoBehaviour
	{
		public static AudioManager instance;

		private List<AudioSource> players = new List<AudioSource>();

		/// <summary>
		/// Plays a new sound.
		/// </summary>
		/// <param name="clip">The sound to play.</param>
		/// <param name="volume">The volume (0-1)</param>
		/// <param name="pitch">The pitch.</param>
		public static void PlayNew( AudioClip clip, float volume, float pitch )
		{
			foreach( AudioSource s in instance.players )
			{
				if( s.isPlaying )
				{
					continue;
				}
				s.clip = clip;
				s.volume = volume;
				s.pitch = pitch;
				s.Play();
				instance.StartCoroutine( instance.StopAfterEnd( s, clip.length ) );
				return;
			}
			GameObject player = new GameObject( "AudioSource" );
			player.transform.SetParent( instance.transform );

			AudioSource source = player.AddComponent<AudioSource>();
			source.volume = volume;
			source.pitch = pitch;
			source.clip = clip;
			source.Play();
			instance.StartCoroutine( instance.StopAfterEnd( source, clip.length ) );
			instance.players.Add( source );
		}

		void Awake()
		{
			if( instance != null )
			{
				Debug.LogError( "There is another AudioManager instance" );
			}
			instance = this;
		}
		
		private IEnumerator StopAfterEnd( AudioSource s, float duration )
		{
			yield return new WaitForSeconds( duration );

			s.Stop();
		}
	}
}