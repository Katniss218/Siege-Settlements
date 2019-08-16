using System.IO;
using UnityEngine;

namespace Katniss.Utils
{
	public static class WavImporter
	{
		private static float[] leftChannel; // left or mono.
		private static float[] rightChannel; // only for stereo.
		
		/// <summary>
		/// The audio file must be an uncompressed .wav file, 16 bits per sample, with no additional info attached.
		/// </summary>
		public static AudioClip Import( string path )
		{
			byte[] bytes = File.ReadAllBytes( path );
			
			int channelCount = bytes[22]; // Mono / Stereo.

			int frequency = BytesToInt( bytes, 24 ); // sample rate

			// Get past all the other sub chunks to get to the data subchunk:
			int pos = 12; // First Subchunk ID from 12 to 16.

			// Keep iterating until we find the data chunk (the chunk contains the word 'data' - '0x64617461' ...... (i.e. 100 97 116 97 in decimal)).
			while( !(bytes[pos] == 100 && bytes[pos + 1] == 97 && bytes[pos + 2] == 116 && bytes[pos + 3] == 97) )
			{
				pos += 4;
				int subchunk2Size = bytes[pos] + bytes[pos + 1] * 256 + bytes[pos + 2] * 65536 + bytes[pos + 3] * 16777216;
				pos += 4 + subchunk2Size;
			}
			pos += 8;

			// Pos is now positioned to start of actual sound data.
			int sampleCount = (bytes.Length - pos) / 2; // 2 bytes per sample (1x 16 bit sound - mono).
			if( channelCount == 2 )
			{
				sampleCount /= 2; // 4 bytes per sample (2x 16 bit - stereo) - Each sample consists of both channels.
			}

			// Initialize the channel arrays.
			leftChannel = new float[sampleCount];
			if( channelCount == 2 )
			{
				rightChannel = new float[sampleCount];
			}
			else
			{
				rightChannel = null;
			}

			// Write to the channel arrays.
			int i = 0;
			while( pos < bytes.Length )
			{
				leftChannel[i] = BytesToFloat01( bytes[pos], bytes[pos + 1] );
				pos += 2;
				if( channelCount == 2 )
				{
					rightChannel[i] = BytesToFloat01( bytes[pos], bytes[pos + 1] );
					pos += 2;
				}
				i++;
			}

			AudioClip clip;
			if( channelCount == 2 )
			{
				float[] bothChannelsInterlaced = new float[leftChannel.Length * 2];
				for( i = 0; i < leftChannel.Length; i++ )
				{
					bothChannelsInterlaced[(2 * i)] = leftChannel[i];
					bothChannelsInterlaced[(2 * i) + 1] = rightChannel[i];
				}
				clip = AudioClip.Create( path, sampleCount, 2, frequency, false );
				clip.SetData( bothChannelsInterlaced, 0 );
			}
			else
			{
				clip = AudioClip.Create( path, sampleCount, 1, frequency, false );
				clip.SetData( leftChannel, 0 );
			}

			return clip;
		}

		// Convert two bytes to one float in the range -1 to 1
		private static float BytesToFloat01( byte b1, byte b2 )
		{
			return (short)((b2 << 8) | b1) / 32768.0f;
		}

		// Convert 4 bytes to an Int32.
		private static int BytesToInt( byte[] bytes, int offset = 0 )
		{
			int value = 0;
			for( int i = 0; i < 4; i++ )
			{
				value |= (bytes[offset + i]) << (i * 8);
			}
			return value;
		}
	}
}