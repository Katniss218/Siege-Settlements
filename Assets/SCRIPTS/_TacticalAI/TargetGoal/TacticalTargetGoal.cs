using SS.Objects;
using SS.Objects.Modules;
using UnityEngine;
using UnityEngine.AI;

namespace SS.AI.Goals
{
	public class TacticalTargetGoal : TacticalGoal
	{
		public const float STOPPING_FRACTION = 0.75f;
		public const float MOVING_FACTION = 0.85f;
		
		public Damageable target { get; set; }


		private Vector3 oldDestination;
		private NavMeshAgent navMeshAgent;
		private IAttackModule[] attackModules;

		public TacticalTargetGoal()
		{

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
					this.navMeshAgent.ResetPath();
					return;
				}

#warning TODO! - proper per-object view range.
#warning TODO! - proper per-module check if it can target it.
#warning TODO! - Start moving towards the target when the object has the target set, and is outside range. Set the target when the object is in the global view range.

				if( Vector3.Distance( controller.transform.position, this.target.transform.position ) <= this.attackModules[0].searchRange * STOPPING_FRACTION )
				{
					this.navMeshAgent.ResetPath();
				}

				else if( Vector3.Distance( controller.transform.position, this.target.transform.position ) >= this.attackModules[0].searchRange * MOVING_FACTION )
				{
					Vector3 currDestPos = this.target.transform.position;

					if( this.oldDestination != currDestPos )
					{
						this.navMeshAgent.SetDestination( this.target.transform.position );
					}

					this.oldDestination = currDestPos;
				}
			}
		}

		private void UpdateTargeting( TacticalGoalController controller )
		{
			IFactionMember fac = controller.GetComponent<IFactionMember>();
			for( int i = 0; i < this.attackModules.Length; i++ )
			{
				if( !Targeter.CanTarget( controller.transform.position, this.attackModules[i].targeter.searchRange, this.attackModules[i].targeter.target, fac.factionMember ) )
				{
					this.attackModules[i].targeter.target = null;
#warning TODO! - needs to set the global target to null and stop chasing.
				}
			}

			// If the target was destroyed - find new target.
			if( this.target == null )
			{
				this.target = Targeter.FindTargetArbitrary( controller.transform.position, this.attackModules[0].searchRange, this.attackModules[0].targeter.layers, fac.factionMember );
			}

			// if the target is overriden by the user, it starts to attack that unit, and chase it.
			// if the target is null (not overriden or dead) find a new target, and start chasing the new target.

			
			for( int i = 0; i < this.attackModules.Length; i++ )
			{
				if( this.attackModules[i].isReadyToAttack )
				{
					this.attackModules[i].targeter.TrySetTarget( controller.transform.position, Targeter.TargetingMode.TARGET, this.target );
				}
			}
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
				targetGuid = this.target.GetComponent<SSObject>().guid
			};
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalTargetGoalData data = (TacticalTargetGoalData)_data;
			
			this.target = Main.GetSSObject( data.targetGuid.Value ).GetComponent<Damageable>();
		}
	}
}