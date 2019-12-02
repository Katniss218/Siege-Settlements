using KFF;
using SS.Objects.Modules;
using System;
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
			try
			{
				this.trainedUnitId = serializer.ReadString( "TrainedUnitId" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'TrainedUnitId' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.trainProgress = serializer.ReadFloat( "TrainProgress" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'TrainProgress' (" + serializer.file.fileName + ")." );
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
			else
			{
				throw new Exception( "Missing 'ResourcesRemaining' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.rallyPoint = serializer.ReadVector3( "RallyPoint" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'RallyPoint' (" + serializer.file.fileName + ")." );
			}
		}
		
		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "TrainedUnitId", this.trainedUnitId );
			serializer.WriteFloat( "", "TrainProgress", this.trainProgress );

			serializer.WriteVector3( "", "RallyPoint", this.rallyPoint );

			
			serializer.WriteList( "", "ResourcesRemaining" );
			if( this.resourcesRemaining != null )
			{
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