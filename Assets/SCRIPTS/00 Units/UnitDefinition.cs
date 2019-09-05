using Katniss.Utils;
using KFF;
using SS.Data;
using SS.ResourceSystem;
using SS.Technologies;
using System;
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
		

		// Movement-related
		public float movementSpeed { get; set; }
		public float rotationSpeed { get; set; }

		public float radius { get; set; }
		public float height { get; set; }

		// Production-related.
		public ResourceStack[] cost { get; private set; }
		public float buildTime { get; private set; }
		public string[] techsRequired { get; private set; } // the default techs required to unlock. TODO ----- interface for this? IUnlockable or sth

		// Display-related
		public Tuple<string, Mesh> mesh { get; private set; }
		public Tuple<string, Texture2D> albedo { get; private set; }
		public Tuple<string, Texture2D> normal { get; private set; }
		public Tuple<string, Sprite> icon { get; private set; }

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

			this.movementSpeed = serializer.ReadFloat( "MovementSpeed" );
			this.rotationSpeed = serializer.ReadFloat( "RotationSpeed" );
			this.radius = serializer.ReadFloat( "Radius" );
			this.height = serializer.ReadFloat( "Height" );

			analysisData = serializer.Analyze( "Cost" );
			this.cost = new ResourceStack[analysisData.childCount];
			for( int i = 0; i < this.cost.Length; i++ )
			{
				this.cost[i] = new ResourceStack( "unused", 0 );
			}
			serializer.DeserializeArray( "Cost", this.cost );
			this.buildTime = serializer.ReadFloat( "BuildTime" );
			this.techsRequired = serializer.ReadStringArray( "TechsRequired" );

			this.mesh = serializer.ReadMeshFromAssets( "Mesh" );
			this.albedo = serializer.ReadTexture2DFromAssets( "AlbedoTexture", TextureType.Albedo );
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

			serializer.WriteFloat( "", "MovementSpeed", this.movementSpeed );
			serializer.WriteFloat( "", "RotationSpeed", this.rotationSpeed );
			serializer.WriteFloat( "", "Radius", this.radius );
			serializer.WriteFloat( "", "Height", this.height );

			serializer.SerializeArray( "", "Cost", this.cost );
			serializer.WriteFloat( "", "BuildTime", this.buildTime );
			serializer.WriteStringArray( "", "TechsRequired", this.techsRequired );

			serializer.WriteString( "", "Mesh", this.mesh.Item1 );
			serializer.WriteString( "", "AlbedoTexture", this.albedo.Item1 );
			serializer.WriteString( "", "NormalTexture", this.normal.Item1 );
			serializer.WriteString( "", "Icon", this.icon.Item1 );
		}
	}
}