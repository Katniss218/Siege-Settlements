using SS.Content;
using UnityEngine;

namespace SS
{
	public static class MaterialManager
	{
		private const string OPAQUE_ID = AssetManager.BUILTIN_ASSET_ID + "Materials/Opaque";
		private const string TRANSPARENT_ID = AssetManager.BUILTIN_ASSET_ID + "Materials/Transparent";
		private const string PLANT_OPAQUE_ID = AssetManager.BUILTIN_ASSET_ID + "Materials/Plant_Opaque";
		private const string PLANT_TRANSPARENT_ID = AssetManager.BUILTIN_ASSET_ID + "Materials/Plant_Transparent";
		private const string COLORED_ID = AssetManager.BUILTIN_ASSET_ID + "Materials/Colored";
		private const string COLORED_DESTROYABLE_ID = AssetManager.BUILTIN_ASSET_ID + "Materials/Colored_Destroyable";
		private const string COLORED_CONSTRUCTIBLE_ID = AssetManager.BUILTIN_ASSET_ID + "Materials/Colored_Constructible";
		private const string PLACEMENT_PREVIEW_ID = AssetManager.BUILTIN_ASSET_ID + "Materials/PlacementPreview";
		private const string PARTICLES_ID = AssetManager.BUILTIN_ASSET_ID + "Materials/Particles";

		

		public static Material CreateMaterial( MaterialDefinition data )
		{
#warning depending on the base material used, it will try to load additional properties (textures, floats, etc.).
			if( data.materialType == MaterialType.Opaque )
			{
				return CreateOpaque( data.colorMap, data.normalMap, data.emissionMap, data.metallicMap, data.smoothnessMap );
			}
			if( data.materialType == MaterialType.Transparent )
			{
				return CreateTransparent( data.colorMap, data.normalMap, data.emissionMap, data.metallicMap, data.smoothnessMap );
			}
			if( data.materialType == MaterialType.PlantOpaque )
			{
				return CreatePlantOpaque( data.colorMap, data.normalMap, data.emissionMap, data.metallicMap, data.smoothnessMap, data.windDisplacementScale.Value );
			}
			if( data.materialType == MaterialType.PlantTransparent )
			{
				return CreatePlantTransparent( data.colorMap, data.normalMap, data.emissionMap, data.metallicMap, data.smoothnessMap, data.windDisplacementScale.Value );
			}
			if( data.materialType == MaterialType.Colored )
			{
				return CreateColored( default( Color ), data.colorMap, data.normalMap, data.emissionMap, data.metallicMap, data.smoothnessMap );
			}
			if( data.materialType == MaterialType.ColoredDestroyable )
			{
				return CreateColoredDestroyable( default( Color ), data.colorMap, data.normalMap, data.emissionMap, data.metallicMap, data.smoothnessMap, default( float ) );
			}
			if( data.materialType == MaterialType.ColoredConstructible )
			{
				return CreateColoredConstructible( default( Color ), data.colorMap, data.normalMap, data.emissionMap, data.metallicMap, data.smoothnessMap, default( float ) );
			}
			if( data.materialType == MaterialType.Particles )
			{
				return CreateParticles( data.colorMap );
			}
			throw new System.Exception( "Unknown materialType '" + data.materialType + "'." );
		}

		/// <summary>
		/// Creates a new opaque material.
		/// </summary>
		public static Material CreateOpaque( Texture2D color, Texture2D normal, Texture2D emission, Texture2D metallicMap, Texture2D smoothnessMap )
		{
			Material material = new Material( AssetManager.GetMaterialPrototype( OPAQUE_ID ) );
			
			material.EnableKeyword( "_NORMALMAP" );
			material.SetTexture( "_BaseMap", color );
			material.SetTexture( "_BumpMap", normal );
			material.SetTexture( "_EmissionMap", emission );
			material.SetTexture( "_MetallicMap", metallicMap );
			material.SetTexture( "_SmoothnessMap", smoothnessMap );

			return material;
		}

		/// <summary>
		/// Creates a new transparent material.
		/// </summary>
		public static Material CreateTransparent( Texture2D color, Texture2D normal, Texture2D emission, Texture2D metallicMap, Texture2D smoothnessMap )
		{
			Material material = new Material( AssetManager.GetMaterialPrototype( TRANSPARENT_ID ) );

			material.SetTexture( "_Albedo", color );
			material.SetTexture( "_Normal", normal );
			material.SetTexture( "_Emission", emission );
			material.SetTexture( "_MetallicMap", metallicMap );
			material.SetTexture( "_SmoothnessMap", smoothnessMap );

			return material;
		}

