using Katniss.Utils;
using KFF;
using SS.Data;
using SS.ResourceSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Technologies
{
	public class TechnologyDefinition : Definition, IKFFSerializable
	{
		public string displayName { get; set; }

		public ResourceStack[] cost { get; private set; }

		public Tuple<string, Sprite> icon { get; private set; }

		public TechnologyDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.displayName = serializer.ReadString( "DisplayName" );

			serializer.Analyze( "Cost" );
			this.cost = new ResourceStack[serializer.aChildCount];
			for( int i = 0; i < this.cost.Length; i++ )
			{
				this.cost[i] = new ResourceStack( "unused", 0 );
			}
			serializer.DeserializeArray( "Cost", this.cost );

			this.icon = serializer.ReadSpriteFromAssets( "Icon" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );

			serializer.SerializeArray( "", "Cost", this.cost );

			serializer.WriteString( "", "Icon", this.icon.Item1 );
		}
	}
}