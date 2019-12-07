using KFF;
using System;
using UnityEngine;

namespace SS.AI.Goals
{
	public class TacticalDropOffGoalData : TacticalGoalData
	{
		public string resourceId { get; set; }
		public TacticalDropOffGoal.DestinationType destination { get; set; }

		public Vector3? destinationPosition { get; set; }
		public Guid? destinationObjectGuid { get; set; }

		public bool isHostile { get; set; }

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.resourceId = serializer.ReadString( "ResourceId" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'ResourceId' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.destination = (TacticalDropOffGoal.DestinationType)serializer.ReadByte( "Destination" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Destination' (" + serializer.file.fileName + ")." );
			}

			if( this.destination == TacticalDropOffGoal.DestinationType.POSITION )
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
			if( this.destination == TacticalDropOffGoal.DestinationType.OBJECT )
			{
				try
				{
					this.destinationObjectGuid = Guid.ParseExact( serializer.ReadString( "DestinationObjectGuid" ), "D" );
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
			serializer.WriteString( "", "ResourceId", this.resourceId );

			serializer.WriteByte( "", "Destination", (byte)this.destination );

			if( this.destination == TacticalDropOffGoal.DestinationType.POSITION )
			{
				serializer.WriteVector3( "", "DestinationPosition", this.destinationPosition.Value );
			}
			if( this.destination == TacticalDropOffGoal.DestinationType.OBJECT )
			{
#warning Move this to WriteGuid / ReadGuid.
				serializer.WriteString( "", "DestinationObjectGuid", this.destinationObjectGuid.Value.ToString( "D" ) );
			}

			serializer.WriteBool( "", "IsHostile", this.isHostile );
		}
	}
}