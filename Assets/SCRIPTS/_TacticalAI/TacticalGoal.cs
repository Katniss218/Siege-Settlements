using SS.Objects;
using SS.Objects.Modules;
using UnityEngine;

namespace SS.AI.Goals
{
	public abstract class TacticalGoal
	{
		// Tactical goals are bound to a specific object through the TacticalGoalController.

		// They only control a single object - the one they're bound to.
		// They don't coordinate their actions with other objects.

		/// <summary>
		/// Use this to check if the tactical goal can be added to this specific object. Can be used to restrict.
		/// </summary>
		public abstract bool CanBeAddedTo( SSObject obj );

		// Start is mostly used to initialize the goal's cached variables.
		public abstract void Start( TacticalGoalController controller );
		
		//Update is used to actually control the object.
		public abstract void Update( TacticalGoalController controller );

		/// <summary>
		/// Updates the targeting of a specified group of attack modules.
		/// </summary>
		/// <param name="controller">The controller passed through 'Start' or 'Update' method.</param>
		/// <param name="isHostile">Should the attacking modules attack or not.</param>
		/// <param name="attackModules">The array of attack modules to affect.</param>
		protected virtual void UpdateTargeting( TacticalGoalController controller, bool isHostile, IAttackModule[] attackModules )
		{
			if( isHostile )
			{
				SSObjectDFS ssobj = (SSObjectDFS)controller.ssObject;

				for( int i = 0; i < attackModules.Length; i++ )
				{
					if( attackModules[i].target == null )
					{
						continue;
					}
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

#warning I need the ability to create compound tactical goals. Goals have an exit condition, and can be chained together. They also can be forced to exit prematurely.
		// dropOff, PickUp don't move. that's accomplished by chaining. They have an entry cooldown (take 0.5s just sitting around before picking up and exiting - doesn't pick up if too far away).
		// if there's no goal leftover, it can be notified via reset to default goal.
		
		// resource collector:
		// inventory can hold more   :: -> moveTo(deposit) -> pickUp -> moveTo(storage) -> dropOff -> ::
		// inventory can't hold more :: -> moveTo(storage) -> dropOff -> ::

		// moveTo triggers it's exit when it's at it's destination.
		// moveTo can be flagged to stop triggering (useful for e.g. following an enemy).
		// moveTo can have custom stopping distance.

		// move to, drop off, pick up, etc. all are forced to exit by killing the destination.

		

		public abstract TacticalGoalData GetData();

		public abstract void SetData( TacticalGoalData _data );
	}
}