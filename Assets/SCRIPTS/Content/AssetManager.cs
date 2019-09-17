using Katniss.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SS.Content
{
	public static class AssetManager
	{
		public const string ASSET_ID = "asset:";
		public const string RESOURCE_ID = "resource:";


		private static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
		private static Dictionary<string, TMP_FontAsset> fonts = new Dictionary<string, TMP_FontAsset>();

		private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
		private static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
		private static Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
		private static Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

		private static Dictionary<string, Material> materials = new Dictionary<string, Material>();


		/// <summary>
		/// Returns the path to the "GameData" directory (Read Only).
		/// </summary>
		public static string dirPath
		{
			get
			{
				return Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar + "Assets";
			}
		}

		/// <summary>
		/// Converts relative path into a full system path.
		/// </summary>
		/// <param name="assetsPath">The path starting at GameData directory.</param>
		public static string GetFullPath( string assetsPath )
		{
			return dirPath + System.IO.Path.DirectorySeparatorChar + assetsPath;
		}


		public static GameObject GetPrefab( string path )
		{
			if( prefabs.TryGetValue( path, out GameObject ret ) )
			{
				return ret;
			}

			if( path.StartsWith( ASSET_ID ) )
			{
				throw new System.Exception( "Prefabs can only be loaded via 'Resources.Load'." );
			}
			else if( path.StartsWith( RESOURCE_ID ) )
			{
				prefabs.Add( path, Resources.Load<GameObject>( path.Substring( RESOURCE_ID.Length ) ) );
			}
			else
			{
				throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
			}

			return prefabs[path];
		}

		public static TMP_FontAsset GetFont( string path )
		{
			if( fonts.TryGetValue( path, out TMP_FontAsset ret ) )
			{
				return ret;
			}

			if( path.StartsWith( ASSET_ID ) )
			{
				throw new System.Exception( "Fonts can only be loaded via 'Resources.Load'." );
			}
			else if( path.StartsWith( RESOURCE_ID ) )
			{
				fonts.Add( path, Resources.Load<TMP_FontAsset>( path.Substring( RESOURCE_ID.Length ) ) );
			}
			else
			{
				throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
			}

			return fonts[path];
		}


		public static Mesh GetMesh( string path )
		{
			if( meshes.TryGetValue( path, out Mesh ret ) )
			{
				return ret;
			}

			if( path.StartsWith( ASSET_ID ) )
			{
				meshes.Add( path, ExternalAssetLoader.LoadMesh( GetFullPath( path.Substring( ASSET_ID.Length ) ) ) );
			}
			else if( path.StartsWith( RESOURCE_ID ) )
			{
				meshes.Add( path, Resources.Load<Mesh>( path.Substring( RESOURCE_ID.Length ) ) );
			}
			else
			{
				throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
			}

			return meshes[path];
		}

		// load type represents what type the loader should load if the tex is not loaded.
		public static Texture2D GetTexture2D( string path, TextureType loadType )
		{
			if( textures.TryGetValue( path, out Texture2D ret ) )
			{
				return ret;
			}

			if( path.StartsWith( ASSET_ID ) )
			{
				textures.Add( path, ExternalAssetLoader.LoadTexture2D( GetFullPath( path.Substring( ASSET_ID.Length ) ), loadType ) );
			}
			else if( path.StartsWith( RESOURCE_ID ) )
			{
				textures.Add( path, Resources.Load<Texture2D>( path.Substring( RESOURCE_ID.Length ) ) );
			}
			else
			{
				throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
			}

			return textures[path];
		}

		public static Sprite GetSprite( string path )
		{
			if( sprites.TryGetValue( path, out Sprite ret ) )
			{
				return ret;
			}

			if( path.StartsWith( ASSET_ID ) )
			{
				sprites.Add( path, ExternalAssetLoader.LoadSprite( GetFullPath( path.Substring( ASSET_ID.Length ) ) ) );
			}
			else if( path.StartsWith( RESOURCE_ID ) )
			{
				sprites.Add( path, Resources.Load<Sprite>( path.Substring( RESOURCE_ID.Length ) ) );
			}
			else
			{
				throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
			}

			return sprites[path];
		}

		public static AudioClip GetAudioClip( string path )
		{
			if( audioClips.TryGetValue( path, out AudioClip ret ) )
			{
				return ret;
			}

			if( path.StartsWith( ASSET_ID ) )
			{
				audioClips.Add( path, ExternalAssetLoader.LoadAudioClip( GetFullPath( path.Substring( ASSET_ID.Length ) ) ) );
			}
			else if( path.StartsWith( RESOURCE_ID ) )
			{
				audioClips.Add( path, Resources.Load<AudioClip>( path.Substring( RESOURCE_ID.Length ) ) );
			}
			else
			{
				throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
			}

			return audioClips[path];
		}

		public static Material GetMaterial( string path )
		{
			if( materials.TryGetValue( path, out Material ret ) )
			{
				return ret;
			}

			if( path.StartsWith( ASSET_ID ) )
			{
				throw new System.Exception( "Materials can only be loaded via 'Resources.Load'." );
			}
			else if( path.StartsWith( RESOURCE_ID ) )
			{
				materials.Add( path, Resources.Load<Material>( path.Substring( RESOURCE_ID.Length ) ) );
			}
			else
			{
				throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
			}

			return materials[path];
		}
	}
}