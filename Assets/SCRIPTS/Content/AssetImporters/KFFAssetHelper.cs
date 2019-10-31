using KFF;
using System;
using UnityEngine;

namespace SS.Content
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
		public static AddressableAsset<Texture2D> ReadTexture2DFromAssets( this KFFSerializer serializer, string kffPath, TextureType type )
		{
			string assetPath = serializer.ReadString( kffPath );
			return new AddressableAsset<Texture2D>( assetPath, AssetManager.GetTexture2D( assetPath, type ) );
		}
		
		/// <summary>
		/// Loads a Sprite from assets, using a KFF serializer.
		/// </summary>
		public static AddressableAsset<Sprite> ReadSpriteFromAssets( this KFFSerializer serializer, string kffPath )
		{
			string assetPath = serializer.ReadString( kffPath );
			return new AddressableAsset<Sprite>( assetPath, AssetManager.GetSprite( assetPath ) );
		}

		/// <summary>
		/// Loads a Mesh from assets, using a KFF serializer.
		/// </summary>
		public static AddressableAsset<Mesh> ReadMeshFromAssets( this KFFSerializer serializer, string kffPath )
		{
			string assetPath = serializer.ReadString( kffPath );
			return new AddressableAsset<Mesh>( assetPath, AssetManager.GetMesh( assetPath ) );
		}

		/// <summary>
		/// Loads an AudioClip from assets, using a KFF serializer.
		/// </summary>
		public static AddressableAsset<AudioClip> ReadAudioClipFromAssets( this KFFSerializer serializer, string kffPath )
		{
			string assetPath = serializer.ReadString( kffPath );
			return new AddressableAsset<AudioClip>( assetPath, AssetManager.GetAudioClip( assetPath ) );
		}
	}
}