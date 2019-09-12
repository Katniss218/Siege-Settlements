using Katniss.Utils;
using KFF;
using SS.Data;
using SS.Modules;
using System;
using UnityEngine;

namespace SS.Heroes
{
	public class HeroDefinition : Definition
	{
		public string displayName { get; set; }
		public string displayTitle { get; set; }


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

		// Display-related
		public Tuple<string, Mesh> mesh { get; private set; }
		public Tuple<string, Texture2D> albedo { get; private set; }
		public Tuple<string, Texture2D> normal { get; private set; }
		public Tuple<string, Sprite> icon { get; private set; }

		public HeroDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.displayName = serializer.ReadString( "DisplayName" );
			this.displayTitle = serializer.ReadString( "DisplayTitle" );

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

			this.mesh = serializer.ReadMeshFromAssets( "Mesh" );
			this.albedo = serializer.ReadTexture2DFromAssets( "AlbedoTexture", TextureType.Albedo );
			this.normal = serializer.ReadTexture2DFromAssets( "NormalTexture", TextureType.Normal );
			this.icon = serializer.ReadSpriteFromAssets( "Icon" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );
			serializer.WriteString( "", "DisplayTitle", this.displayTitle );

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

			serializer.WriteString( "", "Mesh", this.mesh.Item1 );
			serializer.WriteString( "", "AlbedoTexture", this.albedo.Item1 );
			serializer.WriteString( "", "NormalTexture", this.normal.Item1 );
			serializer.WriteString( "", "Icon", this.icon.Item1 );
		}
	}
}