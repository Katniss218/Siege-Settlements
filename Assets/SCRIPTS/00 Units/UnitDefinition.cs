using Katniss.Utils;
using KFF;
using SS.Content;
using SS.Modules;
using SS.Modules.Inventories;
using SS.Technologies;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Units
{
	public class UnitDefinition : Definition, ITechsRequired
	{
		// Basic.
		public string displayName { get; set; }

		// Health-related
		public float healthMax { get; set; }

		public Armor armor { get; set; }
		

		public MeleeModuleDefinition melee;

		public RangedModuleDefinition ranged;
		
		public bool isConstructor { get; set; }

		// Movement-related
		public float movementSpeed { get; set; }
		public float rotationSpeed { get; set; }

		public float radius { get; set; }
		public float height { get; set; }

		// Production-related.
		public Dictionary<string, int> cost { get; private set; }

		public float buildTime { get; private set; }
		public string[] techsRequired { get; private set; } // the default techs required to unlock. TODO ----- interface for this? IUnlockable or sth

		// Display-related
		public Tuple<string, Mesh> mesh { get; private set; }
		public Tuple<string, Texture2D> albedo { get; private set; }
		public Tuple<string, Texture2D> normal { get; private set; }
		public Tuple<string, Sprite> icon { get; private set; }


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public UnitDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.displayName = serializer.ReadString( "DisplayName" );

			this.healthMax = serializer.ReadFloat( "MaxHealth" );
			this.armor = new Armor();
			serializer.Deserialize( "Armor", this.armor );

			var analysisData = serializer.Analyze( "MeleeModule" );
			if( analysisData.isSuccess )
			{
				this.melee = new MeleeModuleDefinition();
				serializer.Deserialize( "MeleeModule", this.melee );
			}
			analysisData = serializer.Analyze( "RangedModule" );
			if( analysisData.isSuccess )
			{
				this.ranged = new RangedModuleDefinition();
				serializer.Deserialize( "RangedModule", this.ranged );
			}
			
			this.isConstructor = serializer.ReadBool( "IsConstructor" );

			this.movementSpeed = serializer.ReadFloat( "MovementSpeed" );
			this.rotationSpeed = serializer.ReadFloat( "RotationSpeed" );
			this.radius = serializer.ReadFloat( "Radius" );
			this.height = serializer.ReadFloat( "Height" );

			// Cost
			analysisData = serializer.Analyze( "Cost" );
			this.cost = new Dictionary<string, int>( analysisData.childCount );
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				this.cost.Add( serializer.ReadString( new Path( "Cost.{0}.Id", i ) ), serializer.ReadInt( new Path( "Cost.{0}.Amount", i ) ) );
			}

			this.buildTime = serializer.ReadFloat( "BuildTime" );
			this.techsRequired = serializer.ReadStringArray( "TechsRequired" );

			this.mesh = serializer.ReadMeshFromAssets( "Mesh" );
			this.albedo = serializer.ReadTexture2DFromAssets( "AlbedoTexture", TextureType.Color );
			this.normal = serializer.ReadTexture2DFromAssets( "NormalTexture", TextureType.Normal );
			this.icon = serializer.ReadSpriteFromAssets( "Icon" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );

			serializer.WriteFloat( "", "MaxHealth", this.healthMax );
			serializer.Serialize( "", "Armor", this.armor );

			if( this.melee != null )
			{
				serializer.Serialize( "", "MeleeModule", this.melee );
			}
			if( this.ranged != null )
			{
				serializer.Serialize( "", "RangedModule", this.ranged );
			}

			serializer.WriteBool( "", "IsConstructor", this.isConstructor );

			serializer.WriteFloat( "", "MovementSpeed", this.movementSpeed );
			serializer.WriteFloat( "", "RotationSpeed", this.rotationSpeed );
			serializer.WriteFloat( "", "Radius", this.radius );
			serializer.WriteFloat( "", "Height", this.height );

			// Cost
			serializer.WriteClass( "", "Cost" );
			int i = 0;
			foreach( var kvp in this.cost )
			{
				serializer.AppendClass( "Cost" );
				serializer.WriteString( new Path( "Cost.{0}", i ), "Id", kvp.Key );
				serializer.WriteInt( new Path( "Cost.{0}", i ), "Amount", kvp.Value );
				i++;
			}

			serializer.WriteFloat( "", "BuildTime", this.buildTime );
			serializer.WriteStringArray( "", "TechsRequired", this.techsRequired );

			serializer.WriteString( "", "Mesh", this.mesh.Item1 );
			serializer.WriteString( "", "AlbedoTexture", this.albedo.Item1 );
			serializer.WriteString( "", "NormalTexture", this.normal.Item1 );
			serializer.WriteString( "", "Icon", this.icon.Item1 );
		}
	}
}