using SS.AI;
using SS.AI.Goals;
using SS.Levels;
using SS.Objects.Buildings;
using SS.Objects.Modules;
using SS.ResourceSystem.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SS.Objects.Units
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


#warning clear workplace tai goal on change.
		public WorkplaceModule workplace { get; set; } = null;
		public int workplaceSlotId { get; set; }
		public bool isWorking { get; private set; }

		bool IsGoingToHome( TacticalGoalController goalController )
		{
			if( goalController.currentGoal is TacticalMoveToGoal )
			{
				TacticalMoveToGoal moveToGoal = (TacticalMoveToGoal)goalController.currentGoal;
				if( moveToGoal.destinationInterior != null && moveToGoal.destinationInterior != this.workplace.interior )
				{
					return true;
				}
			}
			return false;
		}

		bool IsGoingToWorkplace( TacticalGoalController goalController )
		{
			if( goalController.currentGoal is TacticalMoveToGoal && ((TacticalMoveToGoal)goalController.currentGoal).destinationInterior == this.workplace.interior )
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

		public bool isOnAutomaticDuty { get; set; }
		IPaymentReceiver automaticDutyReceiver;
		SSObject automaticDutyReceiverObject;
		string automaticDutyResourceId = null;

		void UpdateAutomaticDuty()
		{
			InventoryModule[] inventories = this.unit.GetModules<InventoryModule>();
			if( inventories.Length == 0 )
			{
				this.isOnAutomaticDuty = false;
				return;
			}

			TacticalGoalController goalController = this.GetComponent<TacticalGoalController>();

			InventoryModule selfInventory = inventories[0];

			if( ResourceCollectorWorkplaceModule.IsGoingToPickUp( goalController, automaticDutyResourceId ) )
			{
#warning when has receiver, and has enough of that resource, don't bother filling it up to full (avoids wasting time & also blocking itself by having resources leftover).
				if( !selfInventory.isFull )
				{
					return;
				}
			}

			if( ResourceCollectorWorkplaceModule.IsGoingToDropOff( goalController, automaticDutyResourceId, TacticalDropOffGoal.DropOffMode.PAYMENT_RECEIVER ) )
			{
				return;
			}
#warning turn off automatic duty when goal is assigned by the player.
#warning some sort of timer to prevent insta-actions with perfectly positioned units. Taking items & putting them should take time (less than extracting from deposits, & pickup/dropoff everything, instead of a single item).
			if( selfInventory.isEmpty )
			{
				if( this.automaticDutyReceiver != null )
				{
					Dictionary<string, int> wantedResources = this.automaticDutyReceiver.GetWantedResources();
					// If the receiver no longer needs anything - find new.
					if( wantedResources.Count == 0 )
					{
						this.automaticDutyReceiver = null;
						return;
					}

					bool foundinventory = false;
					foreach( var kvp in wantedResources )
					{
						if( LevelDataManager.factionData[this.unit.factionId].resourcesStoredCache.ContainsKey( kvp.Key ) )
						{
							InventoryModule closestinventory = ResourceCollectorWorkplaceModule.GetClosestInventoryContaining( this.unit.transform.position, this.unit.factionId, kvp.Key );

							if( closestinventory != null )
							{
								this.automaticDutyResourceId = kvp.Key;

								TacticalPickUpGoal goal = new TacticalPickUpGoal();

								goal.SetDestination( closestinventory );
								goal.resources = new Dictionary<string, int>();
#warning only pick up required amount.
								goal.resources.Add( this.automaticDutyResourceId, 5 );
								goalController.SetGoals( goal );

								foundinventory = true;
							}
							break;
						}
					}
					if( !foundinventory ) // else - can't pick up any of the wanted resources.
					{
						// - - - find any receiver that wants resources that can be found (needs cache of all available resources per faction).
						Tuple<SSObject, IPaymentReceiver> receiver = ResourceCollectorWorkplaceModule.GetClosestWantingPayment( this.unit.transform.position, this.unit.factionId, 
							LevelDataManager.factionData[this.unit.factionId].GetResourcesStored().ToArray()
						);
						
						this.automaticDutyReceiver = receiver.Item2; // if null, will be set to null.
						this.automaticDutyReceiverObject = receiver.Item1;
					}
				}
				else
				{
					// - - - find any receiver that wants resources that can be found (needs cache of all available resources per faction).
					Tuple<SSObject, IPaymentReceiver> receiver = ResourceCollectorWorkplaceModule.GetClosestWantingPayment( this.unit.transform.position, this.unit.factionId,
						LevelDataManager.factionData[this.unit.factionId].GetResourcesStored().ToArray()
					);

					this.automaticDutyReceiver = receiver.Item2; // if null, will be set to null.
					this.automaticDutyReceiverObject = receiver.Item1;
				}
			}
			else
			{
				if( this.automaticDutyReceiver != null )
				{
					Dictionary<string, int> wantedResources = this.automaticDutyReceiver.GetWantedResources();
					// If the receiver no longer needs anything - find new.
					if( wantedResources.Count == 0 )
					{
						this.automaticDutyReceiver = null;
						return;
					}

					Dictionary<string, int> resourcesCarried = selfInventory.GetAll();

					foreach( var kvp in wantedResources )
					{
						// - - if receiver wants any of the resources currently carried
						if( resourcesCarried.ContainsKey( kvp.Key ) )
						{
							// - - - MAKE_PAYMENT
							TacticalDropOffGoal goal = new TacticalDropOffGoal();

							goal.SetDestination( this.automaticDutyReceiver );
							goal.resources = new Dictionary<string, int>();
#warning only pick up required amount.
							goal.resources.Add( this.automaticDutyResourceId, 5 );
							goalController.SetGoals( goal );

							break;
						}
					}
				}
				else
				{
					// - - find receiver that wants any of the carried resources.
					// - - - find any receiver that wants resources that can be found (needs cache of all available resources per faction).
					Tuple<SSObject, IPaymentReceiver> receiver = ResourceCollectorWorkplaceModule.GetClosestWantingPayment( this.unit.transform.position, this.unit.factionId,
						selfInventory.GetAll().Keys.ToArray()
					);

					this.automaticDutyReceiver = receiver.Item2; // if null, will be set to null.
					this.automaticDutyReceiverObject = receiver.Item1;
				}
			}
			// IF going to storage to pick up resources
			// - if has enough of this resource in inventory (receiver wants less or equal to how much this civilian has).
			// - - return

			// IF going to payment receiver
			// - return


			// IF doesn't carry resources
			// - IF has receiver
			// - - if any of required resources can be picked up from storage (needs cache of all available resources per faction)
			// - - - PICK_UP (any of the type wanted).
			// - - else
			// - - - find new receiver using resources that can be found (needs cache of all available resources per faction).
			// - ELSE (no receiver)
			// - - find any receiver, using resources that can be found (needs cache of all available resources per faction).
			// ELSE (carries res)
			// - IF has receiver
			// - - if receiver wants resources
			// - - - MAKE_PAYMENT
			// - ELSE (no receiver)
			// - - find receiver wanting any of the carried resources.

			// if has receiver & resources - go pay.
		}


		void Update()
		{
			if( this.workplace == null )
			{
				if( isOnAutomaticDuty )
				{
					UpdateAutomaticDuty();
				}

				return;
			}

#warning stop working & go home if the workplace is damaged.
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
					goalController.SetGoals( goal );
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
					goalController.SetGoals( goal );
				}
			}
		}
	}
}
