using SS.AI;
using SS.AI.Goals;
using SS.Levels.SaveStates;
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


				ResourceDepositModule closestDeposit = SSObjectUtils.GetClosestInRangeContaining( this.aoi.center, this.aoi.radius, this.resourceId );
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


				InventoryModule closestinv = SSObjectUtils.GetClosestWithSpace( worker, worker.transform.position, this.resourceId, worker.factionId );
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

		public override SSModuleData GetData()
		{
			ResourceCollectorWorkplaceModuleData data = new ResourceCollectorWorkplaceModuleData()
			{
				aoi = this.aoi
			};

			return data;
		}

		public override void SetData( SSModuleData _data )
		{
			ResourceCollectorWorkplaceModuleData data = ValidateDataType<ResourceCollectorWorkplaceModuleData>( _data );
		}
	}
}