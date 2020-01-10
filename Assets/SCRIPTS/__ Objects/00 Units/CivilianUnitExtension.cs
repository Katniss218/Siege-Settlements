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

		public const int TAG_MAKING_SPACE = 75;
		public const int TAG_GOING_TO_PICKUP = 76;
		public const int TAG_GOING_TO_PAY = 77;

		void UpdateAutomaticDuty()
		{
			TacticalGoalController goalController = this.GetComponent<TacticalGoalController>();

			if( goalController.goalTag == TacticalGoalQuery.TAG_CUSTOM )
			{
				this.isOnAutomaticDuty = false;
				return;
			}

			if( goalController.goalTag == TAG_GOING_TO_PICKUP )
			{
				//if( !selfInventory.isFull )
				//{
				return;
				//}
			}

			if( goalController.goalTag == TAG_GOING_TO_PAY )
			{
				return;
			}

			if( goalController.goalTag == TAG_MAKING_SPACE )
			{
				return;
			}

			InventoryModule[] inventories = this.unit.GetModules<InventoryModule>();
			if( inventories.Length == 0 )
			{
				this.isOnAutomaticDuty = false;
				return;
			}
			
			InventoryModule selfInventory = inventories[0];


			
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

					bool foundInventory = false;
					foreach( var kvp in wantedResources )
					{
						if( LevelDataManager.factionData[this.unit.factionId].resourcesStoredCache.ContainsKey( kvp.Key ) )
						{
							InventoryModule closestinventory = ResourceCollectorWorkplaceModule.GetClosestInventoryContaining( this.unit.transform.position, this.unit.factionId, kvp.Key );

							if( closestinventory != null )
							{
								TacticalMoveToGoal goal1 = new TacticalMoveToGoal();
								goal1.SetDestination( closestinventory.ssObject );

								TacticalPickUpGoal goal2 = new TacticalPickUpGoal();
								goal2.SetDestination( closestinventory );

								// only pick up required amount. -picks up from a single inventory, picks up every wanted resource type (if possible)
								// then, if at least one of the inv slots is full, goes to the receiver.
								goal2.resources = wantedResources;
								goal2.ApplyResources();
								goalController.SetGoals( TAG_GOING_TO_PICKUP, goal1, goal2 );

								foundInventory = true;
							}
							break;
						}
					}
					if( !foundInventory ) // else - can't pick up any of the wanted resources.
					{
						// - - - find any receiver that wants resources that can be found (needs cache of all available resources per faction).
						IPaymentReceiver receiver = ResourceCollectorWorkplaceModule.GetClosestWantingPayment( this.unit.transform.position, this.unit.factionId, 
							LevelDataManager.factionData[this.unit.factionId].GetResourcesStored().ToArray()
						);
						
						this.automaticDutyReceiver = receiver; // if null, will be set to null.
					}
				}
				else
				{
					// - - - find any receiver that wants resources that can be found (needs cache of all available resources per faction).
					IPaymentReceiver receiver = ResourceCollectorWorkplaceModule.GetClosestWantingPayment( this.unit.transform.position, this.unit.factionId,
						LevelDataManager.factionData[this.unit.factionId].GetResourcesStored().ToArray()
					);

					this.automaticDutyReceiver = receiver; // if null, will be set to null.
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
							TacticalMoveToGoal goal1 = new TacticalMoveToGoal();
							goal1.SetDestination( this.automaticDutyReceiver.ssObject );

							TacticalDropOffGoal goal2 = new TacticalDropOffGoal();
							goal2.SetDestination( this.automaticDutyReceiver );
							goalController.SetGoals( TAG_GOING_TO_PAY, goal1, goal2 );

							return;
						}
					}

					// If the receiver didn't want any of the carried resources... - Drop off at least one of them.

					string[] resourceTypesCarried = resourcesCarried.Keys.ToArray();

					InventoryModule dropOffToThis = ResourceCollectorWorkplaceModule.GetClosestWithSpace( this.unit, this.unit.transform.position, resourceTypesCarried[0], this.unit.factionId );

					if( dropOffToThis != null )
					{
						TacticalMoveToGoal goal1 = new TacticalMoveToGoal();
						goal1.SetDestination( dropOffToThis.ssObject );

						TacticalDropOffGoal goal2 = new TacticalDropOffGoal();
						goal2.SetDestination( dropOffToThis );
						goalController.SetGoals( TAG_MAKING_SPACE, goal1, goal2 );
					}
				}
				// inventory isn't empty, doesn't have a receiver to drop off at.
				else
				{
					string[] resourceTypesCarried = selfInventory.GetAll().Keys.ToArray();
					// - - find receiver that wants any of the carried resources.
					// - - - find any receiver that wants resources that can be found (needs cache of all available resources per faction).
					IPaymentReceiver receiver = ResourceCollectorWorkplaceModule.GetClosestWantingPayment( this.unit.transform.position, this.unit.factionId,
						resourceTypesCarried
					);

					// If can't find a receiver that wants ANY of the carried resources, drop off at least one of the carried resources, and then try again - using faction's stored resources.
					if( receiver == null )
					{
						InventoryModule dropOffToThis = ResourceCollectorWorkplaceModule.GetClosestWithSpace( this.unit, this.unit.transform.position, resourceTypesCarried[0], this.unit.factionId );

						if( dropOffToThis != null )
						{
							TacticalMoveToGoal goal1 = new TacticalMoveToGoal();
							goal1.SetDestination( dropOffToThis.ssObject );

							TacticalDropOffGoal goal2 = new TacticalDropOffGoal();
							goal2.SetDestination( dropOffToThis );
							goalController.SetGoals( TAG_MAKING_SPACE, goal1, goal2 );
						}
					}
					else
					{
						this.automaticDutyReceiver = receiver;
					}
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

		public const int TAG_GOING_TO_HOUSE = -20;
		public const int TAG_GOING_TO_WORKPLACE = -21;

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

			TacticalGoalController controller = this.GetComponent<TacticalGoalController>();

			// If workplace is damaged - stop working, go home.
			if( this.workplace.ssObject is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)this.workplace.ssObject).IsUsable() )
			{
				if( controller.goalTag == TAG_GOING_TO_HOUSE )
				{
					return;
				}

				TacticalMoveToGoal goal = new TacticalMoveToGoal();

				InteriorModule closestHouse = this.GetClosestInteriorBuilding();

				goal.SetDestination( closestHouse, InteriorModule.SlotType.Generic );
				controller.SetGoals( TAG_GOING_TO_HOUSE, goal );

				return;
			}
			
			if( !DaylightCycleController.instance.IsWorkTime() )
			{
				// Unit tries to find nearest unoccupied house. If the house gets occupied, it finds next nearest suitable (unoccupied) house.
				// - In the future, make it so that it coordinates with other units (as a part of strategic AI) & each unit only goes to buildings that won't be occupied by other unit currently on the way there.
				if( controller.goalTag == TAG_GOING_TO_HOUSE )
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
					TacticalMoveToGoal goal = new TacticalMoveToGoal();

					goal.SetDestination( closestHouse, InteriorModule.SlotType.Generic );
					controller.SetGoals( TAG_GOING_TO_HOUSE, goal );
				}

				return;
			}

			if( this.isWorking ) // set after the worker has checked in.
			{
				this.workplace.MakeDoWork( this.unit );
			}
			else
			{
				if( controller.goalTag == TAG_GOING_TO_WORKPLACE )
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
					controller.SetGoals( TAG_GOING_TO_WORKPLACE, goal );
				}
			}
		}
	}
}
