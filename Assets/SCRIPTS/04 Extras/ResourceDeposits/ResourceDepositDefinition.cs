using KFF;
using SS.Content;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Extras
{
	public class ResourceDepositDefinition : Definition
	{
		private string __displayName = "<missing>";
		public string displayName
		{
			get { return this.__displayName; }
			set
			{
				this.__displayName = value ?? throw new Exception( "'DisplayName' can't be null." );
			}
		}

		public Dictionary<string, int> resources { get; private set; }

		public bool isExtracted { get; set; }


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
		public bool isMetallic { get; set; }
		public float smoothness { get; set; }

		public AddressableAsset<AudioClip> mineSound { get; private set; }


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public ResourceDepositDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.displayName = serializer.ReadString( "DisplayName" );
			
			// resources
			var analysisData = serializer.Analyze( "Resources" );
			this.resources = new Dictionary<string, int>( analysisData.childCount );
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				this.resources.Add( serializer.ReadString( "Resources." + i + ".Id" ), serializer.ReadInt( "Resources." + i + ".Capacity" ) );
			}

			this.isExtracted = serializer.ReadBool( "IsExtracted" );
			if( !this.isExtracted )
			{
				this.mineSound = serializer.ReadAudioClipFromAssets( "MineSound" );
			}
			this.size = serializer.ReadVector3( "Size" );
			this.shaderType = (MaterialType)serializer.ReadByte( "ShaderType" );

			this.mesh = serializer.ReadMeshFromAssets( "Mesh" );
			this.albedo = serializer.ReadTexture2DFromAssets( "AlbedoTexture", TextureType.Color );
			this.normal = serializer.ReadTexture2DFromAssets( "NormalTexture", TextureType.Normal );
			this.isMetallic = serializer.ReadBool( "IsMetallic" );
			this.smoothness = serializer.ReadFloat( "Smoothness" );


		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );

			// Cost
			serializer.WriteList( "", "Resources" );
			int i = 0;
			foreach( var kvp in this.resources )
			{
				serializer.AppendClass( "Resources" );
				serializer.WriteString( "Resources." + i, "Id", kvp.Key );
				serializer.WriteInt( "Resources." + i, "Capacity", kvp.Value );
				i++;
			}

			serializer.WriteBool( "", "IsExtracted", this.isExtracted );
			if( !this.isExtracted )
			{
				serializer.WriteString( "", "MineSound", (string)this.mineSound );
			}

			serializer.WriteVector3( "", "Size", this.size );
			serializer.WriteByte( "", "ShaderType", (byte)this.shaderType );

			serializer.WriteString( "", "Mesh", (string)this.mesh );
			serializer.WriteString( "", "AlbedoTexture", (string)this.albedo );
			serializer.WriteString( "", "NormalTexture", (string)this.normal );
			serializer.WriteBool( "", "IsMetallic", this.isMetallic );
			serializer.WriteFloat( "", "Smoothness", this.smoothness );

		}
	}
}