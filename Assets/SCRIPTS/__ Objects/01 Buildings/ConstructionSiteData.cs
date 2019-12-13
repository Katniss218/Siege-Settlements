using KFF;
using System;
using System.Collections.Generic;

namespace SS.Levels.SaveStates
{
	public class ConstructionSiteData : IKFFSerializable
	{
		public Dictionary<string, float> resourcesRemaining { get; set; }
		

		public void DeserializeKFF( KFFSerializer serializer )
		{
			// resources
			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "ResourcesRemaining" );
			if( analysisData.isSuccess )
			{
				this.resourcesRemaining = new Dictionary<string, float>( analysisData.childCount );
				try
				{
					for( int i = 0; i < analysisData.childCount; i++ )
					{
						string id = serializer.ReadString( new Path( "ResourcesRemaining.{0}.Id", i ) );
						int amt = serializer.ReadInt( new Path( "ResourcesRemaining.{0}.Amount", i ) );
						if( amt < 0 )
						{
							throw new Exception( "Missing or invalid value of 'ResourcesRemaining' (" + serializer.file.fileName + ")." );
						}
						this.resourcesRemaining.Add( id, amt );
					}
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'ResourcesRemaining' (" + serializer.file.fileName + ")." );
				}
			}
			else
			{
				throw new Exception( "Missing or invalid value of 'ResourcesRemaining' (" + serializer.file.fileName + ")." );
			}
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			// resources
			serializer.WriteList( "", "ResourcesRemaining" );
			int i = 0;
			foreach( var kvp in this.resourcesRemaining )
			{
				serializer.AppendClass( "ResourcesRemaining" );
				serializer.WriteString( new Path( "ResourcesRemaining.{0}", i ), "Id", kvp.Key );
				serializer.WriteFloat( new Path( "ResourcesRemaining.{0}", i ), "Amount", kvp.Value );
				i++;
			}
		}
	}
}