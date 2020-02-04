using KFF;
using SS.Content;
using UnityEngine;

namespace SS
{
	public class MaterialDefinition : IKFFSerializable
	{
		public MaterialType materialType { get; set; }

		public AddressableAsset<Texture2D> colorMap { get; set; }
		public AddressableAsset<Texture2D> normalMap { get; set; }
		public AddressableAsset<Texture2D> emissionMap { get; set; }
		public AddressableAsset<Texture2D> metallicMap { get; set; }
		public AddressableAsset<Texture2D> smoothnessMap { get; set; } // (1.0-roughness)

		public float? windDisplacementScale { get; set; } // plants only

		
		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.materialType = (MaterialType)serializer.ReadByte( "MaterialType" );

			this.colorMap = serializer.ReadTexture2DFromAssets( "ColorMap" );
			this.normalMap = serializer.ReadTexture2DFromAssets( "NormalMap", TextureType.Normal );
			this.emissionMap = serializer.ReadTexture2DFromAssets( "EmissionMap" );
			this.metallicMap = serializer.ReadTexture2DFromAssets( "MetallicMap" );
			this.smoothnessMap = serializer.ReadTexture2DFromAssets( "SmoothnessMap" );

			if( this.materialType == MaterialType.PlantOpaque || this.materialType == MaterialType.PlantTransparent )
			{
				this.windDisplacementScale = serializer.ReadFloat( "WindDisplacementScale" );
			}
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteByte( "", "MaterialType", (byte)this.materialType );

			serializer.WriteString( "", "ColorMap", (string)this.colorMap );
			serializer.WriteString( "", "NormalMap", (string)this.normalMap );
			serializer.WriteString( "", "EmissionMap", (string)this.emissionMap );
			serializer.WriteString( "", "MetallicMap", (string)this.metallicMap );
			serializer.WriteString( "", "SmoothnessMap", (string)this.smoothnessMap );

			if( this.materialType == MaterialType.PlantOpaque || this.materialType == MaterialType.PlantTransparent )
			{
				serializer.WriteFloat( "", "WindDisplacementScale", this.windDisplacementScale.Value );
			}
		}
	}
}