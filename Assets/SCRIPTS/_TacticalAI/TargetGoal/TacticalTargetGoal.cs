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

#warning SSObjectSelectable, Damageable, FactionMember, and ViewRange are basically the same. They always coexist on objects (U|B|H).
				

#warning TODO! - proper per-DFVobject view range.
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
						this.navMeshAgent.SetDestination( currDestPos );
					}

					this.oldDestination = currDestPos;
				}
			}
		}

		private void UpdateTargeting( TacticalGoalController controller )
		{
			bool allCantTarget = true;
			IFactionMember fac = controller.GetComponent<IFactionMember>();

			// If the target isn't forced - check if it still can be targeted - if it can't be targeted by every targeter - reset the target.
			if( !this.targetForced )
			{
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( !Targeter.CanTarget( controller.transform.position, this.attackModules[i].targeter.searchRange, this.attackModules[i].targeter.target, fac.factionMember ) )
					{
						this.attackModules[i].targeter.target = null;
					}
					else
					{
						allCantTarget = false;
					}
				}
				
				if( allCantTarget )
				{
					this.target = null;
				}
			}

			// If the target was destroyed or can no longer be targeted - find a new target.
			if( this.target == null )
			{
				this.target = Targeter.FindTargetArbitrary( controller.transform.position, this.attackModules[0].searchRange, this.attackModules[0].targeter.layers, fac.factionMember );
			}
			
			// Set the target of each targeter module to the goal's target.
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