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
		public string[] queuedUnits { get; set; }
		public float buildTimeRemaining { get; set; }
		public Dictionary<string, int> resourcesRemaining { get; set; }

		public Vector3? rallyPoint { get; set; }


		public BarracksModuleData()
		{
			this.resourcesRemaining = new Dictionary<string, int>();
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			if( serializer.Analyze( "TrainedUnits" ).isSuccess )
			{
				try
				{
					this.queuedUnits = serializer.ReadStringArray( "TrainedUnits" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'TrainedUnits' (" + serializer.file.fileName + ")." );
				}
			}
			else
			{
				this.queuedUnits = null;
			}

			try
			{
				this.buildTimeRemaining = serializer.ReadFloat( "TrainProgress" );
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

			if( serializer.Analyze( "RallyPoint" ).isSuccess )
			{
				try
				{
					this.rallyPoint = serializer.ReadVector3( "RallyPoint" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'RallyPoint' (" + serializer.file.fileName + ")." );
				}
			}
			else
			{
				this.rallyPoint = null;
			}
		}
		
		public override void SerializeKFF( KFFSerializer serializer )
		{
			if( this.queuedUnits != null )
			{
				serializer.WriteStringArray( "", "TrainedUnits", this.queuedUnits );
			}
			serializer.WriteFloat( "", "TrainProgress", this.buildTimeRemaining );

			if( this.rallyPoint != null )
			{
				serializer.WriteVector3( "", "RallyPoint", this.rallyPoint.Value );
			}

			
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