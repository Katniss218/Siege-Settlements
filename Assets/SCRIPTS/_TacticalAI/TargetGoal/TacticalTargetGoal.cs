using SS.Objects;
using SS.Objects.Modules;
using UnityEngine;
using UnityEngine.AI;

namespace SS.AI.Goals
{
	public class TacticalTargetGoal : TacticalGoal
	{
		public const string KFF_TYPEID = "target";

		public const float STOPPING_FRACTION = 0.75f;
		public const float MOVING_FACTION = 0.85f;
		
		/// <summary>
		/// The object that the goal is going to try and attack.
		/// </summary>
		public Damageable target { get; set; }
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


		public override void Start( TacticalGoalController controller )
		{
			this.navMeshAgent = (controller.ssObject as INavMeshAgent)?.navMeshAgent;
			this.attackModules = controller.GetComponents<IAttackModule>();
			this.initialPosition = controller.transform.position;
		}

		private void UpdatePosition( TacticalGoalController controller )
		{
			// IF is in range - Stop, attack.
			// ELSE			  - Move towards the target (if there is a target).
			if( this.navMeshAgent != null )
			{
				if( this.target == null )
				{
					if( Vector3.Distance( controller.transform.position, this.initialPosition ) <= Main.DEFAULT_NAVMESH_STOPPING_DIST_CUSTOM  )
					{
						this.navMeshAgent.ResetPath();
					}
					else
					{
						Vector3 currDestPos = this.initialPosition;

						if( this.oldDestination != currDestPos )
						{
							this.navMeshAgent.SetDestination( currDestPos );
						}

						this.oldDestination = currDestPos;
					}
					return;
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
				
				if( Vector3.Distance( controller.transform.position, this.target.transform.position ) <= shortestAttackRange * STOPPING_FRACTION )
				{
					this.navMeshAgent.ResetPath();
				}
				else if( Vector3.Distance( controller.transform.position, this.target.transform.position ) >= shortestAttackRange * MOVING_FACTION )
				{
					Vector3 currDestPos = this.target.transform.position;

					if( this.oldDestination != currDestPos )
					{
						this.navMeshAgent.SetDestination( currDestPos );
					}

					this.oldDestination = currDestPos;
				}
			}
		}

		private void UpdateTargeting( TacticalGoalController controller )
		{
			IFactionMember fac = controller.GetComponent<IFactionMember>();

			// If the target isn't forced - check if it still can be targeted - if it can't be targeted by every targeter - reset the target.
			if( !this.targetForced )
			{
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( !Targeter.CanTarget( controller.transform.position, this.attackModules[i].attackRange, this.attackModules[i].targeter.target, fac.factionMember ) )
					{
						this.attackModules[i].targeter.target = null;
					}
				}
			}

			// If the current target is outside of the global view range, or can't be targeted - try and find a new target.
			if( !Targeter.CanTarget( controller.transform.position, fac.factionMember.viewRange, this.target, fac.factionMember ) )
			{
				this.target = null;
			}

			// If the target was destroyed or could no longer be targeted - find a new target.
			if( Random.Range( 0, 5 ) == 0 ) // Recalculate target only 20% of the time (not really noticeable, but gives a nice boost to FPS).
			{
				if( this.target == null )
				{
					this.target = Targeter.FindTargetClosest( controller.transform.position, fac.factionMember.viewRange, ObjectLayer.UNITS_MASK | ObjectLayer.BUILDINGS_MASK | ObjectLayer.HEROES_MASK, fac.factionMember );
				}
			}

			// Set the target of each targeter module to the goal's target.
			for( int i = 0; i < this.attackModules.Length; i++ )
			{
				if( this.attackModules[i].isReadyToAttack )
				{
					this.attackModules[i].targeter.TrySetTarget( controller.transform.position, this.attackModules[i].attackRange, Targeter.TargetingMode.TARGET, this.target );
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
				this.target = Main.GetSSObject( data.targetGuid.Value ).GetComponent<Damageable>();
			}
		}
	}
}