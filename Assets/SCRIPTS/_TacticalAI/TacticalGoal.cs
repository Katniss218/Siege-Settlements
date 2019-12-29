using SS.Objects;

namespace SS.AI.Goals
{
	public abstract class TacticalGoal
	{
		public abstract bool IsOnValidObject( SSObject obj );

		public abstract void Start( TacticalGoalController controller );

		// Update is allowed to change the current state, but not allowed to change the current goal itself.
		public abstract void Update( TacticalGoalController controller );
		
		public abstract TacticalGoalData GetData();
		public abstract void SetData( TacticalGoalData _data );
	}
}