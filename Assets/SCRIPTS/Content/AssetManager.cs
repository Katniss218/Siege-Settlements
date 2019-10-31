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
		/// An identifier prefix used to tell the asset manager to load the asset from the level directory.
		/// </summary>
		public const string EXTERN_ASSET_ID = "extern:";

		/// <summary>
		/// An identifier prefix used to tell the asset manager to load the asset from the internal 'Resources' directory.
		/// </summary>
		public const string BUILTIN_ASSET_ID = "builtin:";


		// Asset cache.

		private static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
		private static Dictionary<string, TMP_FontAsset> fonts = new Dictionary<string, TMP_FontAsset>();

		private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
		private static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
		private static Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
		private static Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

		private static Dictionary<string, Material> materials = new Dictionary<string, Material>();
		

		/// <summary>
		/// Contains the level ID from which the asset manager is going to load external assets.
		/// </summary>
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
		
		public static AddressableAsset<GameObject> GetPrefab( string path )
		{
			if( prefabs.TryGetValue( path, out GameObject ret ) )
			{
				return new AddressableAsset<GameObject>( path, ret );
			}

			if( path.StartsWith( EXTERN_ASSET_ID ) )
			{
				throw new System.Exception( "Prefabs can only be loaded internally." );
			}
			else if( path.StartsWith( BUILTIN_ASSET_ID ) )
			{
				prefabs.Add( path, Resources.Load<GameObject>( path.Substring( BUILTIN_ASSET_ID.Length ) ) );
			}
			else
			{
				throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
			}

			return new AddressableAsset<GameObject>( path, prefabs[path] );
		}

		public static AddressableAsset<TMP_FontAsset> GetFont( string path )
		{
			if( fonts.TryGetValue( path, out TMP_FontAsset ret ) )
			{
				return new AddressableAsset<TMP_FontAsset>( path, ret );
			}

			if( path.StartsWith( EXTERN_ASSET_ID ) )
			{
				throw new System.Exception( "Fonts can only be loaded via internally." );
			}
			else if( path.StartsWith( BUILTIN_ASSET_ID ) )
			{
				fonts.Add( path, Resources.Load<TMP_FontAsset>( path.Substring( BUILTIN_ASSET_ID.Length ) ) );
			}
			else
			{
				throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
			}

			return new AddressableAsset<TMP_FontAsset>( path, fonts[path] );
		}


		public static AddressableAsset<Mesh> GetMesh( string path )
		{
			if( meshes.TryGetValue( path, out Mesh ret ) )
			{
				return new AddressableAsset<Mesh>( path, ret );
			}

			if( path.StartsWith( EXTERN_ASSET_ID ) )
			{
				if( string.IsNullOrEmpty( sourceLevelId ) )
				{
					throw new System.Exception( "Can't load external asset '" + path + "'. The source level ID has not been set." );
				}
				meshes.Add( path, ExternalAssetLoader.LoadMesh( LevelManager.GetFullAssetsPath( sourceLevelId, path.Substring( EXTERN_ASSET_ID.Length ) ) ) );

				return new AddressableAsset<Mesh>( path, meshes[path] );
			}

			if( path.StartsWith( BUILTIN_ASSET_ID ) )
			{
				meshes.Add( path, Resources.Load<Mesh>( path.Substring( BUILTIN_ASSET_ID.Length ) ) );

				return new AddressableAsset<Mesh>( path, meshes[path] );
			}

			throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
		}

		// load type represents what type the loader should load if the tex is not loaded.
		public static AddressableAsset<Texture2D> GetTexture2D( string path, TextureType loadType )
		{
			if( textures.TryGetValue( path, out Texture2D ret ) )
			{
				return new AddressableAsset<Texture2D>( path, ret );
			}

			if( path.StartsWith( EXTERN_ASSET_ID ) )
			{
				if( string.IsNullOrEmpty( sourceLevelId ) )
				{
					throw new System.Exception( "Can't load asset '" + path + "'. The source level ID has not been set." );
				}
				textures.Add( path, ExternalAssetLoader.LoadTexture2D( LevelManager.GetFullAssetsPath( sourceLevelId, path.Substring( EXTERN_ASSET_ID.Length ) ), loadType ) );
			}
			else if( path.StartsWith( BUILTIN_ASSET_ID ) )
			{
				textures.Add( path, Resources.Load<Texture2D>( path.Substring( BUILTIN_ASSET_ID.Length ) ) );
			}
			else
			{
				throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
			}

			return new AddressableAsset<Texture2D>( path, textures[path] );
		}

		public static AddressableAsset<Sprite> GetSprite( string path )
		{
			if( sprites.TryGetValue( path, out Sprite ret ) )
			{
				return new AddressableAsset<Sprite>( path, ret );
			}

			if( path.StartsWith( EXTERN_ASSET_ID ) )
			{
				if( string.IsNullOrEmpty( sourceLevelId ) )
				{
					throw new System.Exception( "Can't load asset '" + path + "'. The source level ID has not been set." );
				}
				sprites.Add( path, ExternalAssetLoader.LoadSprite( LevelManager.GetFullAssetsPath( sourceLevelId, path.Substring( EXTERN_ASSET_ID.Length ) ) ) );
			}
			else if( path.StartsWith( BUILTIN_ASSET_ID ) )
			{
				sprites.Add( path, Resources.Load<Sprite>( path.Substring( BUILTIN_ASSET_ID.Length ) ) );
			}
			else
			{
				throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
			}

			return new AddressableAsset<Sprite>( path, sprites[path] );
		}

		public static AddressableAsset<AudioClip> GetAudioClip( string path )
		{
			if( audioClips.TryGetValue( path, out AudioClip ret ) )
			{
				return new AddressableAsset<AudioClip>( path, ret );
			}

			if( path.StartsWith( EXTERN_ASSET_ID ) )
			{
				if( string.IsNullOrEmpty( sourceLevelId ) )
				{
					throw new System.Exception( "Can't load asset '" + path + "'. The source level ID has not been set." );
				}
				audioClips.Add( path, ExternalAssetLoader.LoadAudioClip( LevelManager.GetFullAssetsPath( sourceLevelId, path.Substring( EXTERN_ASSET_ID.Length ) ) ) );
			}
			else if( path.StartsWith( BUILTIN_ASSET_ID ) )
			{
				audioClips.Add( path, Resources.Load<AudioClip>( path.Substring( BUILTIN_ASSET_ID.Length ) ) );
			}
			else
			{
				throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
			}

			return new AddressableAsset<AudioClip>( path, audioClips[path] );
		}

		public static AddressableAsset<Material> GetMaterial( string path )
		{
			if( materials.TryGetValue( path, out Material ret ) )
			{
				return new AddressableAsset<Material>( path, ret );
			}

			if( path.StartsWith( EXTERN_ASSET_ID ) )
			{
				throw new System.Exception( "Materials can only be loaded via internally." );
			}
			else if( path.StartsWith( BUILTIN_ASSET_ID ) )
			{
				materials.Add( path, Resources.Load<Material>( path.Substring( BUILTIN_ASSET_ID.Length ) ) );
			}
			else
			{
				throw new System.Exception( "Invalid asset identifier in path '" + path + "'." );
			}

			return new AddressableAsset<Material>( path, materials[path] );
		}
	}
}