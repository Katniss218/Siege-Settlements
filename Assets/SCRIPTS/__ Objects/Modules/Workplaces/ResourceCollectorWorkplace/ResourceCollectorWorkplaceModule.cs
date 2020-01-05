using SS.AI;
using SS.AI.Goals;
using SS.Levels.SaveStates;
using SS.Objects.Buildings;
using SS.Objects.Extras;
using SS.Objects.Units;
using SS.ResourceSystem.Payment;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class ResourceCollectorWorkplaceModule : WorkplaceModule
	{
		public const string KFF_TYPEID = "workplace_resource_collector";

		public string resourceId { get; set; }

		public AreaOfInfluence aoi { get; set; }

		void Update()
		{
			
		}

		public static ResourceDepositModule GetClosestInRangeContaining( Vector3 pos, float range, string resourceId )
		{
			Extra[] extras = SSObject.GetAllExtras();

			ResourceDepositModule ret = null;
			float dstSq = range*range;
			for( int i = 0; i < extras.Length; i++ )
			{
				// If is in range.
				float newDstSq = (pos - extras[i].transform.position).sqrMagnitude;
				if( newDstSq > dstSq )
				{
					continue;
				}
				dstSq = newDstSq;

				// If has resource deposit.
				ResourceDepositModule[] resourceDeposits = extras[i].GetModules<ResourceDepositModule>();

				if( resourceDeposits.Length == 0 )
				{
					continue;
				}

				// If deposit contains wanted resource.
				for( int j = 0; j < resourceDeposits.Length; j++ )
				{
					if( resourceDeposits[j].GetAll().ContainsKey( resourceId ) )
					{
						ret = resourceDeposits[j];
						break; // break inner loop
					}
				}
			}
			return ret;
		}

		public static IPaymentReceiver GetClosestWantingPayment( Vector3 pos, int factionId, string resourceId = null )
		{
			SSObject[] objects = SSObject.GetAll();

			IPaymentReceiver ret = null;
			float dstSq = float.MaxValue;

			Dictionary<string, int> resourcesWanted;
			for( int i = 0; i < objects.Length; i++ )
			{
				// If has resource deposit.
				IPaymentReceiver[] paymentReceivers = objects[i].GetComponents<IPaymentReceiver>();

				if( paymentReceivers.Length == 0 )
				{
					continue;
				}

				if( objects[i] is IFactionMember )
				{
					if( ((IFactionMember)objects[i]).factionId != factionId )
					{
						continue;
					}
				}

				for( int j = 0; j < paymentReceivers.Length; j++ )
				{
					resourcesWanted = paymentReceivers[j].GetWantedResources();
					if( resourcesWanted.Count == 0 )
					{
						break;
					}

					if( resourceId == null || (resourceId != null && resourcesWanted.ContainsKey( resourceId ) ) )
					{
						// If is in range.
						float newDstSq = (pos - objects[i].transform.position).sqrMagnitude;// Vector3.Distance( pos, objects[i].transform.position );
						if( newDstSq <= dstSq )
						{
							dstSq = newDstSq;
							ret = paymentReceivers[j];
							break; // break inner loop
						}
					}

				}
			}
			return ret;
		}


		public InventoryModule GetClosestWithSpace( SSObject self, Vector3 pos, string resourceId, int factionId )
		{
			SSObjectDFS[] objects = SSObject.GetAllDFS();
			
			InventoryModule ret = null;
			float dstSqToLastValid = float.MaxValue;
			for( int i = 0; i < objects.Length; i++ )
			{
				if( objects[i] == self )
				{
					continue;
				}

				// If is in range.
				float newDstSq = (pos - objects[i].transform.position).sqrMagnitude;
				if( newDstSq > dstSqToLastValid )
				{
					continue;
				}

				if( objects[i].factionId != factionId )
				{
					continue;
				}
				
				// If has resource deposit.
				InventoryModule[] inventories = objects[i].GetModules<InventoryModule>();

				if( inventories.Length == 0 )
				{
					continue;
				}
				
				for( int j = 0; j < inventories.Length; j++ )
				{
					if( inventories[j].isStorage )
					{
						if( inventories[j].GetSpaceLeft( resourceId ) > 0 )
						{
							dstSqToLastValid = newDstSq;
							ret = inventories[j];
							break; // break inner loop
						}
					}
				}
			}
			return ret;
		}


		public override void MakeDoWork( Unit worker )
		{
			InventoryModule[] inventories = worker.GetModules<InventoryModule>();
			if( inventories.Length == 0 )
			{
				return;
			}

			this.aoi.center = this.transform.position;

			InventoryModule inventory = inventories[0];


			TacticalGoalController goalController = worker.GetComponent<TacticalGoalController>();
			
			
			

			int spaceLeft = inventory.GetSpaceLeft( this.resourceId );
			
			if( spaceLeft > 0 )
			{
				if( IsGoingToPickUp( goalController, this.resourceId ) )
				{
					return;
				}

				ResourceDepositModule closestDeposit = GetClosestInRangeContaining( this.aoi.center, this.aoi.radius, this.resourceId );
				if( closestDeposit != null )
				{
					TacticalPickUpGoal goal = new TacticalPickUpGoal();

					goal.destinationObject = closestDeposit.ssObject;
					goal.resourceId = this.resourceId;
					goalController.goal = goal;
				}
			}
			else
			{
				if( IsGoingToDropOff( goalController, this.resourceId, TacticalDropOffGoal.ObjectDropOffMode.INVENTORY ) )
				{
					return;
				}

				InventoryModule closestinv = GetClosestWithSpace( worker, worker.transform.position, this.resourceId, worker.factionId );
				
				if( closestinv != null )
				{
					TacticalDropOffGoal goal = new TacticalDropOffGoal();

					goal.SetDestination( closestinv.ssObject );
					goal.objectDropOffMode = TacticalDropOffGoal.ObjectDropOffMode.INVENTORY;
					goal.resourceId = this.resourceId;
					goalController.goal = goal;
				}
			}
		}

		public static bool IsGoingToPickUp( TacticalGoalController goalController, string resourceId )
		{
			if( goalController.goal is TacticalPickUpGoal && ((TacticalPickUpGoal)goalController.goal).resourceId == resourceId )
			{
				return true;
			}
			return false;
		}

		public static bool IsGoingToDropOff( TacticalGoalController goalController, string resourceId, TacticalDropOffGoal.ObjectDropOffMode dropOffMode )
		{
			if( goalController.goal is TacticalDropOffGoal )
			{
				TacticalDropOffGoal dropOffGoal = (TacticalDropOffGoal)goalController.goal;
				if( dropOffGoal.resourceId == resourceId && dropOffGoal.objectDropOffMode == dropOffMode )
				{
					return true;
				}
			}
			return false;
		}

		public override ModuleData GetData()
		{
			ResourceCollectorWorkplaceModuleData data = new ResourceCollectorWorkplaceModuleData()
			{
				aoi = this.aoi
			};

			return data;
		}

		public override void SetData( ModuleData _data )
		{
			ResourceCollectorWorkplaceModuleData data = ValidateDataType<ResourceCollectorWorkplaceModuleData>( _data );
		}
	}
}
