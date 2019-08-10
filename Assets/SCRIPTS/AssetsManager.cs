using Katniss.Utils;
using KFF;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SS
{
	public static class AssetsManager
	{
		public static string dirPath
		{
			get
			{
				return Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar + "GameAssets";
			}
		}

		public static string GetFullPath( string assetsPath )
		{
			return dirPath + System.IO.Path.DirectorySeparatorChar + assetsPath;
		}


		static Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
		static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
		static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
		static Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

		public static Mesh GetMesh( string assetsPath )
		{
			if( !meshes.ContainsKey( assetsPath ) )
			{
				LoadMesh( assetsPath );
			}
			return meshes[assetsPath];
		}

		// load type represents what type the loader should load if the tex is missing.
		public static Texture2D GetTexture2D( string assetsPath, TextureType loadType )
		{
			if( !textures.ContainsKey( assetsPath ) )
			{
				LoadTexture2D( assetsPath, loadType );
			}
			return textures[assetsPath];
		}

		public static Sprite GetSprite( string assetsPath )
		{
			if( !sprites.ContainsKey( assetsPath ) )
			{
				LoadSprite( assetsPath );
			}
			return sprites[assetsPath];
		}

		public static AudioClip getAudioClip( string assetsPath )
		{
			if( !audioClips.ContainsKey( assetsPath ) )
			{
				LoadAudioClip( assetsPath );
			}
			return audioClips[assetsPath];
		}

		public static void LoadMesh( string assetsPath )
		{
			string filePath = GetFullPath( assetsPath );

			KFFSerializer s = KFFSerializer.ReadFromFile( filePath, Encoding.UTF8 );
			Mesh[] m = MeshKFFSerializer.DeserializeKFF( s );
			meshes.Add( assetsPath, m[0] );
		}

		public static void LoadTexture2D( string assetsPath, TextureType type )
		{
			string filePath = GetFullPath( assetsPath );

			Texture2D t = PngImporter.Import( filePath, type );
			textures.Add( assetsPath, t );
		}

		public static void LoadSprite( string pngPath )
		{
			throw new System.Exception();
			// sprites have a texture (path.png) and KFF data file (path.kff).
		}

		public static void LoadAudioClip( string assetsPath )
		{
			AudioClip audioClip = WavImporter.Import( dirPath + System.IO.Path.DirectorySeparatorChar + assetsPath );
			
			audioClips.Add( assetsPath, audioClip );
		}

		public static void LoadDefaults()
		{
			
		}

		public static void LoadFromLevel( string pathToLevel )
		{

		}
	}
}