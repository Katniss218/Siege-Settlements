using KFF;
using System.Collections.Generic;

namespace SS.Levels.SaveStates
{
	public class ConstructionSiteData : IKFFSerializable
	{
		public Dictionary<string, float> resourcesRemaining { get; set; }
		

		public void DeserializeKFF( KFFSerializer serializer )
		{
			// resources
			var analysisData = serializer.Analyze( "ResourcesRemaining" );
			this.resourcesRemaining = new Dictionary<string, float>( analysisData.childCount );
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				this.resourcesRemaining.Add( serializer.ReadString( new Path( "ResourcesRemaining.{0}.Id", i ) ), serializer.ReadFloat( new Path( "ResourcesRemaining.{0}.Amount", i ) ) );
			}
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			// resources
			serializer.WriteClass( "", "ResourcesRemaining" );
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