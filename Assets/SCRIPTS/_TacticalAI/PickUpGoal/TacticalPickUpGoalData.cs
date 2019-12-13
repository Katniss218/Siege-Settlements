using System;
using KFF;

namespace SS.AI.Goals
{
	public class TacticalPickUpGoalData : TacticalGoalData
	{
		public string resourceId { get; set; }

		public Guid destinationObjectGuid { get; set; }

		public bool isHostile { get; set; }


		public override TacticalGoal GetInstance()
		{
			TacticalPickUpGoal goal = new TacticalPickUpGoal();
			goal.SetData( this );
			return goal;
		}

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
				this.destinationObjectGuid = serializer.ReadGuid( "DestinationObjectGuid" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DestinationObjectGuid' (" + serializer.file.fileName + ")." );
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

			serializer.WriteGuid( "", "DestinationObjectGuid", this.destinationObjectGuid );

			serializer.WriteBool( "", "IsHostile", this.isHostile );
		}
	}
}