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
		public string[] queuedTechnologies { get; set; }
		public float researchTimeRemaining { get; set; }
		public Dictionary<string, int> resourcesRemaining { get; set; }


		public ResearchModuleData()
		{
			this.resourcesRemaining = new Dictionary<string, int>();
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			if( serializer.Analyze( "ResearchedTechnologies" ).isSuccess )
			{
				try
				{
					this.queuedTechnologies = serializer.ReadStringArray( "ResearchedTechnologies" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'ResearchedTechnologies' (" + serializer.file.fileName + ")." );
				}
			}
			else
			{
				this.queuedTechnologies = null;
			}

			try
			{
				this.researchTimeRemaining = serializer.ReadFloat( "ResearchProgress" );
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
			serializer.WriteStringArray( "", "ResearchedTechnologies", this.queuedTechnologies );
			serializer.WriteFloat( "", "ResearchProgress", this.researchTimeRemaining );

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