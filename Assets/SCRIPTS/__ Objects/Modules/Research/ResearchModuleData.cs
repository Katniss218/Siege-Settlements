using KFF;
using SS.Objects.Modules;
using System;
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
			try
			{
				this.researchedTechnologyId = serializer.ReadString( "ResearchedTechnologyId" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'ResearchedTechnologyId' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.researchProgress = serializer.ReadFloat( "ResearchProgress" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'ResearchProgress' (" + serializer.file.fileName + ")." );
			}

			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "ResourcesRemaining" );
			if( analysisData.isSuccess )
			{
				this.resourcesRemaining = new Dictionary<string, int>( analysisData.childCount );
				try
				{
					for( int i = 0; i < analysisData.childCount; i++ )
					{
						string id = serializer.ReadString( "ResourcesRemaining." + i + ".Id" );
						int amount = serializer.ReadInt( "ResourcesRemaining." + i + ".Amount" );
						this.resourcesRemaining.Add( id, amount );
					}
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'ResourcesRemaining' (" + serializer.file.fileName + ")." );
				}
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