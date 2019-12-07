using SS.Objects.Modules;
using UnityEngine;

namespace SS.AI.Goals
{
	public class TacticalTargetGoal : TacticalGoal
	{
		public enum TargetingMode : byte
		{
			ARBITRARY,
			CLOSEST,
			TARGET
		}

		public TargetingMode targetingMode { get; set; }
		public Damageable target { get; set; }


		private IAttackModule[] attackModules;

		public TacticalTargetGoal()
		{
			this.targetingMode = TargetingMode.ARBITRARY;
		}


		public override void Start( TacticalGoalController controller )
		{
			this.attackModules = controller.GetComponents<IAttackModule>();
		}

		public override void Update( TacticalGoalController controller )
		{
			throw new System.NotImplementedException();
		}


		public override TacticalGoalData GetData()
		{
			throw new System.NotImplementedException();
		}

		public override void SetData( TacticalGoalData _data )
		{
			throw new System.NotImplementedException();
		}
	}
}