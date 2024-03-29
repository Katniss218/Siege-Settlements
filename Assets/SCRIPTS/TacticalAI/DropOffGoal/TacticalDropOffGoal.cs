﻿using Katniss.Utils;
using SS.Content;
using SS.Levels.SaveStates;
using SS.Objects;
using SS.Objects.Buildings;
using SS.Objects.Extras;
using SS.Objects.Modules;
using SS.ResourceSystem;
using SS.ResourceSystem.Payment;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.AI.Goals
{
	public class TacticalDropOffGoal : TacticalGoal
	{
		public const string KFF_TYPEID = "drop_off";

		public enum GoalHostileMode : byte
		{
			ALL,
			NONE
		}

		public enum DropOffMode : byte
		{
			POSITION,
			INVENTORY,
			PAYMENT_RECEIVER
		}

		private const float INTERACTION_DISTANCE = 0.75f;
		private const float INTERACTION_DELAY = 0.3f;


		public DropOffMode dropOffMode { get; private set; }
		public Vector3 destinationPos { get; private set; }
		private SSObject destination;
		public InventoryModule destinationInventory { get; private set; }

		public IPaymentReceiver destinationPaymentReceiver { get; private set; }

		/// <summary>
		/// Specified which resources to pick up (set to null to take any and all resources).
		/// </summary>
		public Dictionary<string, int> resources { get; set; } = null;

		public void ApplyResources()
		{
			this.resourcesRemaining = this.resources;
		}

		public bool isHostile { get; set; }


		private Dictionary<string, int> resourcesRemaining;

		private InventoryModule inventory;
		private IAttackModule[] attackModules;

		public TacticalDropOffGoal()
		{
			this.isHostile = true;
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public void SetDestination( Vector3 position )
		{
			position.y = 0;

			this.dropOffMode = DropOffMode.POSITION;
			this.destinationPos = position;
			this.destinationInventory = null;
			this.destinationPaymentReceiver = null;
		}

		public void SetDestination( InventoryModule inventory )
		{
			this.dropOffMode = DropOffMode.INVENTORY;
			this.destination = inventory.ssObject;
			this.destinationInventory = inventory;
			this.destinationPaymentReceiver = null;
		}

		public void SetDestination( IPaymentReceiver paymentReceiver )
		{
			this.dropOffMode = DropOffMode.PAYMENT_RECEIVER;
			this.destination = paymentReceiver.ssObject;
			this.destinationInventory = null;
			this.destinationPaymentReceiver = paymentReceiver;
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

		public static void ExtractAndDrop( Vector3 position, Quaternion rotation, string id, int amount )
		{
			ResourceDefinition resourceDef = DefinitionManager.GetResource( id );

			ExtraDefinition def = DefinitionManager.GetExtra( resourceDef.defaultDeposit );

			ResourceDepositModuleDefinition depositDef = def.GetModule<ResourceDepositModuleDefinition>();
			int capacity = 0;
			for( int i = 0; i < depositDef.slots.Length; i++ )
			{
				if( depositDef.slots[i].resourceId == id )
				{
					capacity = depositDef.slots[i].capacity;
				}
			}
			if( capacity != 0 )
			{
				int remaining = amount;
				while( remaining > 0 )
				{
					int resAmount = capacity;
					if( remaining < capacity )
					{
						resAmount = remaining;
					}
					remaining -= resAmount;

					ExtraData data = new ExtraData();
					data.guid = Guid.NewGuid();
					data.position = position;
					data.rotation = rotation;


					Extra extra = ExtraCreator.Create( def, data.guid );
					ExtraCreator.SetData( extra, data );

					ResourceDepositModule resDepo = extra.GetComponent<ResourceDepositModule>();
					foreach( var slot in def.GetModule<ResourceDepositModuleDefinition>().slots )
					{
						resDepo.Add( slot.resourceId, resAmount );
					}
					AudioManager.PlaySound( resourceDef.dropoffSound, position );
				}
			}
		}

		private static Dictionary<string, int> GetClampedRes( Dictionary<string, int> max, Dictionary<string, int> preferred, out bool didClamp )
		{
			// Resources that will be dropped (either this.resources, or resources carried, depends if the resources carried contains enough).
			// If resources is not specified, whole inventory will be dropped.
			Dictionary<string, int> resourcesToDrop = new Dictionary<string, int>();
			// if failed is true, not all resources were able to be dropped (not enough in inv).
			didClamp = false;

			foreach( var kvp in max )
			{
				int amtClamped = kvp.Value;
				if( preferred != null )
				{
					// if preferred doesn't contain this specific resource - don't consider it - don't clamp, or anything - skip.
					if( !preferred.TryGetValue( kvp.Key, out amtClamped ) || amtClamped == 0 /* if returned value is 0 */ )
					{
						continue;
					}

					// If wants to drop off more than it has - set failed to true and clamp the amount.
					if( amtClamped > kvp.Value )
					{
						didClamp = true;
						amtClamped = kvp.Value;
					}
				}

				// Add each resource that will be dropped to the dict & remove it from the inventory.
				resourcesToDrop.Add( kvp.Key, amtClamped );
			}
			return resourcesToDrop;
		}

		private bool OnArrivalPosition( TacticalGoalController controller )
		{
			// Tries to drop off contents of it's inventory on the ground.
			// Fails if the position is not on/above/below the map.

			if( Physics.Raycast( this.destinationPos + new Vector3( 0, 100.0f, 0 ), Vector3.down, out RaycastHit hitInfo ) )
			{
				if( hitInfo.collider.gameObject.layer == ObjectLayer.TERRAIN )
				{
					// Places deposits on a vertical axis going through the destinationPos, at the point of collision with the ground.
					Vector3 depositPosition = hitInfo.point;

					Dictionary<string, int> resourcesCarried = this.inventory.GetAll();

					bool didClamp; // if true, the resources were clamped (not all dropped).
					Dictionary<string, int> resourcesToDrop = GetClampedRes( resourcesCarried, this.resources, out didClamp );

					foreach( var kvp in resourcesToDrop )
					{
						this.inventory.Remove( kvp.Key, kvp.Value );
						ExtractAndDrop( depositPosition, Quaternion.identity, kvp.Key, kvp.Value );
					}

					return didClamp;
				}
			}
			return false;
		}

		private bool OnArrivalInventory( TacticalGoalController controller )
		{
			// Don't drop off resources in unusable inventories.
			if( this.destination is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)this.destination).isUsable )
			{
				return false;
			}

			Dictionary<string, int> resourcesCarried = this.inventory.GetAll();

			bool didClamp; // if true, the resources were clamped (not all dropped).
			Dictionary<string, int> resourcesToDrop = GetClampedRes( resourcesCarried, this.resources, out didClamp );

			bool failed = false;
			foreach( var kvp in resourcesToDrop )
			{
				int amountDroppedOff = this.destinationInventory.Add( kvp.Key, kvp.Value );
				if( amountDroppedOff < kvp.Value )
				{
					failed = true;
				}

				if( amountDroppedOff == 0 )
				{
					continue;
				}

				this.inventory.Remove( kvp.Key, amountDroppedOff );

				ResourceDefinition def = DefinitionManager.GetResource( kvp.Key );
				AudioManager.PlaySound( def.dropoffSound, controller.transform.position );
			}
			if( didClamp ) // if not every resource was delivered - fail.
			{
				return false;
			}
			return !failed;
		}

		private bool OnArrivalPayment( TacticalGoalController controller )
		{
			// If the destination is an unusable object, and the receiver we want to pay to, is not the unusable's receiver - fail goal.
			// Only ISSObjectUsableUnusable.paymentReceiver is allowed to receive payments when the ISSObjectUsableUnusable is not usable.
			if( this.destination is ISSObjectUsableUnusable )
			{
				ISSObjectUsableUnusable usableUnusable = (ISSObjectUsableUnusable)this.destination;
				if( !usableUnusable.isUsable )
				{
					if( this.destinationPaymentReceiver != usableUnusable.paymentReceiver )
					{
						return false;
					}
				}
			}

			Dictionary<string, int> resourcesCarried = this.inventory.GetAll();

			bool didClamp; // if true, the resources were clamped (not all dropped).
			Dictionary<string, int> resourcesToDrop = GetClampedRes( resourcesCarried, this.resources, out didClamp );

			Dictionary<string, int> resourcesWanted = this.destinationPaymentReceiver.GetWantedResources();

			bool failed = false;
			foreach( var kvp in resourcesToDrop )
			{
				int amountWanted = 0;
				if( !resourcesWanted.TryGetValue( kvp.Key, out amountWanted ) )
				{
					failed = true; // didn't want the resource we assigned to it.
					continue;
				}

				int amountPaid = amountWanted > kvp.Value ? kvp.Value : amountWanted;

				if( amountPaid == 0 )
				{
					continue;
				}

				// Only pay the amount it wants. Otherwise it will scream at you.
				this.destinationPaymentReceiver.ReceivePayment( kvp.Key, amountPaid );
				this.inventory.Remove( kvp.Key, amountPaid );

				ResourceDefinition def = DefinitionManager.GetResource( kvp.Key );
				AudioManager.PlaySound( def.dropoffSound, controller.transform.position );
			}
			if( didClamp ) // if not every resource was delivered - fail.
			{
				return false;
			}
			return !failed;
		}



		private float delayTimeStamp;

		public override void Update( TacticalGoalController controller )
		{
			if( this.destination == null ) // if the object was killed, fail goal.
			{
				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				return;
			}

			if( this.inventory.isEmpty )
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


			// Wait for 'INTERACTION_DELAY' seconds since the assignment of the goal before proceeding.
			// Prevents spam-firing the pickup-dropoff goal pairs when the unit is in range of both the receiver & source (storage).
			if( Time.time < this.delayTimeStamp )
			{
				return;
			}

			if( this.dropOffMode == DropOffMode.POSITION )
			{
				// Don't drop off resources when too far away.
				if( !DistanceUtils.IsInRange( controller.transform.position, this.destinationPos, INTERACTION_DISTANCE ) )
				{
					controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
					return;
				}

				bool outcome = this.OnArrivalPosition( controller );

				controller.ExitCurrent( outcome ? TacticalGoalExitCondition.SUCCESS : TacticalGoalExitCondition.FAILURE );
				return;
			}
			if( this.dropOffMode == DropOffMode.INVENTORY )
			{
				// Don't drop off resources when too far away.
				if( !DistanceUtils.IsInRangePhysical( controller.transform, this.destinationInventory.transform, INTERACTION_DISTANCE ) )
				{
					controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
					return;
				}

				bool outcome = this.OnArrivalInventory( controller );

				controller.ExitCurrent( outcome ? TacticalGoalExitCondition.SUCCESS : TacticalGoalExitCondition.FAILURE );
				return;
			}
			if( this.dropOffMode == DropOffMode.PAYMENT_RECEIVER )
			{
				// Don't drop off resources when too far away.
				if( !DistanceUtils.IsInRangePhysical( controller.transform, this.destination.transform, INTERACTION_DISTANCE ) )
				{
					controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
					return;
				}

				bool outcome = this.OnArrivalPayment( controller );

				controller.ExitCurrent( outcome ? TacticalGoalExitCondition.SUCCESS : TacticalGoalExitCondition.FAILURE );
				return;
			}
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override TacticalGoalData GetData()
		{
			TacticalDropOffGoalData data = new TacticalDropOffGoalData()
			{
				isHostile = this.isHostile
			};
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
			data.dropOffMode = this.dropOffMode;
			if( this.dropOffMode == DropOffMode.POSITION )
			{
				data.destinationPos = this.destinationPos;
			}
			else if( this.dropOffMode == DropOffMode.INVENTORY )
			{
				data.destinationGuid = new Tuple<Guid, Guid?>( this.destinationInventory.ssObject.guid, this.destinationInventory.moduleId );
			}
			else if( this.dropOffMode == DropOffMode.PAYMENT_RECEIVER )
			{
				Guid obj = this.destinationPaymentReceiver.ssObject.guid;
				Guid? module = null;
				if( this.destinationPaymentReceiver is SSModule )
				{
					module = ((SSModule)this.destinationPaymentReceiver).moduleId;
				}
				data.destinationGuid = new Tuple<Guid, Guid?>( obj, module );
			}

			return data;
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalDropOffGoalData data = (TacticalDropOffGoalData)_data;

			if( data.resources != null )
			{
				this.resources = new Dictionary<string, int>();
				this.resourcesRemaining = new Dictionary<string, int>();
				foreach( var kvp in data.resources )
				{
					this.resources.Add( kvp.Key, kvp.Value.Item1 );
					this.resourcesRemaining.Add( kvp.Key, kvp.Value.Item2 );
				}
			}

			this.dropOffMode = data.dropOffMode;
			if( this.dropOffMode == DropOffMode.POSITION )
			{
				this.SetDestination( data.destinationPos );
			}
			else if( this.dropOffMode == DropOffMode.INVENTORY )
			{
				// Don't set this to null for inventories. Since an inventory can't be an object (must be a module).
				this.SetDestination( SSObject.Find( data.destinationGuid.Item1 ).GetModule<InventoryModule>( data.destinationGuid.Item2.Value ) );
			}
			else if( this.dropOffMode == DropOffMode.PAYMENT_RECEIVER )
			{
#warning What if specific extensions of objects can receive payments?
				if( data.destinationGuid.Item2 == null )
				{
					ISSObjectUsableUnusable usableUnusable = (ISSObjectUsableUnusable)SSObject.Find( data.destinationGuid.Item1 );
					//Debug.Log( usableUnusable.GetType() );
					//Debug.Log( usableUnusable.paymentReceiver.GetType() );
#warning Construction site was destroyed, but the reference to it is an interface, so it persisted. IPaymentReceiver remained not-null.
#warning  Destination wasn't set to null either, since the object itself is alive and well, it's just the construction site that's dead.
					//if( usableUnusable.paymentReceiver != null )
					//{
					this.SetDestination( usableUnusable.paymentReceiver );
					//}
				}
				else
				{
					this.SetDestination( (IPaymentReceiver)SSObject.Find( data.destinationGuid.Item1 ).GetModule( data.destinationGuid.Item2.Value ) );
				}
			}

			this.isHostile = data.isHostile;
		}
	}
}