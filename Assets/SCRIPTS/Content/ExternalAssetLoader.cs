using KFF;
using System;
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
			Mesh ret = KSMImporter.Import( filePath );
		
			return ret;
		}

		public static Texture2D LoadTexture2D( string filePath, TextureType type )
		{
			try
			{
				Texture2D ret = PngImporter.Import( filePath, type );

				return ret;
			}
			catch( Exception )
			{
				throw new Exception( "Can't load texture '" + filePath + "'." );
			}
		}

		public static Sprite LoadSprite( string imgPath )
		{
			try
			{
				// sprites have a texture (path.png) and KFF data file (path.kff).

				string kffPath = System.IO.Path.GetDirectoryName( imgPath ) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension( imgPath ) + ".kff";

				Texture2D tex = PngImporter.Import( imgPath, TextureType.Color );

				KFFSerializer serializer = KFFSerializer.ReadFromFile( kffPath, Encoding.UTF8 );
				Rect rect = serializer.ReadRect( "Rect" );
				string pivotUnitMode = serializer.ReadString( "PivotUnitMode" );
				Vector2 pivot = serializer.ReadVector2( "Pivot" );

				if( pivotUnitMode == "Normalized" )
				{
					pivot.Scale( rect.size ); // if the pivot is given in range 0-1, scale it to the range 0-width/height
				}
				else if( pivotUnitMode != "Pixels" )
				{
					throw new Exception( "Invalid value: '" + pivotUnitMode + "', expected 'Pixels' or 'Normalized'." );
				}

				Sprite ret = Sprite.Create( tex, rect, pivot );

				return ret;
			}
			catch( Exception )
			{
				throw new Exception( "Can't load sprite '" + imgPath + "'." );
			}
		}

		public static AudioClip LoadAudioClip( string filePath )
		{
			try
			{
				AudioClip ret = WavImporter.Import( filePath );

				return ret;
			}
			catch( Exception )
			{
				throw new Exception( "Can't load audio clip '" + filePath + "'." );
			}
		}
	}
}