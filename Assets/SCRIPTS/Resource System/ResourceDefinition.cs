using Katniss.Utils;
using KFF;
using SS.Data;
using System;
using UnityEngine;

namespace SS.ResourceSystem
{
	public class ResourceDefinition : Definition
	{
		public string displayName { get; private set; }

		public Tuple<string, Sprite> icon { get; private set; }

		// TODO ----- needs default deposit type for dropping off resource.

		public ResourceDefinition( string id ) : base( id )
		{
			
		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.displayName = serializer.ReadString( "DisplayName" );
			string iconPath = serializer.ReadString( "Icon" );
			this.icon = new Tuple<string, Sprite>( iconPath, AssetsManager.GetTexture2D( iconPath, TextureType.Albedo ).MakeSprite() );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );
			serializer.WriteString( "", "Icon", this.icon.Item1 );
		}
	}
}