using System;
using KFF;

namespace SS.AI.Goals
{
	public class TacticalIdleGoalData : TacticalGoalData
	{
		public TacticalIdleGoal.GoalHostileMode hostileMode { get; set; }


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.hostileMode = (TacticalIdleGoal.GoalHostileMode)serializer.ReadByte( "HostileMode" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'HostileMode' (" + serializer.file.fileName + ")." );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteByte( "", "HostileMode", (byte)this.hostileMode );
		}
	}
}