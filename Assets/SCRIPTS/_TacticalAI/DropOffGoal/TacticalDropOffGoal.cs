using Katniss.Utils;
using SS.Content;
using SS.Levels.SaveStates;
using SS.Objects;
using SS.Objects.Buildings;
using SS.Objects.Extras;
using SS.Objects.Modules;
using SS.Objects.Units;
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

		private static float INTERACTION_DISTANCE = 0.75f;


		/// <summary>
		/// Specifies where to drop off resources.
		/// </summary>
		public DropOffMode dropOffMode { get; private set; }
		public Vector3 destinationPos { get; private set; }
		public InventoryModule destinationInventory { get; private set; }

		public IPaymentReceiver destinationPaymentReceiver { get; private set; }
#warning this needs to be identifyable by GUID or something (construction sites pose MAJOR problem, as they can't be identified using anything else than memory reference - doesn't work for saving)
#warning also, what if I decide that buildings/units themselves can receive payments? (technically, CS's are part of buildings).
		private MonoBehaviour destinationPaymentReceiverBeh;


#warning this won't account for adding via Dictionary.Add(...). Need a custom method for setting the resources.
		Dictionary<string, int> __resources = null;
		/// <summary>
		/// Specified which resources to pick up (set to null to take any and all resources).
		/// </summary>
		public Dictionary<string, int> resources
		{
			get
			{
				return this.__resources;
			}
			set
			{
				this.__resources = value;
				this.resourcesRemaining = value;
			}
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
			this.destinationInventory = inventory;
			this.destinationPaymentReceiver = null;
		}

		public void SetDestination( IPaymentReceiver paymentReceiver )
		{
			this.dropOffMode = DropOffMode.PAYMENT_RECEIVER;
			this.destinationInventory = null;
			this.destinationPaymentReceiver = paymentReceiver;
			this.destinationPaymentReceiverBeh = (MonoBehaviour)paymentReceiver;
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


					GameObject extra = ExtraCreator.Create( def, data.guid );
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
			if( Physics.Raycast( this.destinationPos + new Vector3( 0, 100.0f, 0 ), Vector3.down, out RaycastHit hitInfo ) )
			{
				if( hitInfo.collider.gameObject.layer == ObjectLayer.TERRAIN )
				{
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
			Dictionary<string, int> resourcesCarried = this.inventory.GetAll();

			bool didClamp; // if true, the resources were clamped (not all dropped).
			Dictionary<string,int> resourcesToDrop = GetClampedRes( resourcesCarried, this.resources, out didClamp );

			bool failed = false;
			foreach( var kvp in resourcesToDrop )
			{
				int amountDroppedOff = this.destinationInventory.Add( kvp.Key, kvp.Value );
				if( amountDroppedOff < kvp.Value )
				{
					failed = true;
				}

				this.inventory.Remove( kvp.Key, amountDroppedOff );

				ResourceDefinition def = DefinitionManager.GetResource( kvp.Key );
				AudioManager.PlaySound( def.dropoffSound, controller.transform.position );
			}
			if( didClamp )
			{
				return false;
			}
			return !failed;
		}

		private bool OnArrivalPayment( TacticalGoalController controller )
		{
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

				// Only pay the amount it wants. Otherwise it will scream at you.
				this.destinationPaymentReceiver.ReceivePayment( kvp.Key, amountPaid );
				this.inventory.Remove( kvp.Key, amountPaid );

				ResourceDefinition def = DefinitionManager.GetResource( kvp.Key );
				AudioManager.PlaySound( def.dropoffSound, controller.transform.position );
			}
			if( didClamp )
			{
				return false;
			}
			return !failed;
		}

		



		public override void Update( TacticalGoalController controller )
		{
			if( (this.dropOffMode == DropOffMode.INVENTORY) && (this.destinationInventory == null) )
			{
				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				return;
			}
			if( (this.dropOffMode == DropOffMode.PAYMENT_RECEIVER) && (this.destinationPaymentReceiver == null) )
			{
				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				return;
			}

			if( controller.ssObject is IUsableSSObject && !(controller.ssObject as IUsableSSObject).IsUsable() )
			{
				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				return;
			}

			if( this.inventory.isEmpty )
			{
				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				return;
			}

			if( attackModules.Length > 0 )
			{
				this.UpdateTargeting( controller, this.isHostile, this.attackModules );
			}

			if( this.dropOffMode == DropOffMode.POSITION )
			{
				if( Vector3.Distance( controller.transform.position, this.destinationPos ) <= INTERACTION_DISTANCE )
				{
					bool outcome = this.OnArrivalPosition( controller );

					controller.ExitCurrent( outcome ? TacticalGoalExitCondition.SUCCESS : TacticalGoalExitCondition.FAILURE );
					return;
				}

				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				return;
			}
			if( this.dropOffMode == DropOffMode.INVENTORY )
			{
				if( PhysicsDistance.OverlapInRange( controller.transform, this.destinationInventory.transform, INTERACTION_DISTANCE ) )
				{
					bool outcome = this.OnArrivalInventory( controller );

					controller.ExitCurrent( outcome ? TacticalGoalExitCondition.SUCCESS : TacticalGoalExitCondition.FAILURE );
					return;
				}

				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				return;
			}
			if( this.dropOffMode == DropOffMode.PAYMENT_RECEIVER )
			{
				if( PhysicsDistance.OverlapInRange( controller.transform, this.destinationPaymentReceiverBeh.transform, INTERACTION_DISTANCE ) )
				{
					bool outcome = this.OnArrivalPayment( controller );

					controller.ExitCurrent( outcome ? TacticalGoalExitCondition.SUCCESS : TacticalGoalExitCondition.FAILURE );
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
			TacticalDropOffGoalData data = new TacticalDropOffGoalData()
			{
				isHostile = this.isHostile
			};
			data.dropOffMode = this.dropOffMode;
			if( this.dropOffMode == DropOffMode.POSITION )
			{
				data.destinationPos = this.destinationPos;
			}
			else if( this.dropOffMode == DropOffMode.INVENTORY )
			{
				data.destinationGuid = new Tuple<Guid, Guid>( this.destinationInventory.ssObject.guid, this.destinationInventory.moduleId );
			}
			else if( this.dropOffMode == DropOffMode.PAYMENT_RECEIVER )
			{
#warning need a way to perststently save payment receivers (either object or module).
				data.destinationGuid = new Tuple<Guid, Guid>( this.destinationPaymentReceiver.ssObject.guid, this.destinationPaymentReceiver.moduleId );
				data.destinationPos = this.destinationPos;
			}

			return data;
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalDropOffGoalData data = (TacticalDropOffGoalData)_data;

			this.resources = new Dictionary<string, int>();
			this.resourcesRemaining = new Dictionary<string, int>();
			foreach( var kvp in data.resources )
			{
				this.resources.Add( kvp.Key, kvp.Value.Item1 );
				this.resourcesRemaining.Add( kvp.Key, kvp.Value.Item2 );
			}

			this.dropOffMode = data.dropOffMode;
			if( this.dropOffMode == DropOffMode.POSITION )
			{
				this.SetDestination( data.destinationPos );
			}
			else if( this.dropOffMode == DropOffMode.INVENTORY )
			{
				this.SetDestination( SSObject.Find( data.destinationGuid.Item1 ).GetModule<InventoryModule>( data.destinationGuid.Item2 ) );
			}
			else if( this.dropOffMode == DropOffMode.PAYMENT_RECEIVER )
			{
#warning need a way to perststently save payment receivers (either object or module).
				this.SetDestination( SSObject.Find( data.destinationGuid.Item1 ).GetModule<IPaymentReceiver>( data.destinationGuid.Item2 ) );
			}
			
			this.isHostile = data.isHostile;
		}
	}
}