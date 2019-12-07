using SS.Objects;
using SS.Objects.Modules;
using UnityEngine;
using UnityEngine.AI;

namespace SS.AI.Goals
{
	public class TacticalMoveToGoal : TacticalGoal
	{
		public enum DestinationType : byte
		{
			POSITION,
			OBJECT
		}

		public enum GoalHostileMode : byte
		{
			ALL,
			DESTINATION,
			NONE
		}

		public DestinationType destination { get; private set; }
		public Vector3? destinationPos { get; private set; }
		public SSObject destinationObject { get; private set; }

		public GoalHostileMode hostileMode { get; set; }

		private Vector3 oldDestination;
		private NavMeshAgent navMeshAgent;
		private IAttackModule[] attackModules;

		public TacticalMoveToGoal()
		{
			this.hostileMode = GoalHostileMode.ALL;
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-

		
		public void SetDestination( Vector3 destination )
		{
			this.destination = DestinationType.POSITION;
			this.destinationPos = destination;
			this.destinationObject = null;
		}

		public void SetDestination( SSObject destination )
		{
#warning TODO! - disable assigning itself as the destination.
			this.destination = DestinationType.OBJECT;
			this.destinationPos = null;
			this.destinationObject = destination;
			this.oldDestination = this.destinationObject.transform.position;
		}


		public override void Start( TacticalGoalController controller )
		{
			this.navMeshAgent = (controller.ssObject as INavMeshAgent).navMeshAgent;
			this.attackModules = controller.GetComponents<IAttackModule>();
		}

		private void UpdatePosition( TacticalGoalController controller )
		{
			if( this.destination == DestinationType.POSITION )
			{
				Vector3 currDestPos = this.destinationPos.Value;
				if( this.oldDestination != currDestPos )
				{
					this.navMeshAgent.SetDestination( currDestPos );
				}

				// If the agent has travelled to the destination - switch back to the Idle Goal.
				if( Vector3.Distance( currDestPos, controller.transform.position ) <= this.navMeshAgent.stoppingDistance )
				{
					this.navMeshAgent.ResetPath();
					controller.goal = TacticalGoalController.GetDefaultGoal();
				}

				this.oldDestination = currDestPos;

				return;
			}
			if( this.destination == DestinationType.OBJECT )
			{
				Vector3 currDestPos = this.destinationObject.transform.position;
				if( this.oldDestination != currDestPos )
				{
					this.navMeshAgent.SetDestination( currDestPos );
				}

				this.oldDestination = currDestPos;

				return;
			}
		}

		private void UpdateTargeting( TacticalGoalController controller )
		{
			if( hostileMode == GoalHostileMode.NONE )
			{
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( this.attackModules[i].targeter.target != null )
					{
						this.attackModules[i].targeter.target = null;
					}
				}
				return;
			}

			if( hostileMode == GoalHostileMode.DESTINATION )
			{
				if( this.destination != DestinationType.OBJECT )
				{
#warning TODO! - prevent setting the hostile to DESTINATION if the destination is a position.
					return;
				}
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

				if( Random.Range( 0, 5 ) == 0 ) // Recalculate target only 20% of the time (not really noticeable, but gives a nice boost to FPS).
				{
					for( int i = 0; i < this.attackModules.Length; i++ )
					{
						if( this.attackModules[i].isReadyToAttack )
						{
#warning Need to prevent targeting other objects.
							this.attackModules[i].targeter.TrySetTarget( controller.transform.position, (destinationObject as IDamageable).damageable );
						}
					}
				}

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

				if( Random.Range( 0, 5 ) == 0 ) // Recalculate target only 20% of the time (not really noticeable, but gives a nice boost to FPS).
				{
					for( int i = 0; i < this.attackModules.Length; i++ )
					{
						if( this.attackModules[i].isReadyToAttack )
						{
							this.attackModules[i].targeter.TrySetTarget( controller.transform.position );
						}
					}
				}

				return;
			}
		}

		public override void Update( TacticalGoalController controller )
		{
			// If the object was picked up/destroyed/etc. (is no longer on the map), stop the Goal.
			if( this.destination == DestinationType.OBJECT )
			{
				if( this.destinationObject == null )
				{
					this.navMeshAgent.ResetPath();
					controller.goal = TacticalGoalController.GetDefaultGoal();
					return;
				}
			}
			this.UpdatePosition( controller );
			this.UpdateTargeting( controller );
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override TacticalGoalData GetData()
		{
			return new TacticalMoveToGoalData()
			{
				destination = this.destination,
				destinationObjectGuid = this.destinationObject.guid,
				destinationPosition = this.destinationPos,
				hostileMode = this.hostileMode
			};
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalMoveToGoalData data = (TacticalMoveToGoalData)_data;

			this.destination = data.destination;

			if( this.destination == DestinationType.OBJECT )
			{
				this.destinationObject = Main.GetSSObject( data.destinationObjectGuid.Value );
			}
			else if( this.destination == DestinationType.POSITION )
			{
				this.destinationPos = data.destinationPosition;
			}

			this.hostileMode = data.hostileMode;
		}
	}
}