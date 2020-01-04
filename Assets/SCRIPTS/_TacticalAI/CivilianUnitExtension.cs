using SS.AI.Goals;
using SS.Objects;
using SS.Objects.Buildings;
using SS.Objects.Modules;
using SS.Objects.Units;
using UnityEngine;

namespace SS.AI
{
	public class CivilianUnitExtension : MonoBehaviour
	{
		private Unit __unit = null;
		public Unit unit
		{
			get
			{
				if( this.__unit == null )
				{
					this.__unit = this.GetComponent<Unit>();
				}
				return this.__unit;
			}
		}


		public WorkplaceModule workplace { get; set; } = null;
		public int workplaceSlotId { get; set; }
		public bool isWorking;// { get; private set; }

		bool IsGoingToHome( TacticalGoalController goalController )
		{
			if( goalController.goal is TacticalMoveToGoal )
			{
				TacticalMoveToGoal moveToGoal = (TacticalMoveToGoal)goalController.goal;
				if( moveToGoal.destinationInterior != null && moveToGoal.destinationInterior != this.workplace.interior )
				{
					return true;
				}
			}
			return false;
		}

		bool IsGoingToWorkplace( TacticalGoalController goalController )
		{
			if( goalController.goal is TacticalMoveToGoal && ((TacticalMoveToGoal)goalController.goal).destinationInterior == this.workplace.interior )
			{
				return true;
			}
			return false;
		}

		private InteriorModule GetClosestInteriorBuilding()
		{
			Building[] b = SSObject.GetAllBuildings();

			InteriorModule interior = null;
			float dst = float.MaxValue;
			for( int i = 0; i < b.Length; i++ )
			{
				if( b[i].factionId != this.unit.factionId )
				{
					continue;
				}

				InteriorModule[] interiors = b[i].GetModules<InteriorModule>();
				if( interiors.Length == 0 )
				{
					continue;
				}

				if( interiors[0].GetFirstValid( InteriorModule.SlotType.Generic, this.unit ) == null )
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
			if( this.workplace == null )
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
				if( this.unit.interior != null && this.unit.interior != this.workplace.interior )
				{
					return;
				}

				// stops work w/o the need to go back to the workplace.
				this.isWorking = false;

				InteriorModule closestHouse = this.GetClosestInteriorBuilding();

				if( closestHouse != null )
				{
					// goes to sleep normally, enters building when near it.

					// when time comes to go out to work, it either appears at workplace, or at home.


					TacticalMoveToGoal goal = new TacticalMoveToGoal();

					goal.SetDestination( closestHouse, InteriorModule.SlotType.Generic );
					goalController.goal = goal;
				}

				return;
			}

			if( this.isWorking ) // set after the worker has checked in.
			{
				this.workplace.MakeDoWork( this.unit );
			}
			else
			{
				TacticalGoalController goalController = this.GetComponent<TacticalGoalController>();
				if( this.IsGoingToWorkplace( goalController ) )
				{
					return;
				}
				
				if( this.unit.interior == this.workplace.interior && this.unit.slotType == InteriorModule.SlotType.Worker && !this.isWorking )
				{
					this.isWorking = true;
				}
				else
				{
					TacticalMoveToGoal goal = new TacticalMoveToGoal();
					goal.SetDestination( this.workplace.interior, InteriorModule.SlotType.Worker );
					goalController.goal = goal;
				}
			}
		}
	}
}
