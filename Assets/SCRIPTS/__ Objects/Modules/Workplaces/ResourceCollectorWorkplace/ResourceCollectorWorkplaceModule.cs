using SS.AI;
using SS.AI.Goals;
using SS.Levels.SaveStates;
using SS.Objects.Buildings;
using SS.Objects.Extras;
using SS.Objects.Units;
using SS.ResourceSystem.Payment;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

		public static InventoryModule GetClosestInventoryContaining( Vector3 pos, int factionId, string resourceId )
		{
			SSObjectDFSC[] objects = SSObject.GetAllDFS();

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

				// discard any objects that are unusable.
				if( objects[i] is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)objects[i]).isUsable )
				{
					continue;
				}

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
			if( NavMesh.SamplePosition( pos, out NavMeshHit hit, range, int.MaxValue ) )
			{
				pos = hit.position;

				Extra[] extras = SSObject.GetAllExtras();
				ResourceDepositModule ret = null;

				float dstSq = range * range;

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
			else
			{
				// nothing can be reached.
				return null;
			}
		}


		public static IPaymentReceiver GetClosestWantingPayment( Vector3 pos, int factionId, string[] resourceIds )
		{
			SSObjectDFSC[] objects = SSObject.GetAllDFS();

			IPaymentReceiver ret = null;
			float dstSqToLastValid = float.MaxValue;

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
				if( newDstSq > dstSqToLastValid )
				{
					continue;
				}


				paymentReceivers = objects[i].GetAvailableReceivers();


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
								dstSqToLastValid = newDstSq;
								ret = paymentReceivers[j];
								break; // break inner loop
							}
						}
					}
				}
			}
			return ret;
		}


		public static InventoryModule GetClosestWithSpace( SSObject self, Vector3 pos, string resourceId, int factionId )
		{
			SSObjectDFSC[] objects = SSObject.GetAllDFS();

			InventoryModule ret = null;
			float dstSqToLastValid = float.MaxValue;
			for( int i = 0; i < objects.Length; i++ )
			{
				if( objects[i] == self )
				{
					continue;
				}

				if( !objects[i].hasInventoryModule )
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

				// discard any objects that are unusable.
				if( objects[i] is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)objects[i]).isUsable )
				{
					continue;
				}

				InventoryModule[] inventories = objects[i].GetModules<InventoryModule>();

				for( int j = 0; j < inventories.Length; j++ )
				{
					if( inventories[j].isStorage )
					{
						if( inventories[j].GetSpaceLeft( resourceId ) > 0 )
						{
							dstSqToLastValid = newDstSq; // only mark distance to an actual valid objects.
							ret = inventories[j];
							break; // break inner loop
						}
					}
				}
			}
			return ret;
		}

		public const int TAG_GOING_TO_DROPOFF = 55;
		public const int TAG_GOING_TO_PICKUP = 56;

		public override void MakeDoWork( Unit worker )
		{
			InventoryModule[] inventories = worker.GetModules<InventoryModule>();
			if( inventories.Length == 0 )
			{
				return;
			}

			this.aoi.center = this.transform.position;

			InventoryModule inventory = inventories[0];

			TacticalGoalController controller = worker.controller;




			int spaceLeft = inventory.GetSpaceLeft( this.resourceId );

			if( spaceLeft > 0 )
			{
				// If already going to pick up something - don't re-assign it.
				if( controller.goalTag == TAG_GOING_TO_PICKUP )
				{
					return;
				}


				ResourceDepositModule closestDeposit = GetClosestInRangeContaining( this.aoi.center, this.aoi.radius, this.resourceId );
				if( closestDeposit != null )
				{
					TacticalMoveToGoal goal1 = new TacticalMoveToGoal();
					goal1.SetDestination( closestDeposit.ssObject );
					goal1.isHostile = false;

					TacticalPickUpGoal goal2 = new TacticalPickUpGoal();
					goal2.SetDestination( closestDeposit );
					goal2.resources = new Dictionary<string, int>
					{
						{ this.resourceId, int.MaxValue }
					};
					goal2.ApplyResources();
					controller.SetGoals( TAG_GOING_TO_PICKUP, goal1, goal2 );
				}
			}
			else
			{
				// If already going to drop off something - don't re-assign it.
				if( controller.goalTag == TAG_GOING_TO_DROPOFF )
				{
					return;
				}


				InventoryModule closestinv = GetClosestWithSpace( worker, worker.transform.position, this.resourceId, worker.factionId );
				if( closestinv != null )
				{
					TacticalMoveToGoal goal1 = new TacticalMoveToGoal();
					goal1.SetDestination( closestinv.ssObject );
					goal1.isHostile = false;


					TacticalDropOffGoal goal2 = new TacticalDropOffGoal();
					goal2.SetDestination( closestinv );
					goal2.resources = new Dictionary<string, int>
					{
						{ this.resourceId, int.MaxValue }
					};
					goal2.ApplyResources();
					controller.SetGoals( TAG_GOING_TO_DROPOFF, goal1, goal2 );
				}
			}
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