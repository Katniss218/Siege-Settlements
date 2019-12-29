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
			// If the unit is in view range, but not in attack range - disregard.
			// If unit is in view range, and in attack range - attack.
			// If unit is outside view range - unset target.

			if( this.isHostile )
			{
				SSObjectDFS ssobj = (SSObjectDFS)controller.ssObject;
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( !Targeter.CanTarget( controller.transform.position, this.attackModules[i].attackRange, this.attackModules[i].targeter.target, ssobj ) )
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
							this.attackModules[i].targeter.TrySetTarget( controller.transform.position, this.attackModules[i].attackRange, Targeter.TargetingMode.CLOSEST );
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