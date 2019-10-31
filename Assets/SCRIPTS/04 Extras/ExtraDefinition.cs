using KFF;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Extras
{
	public class ExtraDefinition : Definition
	{

		//--------------------------------------------------------------------
		//  ASSETS
		//--------------------------------------

		public MaterialType shaderType { get; set; }

		public AddressableAsset<Mesh> mesh { get; set; }
		public AddressableAsset<Texture2D> albedo { get; set; }
		public AddressableAsset<Texture2D> normal { get; set; }
		public float metallic { get; set; }
		public float smoothness { get; set; }


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public ExtraDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );

			this.shaderType = (MaterialType)serializer.ReadByte( "ShaderType" );

			this.mesh = serializer.ReadMeshFromAssets( "Mesh" );
			this.albedo = serializer.ReadTexture2DFromAssets( "AlbedoTexture", TextureType.Color );
			this.normal = serializer.ReadTexture2DFromAssets( "NormalTexture", TextureType.Normal );
			this.metallic = serializer.ReadFloat( "Metallic" );
			this.smoothness = serializer.ReadFloat( "Smoothness" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );

			serializer.WriteByte( "", "ShaderType", (byte)this.shaderType );

			serializer.WriteString( "", "Mesh", (string)this.mesh );
			serializer.WriteString( "", "AlbedoTexture", (string)this.albedo );
			serializer.WriteString( "", "NormalTexture", (string)this.normal );
			serializer.WriteFloat( "", "Metallic", this.metallic );
			serializer.WriteFloat( "", "Smoothness", this.smoothness );
		}
	}
}