using Katniss.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SS.Content
{
	/// <summary>
	/// A class for managing Assets (models/textures/sounds/fonts/materials/etc).
	/// </summary>
	public static class AssetManager
	{
		/// <summary>
		/// The identifier prefix used to tell the asset manager to load the asset from the level directory.
		/// </summary>
		public const string EXTERN_ASSET_IDENTIFIER = "asset:"; // FIXME - change these to 'extern' and 'builtin'

		/// <summary>
		/// The identifier prefix used to tell the asset manager to load the asset from the internal 'Resources' directory.
		/// </summary>
		public const string BUILTIN_ASSET_IDENTIFIER = "resource:";



		private static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
		private static Dictionary<string, TMP_FontAsset> fonts = new Dictionary<string, TMP_FontAsset>();

		private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
		private static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
		private static Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
		private static Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

		private static Dictionary<string, Material> materials = new Dictionary<string, Material>();

#warning incomplete - should load the assets from either: resources.load (built-in textures, unmodifyable) / current level.
//  we want to have internal textures consistent as to not break multiplayer sync.

			
			#warning if a level is not loaded, the level-based asset can't be loaded and should throw exception.

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

		/// <summary>
		/// Clears every asset.
		/// </summary>
		public static void Purge()
		{
#warning incomplete
		}

		public static GameObject GetPrefab( string path )
		{
			if( prefabs.TryGetValue( path, out GameObject ret ) )
			{
				return ret;
			}

			if( path.StartsWith( EXTERN_ASSET_IDENTIFIER ) )
			{
				throw new System.Exception( "Prefabs can only be loaded via 'Resources.Load'." );
			}
			else if( path.StartsWith( BUILTIN_ASSET_IDENTIFIER ) )
			{
				prefabs.Add( path, Resources.Load<GameObject>( path.Substring( BUILTIN_ASSET_IDENTIFIER.Length ) ) );
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

			if( path.StartsWith( EXTERN_ASSET_IDENTIFIER ) )
			{
				throw new System.Exception( "Fonts can only be loaded via 'Resources.Load'." );
			}
			else if( path.StartsWith( BUILTIN_ASSET_IDENTIFIER ) )
			{
				fonts.Add( path, Resources.Load<TMP_FontAsset>( path.Substring( BUILTIN_ASSET_IDENTIFIER.Length ) ) );
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

			if( path.StartsWith( EXTERN_ASSET_IDENTIFIER ) )
			{
				meshes.Add( path, ExternalAssetLoader.LoadMesh( GetFullPath( path.Substring( EXTERN_ASSET_IDENTIFIER.Length ) ) ) );
			}
			else if( path.StartsWith( BUILTIN_ASSET_IDENTIFIER ) )
			{
				meshes.Add( path, Resources.Load<Mesh>( path.Substring( BUILTIN_ASSET_IDENTIFIER.Length ) ) );
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

			if( path.StartsWith( EXTERN_ASSET_IDENTIFIER ) )
			{
				textures.Add( path, ExternalAssetLoader.LoadTexture2D( GetFullPath( path.Substring( EXTERN_ASSET_IDENTIFIER.Length ) ), loadType ) );
			}
			else if( path.StartsWith( BUILTIN_ASSET_IDENTIFIER ) )
			{
				textures.Add( path, Resources.Load<Texture2D>( path.Substring( BUILTIN_ASSET_IDENTIFIER.Length ) ) );
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

			if( path.StartsWith( EXTERN_ASSET_IDENTIFIER ) )
			{
				sprites.Add( path, ExternalAssetLoader.LoadSprite( GetFullPath( path.Substring( EXTERN_ASSET_IDENTIFIER.Length ) ) ) );
			}
			else if( path.StartsWith( BUILTIN_ASSET_IDENTIFIER ) )
			{
				sprites.Add( path, Resources.Load<Sprite>( path.Substring( BUILTIN_ASSET_IDENTIFIER.Length ) ) );
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

			if( path.StartsWith( EXTERN_ASSET_IDENTIFIER ) )
			{
				audioClips.Add( path, ExternalAssetLoader.LoadAudioClip( GetFullPath( path.Substring( EXTERN_ASSET_IDENTIFIER.Length ) ) ) );
			}
			else if( path.StartsWith( BUILTIN_ASSET_IDENTIFIER ) )
			{
				audioClips.Add( path, Resources.Load<AudioClip>( path.Substring( BUILTIN_ASSET_IDENTIFIER.Length ) ) );
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

			if( path.StartsWith( EXTERN_ASSET_IDENTIFIER ) )
			{
				throw new System.Exception( "Materials can only be loaded via 'Resources.Load'." );
			}
			else if( path.StartsWith( BUILTIN_ASSET_IDENTIFIER ) )
			{
				materials.Add( path, Resources.Load<Material>( path.Substring( BUILTIN_ASSET_IDENTIFIER.Length ) ) );
			}
			else
			{
				throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
			}

			return materials[path];
		}
	}
}