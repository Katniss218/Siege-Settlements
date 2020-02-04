using SS.AI;
using SS.AI.Goals;
using SS.Content;
using SS.Levels;
using SS.Objects.Buildings;
using SS.Objects.Modules;
using SS.ResourceSystem.Payment;
using SS.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SS.Objects.Units
{
	public class CivilianUnitExtension : SSObjectExtension<Unit>
	{
		private static int avoidancePriority = 10;
		/// <summary>
		/// Returns a value for the avoidance priority (helps prevent units blocking each other in tight spaces - they'll just push the troublesome unit aside).
		/// </summary>
		public static int NextAvoidancePriority( bool employed )
		{
			// 10 - 49, 50 - 89
			avoidancePriority++;
			if( avoidancePriority == 50 )
			{
				avoidancePriority = 10;
			}
			return employed ? avoidancePriority + 40 : avoidancePriority;
		}
		

		/// <summary>
		/// Use WorkplaceModule.Employ or WorkplaceModule.Unemploy to toggle.
		/// </summary>
		public WorkplaceModule workplace { get; internal set; } = null;
		public bool isEmployed
		{
			get
			{
				return this.workplace != null;
			}
		}

		/// <summary>
		/// Use WorkplaceModule.Employ or WorkplaceModule.Unemploy to toggle.
		/// </summary>
		public int workplaceSlotIndex { get; internal set; }
		public bool isWorking { get; set; }

		public UnityEvent onAutomaticDutyToggle { get; private set; } = new UnityEvent();
		public UnityEvent onEmploy { get; private set; } = new UnityEvent();
		public UnityEvent onUnemploy { get; private set; } = new UnityEvent();

#warning automatic duty & employment are parts of the strategic AI. It should take into account other units already going to pay / to houses, etc.
		// this would avoid 10 civilians going to a house that has only 4 slots.

		public bool isOnAutomaticDuty { get; private set; } = false;
		private SSObject automaticDutyReceiverObject = null;
		private IPaymentReceiver automaticDutyReceiver = null;
		private InventoryModule inventory = null;



		public void SetAutomaticDuty( bool value )
		{
			if( this.isEmployed )
			{
				this.isOnAutomaticDuty = false;
				return;
			}

			this.isOnAutomaticDuty = value;
			// if automatic duty assigned any goals - clear them.
			if( !value )
			{
				this.automaticDutyReceiver = null;
				this.obj.controller.SetGoals( TacticalGoalController.DEFAULT_GOAL_TAG, TacticalGoalController.GetDefaultGoal() );
			}
			this.onAutomaticDutyToggle?.Invoke();
		}


		
		private void Start()
		{
			InventoryModule[] inventories = this.obj.GetModules<InventoryModule>();
			if( inventories.Length > 0 )
			{
				this.inventory = inventories[0];
			}

			this.onAutomaticDutyToggle.AddListener( () =>
			{
				if( !Selection.IsDisplayed( this.obj ) )
				{
					return;
				}

				Transform t = ActionPanel.instance.GetActionButton( "civilian.autoduty" );
				if( this.isOnAutomaticDuty )
				{
					t.GetComponent<Image>().sprite = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/autodutyoff" );
				}
				else
				{
					t.GetComponent<Image>().sprite = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/autoduty" );
				}
			} );

			this.onEmploy.AddListener( () =>
			{
				if( !Selection.IsDisplayed( this.obj ) )
				{
					return;
				}

				ActionPanel.instance.Clear( "civilian.autoduty" );
				ActionPanel.instance.Clear( "civilian.employ" );
				UnitDisplayManager.CreateUnemployButton( this );
				ActionPanel.instance.Clear( "unit.ap.pickup" );
				ActionPanel.instance.Clear( "unit.ap.dropoff" );
			} );

			this.onUnemploy.AddListener( () =>
			{
				if( !Selection.IsDisplayed( this.obj ) )
				{
					return;
				}

				ActionPanel.instance.Clear( "civilian.unemploy" );
				UnitDisplayManager.CreateAutodutyButton( this );
				UnitDisplayManager.CreateEmployButton( this );
				UnitDisplayManager.CreateQueryButtons();
			} );
		}







		//
		//
		//
		



		private InteriorModule GetClosestInteriorBuilding()
		{
			Building[] b = SSObject.GetAllBuildings();

			InteriorModule interior = null;
			float dstSq = float.MaxValue;
			for( int i = 0; i < b.Length; i++ )
			{
				if( b[i].factionId != this.obj.factionId )
				{
					continue;
				}

				if( !b[i].isUsable )
				{
					continue;
				}

				InteriorModule[] interiors = b[i].GetModules<InteriorModule>();
				if( interiors.Length == 0 )
				{
					continue;
				}

				if( interiors[0].GetFirstValid( InteriorModule.SlotType.Generic, this.obj ) == null )
				{
					continue;
				}

				float newDstSq = (b[i].transform.position - this.transform.position).sqrMagnitude;
				if( newDstSq >= dstSq )
				{
					continue;
				}

				interior = interiors[0];
			}
			return interior;
		}



		public const int TAG_MAKING_SPACE = 75;
		public const int TAG_GOING_TO_PICKUP = 76;
		public const int TAG_GOING_TO_PAY = 77;

		void UpdateAutomaticDuty()
		{
			if( this.inventory == null )
			{
				this.SetAutomaticDuty( false );
				return;
			}

			TacticalGoalController controller = this.obj.controller;

			if( controller.goalTag == TacticalGoalQuery.TAG_CUSTOM )
			{
				this.SetAutomaticDuty( false );
				return;
			}

			if( controller.goalTag == TAG_GOING_TO_PICKUP )
			{
				return;
			}

			if( controller.goalTag == TAG_GOING_TO_PAY )
			{
				return;
			}

			if( controller.goalTag == TAG_MAKING_SPACE )
			{
				return;
			}

			
			// controller's goal tag is what?

			if( this.inventory.isEmpty )
			{
				if( this.automaticDutyReceiver != null && automaticDutyReceiverObject != null )
				{
					Dictionary<string, int> wantedResources = this.automaticDutyReceiver.GetWantedResources();
					// If the receiver no longer needs anything - find new.
					if( wantedResources.Count == 0 )
					{
						this.automaticDutyReceiver = null;
						this.automaticDutyReceiverObject = null;
						return;
					}

					bool foundInventory = false;
					foreach( var kvp in wantedResources )
					{
						if( LevelDataManager.factionData[this.obj.factionId].resourcesStoredCache.ContainsKey( kvp.Key ) )
						{
							InventoryModule closestinventory = ResourceCollectorWorkplaceModule.GetClosestInventoryContaining( this.obj.transform.position, this.obj.factionId, kvp.Key );

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
								controller.SetGoals( TAG_GOING_TO_PICKUP, goal1, goal2 );

								foundInventory = true;
							}
							break;
						}
					}
					if( !foundInventory ) // else - can't pick up any of the wanted resources.
					{
						// - - - find any receiver that wants resources that can be found (needs cache of all available resources per faction).
						IPaymentReceiver receiver = ResourceCollectorWorkplaceModule.GetClosestWantingPayment( this.obj.transform.position, this.obj.factionId,
							LevelDataManager.factionData[this.obj.factionId].GetResourcesStored().ToArray()
						);

						this.automaticDutyReceiver = receiver; // if null, will be set to null.
						this.automaticDutyReceiverObject = receiver == null ? null : receiver.ssObject;
					}
				}
				else
				{
					// - - - find any receiver that wants resources that can be found (needs cache of all available resources per faction).
					IPaymentReceiver receiver = ResourceCollectorWorkplaceModule.GetClosestWantingPayment( this.obj.transform.position, this.obj.factionId,
						LevelDataManager.factionData[this.obj.factionId].GetResourcesStored().ToArray()
					);

					this.automaticDutyReceiver = receiver; // if null, will be set to null.
					this.automaticDutyReceiverObject = receiver == null ? null : receiver.ssObject;
				}
			}
			else
			{
				if( this.automaticDutyReceiver != null && automaticDutyReceiverObject != null )
				{
					Dictionary<string, int> wantedResources = this.automaticDutyReceiver.GetWantedResources();
					// If the receiver no longer needs anything - find new.
					if( wantedResources.Count == 0 )
					{
						this.automaticDutyReceiver = null;
						this.automaticDutyReceiverObject = null;
						return;
					}

					Dictionary<string, int> resourcesCarried = inventory.GetAll();

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
							controller.SetGoals( TAG_GOING_TO_PAY, goal1, goal2 );

							return;
						}
					}

					// If the receiver didn't want any of the carried resources... - Drop off at least one of them.

					string[] resourceTypesCarried = resourcesCarried.Keys.ToArray();

					InventoryModule dropOffToThis = ResourceCollectorWorkplaceModule.GetClosestWithSpace( this.obj, this.obj.transform.position, resourceTypesCarried[0], this.obj.factionId );

					if( dropOffToThis != null )
					{
						TacticalMoveToGoal goal1 = new TacticalMoveToGoal();
						goal1.SetDestination( dropOffToThis.ssObject );

						TacticalDropOffGoal goal2 = new TacticalDropOffGoal();
						goal2.SetDestination( dropOffToThis );
						controller.SetGoals( TAG_MAKING_SPACE, goal1, goal2 );
					}
				}
				// inventory isn't empty, doesn't have a receiver to drop off at.
				else
				{
					string[] resourceTypesCarried = inventory.GetAll().Keys.ToArray();
					// - - find receiver that wants any of the carried resources.
					// - - - find any receiver that wants resources that can be found (needs cache of all available resources per faction).
					IPaymentReceiver receiver = ResourceCollectorWorkplaceModule.GetClosestWantingPayment( this.obj.transform.position, this.obj.factionId,
						resourceTypesCarried
					);

					// If can't find a receiver that wants ANY of the carried resources, drop off at least one of the carried resources, and then try again - using faction's stored resources.
					if( receiver == null )
					{
						InventoryModule dropOffToThis = ResourceCollectorWorkplaceModule.GetClosestWithSpace( this.obj, this.obj.transform.position, resourceTypesCarried[0], this.obj.factionId );

						if( dropOffToThis != null )
						{
							TacticalMoveToGoal goal1 = new TacticalMoveToGoal();
							goal1.SetDestination( dropOffToThis.ssObject );

							TacticalDropOffGoal goal2 = new TacticalDropOffGoal();
							goal2.SetDestination( dropOffToThis );
							controller.SetGoals( TAG_MAKING_SPACE, goal1, goal2 );
						}
					}
					else
					{
						this.automaticDutyReceiver = receiver;
						this.automaticDutyReceiverObject = receiver.ssObject;
					}
				}
			}
		}

		public const int TAG_GOING_TO_HOUSE = -20;
		public const int TAG_GOING_TO_WORKPLACE = -21;

		private void UpdateWorkerSchedule()
		{
			TacticalGoalController controller = this.obj.controller;

			// If workplace is damaged - stop working, go home.
			if( this.workplace.ssObject is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)this.workplace.ssObject).isUsable )
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
				if( this.obj.interior != null && this.obj.interior != this.workplace.interior )
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
				this.workplace.MakeDoWork( this.obj );
			}
			else
			{
				if( controller.goalTag == TAG_GOING_TO_WORKPLACE )
				{
					return;
				}

				if( this.obj.interior == this.workplace.interior && this.obj.slotType == InteriorModule.SlotType.Worker && !this.isWorking )
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


		void Update()
		{
			if( this.isEmployed )
			{
				this.UpdateWorkerSchedule();
			}
			else
			{
				if( this.isOnAutomaticDuty )
				{
					this.UpdateAutomaticDuty();
				}
			}
		}
	}
}