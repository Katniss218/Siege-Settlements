using System.IO;
using UnityEngine;

namespace Katniss.Utils
{
	public static class WavImporter
	{
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

		private static float[] leftChannel;
		private static float[] rightChannel;
		
		/// <summary>
		/// The audio file must be an uncompressed .wav file.
		/// </summary>
		public static AudioClip Import( string path )
		{
			byte[] bytes = File.ReadAllBytes( path );

			int channelCount = bytes[22]; // Mono or Stereo.

			int freq = BytesToInt( bytes, 24 );

			// Get past all the other sub chunks to get to the data subchunk:
			int pos = 12; // First Subchunk ID from 12 to 16.

			// Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal)).
			while( !(bytes[pos] == 100 && bytes[pos + 1] == 97 && bytes[pos + 2] == 116 && bytes[pos + 3] == 97) )
			{
				pos += 4;
				int chunkSize = bytes[pos] + bytes[pos + 1] * 256 + bytes[pos + 2] * 65536 + bytes[pos + 3] * 16777216;
				pos += 4 + chunkSize;
			}
			pos += 8;

			// Pos is now positioned to start of actual sound data.
			int sampleCount = (bytes.Length - pos) / 2; // 2 bytes per sample (16 bit sound mono).
			if( channelCount == 2 )
			{
				sampleCount /= 2; // 4 bytes per sample (16 bit stereo).
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

			AudioClip clip = AudioClip.Create( path, sampleCount, 1, freq, false );
			clip.SetData( leftChannel, 0 );

			return clip;
		}
	}
}