using Katniss.Utils;
using SS.Levels;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SS.Content
{
	/// <summary>
	/// A class for managing game's currently loaded Assets (models/textures/sounds/fonts/materials/etc).
	/// </summary>
	public static class AssetManager
	{
		/// <summary>
		/// The identifier prefix used to tell the asset manager to load the asset from the level directory.
		/// </summary>
		public const string EXTERN_ASSET_IDENTIFIER = "extern:";

		/// <summary>
		/// The identifier prefix used to tell the asset manager to load the asset from the internal 'Resources' directory.
		/// </summary>
		public const string BUILTIN_ASSET_IDENTIFIER = "builtin:";


		// Asset cache.

		private static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
		private static Dictionary<string, TMP_FontAsset> fonts = new Dictionary<string, TMP_FontAsset>();

		private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
		private static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
		private static Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
		private static Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

		private static Dictionary<string, Material> materials = new Dictionary<string, Material>();
		

		public static string sourceLevelId { get; set; }


		/// <summary>
		/// Clears the cached assets (typicaly used when level is unloaded).
		/// </summary>
		public static void Purge()
		{
			prefabs.Clear();
			fonts.Clear();

			textures.Clear();
			sprites.Clear();
			meshes.Clear();
			audioClips.Clear();

			materials.Clear();
		}

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
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
				if( path.EndsWith( "ksm" ) )
					meshes.Add( path, ExternalAssetLoader.LoadMesh2( LevelManager.GetFullAssetsPath( sourceLevelId, path.Substring( EXTERN_ASSET_IDENTIFIER.Length ) ) ) );
				else
					meshes.Add( path, ExternalAssetLoader.LoadMesh( LevelManager.GetFullAssetsPath( sourceLevelId, path.Substring( EXTERN_ASSET_IDENTIFIER.Length ) ) ) );
			}
			else if( path.StartsWith( BUILTIN_ASSET_IDENTIFIER ) )
			{
				if( string.IsNullOrEmpty( sourceLevelId ) )
				{
					throw new System.Exception( "Can't load asset '" + path + "'. The source level ID has not been set." );
				}
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
				if( string.IsNullOrEmpty( sourceLevelId ) )
				{
					throw new System.Exception( "Can't load asset '" + path + "'. The source level ID has not been set." );
				}
				textures.Add( path, ExternalAssetLoader.LoadTexture2D( LevelManager.GetFullAssetsPath( sourceLevelId, path.Substring( EXTERN_ASSET_IDENTIFIER.Length ) ), loadType ) );
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
				if( string.IsNullOrEmpty( sourceLevelId ) )
				{
					throw new System.Exception( "Can't load asset '" + path + "'. The source level ID has not been set." );
				}
				sprites.Add( path, ExternalAssetLoader.LoadSprite( LevelManager.GetFullAssetsPath( sourceLevelId, path.Substring( EXTERN_ASSET_IDENTIFIER.Length ) ) ) );
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
				if( string.IsNullOrEmpty( sourceLevelId ) )
				{
					throw new System.Exception( "Can't load asset '" + path + "'. The source level ID has not been set." );
				}
				audioClips.Add( path, ExternalAssetLoader.LoadAudioClip( LevelManager.GetFullAssetsPath( sourceLevelId, path.Substring( EXTERN_ASSET_IDENTIFIER.Length ) ) ) );
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