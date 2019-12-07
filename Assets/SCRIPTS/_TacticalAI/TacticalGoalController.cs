using SS.AI.Goals;
using SS.Objects;
using System;
using UnityEngine;

namespace SS.AI
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(SSObject))]
	[RequireComponent(typeof(IFactionMember))]
	public class TacticalGoalController : MonoBehaviour
	{
		private TacticalGoal __goal { get; set; }
		public TacticalGoal goal
		{
			get
			{
				return this.__goal;
			}
			set
			{
				if( value == null )
				{
					value = GetDefaultGoal();
				}
				this.__goal = value;
				this.__goal.Start( this );
				Debug.Log( this.gameObject.name + ": Set the TacticalGoal to: " + value.GetType().Name );
			}
		}

		private SSObject __ssObject = null;
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

		public static TacticalIdleGoal GetDefaultGoal()
		{
			return new TacticalIdleGoal()
			{
				hostileMode = TacticalIdleGoal.GoalHostileMode.ALL
			};
		}
		
		void Start()
		{
			if( this.goal == null )
			{
				this.goal = GetDefaultGoal();
			}
		}
		
		void Update()
		{
			this.goal.Update( this );
		}
	}
}