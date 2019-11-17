using KFF;
using SS.Modules;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	public class ResourceDepositModuleData : ModuleData
	{
		public const string KFF_TYPEID = "resource_deposit";

		public Dictionary<string, int> items { get; set; }


		public ResourceDepositModuleData()
		{
			this.items = new Dictionary<string, int>();
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			var analysisData = serializer.Analyze( "Items" );
			this.items = new Dictionary<string, int>( analysisData.childCount );
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				this.items.Add( serializer.ReadString( new Path( "Items.{0}.Id", i ) ), serializer.ReadInt( new Path( "Items.{0}.Amount", i ) ) );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteList( "", "Items" );
			int i = 0;
			foreach( var kvp in this.items )
			{
				serializer.AppendClass( "Items" );
				serializer.WriteString( new Path( "Items.{0}", i ), "Id", kvp.Key );
				serializer.WriteInt( new Path( "Items.{0}", i ), "Amount", kvp.Value );
				i++;
			}
		}
	}
}