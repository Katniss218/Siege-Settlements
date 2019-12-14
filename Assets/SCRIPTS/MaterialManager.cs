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

		

		public static Material CreateMaterial( MaterialDefinition def )
		{
			// Acts as an intermediate between MaterialDefinition and shader property names.
			if( def.materialType == MaterialType.Opaque )
			{
				return CreateOpaque( def.colorMap, def.normalMap, def.emissionMap, def.metallicMap, def.smoothnessMap );
			}
			if( def.materialType == MaterialType.Transparent )
			{
				return CreateTransparent( def.colorMap, def.normalMap, def.emissionMap, def.metallicMap, def.smoothnessMap );
			}
			if( def.materialType == MaterialType.PlantOpaque )
			{
				return CreatePlantOpaque( def.colorMap, def.normalMap, def.emissionMap, def.metallicMap, def.smoothnessMap, def.windDisplacementScale.Value );
			}
			if( def.materialType == MaterialType.PlantTransparent )
			{
				return CreatePlantTransparent( def.colorMap, def.normalMap, def.emissionMap, def.metallicMap, def.smoothnessMap, def.windDisplacementScale.Value );
			}
			if( def.materialType == MaterialType.Colored )
			{
				return CreateColored( default( Color ), def.colorMap, def.normalMap, def.emissionMap, def.metallicMap, def.smoothnessMap );
			}
			if( def.materialType == MaterialType.ColoredDestroyable )
			{
				return CreateColoredDestroyable( default( Color ), def.colorMap, def.normalMap, def.emissionMap, def.metallicMap, def.smoothnessMap, default( float ) );
			}
			if( def.materialType == MaterialType.ColoredConstructible )
			{
				return CreateColoredConstructible( default( Color ), def.colorMap, def.normalMap, def.emissionMap, def.metallicMap, def.smoothnessMap, default( float ) );
			}
			if( def.materialType == MaterialType.Particles )
			{
				return CreateParticles( def.colorMap, default( Color ) );
			}
			throw new System.Exception( "Unknown materialType '" + def.materialType + "'." );
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
			material.SetTexture( "_Emission", emission );
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
			
			material.SetColor( "_FactionColor", color );
			material.SetTexture( "_Albedo", overlay );
			material.SetTexture( "_Normal", normal );
			material.SetTexture( "_Emission", emission );
			material.SetTexture( "_MetallicMap", metallicMap );
			material.SetTexture( "_SmoothnessMap", smoothnessMap );

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
		public static Material CreateParticles( Texture2D texture, Color? emissionColor )
		{
			Material material = new Material( AssetManager.GetMaterialPrototype( PARTICLES_ID ) );

			material.SetTexture( "_BaseMap", texture );
			if( emissionColor != null )
			{
				// Emission Keyword must be enabled for all materials anyway. Otherwise it doesn't work (it won't enable on the specific instance).
				material.EnableKeyword( "_EMISSION" );
				material.SetColor( "_EmissionColor", emissionColor.Value );
			}

			return material;
		}
	}
}