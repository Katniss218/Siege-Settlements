using SS.AI.Goals;
using SS.Objects;
using System;
using UnityEngine;

namespace SS.AI
{
	[DisallowMultipleComponent]
	[RequireComponent( typeof( SSObject ) )]
	[RequireComponent( typeof( IFactionMember ) )]
	public class TacticalGoalController : MonoBehaviour
	{
		private TacticalGoal[] goals;
		private int goalCounter;
		public int goalTag { get; private set; }

		public TacticalGoal currentGoal { get; private set; } = null;
		public TacticalGoalExitCondition lastGoalExitCondition { get; private set; }

		private void AdvanceAndStart()
		{
			this.goalCounter++;

			this.currentGoal = this.goals[this.goalCounter];
			this.currentGoal.Start( this );
		}

		/// <summary>
		/// Sets goals from save data.
		/// </summary>
		public void SetGoalData( TacticalGoalData[] data, int tag )
		{
			TacticalGoal[] goals = new TacticalGoal[data.Length];

			for( int i = 0; i < data.Length; i++ )
			{
				goals[i] = data[i].GetGoal();
			}

			this.SetGoals( tag, goals );
		}

		/// <summary>
		/// Returns the save data of all goals currently running.
		/// </summary>
		public TacticalGoalData[] GetGoalData()
		{
			TacticalGoalData[] data = new TacticalGoalData[this.goals.Length];

			for( int i = 0; i < this.goals.Length; i++ )
			{
				data[i] = this.goals[i].GetData();
			}

			return data;
		}

		
		public void SetGoals( int goalTag, params TacticalGoal[] goals )
		{
			for( int i = 0; i < goals.Length; i++ )
			{
				if( goals[i] == null )
				{
					throw new Exception( "Tactical Goal 'null' was added to an object." );
				}
				if( !goals[i].CanBeAddedTo( this.ssObject ) )
				{
					throw new Exception( "Tactical Goal '" + goals[i].GetType().Name + "' was added to a '" + this.ssObject.definitionId + "' object." );
				}
			}

			this.goals = goals;
			this.goalTag = goalTag;
			this.goalCounter = -1; // advance&start will increment to 0.

			this.AdvanceAndStart();
		}

		private SSObject __ssObject = null;
		/// <summary>
		/// Returns the ss object that this goal controller is associated with.
		/// </summary>
		public SSObject ssObject
		{
			get
			{
				if( this.__ssObject == null )
				{
					this.__ssObject = this.GetComponent<SSObject>() ?? throw new Exception( this.gameObject.name + ": This TacticalGoalController was added to a non-SSObject." );
				}
				return this.__ssObject;
			}
		}

		public const int DEFAULT_GOAL_TAG = -1;

		/// <summary>
		/// Returns a default goal (idle).
		/// </summary>
		public static TacticalIdleGoal GetDefaultGoal()
		{
			return new TacticalIdleGoal()
			{
				isHostile = true
			};
		}

		/// <summary>
		/// makes the controller move to the next goal or reset to default (if failure). You're not allowed to call this method from Start().
		/// </summary>
		public void ExitCurrent( TacticalGoalExitCondition exitCondition )
		{
			if( this.goalCounter >= this.goals.Length - 1 )
			{
				this.SetGoals( DEFAULT_GOAL_TAG, TacticalGoalController.GetDefaultGoal() );
			}
			else
			{
				if( exitCondition == TacticalGoalExitCondition.SUCCESS )
				{
					this.AdvanceAndStart();
				}
				else
				{
					this.SetGoals( DEFAULT_GOAL_TAG, TacticalGoalController.GetDefaultGoal() );
				}
			}
			this.lastGoalExitCondition = exitCondition;
		}

		void Start()
		{
			if( this.currentGoal == null )
			{
				this.SetGoals( DEFAULT_GOAL_TAG, TacticalGoalController.GetDefaultGoal() );
			}
		}

		void Update()
		{
			this.currentGoal.Update( this );
		}
	}
}