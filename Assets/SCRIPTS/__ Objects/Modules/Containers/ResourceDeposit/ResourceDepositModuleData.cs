using KFF;
using SS.Objects.Modules;
using System;
using System.Collections.Generic;

namespace SS.Levels.SaveStates
{
	public class ResourceDepositModuleData : ModuleData
	{
		public Dictionary<string, int> items { get; set; }


		public ResourceDepositModuleData()
		{
			this.items = new Dictionary<string, int>();
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "Items" );
			if( analysisData.isSuccess )
			{
				this.items = new Dictionary<string, int>( analysisData.childCount );
				try
				{
					for( int i = 0; i < analysisData.childCount; i++ )
					{
						this.items.Add( serializer.ReadString( new Path( "Items.{0}.Id", i ) ), serializer.ReadInt( new Path( "Items.{0}.Amount", i ) ) );
					}
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'Items' (" + serializer.file.fileName + ")." );
				}
			}
			else
			{
				throw new Exception( "Missing 'Items' (" + serializer.file.fileName + ")." );
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