using Katniss.Utils;
using KFF;
using System.Text;
using UnityEngine;

namespace SS.Content
{
	/// <summary>
	/// Contains methods for loading various types of assets from files.
	/// </summary>
	public static class ExternalAssetLoader
	{
		public static Mesh LoadMesh( string filePath )
		{
			Mesh[] ret = KMKFFImporter.Import( filePath );

			return ret[0];
		}

		public static Texture2D LoadTexture2D( string filePath, TextureType type )
		{
			Texture2D ret = PngImporter.Import( filePath, type );

			return ret;
		}

		public static Sprite LoadSprite( string filePathNoExt )
		{
			// sprites have a texture (path.png) and KFF data file (path.kff).
			
			Texture2D tex = PngImporter.Import( filePathNoExt + ".png", TextureType.Color );

			string kffPath = filePathNoExt + ".kff";
			KFFSerializer serializer = KFFSerializer.ReadFromFile( kffPath, Encoding.UTF8 );
			Rect rect = serializer.ReadRect( "Rect" );
			string pivotUnitMode = serializer.ReadString( "PivotUnitMode" );
			Vector2 pivot = serializer.ReadVector2( "Pivot" );

			if( pivotUnitMode == "normalized" )
			{
				pivot.Scale( rect.size ); // if the pivot is given in range 0-1, scale it to the range 0-width/height
			}
			else if( pivotUnitMode != "pixels" )
			{
				throw new System.Exception( "Invalid value: '" + pivotUnitMode + "', expected 'pixels' or 'normalized'." );
			}

			Sprite ret = Sprite.Create( tex, rect, pivot );

			return ret;
		}

		public static AudioClip LoadAudioClip( string filePath )
		{
			AudioClip ret = WavImporter.Import( filePath );

			return ret;
		}
	}
}