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
		}
	}
}