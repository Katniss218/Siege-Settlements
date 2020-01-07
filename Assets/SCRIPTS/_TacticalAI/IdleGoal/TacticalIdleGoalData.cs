using System;
using KFF;

namespace SS.AI.Goals
{
	public class TacticalIdleGoalData : TacticalGoalData
	{
		public bool isHostile { get; set; }


		public override TacticalGoal GetGoal()
		{
			TacticalIdleGoal goal = new TacticalIdleGoal();
			goal.SetData( this );
			return goal;
		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
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
			serializer.WriteBool( "", "IsHostile", this.isHostile );
		}
	}
}