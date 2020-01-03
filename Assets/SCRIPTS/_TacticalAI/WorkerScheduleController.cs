using SS.AI.Goals;
using SS.Objects;
using SS.Objects.Buildings;
using SS.Objects.Modules;
using SS.Objects.Units;
using UnityEngine;

namespace SS.AI
{
	public class WorkerScheduleController : MonoBehaviour
	{
		private Unit __civilian = null;
		public Unit civilian
		{
			get
			{
				if( this.__civilian == null )
				{
					this.__civilian = this.GetComponent<Unit>();
				}
				return this.__civilian;
			}
		}


		public bool isWorking { get; private set; }

		bool IsGoingToHome( TacticalGoalController goalController )
		{
			if( goalController.goal is TacticalMoveToGoal )
			{
				TacticalMoveToGoal moveToGoal = (TacticalMoveToGoal)goalController.goal;
				if( moveToGoal.destinationInterior != null && moveToGoal.destinationInterior != this.civilian.workplace.interior )
				{
					return true;
				}
			}
			return false;
		}

		bool IsGoingToWorkplace( TacticalGoalController goalController )
		{
			if( goalController.goal is TacticalMoveToGoal && ((TacticalMoveToGoal)goalController.goal).destinationInterior == this.civilian.workplace.interior )
			{
				return true;
			}
			return false;
		}

		private InteriorModule GetClosestInterior()
		{
			Building[] b = SSObject.GetAllBuildings();

			InteriorModule interior = null;
			float dst = float.MaxValue;
			for( int i = 0; i < b.Length; i++ )
			{
				if( b[i].factionId != this.civilian.factionId )
				{
					continue;
				}

				InteriorModule[] interiors = b[i].GetModules<InteriorModule>();
				if( interiors.Length == 0 )
				{
					continue;
				}

				if( interiors[0].GetFirstValid( InteriorModule.SlotType.Generic, this.civilian ) == null )
				{
					continue;
				}

				float newDst = Vector3.Distance( this.transform.position, b[i].transform.position );
				if( newDst >= dst )
				{
					continue;
				}

				interior = interiors[0];
			}
			return interior;
		}


		void Update()
		{
			if( this.civilian.workplace == null )
			{
				return;
			}

			if( !DaylightCycleController.instance.IsWorkTime() )
			{
				TacticalGoalController goalController = this.GetComponent<TacticalGoalController>();
				// Unit tries to find nearest unoccupied house. If the house gets occupied, it finds next nearest suitable (unoccupied) house.
				// - In the future, make it so that it coordinates with other units (as a part of strategic AI) & each unit only goes to buildings that won't be occupied by other unit currently on the way there.
				if( this.IsGoingToHome( goalController ) )
				{
					return;
				}

				// if is at home
				if( this.civilian.interior != null && this.civilian.interior != this.civilian.workplace.interior )
				{
					return;
				}

				// stops work w/o the need to go back to the workplace.
				Debug.LogWarning( "Stopping work" );
				this.isWorking = false;

				InteriorModule closestHouse = this.GetClosestInterior();

				if( closestHouse != null )
				{
					Debug.LogWarning( "Going home" );
					TacticalMoveToGoal goal = new TacticalMoveToGoal();

					goal.SetDestination( closestHouse, InteriorModule.SlotType.Generic );
					goalController.goal = goal;
				}

				return;
			}

			if( this.isWorking ) // set after the worker has checked in.
			{
				this.civilian.workplace.MakeDoWork( this.civilian );
			}
			else
			{
				TacticalGoalController goalController = this.GetComponent<TacticalGoalController>();
				if( this.IsGoingToWorkplace( goalController ) )
				{
					return;
				}
				
				if( this.civilian.interior == this.civilian.workplace.interior && this.civilian.slotType == InteriorModule.SlotType.Worker && !this.isWorking )
				{
					Debug.LogWarning( "starting work" );
					this.isWorking = true;
				}
				else
				{
					Debug.LogWarning( "Going to work" );
					TacticalMoveToGoal goal = new TacticalMoveToGoal();
					goal.SetDestination( this.civilian.workplace.interior, InteriorModule.SlotType.Worker );
					goalController.goal = goal;
				}
			}
		}
	}
}
