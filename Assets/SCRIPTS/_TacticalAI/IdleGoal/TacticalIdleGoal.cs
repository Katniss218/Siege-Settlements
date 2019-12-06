using SS.Objects;
using SS.Objects.Modules;

namespace SS.AI.Goals
{
	public class TacticalIdleGoal : TacticalGoal
	{
		public enum GoalHostileMode : byte
		{
			ALL,
			NONE
		}

		public GoalHostileMode hostileMode { get; set; }

		

		private IAttackModule[] attackModules;

		public TacticalIdleGoal()
		{
			this.hostileMode = GoalHostileMode.ALL;
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override void Start( TacticalGoalController controller )
		{
			this.attackModules = controller.GetComponents<IAttackModule>();
		}

		public override void Update( TacticalGoalController controller )
		{
			if( hostileMode == GoalHostileMode.NONE )
			{
#warning TODO! - needs to stop targeting whatever it was targeting (if applicable).
				return;
			}

			if( hostileMode == GoalHostileMode.ALL )
			{
				// If it's not usable - return, don't attack.
				if( controller.ssObject is IUsableToggle && !(controller.ssObject as IUsableToggle).IsUsable() )
				{
					return;
				}

				IFactionMember fac = controller.GetComponent<IFactionMember>();
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( !Targeter.CanTarget( fac.factionMember, this.attackModules[i].targeter.target, controller.transform.position, this.attackModules[i].targeter.searchRange ) )
					{
						this.attackModules[i].targeter.target = null;
					}
				}
								
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( this.attackModules[i].isReadyToAttack )
					{
						this.attackModules[i].targeter.TrySetTarget( controller.transform.position );
					}
				}
				

				return;
			}
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-

		
		public override TacticalGoalData GetData()
		{
			return new TacticalIdleGoalData()
			{
				hostileMode = this.hostileMode
			};
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalIdleGoalData data = (TacticalIdleGoalData)_data;

			this.hostileMode = data.hostileMode;
		}
	}
}