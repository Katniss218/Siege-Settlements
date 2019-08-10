using Katniss.Utils;
using KFF;
using SS.DataStructures;
using System;
using UnityEngine;

namespace SS.Units
{
	public class UnitDefinition : Definition
	{
		public float healthMax { get; set; }
		public float slashArmor { get; set; }
		public float pierceArmor { get; set; }
		public float concussionArmor { get; set; }

		public float movementSpeed { get; set; }
		public float rotationSpeed { get; set; }

		public float radius { get; set; }
		public float height { get; set; }

		public int inventorySize { get; set; }

		public Tuple<string, Mesh> mesh { get; private set; }
		public Tuple<string, Texture2D> albedo { get; private set; }
		public Tuple<string, Texture2D> normal { get; private set; }

		public UnitDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.healthMax = serializer.ReadFloat( "MaxHealth" );
			this.slashArmor = serializer.ReadFloat( "SlashArmor" );
			this.pierceArmor = serializer.ReadFloat( "PierceArmor" );
			this.concussionArmor = serializer.ReadFloat( "ConcussionArmor" );
			this.movementSpeed = serializer.ReadFloat( "MovementSpeed" );
			this.rotationSpeed = serializer.ReadFloat( "RotationSpeed" );
			this.radius = serializer.ReadFloat( "Radius" );
			this.height = serializer.ReadFloat( "Height" );
			this.inventorySize = serializer.ReadInt( "InventorySize" );
			string meshPath = serializer.ReadString( "Mesh" );
			this.mesh = new Tuple<string, Mesh>( meshPath, AssetsManager.GetMesh( meshPath ) );
			string albedoPath = serializer.ReadString( "AlbedoTexture" );
			this.albedo = new Tuple<string, Texture2D>( albedoPath, AssetsManager.GetTexture2D( albedoPath, TextureType.Albedo ) );
			string normalPath = serializer.ReadString( "NormalTexture" );
			this.normal = new Tuple<string, Texture2D>( normalPath, AssetsManager.GetTexture2D( normalPath, TextureType.Normal ) );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteFloat( "", "MaxHealth", this.healthMax );
			serializer.WriteFloat( "", "SlashArmor", this.slashArmor );
			serializer.WriteFloat( "", "PierceArmor", this.pierceArmor );
			serializer.WriteFloat( "", "ConcussionArmor", this.concussionArmor );
			serializer.WriteFloat( "", "MovementSpeed", this.movementSpeed );
			serializer.WriteFloat( "", "RotationSpeed", this.rotationSpeed );
			serializer.WriteFloat( "", "Radius", this.radius );
			serializer.WriteFloat( "", "Height", this.height );
			serializer.WriteInt( "", "InventorySize", this.inventorySize );
			serializer.WriteString( "", "Mesh", this.mesh.Item1 );
			serializer.WriteString( "", "AlbedoTexture", this.albedo.Item1 );
			serializer.WriteString( "", "NormalTexture", this.normal.Item1 );
		}
	}
}