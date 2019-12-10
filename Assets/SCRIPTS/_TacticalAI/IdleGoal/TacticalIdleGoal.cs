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
		
		public override void Start( TacticalGoalController controller )
		{
			this.attackModules = controller.GetComponents<IAttackModule>();
		}

		public override void Update( TacticalGoalController controller )
		{			
			if( this.isHostile )
			{
#warning TODO! - optimise by having only 1 overlapsphere (the largest radius, and filtering for smaller targeters).

				// Targeter.SetTargets( IAttackModule[] array, ... other );
				// ???

				IFactionMember fac = (IFactionMember)controller.ssObject;
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( !Targeter.CanTarget( controller.transform.position, this.attackModules[i].targeter.searchRange, this.attackModules[i].targeter.target, fac.factionMember ) )
					{
						this.attackModules[i].targeter.target = null;
					}
				}

				if( Random.Range( 0, 5 ) == 0 ) // Recalculate target only 20% of the time (not really noticeable, but gives a nice boost to FPS).
				{
					for( int i = 0; i < this.attackModules.Length; i++ )
					{
						if( this.attackModules[i].isReadyToAttack )
						{
							this.attackModules[i].targeter.TrySetTarget( controller.transform.position, Targeter.TargetingMode.CLOSEST );
						}
					}
				}
			}
			else
			{
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( this.attackModules[i].targeter.target != null )
					{
						this.attackModules[i].targeter.target = null;
					}
				}
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