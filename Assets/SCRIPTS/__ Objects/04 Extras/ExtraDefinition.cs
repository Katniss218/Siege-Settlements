using KFF;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Extras
{
	public class ExtraDefinition : ObjectDefinition
	{
		public string displayName { get; set; }

		//--------------------------------------------------------------------
		//  SIZE-RELATED
		//--------------------------------------

		private Vector3 __size = new Vector3( 0.25f, 0.25f, 0.25f );
		public Vector3 size
		{
			get { return this.__size; }
			set
			{
				if( value.x <= 0.0f
				 || value.y <= 0.0f
				 || value.z <= 0.0f )
				{
					throw new Exception( "'Size' can't have values less than or equal to 0." );
				}
				this.__size = value;
			}
		}

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
			this.displayName = serializer.ReadString( "DisplayName" );

			this.size = serializer.ReadVector3( "Size" );

			this.shaderType = (MaterialType)serializer.ReadByte( "ShaderType" );

			this.mesh = serializer.ReadMeshFromAssets( "Mesh" );
			this.albedo = serializer.ReadTexture2DFromAssets( "AlbedoTexture", TextureType.Color );
			this.normal = serializer.ReadTexture2DFromAssets( "NormalTexture", TextureType.Normal );
			this.metallic = serializer.ReadFloat( "Metallic" );
			this.smoothness = serializer.ReadFloat( "Smoothness" );

			this.DeserializeModulesKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );

			serializer.WriteVector3( "", "Size", this.size );

			serializer.WriteByte( "", "ShaderType", (byte)this.shaderType );

			serializer.WriteString( "", "Mesh", (string)this.mesh );
			serializer.WriteString( "", "AlbedoTexture", (string)this.albedo );
			serializer.WriteString( "", "NormalTexture", (string)this.normal );
			serializer.WriteFloat( "", "Metallic", this.metallic );
			serializer.WriteFloat( "", "Smoothness", this.smoothness );

			this.SerializeModulesKFF( serializer );
		}
	}
}