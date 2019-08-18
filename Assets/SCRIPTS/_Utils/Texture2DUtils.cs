using UnityEngine;

namespace katniss.Utils
{
	public static class Texture2DUtils
	{
		public static Texture2D CreateBlank()
		{
			Texture2D texture = new Texture2D( 1, 1 );
			texture.SetPixel( 0, 0, Color.white );
			return texture;
		}
		public static Texture2D CreateBlank( Color color )
		{
			Texture2D texture = new Texture2D( 1, 1 );
			texture.SetPixel( 0, 0, color );
			return texture;
		}
	}
}