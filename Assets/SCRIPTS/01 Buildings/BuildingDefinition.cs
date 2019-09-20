using Katniss.Utils;
using KFF;
using SS.Content;
using SS.Modules;
using SS.Technologies;
using System;
using System.Collections.Generic;
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

		public Vector3[] placementNodes { get; set; } // len = 0, if empty

		public Vector3 entrance { get; set; }

		public Dictionary<string, int> cost { get; private set; }
		
		public BarracksModuleDefinition barracks;
		public ResearchModuleDefinition research;
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

			this.placementNodes = serializer.ReadVector3Array( "PlacementNodes" );

			this.entrance = serializer.ReadVector3( "Entrance" );

			// Cost
			var analysisData = serializer.Analyze( "Cost" );
			this.cost = new Dictionary<string, int>( analysisData.childCount );
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				this.cost.Add( serializer.ReadString( "Cost." + i + ".Id" ), serializer.ReadInt( "Cost." + i + ".Amount" ) );
			}

			if( serializer.Analyze( "BarracksModule" ).isSuccess )
			{
				this.barracks = new BarracksModuleDefinition();
				serializer.Deserialize( "BarracksModule", this.barracks );
			}
			
			if( serializer.Analyze( "ResearchModule" ).isSuccess )
			{
				this.research = new ResearchModuleDefinition();
				serializer.Deserialize( "ResearchModule", this.research );
			}


			this.techsRequired = serializer.ReadStringArray( "TechsRequired" );

			this.mesh = serializer.ReadMeshFromAssets( "Mesh" );
			this.albedo = serializer.ReadTexture2DFromAssets( "AlbedoTexture", TextureType.Color );
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

			serializer.WriteVector3Array( "", "PlacementNodes", this.placementNodes );

			serializer.WriteVector3( "", "Entrance", this.entrance );

			// Cost
			serializer.WriteClass( "", "Cost" );
			int i = 0;
			foreach( var kvp in this.cost )
			{
				serializer.AppendClass( "Cost" );
				serializer.WriteString( "Cost." + i, "Id", kvp.Key );
				serializer.WriteInt( "Cost." + i, "Amount", kvp.Value );
				i++;
			}


			if( this.barracks != null )
			{
				serializer.Serialize( "", "BarracksModule", this.barracks );
			}
			if( this.research != null )
			{
				serializer.Serialize( "", "ResearchModule", this.research );
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