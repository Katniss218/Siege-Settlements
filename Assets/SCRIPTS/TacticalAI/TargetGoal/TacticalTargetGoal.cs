using Katniss.Utils;
using SS.Objects;
using SS.Objects.Modules;
using SS.Objects.Units;
using UnityEngine;
using UnityEngine.AI;

namespace SS.AI.Goals
{
	public class TacticalTargetGoal : TacticalGoal
	{
		public const string KFF_TYPEID = "target";

		// the target goal will stop the object from pursuing when it getsd that close (based on the attack module with the shortest range, to make surte that every module can attack).
		public const float STOPPING_FRACTION = 0.75f;
		public const float MOVING_FACTION = 0.85f;
		
		/// <summary>
		/// The object that the goal is going to try and attack.
		/// </summary>
		public SSObjectDFSC target { get; set; }

		/// <summary>
		/// If set to true, the goal won't check if the target can be targeted (e.g. outside range, wrong faction, etc.). Useful when you want to attack objects outside of the view range.
		/// </summary>
		public bool targetForced { get; set; }


		private Vector3 initialPosition;
		private Vector3 oldDestination;
		private NavMeshAgent navMeshAgent;
		private IAttackModule[] attackModules;

		public TacticalTargetGoal()
		{

		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override bool CanBeAddedTo( SSObject ssObject )
		{
			return ssObject.GetComponents<IAttackModule>().Length > 0;
		}

		public override void Start( TacticalGoalController controller )
		{
			this.navMeshAgent = (controller.ssObject as IMovable)?.navMeshAgent;
			this.attackModules = controller.GetComponents<IAttackModule>();
			this.initialPosition = controller.transform.position;
		}

		private void UpdatePosition( TacticalGoalController controller )
		{
			// IF is in range - Stop, attack.
			// ELSE			  - Move towards the target (if there is a target).
			if( this.navMeshAgent != null )
			{
				bool isInside = false;
				if( controller.ssObject is Unit )
				{
					if( ((Unit)controller.ssObject).isInside )
					{
						isInside = true;
					}
				}

				float shortestAttackRange = float.MaxValue;
				if( this.attackModules.Length == 0 )
				{
					shortestAttackRange = 0.0f;
				}
				else
				{
					for( int i = 0; i < this.attackModules.Length; i++ )
					{
						if( shortestAttackRange > this.attackModules[i].attackRange )
						{
							shortestAttackRange = this.attackModules[i].attackRange;
						}
					}
				}

				if( !isInside )
				{
					if( this.target == null )
					{
						if( DistanceUtils.IsInRange( controller.transform.position, this.initialPosition, Main.DEFAULT_NAVMESH_STOPPING_DIST_CUSTOM ) )
						{
							this.navMeshAgent.ResetPath();
						}
						else
						{
							Vector3 currDestPos = this.initialPosition;

							if( this.oldDestination != currDestPos )
							{
								this.navMeshAgent.SetDestination( currDestPos + ((controller.transform.position - currDestPos).normalized * 0.025f) );
							}

							this.oldDestination = currDestPos;
						}

						return;
					}
					if( DistanceUtils.IsInRange( controller.transform, this.target.transform, (shortestAttackRange * STOPPING_FRACTION) ) )
					{
						this.navMeshAgent.ResetPath();
					}
					else if( !DistanceUtils.IsInRange( controller.transform, this.target.transform, (shortestAttackRange * MOVING_FACTION) ) )
					{
						Vector3 currDestPos = this.target.transform.position;

						if( this.oldDestination != currDestPos )
						{
							this.navMeshAgent.SetDestination( currDestPos + ((controller.transform.position - currDestPos).normalized * 0.025f) );
						}

						this.oldDestination = currDestPos;
					}
				}
			}
		}

		private void UpdateTargeting( TacticalGoalController controller )
		{
			SSObjectDFSC ssobj = (SSObjectDFSC)controller.ssObject;
			
			// If the target isn't forced - check if it still can be targeted - if it can't be targeted by every targeter - reset the target.
			if( !this.targetForced )
			{
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( this.attackModules[i].target == null )
					{
						continue;
					}
					if( !Targeter.CanTarget( controller.transform.position, this.attackModules[i].attackRange, this.attackModules[i].target, ssobj ) )
					{
						this.attackModules[i].target = null;
					}
				}

				// If the current target is outside of the global view range, or can't be targeted - try and find a new target.
				if( !Targeter.CanTarget( controller.transform.position, ssobj.viewRange, this.target, ssobj ) )
				{
					this.target = null;
				}
			}
			else
			{
				// If the target can no longer be targeted.
				if( this.target != null )
				{
					if( !ssobj.CanTargetAnother( this.target ) )
					{
						this.target = null;
					}
				}

				// If the target can no longer be targeted.
				if( this.target == null )
				{
					if( controller.ssObject is IInteriorUser )
					{
						if( !((IInteriorUser)controller.ssObject).isInside )
						{
							this.navMeshAgent.ResetPath();
						}
					}
					controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
					return;
				}
			}

			if( this.target == null )
			{
				// If the target was destroyed or could no longer be targeted - find a new target.
				if( Random.Range( 0, 5 ) == 0 ) // Recalculate target only 20% of the time (not really noticeable, but gives a nice boost to FPS).
				{
					this.target = Targeter.FindTargetClosest( controller.transform.position, ssobj.viewRange, ssobj, true );
				}
			}
		
			// Set the target of each targeter module to the goal's target.
			for( int i = 0; i < this.attackModules.Length; i++ )
			{
				if( this.attackModules[i].isReadyToAttack )
				{
					this.attackModules[i].TrySetTarget( this.target );
				}
			}
		}

		public override void Update( TacticalGoalController controller )
		{
			// If it's not usable - return, don't attack.
			if( controller.ssObject is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)controller.ssObject).isUsable )
			{
				return;
			}

			this.UpdatePosition( controller );
			this.UpdateTargeting( controller );
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override TacticalGoalData GetData()
		{
			TacticalTargetGoalData data = new TacticalTargetGoalData();

			if( this.target == null )
			{
				data.targetGuid = null;
			}
			else
			{
				data.targetGuid = this.target.GetComponent<SSObject>().guid;
			}

			return data;
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalTargetGoalData data = (TacticalTargetGoalData)_data;

			if( data.targetGuid == null )
			{
				this.target = null;
			}
			else
			{
				this.target = SSObject.Find( data.targetGuid.Value ) as SSObjectDFSC;
			}
		}
	}
}