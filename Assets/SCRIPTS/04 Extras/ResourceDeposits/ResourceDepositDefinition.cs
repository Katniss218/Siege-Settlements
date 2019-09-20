using Katniss.Utils;
using KFF;
using SS.Content;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Extras
{
	public class ResourceDepositDefinition : Definition
	{
		public string displayName { get; set; }

		public Dictionary<string, int> resources { get; private set; }

		public bool isExtracted { get; set; }

		public Vector3 size { get; set; }

		public MaterialType shaderType { get; set; }

		public Tuple<string, Mesh> mesh { get; private set; }
		public Tuple<string, Texture2D> albedo { get; private set; }
		public Tuple<string, Texture2D> normal { get; private set; }
		public bool isMetallic { get; set; }
		public float smoothness { get; set; }

		public Tuple<string, AudioClip> pickupSoundEffect { get; private set; }
		public Tuple<string, AudioClip> dropoffSoundEffect { get; private set; }


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
			this.size = serializer.ReadVector3( "Size" );
			this.shaderType = (MaterialType)serializer.ReadByte( "ShaderType" );

			this.mesh = serializer.ReadMeshFromAssets( "Mesh" );
			this.albedo = serializer.ReadTexture2DFromAssets( "AlbedoTexture", TextureType.Color );
			this.normal = serializer.ReadTexture2DFromAssets( "NormalTexture", TextureType.Normal );
			this.isMetallic = serializer.ReadBool( "IsMetallic" );
			this.smoothness = serializer.ReadFloat( "Smoothness" );

			this.pickupSoundEffect = serializer.ReadAudioClipFromAssets( "PickupSound" );
			this.dropoffSoundEffect = serializer.ReadAudioClipFromAssets( "DropoffSound" );

		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );

			// Cost
			serializer.WriteClass( "", "Resources" );
			int i = 0;
			foreach( var kvp in this.resources )
			{
				serializer.AppendClass( "Resources" );
				serializer.WriteString( "Resources." + i, "Id", kvp.Key );
				serializer.WriteInt( "Resources." + i, "Capacity", kvp.Value );
				i++;
			}

			serializer.WriteBool( "", "IsExtracted", this.isExtracted );
			serializer.WriteVector3( "", "Size", this.size );
			serializer.WriteByte( "", "ShaderType", (byte)this.shaderType );

			serializer.WriteString( "", "Mesh", this.mesh.Item1 );
			serializer.WriteString( "", "AlbedoTexture", this.albedo.Item1 );
			serializer.WriteString( "", "NormalTexture", this.normal.Item1 );
			serializer.WriteBool( "", "IsMetallic", this.isMetallic );
			serializer.WriteFloat( "", "Smoothness", this.smoothness );

			serializer.WriteString( "", "PickupSound", this.pickupSoundEffect.Item1 );
			serializer.WriteString( "", "DropoffSound", this.dropoffSoundEffect.Item1 );
		}
	}
}