using SS.Objects;
using SS.Objects.Modules;
using UnityEngine;
using UnityEngine.AI;

namespace SS.AI.Goals
{
	public class TacticalTargetGoal : TacticalGoal
	{
		public const float STOPPING_FRACTION = 0.6666f;
		public const float MOVING_FACTION = 0.9f;

		public Targeter.TargetingMode targetingMode { get; set; }
		public Damageable target { get; set; }


		private NavMeshAgent navMeshAgent;
		private IAttackModule[] attackModules;

		public TacticalTargetGoal()
		{
			this.targetingMode = Targeter.TargetingMode.ARBITRARY;
		}


		public override void Start( TacticalGoalController controller )
		{
			this.navMeshAgent = (controller.ssObject as INavMeshAgent)?.navMeshAgent;
			this.attackModules = controller.GetComponents<IAttackModule>();
		}

		private void UpdatePosition( TacticalGoalController controller )
		{
			// IF is in range - Stop, attack.
			// ELSE			  - Move towards the target (if there is a target).
			if( this.navMeshAgent != null )
			{
				if( this.target == null )
				{
					return;
				}

#warning TODO! - proper search range.
				if( Vector3.Distance( controller.transform.position, this.target.transform.position ) <= this.attackModules[0].searchRange * STOPPING_FRACTION )
				{

					this.navMeshAgent.ResetPath();
				}

				else if( Vector3.Distance( controller.transform.position, this.target.transform.position ) >= this.attackModules[0].searchRange * MOVING_FACTION )
				{
					if( this.navMeshAgent.hasPath )
					{
						return;
					}

					this.navMeshAgent.SetDestination( this.target.transform.position );
				}
			}
		}

		private void UpdateTargeting( TacticalGoalController controller )
		{
			IFactionMember fac = controller.GetComponent<IFactionMember>();
			for( int i = 0; i < this.attackModules.Length; i++ )
			{
				if( !Targeter.CanTarget( fac.factionMember, this.attackModules[i].targeter.target, controller.transform.position, this.attackModules[i].targeter.searchRange ) )
				{
					this.attackModules[i].targeter.target = null;
				}
			}

			// If the target was destroyed - switch to guarding the area.
#warning TODO! - maybe change this to idle goal or something instead.
			if( this.target == null )
			{
				if( this.targetingMode == Targeter.TargetingMode.TARGET )
				{
					this.targetingMode = Targeter.TargetingMode.CLOSEST;
				}
			}
#warning TODO! - set the goal's target to whatever obj is attacked. Making the goal chase the enemy.

			// if the target is overriden by the user, it starts to attack that unit, and chase it.
			// if the target is null (not overriden or dead) find a new target, and start chasing the new target.


			//if( Random.Range( 0, 5 ) == 0 ) // Recalculate target only 20% of the time (not really noticeable, but gives a nice boost to FPS).
			//{
			for( int i = 0; i < this.attackModules.Length; i++ )
			{
				if( this.attackModules[i].isReadyToAttack )
				{
					this.attackModules[i].targeter.TrySetTarget( controller.transform.position, this.targetingMode, this.target );
				}
			}
			//}
		}

		public override void Update( TacticalGoalController controller )
		{
			// If it's not usable - return, don't attack.
			if( controller.ssObject is IUsableToggle && !(controller.ssObject as IUsableToggle).IsUsable() )
			{
				return;
			}

			this.UpdatePosition( controller );
			this.UpdateTargeting( controller );
		}


		public override TacticalGoalData GetData()
		{
			return new TacticalTargetGoalData()
			{
				targetingMode = this.targetingMode,
				targetGuid = this.target.GetComponent<SSObject>().guid
			};
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalTargetGoalData data = (TacticalTargetGoalData)_data;

			this.targetingMode = data.targetingMode;
			this.target = Main.GetSSObject( data.targetGuid.Value ).GetComponent<Damageable>();
		}
	}
}