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


		public override bool CanBeAddedTo( SSObject ssObject )
		{
			return true;
		}

		public override void Start( TacticalGoalController controller )
		{
			this.attackModules = controller.GetComponents<IAttackModule>();
		}

		public override void Update( TacticalGoalController controller )
		{
			if( attackModules.Length > 0 )
			{
				if( controller.ssObject is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)controller.ssObject).IsUsable() )
				{
					// don't exit, since exiting would make it return to this specific goal type anyway.
					return;
				}

				this.UpdateTargeting( controller, this.isHostile, this.attackModules );
			}
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