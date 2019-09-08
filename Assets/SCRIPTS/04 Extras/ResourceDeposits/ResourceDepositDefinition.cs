using Katniss.Utils;
using KFF;
using SS.Data;
using System;
using UnityEngine;

namespace SS.Extras
{
	public class ResourceDepositDefinition : Definition
	{
		public string displayName { get; set; }
		public string resourceId { get; set; }

		public bool isExtracted { get; set; }

		public Vector3 size { get; set; }

		public ShaderType shaderType { get; set; }

		public Tuple<string, Mesh> mesh { get; private set; }
		public Tuple<string, Texture2D> albedo { get; private set; }
		public Tuple<string, Texture2D> normal { get; private set; }
		public bool isMetallic { get; set; }
		public float smoothness { get; set; }


		public ResourceDepositDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.displayName = serializer.ReadString( "DisplayName" );
			this.resourceId = serializer.ReadString( "ResourceId" );
			this.isExtracted = serializer.ReadBool( "IsExtracted" );
			this.size = serializer.ReadVector3( "Size" );
			this.shaderType = (ShaderType)serializer.ReadByte( "ShaderType" );

			this.mesh = serializer.ReadMeshFromAssets( "Mesh" );
			this.albedo = serializer.ReadTexture2DFromAssets( "AlbedoTexture", TextureType.Albedo );
			this.normal = serializer.ReadTexture2DFromAssets( "NormalTexture", TextureType.Normal );
			this.isMetallic = serializer.ReadBool( "IsMetallic" );
			this.smoothness = serializer.ReadFloat( "Smoothness" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );
			serializer.WriteString( "", "ResourceId", this.resourceId );
			serializer.WriteBool( "", "IsExtracted", this.isExtracted );
			serializer.WriteVector3( "", "Size", this.size );
			serializer.WriteByte( "", "ShaderType", (byte)this.shaderType );

			serializer.WriteString( "", "Mesh", this.mesh.Item1 );
			serializer.WriteString( "", "AlbedoTexture", this.albedo.Item1 );
			serializer.WriteString( "", "NormalTexture", this.normal.Item1 );
			serializer.WriteBool( "", "IsMetallic", this.isMetallic );
			serializer.WriteFloat( "", "Smoothness", this.smoothness );
		}
	}
}