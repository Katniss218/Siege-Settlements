using SS.Objects;
using SS.Objects.Modules;
using UnityEngine;

namespace SS.AI.Goals
{
	public class TacticalIdleGoal : TacticalGoal
	{
		public const string KFF_TYPEID = "idle";

		public bool isHostile { get; set; }



		private IAttackModule[] attackModules;

		public TacticalIdleGoal()
		{
			this.isHostile = true;
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override bool IsOnValidObject( SSObject ssObject )
		{
			return true;
		}

		public override void Start( TacticalGoalController controller )
		{
			this.attackModules = controller.GetComponents<IAttackModule>();
		}

		public override void Update( TacticalGoalController controller )
		{
			this.UpdateTargeting( controller, this.isHostile, this.attackModules );
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override TacticalGoalData GetData()
		{
			return new TacticalIdleGoalData()
			{
				isHostile = this.isHostile
			};
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalIdleGoalData data = (TacticalIdleGoalData)_data;

			this.isHostile = data.isHostile;
		}
	}
}