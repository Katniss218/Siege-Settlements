using KFF;
using System;

namespace SS.AI.Goals
{
	public class TacticalTargetGoalData : TacticalGoalData
	{
		public Guid? targetGuid { get; set; }


		public override TacticalGoal GetGoal()
		{
			TacticalTargetGoal goal = new TacticalTargetGoal();
			goal.SetData( this );
			return goal;
		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			if( serializer.Analyze( "TargetGuid" ).isSuccess )
			{
				try
				{
					this.targetGuid = serializer.ReadGuid( "TargetGuid" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'TargetGuid' (" + serializer.file.fileName + ")." );
				}
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			if( this.targetGuid != null )
			{
				serializer.WriteGuid( "", "TargetGuid", this.targetGuid.Value );
			}
		}
	}
}