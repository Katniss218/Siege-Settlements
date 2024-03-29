﻿using Katniss.Utils;
using SS.Content;
using SS.Objects;
using SS.Objects.Modules;
using SS.ResourceSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.AI.Goals
{
	public class TacticalPickUpGoal : TacticalGoal
	{
		public const string KFF_TYPEID = "pick_up";

		public enum GoalHostileMode : byte
		{
			ALL,
			NONE
		}

		public enum PickUpMode : byte
		{
			INVENTORY,
			RESOURCE_DEPOSIT
		}

		private const float INTERACTION_DISTANCE = 0.9f;
		private const float INTERACTION_DELAY = 0.3f;
		
		/// <summary>
		/// Specified which resources to pick up (set to null to take any and all resources).
		/// </summary>
		public Dictionary<string, int> resources { get; set; } = null;

		public void ApplyResources()
		{
			this.resourcesRemaining = this.resources;
		}



		/// <summary>
		/// Specifies where it will look to pick up resources.
		/// </summary>
		public PickUpMode pickUpMode { get; private set; }
		public InventoryModule destinationInventory { get; private set; }
		public ResourceDepositModule destinationResourceDeposit { get; private set; }
		
		public bool isHostile { get; set; }


		private Dictionary<string, int> resourcesRemaining;

		private float amountCollectedFractional = 0.0f;
		
		private InventoryModule inventory;
		private IAttackModule[] attackModules;



		public TacticalPickUpGoal()
		{
			this.isHostile = true;
		}


		public void SetDestination( InventoryModule inventory )
		{
			this.pickUpMode = PickUpMode.INVENTORY;
			this.destinationInventory = inventory;
			this.destinationResourceDeposit = null;
		}

		public void SetDestination( ResourceDepositModule resourceDeposit )
		{
			this.pickUpMode = PickUpMode.RESOURCE_DEPOSIT;
			this.destinationInventory = null;
			this.destinationResourceDeposit = resourceDeposit;
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override bool CanBeAddedTo( SSObject ssObject )
		{
			return ssObject.GetModules<InventoryModule>().Length > 0;
		}

		public override void Start( TacticalGoalController controller )
		{
			this.inventory = controller.ssObject.GetModules<InventoryModule>()[0];
			this.attackModules = controller.GetComponents<IAttackModule>();
			this.delayTimeStamp = Time.time + INTERACTION_DELAY;
		}


		private bool? PickUpFromDeposit( TacticalGoalController controller )
		{
			Dictionary<string, int> resourcesInDeposit = this.destinationResourceDeposit.GetAll();

			foreach( var kvp in resourcesInDeposit )
			{
				int amountRemainingWanted = int.MaxValue;

				// If the goal wants a specific resource - disregard any other resources.
				if( this.resourcesRemaining != null )
				{
					amountRemainingWanted = 0;
					this.resourcesRemaining.TryGetValue( kvp.Key, out amountRemainingWanted );
				}

				// If set to 0, this means that resources remaining for this resource is present & equal to 0.
				// If set to int.MaxValue, this means that there's no 'resources remaining' for this resource.
				if( amountRemainingWanted == 0 )
				{
					continue;
				}

				int spaceLeftSelf = this.inventory.GetSpaceLeft( kvp.Key );
				if( spaceLeftSelf == 0 )
				{
					continue;
				}

				this.amountCollectedFractional += ResourceDepositModule.MINING_SPEED * Time.deltaTime;
				int amountCollectedFloored = Mathf.FloorToInt( amountCollectedFractional );

				// when the amount collected has accumulated to over '1' - take '1' of any (wanted) resource.
				if( amountCollectedFloored >= 1 )
				{
					int amountToTake = amountCollectedFloored > kvp.Value ? kvp.Value : amountCollectedFloored;

					// If tried to take more than can hold - clamped automatically.
					int amountTaken = this.inventory.Add( kvp.Key, amountToTake );
					
					this.amountCollectedFractional -= amountCollectedFloored;

					if( amountTaken > 0 )
					{
						// Remove the actual amount taken by the inventory.
						this.destinationResourceDeposit.Remove( kvp.Key, amountTaken );
						
						AudioManager.PlaySound( this.destinationResourceDeposit.miningSound, controller.transform.position );
					}
					break; // Only pick up '1' of any resource per-pickup.
				}
			}
			
			bool isSomethingRemaining = false;
			// check if there is still more resources remaining.
			if( this.resourcesRemaining != null )
			{
				foreach( var kvp in this.resourcesRemaining )
				{
					if( kvp.Value > 0 )
					{
						isSomethingRemaining = true;
					}
				}

				// only return actual success when there's no this.resourcesRemaining
				// only return actual failure if the inventory is full.
				// otherwise, wait - the operation is not done yet.
				if( isSomethingRemaining )
				{
					if( this.inventory.isFull )
					{
						return false;
					}
					return null; // operation not done yet, can still take more, but picking up from deposits takes time.
				}
				else
				{
					return true;
				}
			}
			else
			{
				if( !this.inventory.isFull )
				{
					return null;
				}
				return true;
			}
		}

		private bool PickUpFromInventory( TacticalGoalController controller )
		{
			Dictionary<string, int> resourcesInInventory = this.destinationInventory.GetAll();

			// if not specified the types of resources, set to true.
			bool tookEverythingWantedOrAny = true;

			foreach( var kvp in resourcesInInventory )
			{
				int amountRemainingWanted = int.MaxValue;

				// If the goal wants a specific resource - disregard any other resources.
				if( this.resourcesRemaining != null )
				{
					amountRemainingWanted = 0;
					this.resourcesRemaining.TryGetValue( kvp.Key, out amountRemainingWanted );
				}

				// If set to 0, this means that resources remaining for this resource is present & equal to 0.
				// If set to int.MaxValue, this means that there's no 'resources remaining' for this resource.
				if( amountRemainingWanted == 0 )
				{
					continue;
				}

				int spaceLeftSelf = this.inventory.GetSpaceLeft( kvp.Key );
				if( spaceLeftSelf == 0 )
				{
					continue;
				}

				// Take either what's left or enough to completely fill the remaining space.
				int amountToTake = amountRemainingWanted > kvp.Value ? kvp.Value : amountRemainingWanted;

				// If tried to take more than can hold - clamped automatically.
				int amountTaken = this.inventory.Add( kvp.Key, amountToTake );

				if( amountTaken > 0 )
				{
					// Remove the actual amount taken by the inventory.
					this.destinationInventory.Remove( kvp.Key, amountTaken );

					ResourceDefinition def = DefinitionManager.GetResource( kvp.Key );
					AudioManager.PlaySound( def.pickupSound, controller.transform.position );
				}
				else
				{
					if( this.resourcesRemaining != null )
					{
						tookEverythingWantedOrAny = false;
					}
				}

			}

			return tookEverythingWantedOrAny; // true for successful exit. false for failure.
		}


		private float delayTimeStamp;

		public override void Update( TacticalGoalController controller )
		{
			if( (this.pickUpMode == PickUpMode.INVENTORY) && (this.destinationInventory == null) )
			{
				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				return;
			}

			if( (this.pickUpMode == PickUpMode.RESOURCE_DEPOSIT) && (this.destinationResourceDeposit == null) )
			{
				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				return;
			}

			if( controller.ssObject is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)controller.ssObject).isUsable )
			{
				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				return;
			}

			if( attackModules.Length > 0 )
			{
				this.UpdateTargeting( controller, this.isHostile, this.attackModules );
			}


			// Prevents spam-firing the pickup-dropoff goal pairs when the unit is in range of both the receiver & source (storage).
			if( Time.time < this.delayTimeStamp )
			{
				return;
			}


			if( this.pickUpMode == PickUpMode.INVENTORY )
			{
				if( this.destinationInventory.ssObject is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)this.destinationInventory.ssObject).isUsable )
				{
					controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
					return;
				}

				if( DistanceUtils.IsInRangePhysical( controller.transform, this.destinationInventory.transform, INTERACTION_DISTANCE ) )
				{
					bool outcome = this.PickUpFromInventory( controller );

					controller.ExitCurrent( outcome ? TacticalGoalExitCondition.SUCCESS : TacticalGoalExitCondition.FAILURE );
					return;
				}
				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				// fail

				return;
			}
			if( this.pickUpMode == PickUpMode.RESOURCE_DEPOSIT )
			{
				if( this.destinationResourceDeposit.ssObject is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)this.destinationResourceDeposit.ssObject).isUsable )
				{
					controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
					return;
				}

				if( DistanceUtils.IsInRangePhysical( controller.transform, this.destinationResourceDeposit.transform, INTERACTION_DISTANCE ) )
				{
					bool? outcome = this.PickUpFromDeposit( controller );

					if( outcome == null ) // not done yet.
					{
						return;
					}

					controller.ExitCurrent( outcome.Value ? TacticalGoalExitCondition.SUCCESS : TacticalGoalExitCondition.FAILURE );
					return;
				}
				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				return;
			}
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override TacticalGoalData GetData()
		{
			TacticalPickUpGoalData data = new TacticalPickUpGoalData()
			{
				isHostile = this.isHostile
			};

			data.pickUpMode = this.pickUpMode;
			if( this.pickUpMode == PickUpMode.INVENTORY )
			{
				data.destinationGuid = new Tuple<Guid, Guid>( this.destinationInventory.ssObject.guid, this.destinationInventory.moduleId );
			}
			else if( this.pickUpMode == PickUpMode.RESOURCE_DEPOSIT )
			{
				data.destinationGuid = new Tuple<Guid, Guid>( this.destinationResourceDeposit.ssObject.guid, this.destinationResourceDeposit.moduleId );
			}
			if( this.resources == null )
			{
				data.resources = null;
			}
			else
			{
				data.resources = new Dictionary<string, Tuple<int, int>>();
				foreach( var kvp in this.resources )
				{
					string id = kvp.Key;
					int amt = kvp.Value;
					int amtRemaining = this.resourcesRemaining[id];

					data.resources.Add( id, new Tuple<int, int>( amt, amtRemaining ) );
				}
			}
			return data;
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalPickUpGoalData data = (TacticalPickUpGoalData)_data;

			this.pickUpMode = data.pickUpMode;
			if( this.pickUpMode == PickUpMode.INVENTORY )
			{
				this.SetDestination( SSObject.Find( data.destinationGuid.Item1 ).GetModule<InventoryModule>( data.destinationGuid.Item2 ) );
			}
			else if( this.pickUpMode == PickUpMode.RESOURCE_DEPOSIT )
			{
				this.SetDestination( SSObject.Find( data.destinationGuid.Item1 ).GetModule<ResourceDepositModule>( data.destinationGuid.Item2 ) );
			}
			if( data.resources == null )
			{
				this.resources = null;
			}
			else
			{
				this.resources = new Dictionary<string, int>();
				this.resourcesRemaining = new Dictionary<string, int>();
				foreach( var kvp in data.resources )
				{
					this.resources.Add( kvp.Key, kvp.Value.Item1 );
					this.resourcesRemaining.Add( kvp.Key, kvp.Value.Item2 );
				}
			}

			this.isHostile = data.isHostile;
		}
	}
}