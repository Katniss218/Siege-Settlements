using System;
using KFF;

namespace SS.AI.Goals
{
	public class TacticalPickUpGoalData : TacticalGoalData
	{
		public string resourceId { get; set; }

		public Guid? destinationObjectGuid { get; set; }

		public TacticalPickUpGoal.GoalHostileMode hostileMode { get; set; }


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
				this.destinationObjectGuid = Guid.ParseExact( serializer.ReadString( "DestinationObjectGuid" ), "D" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DestinationObjectGuid' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.hostileMode = (TacticalPickUpGoal.GoalHostileMode)serializer.ReadByte( "HostileMode" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'HostileMode' (" + serializer.file.fileName + ")." );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "ResourceId", this.resourceId );

			serializer.WriteString( "", "DestinationObjectGuid", this.destinationObjectGuid.Value.ToString( "D" ) );

			serializer.WriteByte( "", "HostileMode", (byte)this.hostileMode );
		}
	}
}