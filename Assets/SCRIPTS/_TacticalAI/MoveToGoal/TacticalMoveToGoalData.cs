using KFF;
using System;
using UnityEngine;

namespace SS.AI.Goals
{
	public class TacticalMoveToGoalData : TacticalGoalData
	{
		public TacticalMoveToGoal.DestinationType destination { get; set; }

		public Vector3? destinationPosition { get; set; }
		public Guid? destinationObjectGuid { get; set; }

		public bool isHostile { get; set; }


		public override TacticalGoal GetInstance()
		{
			TacticalMoveToGoal goal = new TacticalMoveToGoal();
			goal.SetData( this );
			return goal;
		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.destination = (TacticalMoveToGoal.DestinationType)serializer.ReadByte( "Destination" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Destination' (" + serializer.file.fileName + ")." );
			}

			if( this.destination == TacticalMoveToGoal.DestinationType.POSITION )
			{
				try
				{
					this.destinationPosition = serializer.ReadVector3( "DestinationPosition" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'DestinationPosition' (" + serializer.file.fileName + ")." );
				}
			}
			if( this.destination == TacticalMoveToGoal.DestinationType.OBJECT )
			{
				try
				{
					this.destinationObjectGuid = serializer.ReadGuid( "DestinationObjectGuid" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'DestinationObjectGuid' (" + serializer.file.fileName + ")." );
				}
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
			serializer.WriteByte( "", "Destination", (byte)this.destination );

			if( this.destination == TacticalMoveToGoal.DestinationType.POSITION )
			{
				serializer.WriteVector3( "", "DestinationPosition", this.destinationPosition.Value );
			}
			if( this.destination == TacticalMoveToGoal.DestinationType.OBJECT )
			{
				serializer.WriteGuid( "", "DestinationObjectGuid", this.destinationObjectGuid.Value );
			}

			serializer.WriteBool( "", "IsHostile", this.isHostile );
		}
	}
}