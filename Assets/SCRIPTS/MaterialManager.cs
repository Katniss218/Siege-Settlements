﻿using SS.Content;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public static class MaterialManager
	{
		private const string OPAQUE_ID = AssetManager.RESOURCE_ID + "Materials/Opaque";
		private const string TRANSPARENT_ID = AssetManager.RESOURCE_ID + "Materials/Transparent";
		private const string PLANT_OPAQUE_ID = AssetManager.RESOURCE_ID + "Materials/Plant_Opaque";
		private const string PLANT_TRANSPARENT_ID = AssetManager.RESOURCE_ID + "Materials/Plant_Transparent";
		private const string COLORED_ID = AssetManager.RESOURCE_ID + "Materials/Colored";
		private const string COLORED_DESTROYABLE_ID = AssetManager.RESOURCE_ID + "Materials/Colored_Destroyable";
		private const string COLORED_CONSTRUCTIBLE_ID = AssetManager.RESOURCE_ID + "Materials/Colored_Constructible";
		private const string PLACEMENT_PREV_ID = AssetManager.RESOURCE_ID + "Materials/PlacementPrev";

		/// <summary>
		/// Creates a new opaque material.
		/// </summary>
		public static Material CreateOpaque( Texture2D color, Texture2D normal, Texture2D emission, float metallic, float smoothness )
		{
			Material material = new Material( AssetManager.GetMaterial( OPAQUE_ID ) );

			material.EnableKeyword( "_NORMALMAP" );
			material.SetTexture( "_BaseMap", color );
			material.SetTexture( "_BumpMap", normal );
			material.SetTexture( "_EmissionMap", emission );
			material.SetFloat( "_Metallic", metallic );
			material.SetFloat( "_Smoothness", smoothness );

			return material;
		}

		/// <summary>
		/// Creates a new transparent material.
		/// </summary>
		public static Material CreateTransparent( Texture2D color, Texture2D normal, Texture2D emission, float metallic, float smoothness )
		{
			Material material = new Material( AssetManager.GetMaterial( TRANSPARENT_ID ) );

			material.SetTexture( "_Albedo", color );
			material.SetTexture( "_Normal", normal );
			material.SetTexture( "_Emission", emission );
			material.SetFloat( "_Metallic", metallic );
			material.SetFloat( "_Smoothness", smoothness );

			return material;
		}

		/// <summary>
		/// Creates a new opaque material with wind-like displacement.
		/// </summary>
		public static Material CreatePlantOpaque( Texture2D color, Texture2D normal, Texture2D emission, float metallic, float smoothness, float displacementScale )
		{
			Material material = new Material( AssetManager.GetMaterial( PLANT_OPAQUE_ID ) );

			material.SetTexture( "_BaseMap", color );
			material.SetTexture( "_BumpMap", normal );
			material.SetTexture( "_Emission", emission );
			material.SetFloat( "_Metallic", metallic );
			material.SetFloat( "_Smoothness", smoothness );
			material.SetFloat( "_DisplacementMagnitude", displacementScale );

			return material;
		}

		/// <summary>
		/// Creates a new transparent material with wind-like displacement.
		/// </summary>
		public static Material CreatePlantTransparent( Texture2D color, Texture2D normal, Texture2D emission, float metallic, float smoothness, float displacementScale )
		{
			Material material = new Material( AssetManager.GetMaterial( PLANT_TRANSPARENT_ID ) );

			material.SetTexture( "_BaseMap", color );
			material.SetTexture( "_BumpMap", normal );
			material.SetTexture( "_Emission", emission );
			material.SetFloat( "_Metallic", metallic );
			material.SetFloat( "_Smoothness", smoothness );
			material.SetFloat( "_DisplacementMagnitude", displacementScale );

			return material;
		}

		/// <summary>
		/// Creates a new opaque material with a texture overlayed on top of base color.
		/// </summary>
		public static Material CreateColored( Color color, Texture2D overlay, Texture2D normal, Texture2D emission, float metallic, float smoothness )
		{
			Material material = new Material( AssetManager.GetMaterial( COLORED_ID ) );

			material.SetColor( "_FactionColor", color );
			material.SetTexture( "_Albedo", overlay );
			material.SetTexture( "_Normal", normal );
			material.SetTexture( "_Emission", emission );
			material.SetFloat( "_Metallic", metallic );
			material.SetFloat( "_Smoothness", smoothness );

			return material;
		}

		/// <summary>
		/// Creates a new opaque material, that crumples, with a texture overlayed on top of base color.
		/// </summary>
		public static Material CreateColoredDestroyable( Color color, Texture2D overlay, Texture2D normal, Texture2D emission, float metallic, float smoothness, float dest )
		{
			Material material = new Material( AssetManager.GetMaterial( COLORED_DESTROYABLE_ID ) );

			material.SetColor( "_FactionColor", color );
			material.SetTexture( "_Albedo", overlay );
			material.SetTexture( "_Normal", normal );
			material.SetTexture( "_Emission", emission );
			material.SetFloat( "_Metallic", metallic );
			material.SetFloat( "_Smoothness", smoothness );
			material.SetFloat( "_Dest", dest );

			return material;
		}

		/// <summary>
		/// Creates a new opaque material, that can "sink" (just like buildings under construction), with a texture overlayed on top of base color.
		/// </summary>
		public static Material CreateColoredConstructible( Color color, Texture2D overlay, Texture2D normal, Texture2D emission, float metallic, float smoothness, float height, float constructionProgress )
		{
			Material material = new Material( AssetManager.GetMaterial( COLORED_CONSTRUCTIBLE_ID ) );

			material.SetColor( "_FactionColor", color );
			material.SetTexture( "_Albedo", overlay );
			material.SetTexture( "_Normal", normal );
			material.SetTexture( "_Emission", emission );
			material.SetFloat( "_Metallic", metallic );
			material.SetFloat( "_Smoothness", smoothness );
			material.SetFloat( "_Height", height );
			material.SetFloat( "_ConstructionProgress", constructionProgress );

			return material;
		}

		/// <summary>
		/// Creates a new opaque material with a texture overlayed on top of base color.
		/// </summary>
		public static Material CreatePlacementPrev( Color color )
		{
			Material material = new Material( AssetManager.GetMaterial( PLACEMENT_PREV_ID ) );

			material.SetColor( "_FactionColor", color );

			return material;
		}

	}
}