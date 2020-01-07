using SS.AI;
using SS.AI.Goals;
using SS.Levels.SaveStates;
using SS.Objects.Buildings;
using SS.Objects.Extras;
using SS.Objects.Units;
using SS.ResourceSystem.Payment;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class ResourceCollectorWorkplaceModule : WorkplaceModule
	{
		public const string KFF_TYPEID = "workplace_resource_collector";

		public string resourceId { get; set; }

#warning if they get stuck, resetting the AoI recalculates the goals.
		public AreaOfInfluence aoi { get; set; }

		void Update()
		{
			
		}

		public static InventoryModule GetClosestInventoryContaining( Vector3 pos, int factionId, string resourceId )
		{
			SSObjectDFS[] objects = SSObject.GetAllDFS();

			InventoryModule ret = null;
			float dstSq = float.MaxValue;
			for( int i = 0; i < objects.Length; i++ )
			{
				if( !objects[i].hasInventoryModule )
				{
					continue;
				}

				if( objects[i].factionId != factionId )
				{
					continue;
				}

				// If is in range.
				float newDstSq = (pos - objects[i].transform.position).sqrMagnitude;
				if( newDstSq > dstSq )
				{
					continue;
				}


#warning take into account object being unusable.
				// If has resource deposit.
				InventoryModule[] inventories = objects[i].GetModules<InventoryModule>();
				
				// If inventory is storage & contains wanted resource.
				for( int j = 0; j < inventories.Length; j++ )
				{
					if( inventories[j].isStorage )
					{
						if( inventories[j].GetAll().ContainsKey( resourceId ) )
						{
							dstSq = newDstSq;
							ret = inventories[j];
							break; // break inner loop
						}
					}
				}
			}
			return ret;
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
						dstSq = newDstSq;
						ret = resourceDeposits[j];
						break; // break inner loop
					}
				}
			}
			return ret;
		}

		public static Tuple<SSObject, IPaymentReceiver> GetClosestWantingPayment( Vector3 pos, int factionId, string[] resourceIds )
		{
			SSObjectDFS[] objects = SSObject.GetAllDFS();

			IPaymentReceiver ret = null;
			SSObject retObj = null;
			float dstSq = float.MaxValue;

			Dictionary<string, int> resourcesWanted;
			IPaymentReceiver[] paymentReceivers;
			for( int i = 0; i < objects.Length; i++ )
			{
				if( objects[i].factionId != factionId )
				{
					continue;
				}

				if( !objects[i].HasPaymentReceivers() )
				{
					continue;
				}
				
				float newDstSq = (pos - objects[i].transform.position).sqrMagnitude;
				if( newDstSq > dstSq )
				{
					continue;
				}


				// If has resource deposit.
				paymentReceivers = objects[i].GetComponents<IPaymentReceiver>();
				

				for( int j = 0; j < paymentReceivers.Length; j++ )
				{
					resourcesWanted = paymentReceivers[j].GetWantedResources();
					if( resourcesWanted.Count == 0 )
					{
						break;
					}

					if( resourceIds != null )
					{
						for( int k = 0; k < resourceIds.Length; k++ )
						{
							if( resourcesWanted.ContainsKey( resourceIds[k] ) )
							{
								dstSq = newDstSq;
								ret = paymentReceivers[j];
								retObj = objects[i];
								break; // break inner loop
							}
						}
					}
				}
			}
			return new Tuple<SSObject, IPaymentReceiver>( retObj, ret );
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

#warning take into account object being unusable.
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

#warning object might not be able to pick up desired resource due to cluttered inventory.
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
