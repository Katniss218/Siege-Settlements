using KFF;
using SS.Objects.Modules;
using System.Collections.Generic;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Used to round-trip modules, to and from file.
	/// </summary>
	public class ResearchModuleData : ModuleData
	{
		public string researchedTechnologyId { get; set; }
		public float researchProgress { get; set; }
		public Dictionary<string, int> resourcesRemaining { get; set; }


		public ResearchModuleData()
		{
			this.researchedTechnologyId = "";
			this.researchProgress = 0.0f;
			this.resourcesRemaining = new Dictionary<string, int>();
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.researchedTechnologyId = serializer.ReadString( "ResearchedTechnologyId" );
			this.researchProgress = serializer.ReadFloat( "ResearchProgress" );

			var analysisData = serializer.Analyze( "ResourcesRemaining" );
			this.resourcesRemaining = new Dictionary<string, int>( analysisData.childCount );
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				this.resourcesRemaining.Add( serializer.ReadString( new Path( "ResourcesRemaining.{0}.Id", i ) ), serializer.ReadInt( new Path( "ResourcesRemaining.{0}.Amount", i ) ) );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "ResearchedTechnologyId", this.researchedTechnologyId );
			serializer.WriteFloat( "", "ResearchProgress", this.researchProgress );

			if( resourcesRemaining != null )
			{
				serializer.WriteList( "", "ResourcesRemaining" );
				int i = 0;
				foreach( var kvp in this.resourcesRemaining )
				{
					serializer.AppendClass( "ResourcesRemaining" );
					serializer.WriteString( new Path( "ResourcesRemaining.{0}", i ), "Id", kvp.Key );
					serializer.WriteInt( new Path( "ResourcesRemaining.{0}", i ), "Amount", kvp.Value );
					i++;
				}
			}
		}
	}
}