		/// <summary>
		/// Creates a new opaque material with wind-like displacement.
		/// </summary>
		public static Material CreatePlantOpaque( Texture2D color, Texture2D normal, Texture2D emission, Texture2D metallicMap, Texture2D smoothnessMap, float displacementScale )
		{
			Material material = new Material( AssetManager.GetMaterialPrototype( PLANT_OPAQUE_ID ) );

			material.SetTexture( "_BaseMap", color );
			material.SetTexture( "_BumpMap", normal );
			material.SetTexture( "_Emission", emission );
			material.SetTexture( "_MetallicMap", metallicMap );
			material.SetTexture( "_SmoothnessMap", smoothnessMap );
			material.SetFloat( "_DisplacementMagnitude", displacementScale );

			return material;
		}

		/// <summary>
		/// Creates a new transparent material with wind-like displacement.
		/// </summary>
		public static Material CreatePlantTransparent( Texture2D color, Texture2D normal, Texture2D emission, Texture2D metallicMap, Texture2D smoothnessMap, float displacementScale )
		{
			Material material = new Material( AssetManager.GetMaterialPrototype( PLANT_TRANSPARENT_ID ) );

			material.SetTexture( "_BaseMap", color );
			material.SetTexture( "_BumpMap", normal );
			material.SetTexture( "_Emission", emission );
			material.SetTexture( "_MetallicMap", metallicMap );
			material.SetTexture( "_SmoothnessMap", smoothnessMap );
			material.SetFloat( "_DisplacementMagnitude", displacementScale );

			return material;
		}

		/// <summary>
		/// Creates a new opaque material with a texture overlayed on top of base color.
		/// </summary>
		public static Material CreateColored( Color color, Texture2D overlay, Texture2D normal, Texture2D emission, Texture2D metallicMap, Texture2D smoothnessMap )
		{
			Material material = new Material( AssetManager.GetMaterialPrototype( COLORED_ID ) );

			material.SetColor( "_FactionColor", color );
			material.SetTexture( "_Albedo", overlay );
			material.SetTexture( "_Normal", normal );
			material.SetTexture( "_Emission", emission );
			material.SetTexture( "_MetallicMap", metallicMap );
			material.SetTexture( "_SmoothnessMap", smoothnessMap );

			return material;
		}

		/// <summary>
		/// Creates a new opaque material, that crumples, with a texture overlayed on top of base color.
		/// </summary>
		public static Material CreateColoredDestroyable( Color color, Texture2D overlay, Texture2D normal, Texture2D emission, Texture2D metallicMap, Texture2D smoothnessMap, float dest )
		{
			Material material = new Material( AssetManager.GetMaterialPrototype( COLORED_DESTROYABLE_ID ) );

			material.SetColor( "_FactionColor", color );
			material.SetTexture( "_Albedo", overlay );
			material.SetTexture( "_Normal", normal );
			material.SetTexture( "_Emission", emission );
			material.SetTexture( "_MetallicMap", metallicMap );
			material.SetTexture( "_SmoothnessMap", smoothnessMap );
			material.SetFloat( "_Dest", dest );

			return material;
		}

		/// <summary>
		/// Creates a new opaque material, that can "sink" (just like buildings under construction), with a texture overlayed on top of base color.
		/// </summary>
		public static Material CreateColoredConstructible( Color color, Texture2D overlay, Texture2D normal, Texture2D emission, Texture2D metallicMap, Texture2D smoothnessMap, float offsetY )
		{
			Material material = new Material( AssetManager.GetMaterialPrototype( COLORED_CONSTRUCTIBLE_ID ) );

			string[] props = material.GetTexturePropertyNames();
			for( int i = 0; i < props.Length; i++ )
			{
				Debug.Log( props[i] );
			}

			material.SetColor( "_FactionColor", color );
			material.SetTexture( "_Albedo", overlay );
			material.SetTexture( "_Normal", normal );
			material.SetTexture( "_Emission", emission );
			material.SetTexture( "_MetallicMap", metallicMap );
			material.SetTexture( "_SmoothnessMap", smoothnessMap );
#warning replace roughness with smoothness.

#warning TODO! - replace height & construction progress with offsetY.

			//material.SetFloat( "_Height", height );
			//material.SetFloat( "_ConstructionProgress", constructionProgress );

			return material;
		}

		/// <summary>
		/// Creates a new opaque material with a texture overlayed on top of base color.
		/// </summary>
		public static Material CreatePlacementPreview( Color color )
		{
			Material material = new Material( AssetManager.GetMaterialPrototype( PLACEMENT_PREVIEW_ID ) );

			material.SetColor( "_FactionColor", color );

			return material;
		}

		/// <summary>
		/// Creates a new opaque material with a texture overlayed on top of base color.
		/// </summary>
		public static Material CreateParticles( Texture2D texture )
		{
			Material material = new Material( AssetManager.GetMaterialPrototype( PARTICLES_ID ) );

			material.SetTexture( "_BaseMap", texture );
			//material.SetColor( "_BaseColor", tint );

			return material;
		}
	}
}