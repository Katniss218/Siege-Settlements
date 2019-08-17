using KFF;
using SS;
using SS.Data;
using System;
using UnityEngine;

namespace Katniss.Utils
{
	/// <summary>
	/// Used as a project-specific KFF serializer extension.
	/// </summary>
	public static class KFFAssetHelper
	{
		// Everything specific to Siege Settlements' implementation of KFF goes here.

		/// <summary>
		/// Loads a Texture2D from assets, using a KFF serializer.
		/// </summary>
		public static Tuple<string, Texture2D> ReadTexture2DFromAssets( this KFFSerializer serializer, string kffPath, TextureType type )
		{
			string assetPath = serializer.ReadString( kffPath );
			return new Tuple<string, Texture2D>( assetPath, AssetsManager.GetTexture2D( assetPath, type ) );
		}
		
		/// <summary>
		/// Loads a Sprite from assets, using a KFF serializer.
		/// </summary>
		public static Tuple<string, Sprite> ReadSpriteFromAssets( this KFFSerializer serializer, string kffPath )
		{
			string assetPath = serializer.ReadString( kffPath );
			return new Tuple<string, Sprite>( assetPath, AssetsManager.GetSprite( assetPath ) );
		}

		/// <summary>
		/// Loads a Mesh from assets, using a KFF serializer.
		/// </summary>
		public static Tuple<string, Mesh> ReadMeshFromAssets( this KFFSerializer serializer, string kffPath )
		{
			string assetPath = serializer.ReadString( kffPath );
			return new Tuple<string, Mesh>( assetPath, AssetsManager.GetMesh( assetPath ) );
		}

		/// <summary>
		/// Loads an AudioClip from assets, using a KFF serializer.
		/// </summary>
		public static Tuple<string, AudioClip> ReadAudioClipFromAssets( this KFFSerializer serializer, string kffPath )
		{
			string assetPath = serializer.ReadString( kffPath );
			return new Tuple<string, AudioClip>( assetPath, AssetsManager.GetAudioClip( assetPath ) );
		}
	}
}