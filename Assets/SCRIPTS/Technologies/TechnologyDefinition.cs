using KFF;
using SS.Content;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Technologies
{
	public class TechnologyDefinition : AddressableDefinition, IKFFSerializable
	{
		public string displayName { get; set; }

		public Dictionary<string, int> cost { get; private set; }

		public AddressableAsset<Sprite> icon { get; private set; }

		public TechnologyDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.displayName = serializer.ReadString( "DisplayName" );

			// Cost
			var analysisData = serializer.Analyze( "Cost" );
			this.cost = new Dictionary<string, int>( analysisData.childCount );
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				this.cost.Add( serializer.ReadString( new Path( "Cost.{0}.Id", i ) ), serializer.ReadInt( new Path( "Cost.{0}.Amount", i ) ) );
			}


			this.icon = serializer.ReadSpriteFromAssets( "Icon" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );


			// Cost
			serializer.WriteList( "", "Cost" );
			int i = 0;
			foreach( var kvp in this.cost )
			{
				serializer.AppendClass( "Cost" );
				serializer.WriteString( new Path( "Cost.{0}", i ), "Id", kvp.Key );
				serializer.WriteInt( new Path( "Cost.{0}", i ), "Amount", kvp.Value );
				i++;
			}


			serializer.WriteString( "", "Icon", (string)this.icon );
		}
	}
}