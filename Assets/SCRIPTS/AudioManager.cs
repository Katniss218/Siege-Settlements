using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public class AudioManager : MonoBehaviour
	{
		public static AudioManager instance;

		public List<AudioSource> players = new List<AudioSource>();

		public static void PlayNew( AudioClip clip, float volume, float pitch )
		{
			GameObject player = new GameObject( "AudioSource - " + clip.name );

			AudioSource source = player.AddComponent<AudioSource>();
			source.volume = volume;
			source.pitch = pitch;
			source.clip = clip;
			source.Play();
			instance.StartCoroutine( instance.WaitForSound( source, clip.length ) );
		}

		void Awake()
		{
			if( instance != null )
			{
				Debug.LogError( "There is another AudioManager instance" );
			}
			instance = this;
		}

		void Update()
		{

		}

		IEnumerator WaitForSound( AudioSource s, float duration )
		{
			yield return new WaitForSeconds( duration );

			Destroy( s.gameObject );
			players.Remove( s );
		}
	}
}