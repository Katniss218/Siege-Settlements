using Katniss.Utils;
using KFF;
using SS.Data;
using System;
using UnityEngine;

namespace SS.Projectiles
{
	public class ProjectileDefinition : Definition, IKFFSerializable
	{
		public bool hasTrail { get; set; }
		public float trailAmt { get; set; }
		public float trailStartSize { get; set; }
		public float trailEndSize { get; set; }
		public float trailLifetime { get; set; }

		public Tuple<string, Texture2D> trailTexture { get; private set; }
		public Tuple<string, Mesh> mesh { get; private set; }
		public Tuple<string, Texture2D> albedo { get; private set; }
		public Tuple<string, Texture2D> normal { get; private set; }

		public ProjectileDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.hasTrail = serializer.ReadBool( "HasTrail" );
			if( hasTrail )
			{
				this.trailAmt = serializer.ReadFloat( "TrailData.Amount" );
				this.trailLifetime = serializer.ReadFloat( "TrailData.Lifetime" );
				this.trailStartSize = serializer.ReadFloat( "TrailData.StartSize" );
				this.trailEndSize = serializer.ReadFloat( "TrailData.EndSize" );
			}
			string meshPath = serializer.ReadString( "Mesh" );
			this.mesh = new Tuple<string, Mesh>( meshPath, AssetsManager.GetMesh( meshPath ) );
			string albedoPath = serializer.ReadString( "AlbedoTexture" );
			this.albedo = new Tuple<string, Texture2D>( albedoPath, AssetsManager.GetTexture2D( albedoPath, TextureType.Albedo ) );
			string normalPath = serializer.ReadString( "NormalTexture" );
			this.normal = new Tuple<string, Texture2D>( normalPath, AssetsManager.GetTexture2D( normalPath, TextureType.Normal ) );
			string trailTexPath = serializer.ReadString( "TrailData.Texture" );
			this.trailTexture = new Tuple<string, Texture2D>( trailTexPath, AssetsManager.GetTexture2D( trailTexPath, TextureType.Normal ) );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "Mesh", this.mesh.Item1 );
			serializer.WriteString( "", "AlbedoTexture", this.albedo.Item1 );
			serializer.WriteString( "", "NormalTexture", this.normal.Item1 );
			serializer.WriteClass( "", "TrailData" );
			serializer.WriteBool( "", "HasTrail", this.hasTrail );
			if( this.hasTrail )
			{
				serializer.WriteFloat( "TrailData", "Amount", this.trailAmt );
				serializer.WriteFloat( "TrailData", "Lifetime", this.trailLifetime );
				serializer.WriteFloat( "TrailData", "StartSize", this.trailStartSize );
				serializer.WriteFloat( "TrailData", "EndSize", this.trailEndSize );
				serializer.WriteString( "TrailData", "Texture", this.trailTexture.Item1 );
			}
		}
	}
}