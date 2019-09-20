using UnityEngine;

namespace Katniss.Utils
{
	public static class Texture2DUtils
	{
		public static Texture2D CreateBlank()
		{
			Texture2D texture = new Texture2D( 1, 1 );
			texture.SetPixel( 0, 0, Color.white );
			return texture;
		}

		public static Texture2D CreateBlank( Color color, TextureType type )
		{
			Texture2D texture;
			if( type == TextureType.Normal )
			{
				texture = new Texture2D( 2, 2, TextureFormat.RGB24, true, true );
			}
			else
			{
				texture = new Texture2D( 2, 2, TextureFormat.RGBA32, true );
			}
			texture.SetPixel( 0, 0, color );
			return texture;
		}
	}
}