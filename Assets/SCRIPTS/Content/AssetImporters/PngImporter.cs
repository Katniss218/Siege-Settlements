﻿using System.IO;
using UnityEngine;

namespace SS.Content
{
	public static class PngImporter
	{
		private static byte[] bytes;

		/// <summary>
		/// The texture must be in RGB, 24 bit format, 8 bits per channel, 4 channels.
		/// </summary>
		public static Texture2D Import( string path, TextureType type )
		{
			bytes = File.ReadAllBytes( path );
			Texture2D texture = null;
			if( type == TextureType.Normal )
			{
				texture = new Texture2D( 2, 2, TextureFormat.RGB24, true, true );
			}
			else
			{
				texture = new Texture2D( 2, 2 );
			}
			texture.LoadImage( bytes );
			return texture;
		}

		public static Sprite MakeSprite( this Texture2D tex )
		{
			return Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( 0.5f, 0.5f ) );
		}

		public static Sprite MakeSprite( this Texture2D tex, Vector2 pivot )
		{
			return Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), pivot );
		}
	}
}