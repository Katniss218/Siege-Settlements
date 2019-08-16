using Katniss.Utils;
using KFF;
using SS.Data;
using SS.ResourceSystem;
using System;
using UnityEngine;

namespace SS.Buildings
{
	[Serializable]
	public class BuildingDefinition : Definition
	{
		public string displayName { get; set; }

		public float healthMax { get; set; }
		public float slashArmor { get; set; }
		public float pierceArmor { get; set; }
		public float concussionArmor { get; set; }

		public Vector3 size { get; set; }

		public ResourceStack[] cost { get; private set; }

		public Tuple<string, Mesh> mesh { get; private set; }
		public Tuple<string, Texture2D> albedo { get; private set; }
		public Tuple<string, Texture2D> normal { get; private set; }
		public Tuple<string, AudioClip> buildSoundEffect { get; private set; }
		public Tuple<string, AudioClip> deathSoundEffect { get; private set; }


		public BuildingDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.displayName = serializer.ReadString( "DisplayName" );
			this.healthMax = serializer.ReadFloat( "MaxHealth" );
			this.slashArmor = serializer.ReadFloat( "SlashArmor" );
			this.pierceArmor = serializer.ReadFloat( "PierceArmor" );
			this.concussionArmor = serializer.ReadFloat( "ConcussionArmor" );
			this.size = serializer.ReadVector3( "Size" );

			serializer.Analyze( "Cost" );
			this.cost = new ResourceStack[serializer.aChildCount];
			for( int i = 0; i< this.cost.Length; i++ )
			{
				this.cost[i] = new ResourceStack("unused", 0 );
			}
			serializer.DeserializeArray( "Cost", this.cost );

			this.mesh = serializer.ReadMeshFromAssets( "Mesh" );
			this.albedo = serializer.ReadTexture2DFromAssets( "AlbedoTexture", TextureType.Albedo );
			this.normal = serializer.ReadTexture2DFromAssets( "NormalTexture", TextureType.Normal );
			
			this.buildSoundEffect = serializer.ReadAudioClipFromAssets( "BuildSound" );
			this.deathSoundEffect = serializer.ReadAudioClipFromAssets( "DeathSound" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );
			serializer.WriteFloat( "", "MaxHealth", this.healthMax );
			serializer.WriteFloat( "", "SlashArmor", this.slashArmor );
			serializer.WriteFloat( "", "PierceArmor", this.pierceArmor );
			serializer.WriteFloat( "", "ConcussionArmor", this.concussionArmor );
			serializer.WriteVector3( "", "Size", this.size );
			serializer.SerializeArray( "", "Cost", this.cost );

			serializer.WriteString( "", "Mesh", this.mesh.Item1 );
			serializer.WriteString( "", "AlbedoTexture", this.albedo.Item1 );
			serializer.WriteString( "", "NormalTexture", this.normal.Item1 );

			serializer.WriteString( "", "BuildSound", this.buildSoundEffect.Item1 );
			serializer.WriteString( "", "DeathSound", this.deathSoundEffect.Item1 );
		}
	}
}