using KFF;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.AI.Goals
{
	public class TacticalDropOffGoalData : TacticalGoalData
	{
		public Dictionary<string, Tuple<int, int>> resources { get; set; } // initial & remaining in a single thing. null for not specified.


		public TacticalDropOffGoal.DropOffMode dropOffMode { get; set; }
		public Vector3 destinationPos { get; set; }
		public Tuple<Guid,Guid?> destinationGuid { get; set; }

		public bool isHostile { get; set; }

		public override TacticalGoal GetGoal()
		{
			TacticalDropOffGoal goal = new TacticalDropOffGoal();
			goal.SetData( this );
			return goal;
		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "Resources" );
			if( analysisData.isSuccess )
			{
				this.resources = new Dictionary<string, Tuple<int, int>>( analysisData.childCount );
				try
				{
					for( int i = 0; i < analysisData.childCount; i++ )
					{
						string id = serializer.ReadString( new Path( "Resources.{0}.Id", i ) );
						int amt = serializer.ReadInt( new Path( "Resources.{0}.Amount", i ) );
						int amtRemaining = serializer.ReadInt( new Path( "Resources.{0}.AmountRemaining", i ) );
						this.resources.Add( id, new Tuple<int, int>( amt, amtRemaining ) );
					}
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'Resources' (" + serializer.file.fileName + ")." );
				}
			}

			this.dropOffMode = (TacticalDropOffGoal.DropOffMode)serializer.ReadByte( "DropOffMode" );

			try
			{
				if( this.dropOffMode == TacticalDropOffGoal.DropOffMode.POSITION )
				{
					this.destinationPos = serializer.ReadVector3( "Destination" );
				}
				else
				{
					Guid obj = serializer.ReadGuid( "Destination.ObjectGuid" );
					Guid? module = null;
					if( serializer.Analyze( "Destination.ModuleId" ).isSuccess )
					{
						module = serializer.ReadGuid( "Destination.ModuleId" );
					}
					this.destinationGuid = new Tuple<Guid, Guid?>( obj, module );
				}
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Destination' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.isHostile = serializer.ReadBool( "IsHostile" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'HostileMode' (" + serializer.file.fileName + ")." );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			if( this.resources != null )
			{
				serializer.WriteList( "", "Resources" );
				int i = 0;
				foreach( var kvp in this.resources )
				{
					serializer.AppendClass( "Resources" );
					serializer.WriteString( new Path( "Resources.{0}", i ), "Id", kvp.Key );
					serializer.WriteInt( new Path( "Resources.{0}", i ), "Amount", kvp.Value.Item1 );
					serializer.WriteInt( new Path( "Resources.{0}", i ), "AmountRemaining", kvp.Value.Item2 );
					i++;
				}
			}

			serializer.WriteByte( "", "DropOffMode", (byte)this.dropOffMode );

			if( this.dropOffMode == TacticalDropOffGoal.DropOffMode.POSITION )
			{
				serializer.WriteVector3( "", "Destination", this.destinationPos );
			}
			else
			{
				serializer.WriteClass( "", "Destination" );
				serializer.WriteGuid( "Destination", "ObjectGuid", this.destinationGuid.Item1 );
				if( this.destinationGuid.Item2 != null )
				{
					serializer.WriteGuid( "Destination", "ModuleId", this.destinationGuid.Item2.Value );
				}
			}

			serializer.WriteBool( "", "IsHostile", this.isHostile );
		}
	}
}