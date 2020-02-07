using KFF;
using SS.Objects.Modules;
using System;
using UnityEngine;

namespace SS.AI.Goals
{
	public class TacticalMoveToGoalData : TacticalGoalData
	{
		public TacticalMoveToGoal.DestinationType destinationType { get; set; }
		public Vector3 destinationPosition { get; set; }
		public Guid destinationObjectGuid { get; set; }
		public Guid interiorModuleId { get; set; }
		public InteriorModule.SlotType interiorSlotType { get; set; }

		public bool isHostile { get; set; }


		public override TacticalGoal GetGoal()
		{
			TacticalMoveToGoal goal = new TacticalMoveToGoal();
			goal.SetData( this );
			return goal;
		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			//try
			//{
				this.destinationType = (TacticalMoveToGoal.DestinationType)serializer.ReadByte( "Destination" );
			//}
			//catch
			//{
			//	throw new Exception( "Missing or invalid value of 'Destination' (" + serializer.file.fileName + ")." );
			//}

			if( this.destinationType == TacticalMoveToGoal.DestinationType.POSITION )
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
			else if( this.destinationType == TacticalMoveToGoal.DestinationType.OBJECT )
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
			else if( this.destinationType == TacticalMoveToGoal.DestinationType.INTERIOR )
			{
				try
				{
					this.destinationObjectGuid = serializer.ReadGuid( "DestinationObjectGuid" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'DestinationObjectGuid' (" + serializer.file.fileName + ")." );
				}

				try
				{
					this.interiorModuleId = serializer.ReadGuid( "InteriorModuleId" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'InteriorModuleId' (" + serializer.file.fileName + ")." );
				}

				try
				{
					this.interiorSlotType = (InteriorModule.SlotType)serializer.ReadByte( "InteriorSlotType" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'InteriorSlotType' (" + serializer.file.fileName + ")." );
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
			serializer.WriteByte( "", "Destination", (byte)this.destinationType );

			if( this.destinationType == TacticalMoveToGoal.DestinationType.POSITION )
			{
				serializer.WriteVector3( "", "DestinationPosition", this.destinationPosition );
			}
			else if( this.destinationType == TacticalMoveToGoal.DestinationType.OBJECT )
			{
				serializer.WriteGuid( "", "DestinationObjectGuid", this.destinationObjectGuid );
			}
			else if( this.destinationType == TacticalMoveToGoal.DestinationType.INTERIOR )
			{
				serializer.WriteGuid( "", "DestinationObjectGuid", this.destinationObjectGuid );
				serializer.WriteGuid( "", "InteriorModuleId", this.interiorModuleId );
				serializer.WriteByte( "", "InteriorSlotType", (byte)this.interiorSlotType );
			}

			serializer.WriteBool( "", "IsHostile", this.isHostile );
		}
	}
}