using Katniss.Utils;
using KFF;
using SS.Data;
using SS.ResourceSystem;
using SS.Technologies;
using System;
using UnityEngine;

namespace SS.Buildings
{
	[Serializable]
	public class BuildingDefinition : Definition, ITechsRequired
	{
		public string displayName { get; set; }

		public float healthMax { get; set; }

		public Armor armor { get; set; }

		public Vector3 size { get; set; }

		public ResourceStack[] cost { get; private set; }

		public bool isBarracks { get; set; }
		public string[] barracksUnits { get; set; }
		public bool isResearch { get; set; }
		public string[] techsRequired { get; private set; } // the default techs required to unlock.

		public Tuple<string, Mesh> mesh { get; private set; }
		public Tuple<string, Texture2D> albedo { get; private set; }
		public Tuple<string, Texture2D> normal { get; private set; }
		public Tuple<string, AudioClip> buildSoundEffect { get; private set; }
		public Tuple<string, AudioClip> deathSoundEffect { get; private set; }
		public Tuple<string, Sprite> icon { get; private set; }


		public BuildingDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.displayName = serializer.ReadString( "DisplayName" );
			this.healthMax = serializer.ReadFloat( "MaxHealth" );
			this.armor = new Armor();
			serializer.Deserialize( "Armor", this.armor );
			this.size = serializer.ReadVector3( "Size" );

			var analysisData = serializer.Analyze( "Cost" );
			this.cost = new ResourceStack[analysisData.childCount];
			for( int i = 0; i < this.cost.Length; i++ )
			{
				this.cost[i] = new ResourceStack( "unused", 0 );
			}
			serializer.DeserializeArray( "Cost", this.cost );

			analysisData = serializer.Analyze( "BarracksModule" );
			if( analysisData.isSuccess )
			{
				this.isBarracks = true;
				this.barracksUnits = serializer.ReadStringArray( "BarracksModule.CreatableUnits" );
			}
			analysisData = serializer.Analyze( "ResearchModule" );
			if( analysisData.isSuccess )
			{
				this.isResearch = true;
			}
			this.techsRequired = serializer.ReadStringArray( "TechsRequired" );

			this.mesh = serializer.ReadMeshFromAssets( "Mesh" );
			this.albedo = serializer.ReadTexture2DFromAssets( "AlbedoTexture", TextureType.Albedo );
			this.normal = serializer.ReadTexture2DFromAssets( "NormalTexture", TextureType.Normal );

			this.buildSoundEffect = serializer.ReadAudioClipFromAssets( "BuildSound" );
			this.deathSoundEffect = serializer.ReadAudioClipFromAssets( "DeathSound" );

			this.icon = serializer.ReadSpriteFromAssets( "Icon" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );
			serializer.WriteFloat( "", "MaxHealth", this.healthMax );
			serializer.Serialize( "", "Armor", this.armor );
			serializer.WriteVector3( "", "Size", this.size );
			serializer.SerializeArray( "", "Cost", this.cost );

			if( this.isBarracks )
			{
				serializer.WriteClass( "", "BarracksModule" );
				serializer.WriteStringArray( "BarracksModule", "CreatableUnits", barracksUnits );
			}
			if( this.isResearch )
			{
				serializer.WriteClass( "", "ResearchModule" );
			}
			serializer.WriteStringArray( "", "TechsRequired", this.techsRequired );

			serializer.WriteString( "", "Mesh", this.mesh.Item1 );
			serializer.WriteString( "", "AlbedoTexture", this.albedo.Item1 );
			serializer.WriteString( "", "NormalTexture", this.normal.Item1 );

			serializer.WriteString( "", "BuildSound", this.buildSoundEffect.Item1 );
			serializer.WriteString( "", "DeathSound", this.deathSoundEffect.Item1 );
			serializer.WriteString( "", "Icon", this.icon.Item1 );
		}
	}
}