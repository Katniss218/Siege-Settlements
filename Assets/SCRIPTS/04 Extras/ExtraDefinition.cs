﻿using Katniss.Utils;
using KFF;
using SS.Data;
using System;
using UnityEngine;

namespace SS.Extras
{
	public class ExtraDefinition : Definition
	{
		public Tuple<string, Mesh> mesh { get; private set; }
		public Tuple<string, Texture2D> albedo { get; private set; }
		public Tuple<string, Texture2D> normal { get; private set; }

		public ExtraDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
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
			serializer.WriteString( "", "Mesh", this.mesh.Item1 );
			serializer.WriteString( "", "AlbedoTexture", this.albedo.Item1 );
			serializer.WriteString( "", "NormalTexture", this.normal.Item1 );
		}
	}
}