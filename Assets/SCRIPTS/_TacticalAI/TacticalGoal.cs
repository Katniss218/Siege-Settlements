using SS.Objects;
using SS.Objects.Modules;
using UnityEngine;

namespace SS.AI.Goals
{
	public abstract class TacticalGoal
	{
		public abstract bool IsOnValidObject( SSObject obj );

		public abstract void Start( TacticalGoalController controller );

		// Update is allowed to change the current state, but not allowed to change the current goal itself.
		public abstract void Update( TacticalGoalController controller );

		protected virtual void UpdateTargeting( TacticalGoalController controller, bool isHostile, IAttackModule[] attackModules )
		{
			if( isHostile )
			{
				SSObjectDFS ssobj = controller.GetComponent<SSObjectDFS>();
				for( int i = 0; i < attackModules.Length; i++ )
				{
					if( !Targeter.CanTarget( controller.transform.position, attackModules[i].attackRange, attackModules[i].target, ssobj ) )
					{
						attackModules[i].target = null;
					}
				}

				if( Random.Range( 0, 5 ) == 0 ) // Recalculate target only 20% of the time (not really noticeable, but gives a nice boost to FPS).
				{
					for( int i = 0; i < attackModules.Length; i++ )
					{
						if( attackModules[i].isReadyToAttack )
						{
							attackModules[i].FindTargetClosest();
						}
					}
				}
			}
			else
			{
				for( int i = 0; i < attackModules.Length; i++ )
				{
					if( attackModules[i].target != null )
					{
						attackModules[i].target = null;
					}
				}
			}
		}

		public abstract TacticalGoalData GetData();
		public abstract void SetData( TacticalGoalData _data );
	}
}