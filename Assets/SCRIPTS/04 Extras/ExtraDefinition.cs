using Katniss.Utils;
using KFF;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Extras
{
	public class ExtraDefinition : Definition
	{
		public MaterialType shaderType { get; set; }

		public Tuple<string, Mesh> mesh { get; private set; }
		public Tuple<string, Texture2D> albedo { get; private set; }
		public Tuple<string, Texture2D> normal { get; private set; }
		public float metallic { get; set; }
		public float smoothness { get; set; }

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

			serializer.WriteString( "", "Mesh", this.mesh.Item1 );
			serializer.WriteString( "", "AlbedoTexture", this.albedo.Item1 );
			serializer.WriteString( "", "NormalTexture", this.normal.Item1 );
			serializer.WriteFloat( "", "Metallic", this.metallic );
			serializer.WriteFloat( "", "Smoothness", this.smoothness );
		}
	}
}