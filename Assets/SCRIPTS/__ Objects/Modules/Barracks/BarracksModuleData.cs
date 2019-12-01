using KFF;
using SS.Objects.Modules;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Used to round-trip modules, to and from file.
	/// </summary>
	public class BarracksModuleData : ModuleData
	{
		public string trainedUnitId { get; set; }
		public float trainProgress { get; set; }
		public Dictionary<string, int> resourcesRemaining { get; set; }

		public Vector3 rallyPoint { get; set; }


		public BarracksModuleData()
		{
			this.trainedUnitId = "";
			this.trainProgress = 0.0f;
			this.resourcesRemaining = new Dictionary<string, int>();
			this.rallyPoint = Vector3.zero;
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.trainedUnitId = serializer.ReadString( "TrainedUnitId" );
			this.trainProgress = serializer.ReadFloat( "TrainProgress" );

			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "ResourcesRemaining" );
			this.resourcesRemaining = new Dictionary<string, int>( analysisData.childCount );
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				this.resourcesRemaining.Add( serializer.ReadString( "ResourcesRemaining." + i + ".Id" ), serializer.ReadInt( "ResourcesRemaining." + i + ".Amount" ) );
			}

			this.rallyPoint = serializer.ReadVector3( "RallyPoint" );
		}
		
		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "TrainedUnitId", this.trainedUnitId );
			serializer.WriteFloat( "", "TrainProgress", this.trainProgress );

			serializer.WriteVector3( "", "RallyPoint", this.rallyPoint );

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