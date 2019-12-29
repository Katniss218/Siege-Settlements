using Katniss.Utils;
using SS.Content;
using SS.Objects;
using SS.Objects.Modules;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

		/// <summary>
		/// Specified which resource is going to be picked up (null for all).
		/// </summary>
		public string resourceId { get; set; }
		public SSObject destinationObject { get; set; }

		public bool isHostile { get; set; }


		private float amountCollectedDeposit;

		private Vector3 oldDestination;
		private InventoryModule inventory;
		private NavMeshAgent navMeshAgent;
		private IAttackModule[] attackModules;

		public TacticalPickUpGoal()
		{
			this.isHostile = true;
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override bool IsOnValidObject( SSObject ssObject )
		{
			return ssObject is IMovable && ssObject.GetModules<InventoryModule>().Length > 0;
		}

		public override void Start( TacticalGoalController controller )
		{
			this.inventory = controller.ssObject.GetModules<InventoryModule>()[0];
			this.navMeshAgent = (controller.ssObject as IMovable).navMeshAgent;
			this.attackModules = controller.GetComponents<IAttackModule>();
		}


		private void UpdatePosition( TacticalGoalController controller )
		{
			Vector3 currDestPos = this.destinationObject.transform.position;
			if( this.oldDestination != currDestPos )
			{
				this.navMeshAgent.SetDestination( currDestPos );
			}			

			this.oldDestination = currDestPos;
		}

		private void UpdateTargeting( TacticalGoalController controller )
		{
			if( this.isHostile )
			{
				SSObjectDFS ssobj = controller.GetComponent<SSObjectDFS>();
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( !Targeter.CanTarget( controller.transform.position, this.attackModules[i].attackRange, this.attackModules[i].targeter.target, ssobj ) )
					{
						this.attackModules[i].targeter.target = null;
					}
				}

				if( Random.Range( 0, 5 ) == 0 ) // Recalculate target only 20% of the time (not really noticeable, but gives a nice boost to FPS).
				{
					for( int i = 0; i < this.attackModules.Length; i++ )
					{
						if( this.attackModules[i].isReadyToAttack )
						{
							this.attackModules[i].targeter.TrySetTarget( controller.transform.position, this.attackModules[i].attackRange, Targeter.TargetingMode.CLOSEST );
						}
					}
				}
			}
			else
			{
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( this.attackModules[i].targeter.target != null )
					{
						this.attackModules[i].targeter.target = null;
					}
				}
			}
		}

		private void OnArrivalDeposit( TacticalGoalController controller, ResourceDepositModule destinationDeposit )
		{			
			Dictionary<string, int> resourcesInDeposit = destinationDeposit.GetAll();
			foreach( var kvp in resourcesInDeposit )
			{
				if( kvp.Value == 0 )
				{
					continue;
				}
				// If the goal wants a specific resource - disregard any other resources.
				if( !string.IsNullOrEmpty( this.resourceId ) && kvp.Key != this.resourceId )
				{
					continue;
				}
				if( this.inventory.GetSpaceLeft( kvp.Key ) != 0 )
				{
					this.amountCollectedDeposit += ResourceDepositModule.MINING_SPEED * Time.deltaTime;
					int amountCollectedFloored = Mathf.FloorToInt( amountCollectedDeposit );
					if( amountCollectedFloored >= 1 )
					{
						// Get the actual amount that can be picked up & put in the inventory.
						string pickedUpId = kvp.Key;
						int pickedUpAmount = this.inventory.Add( kvp.Key, amountCollectedFloored );

						this.amountCollectedDeposit -= amountCollectedFloored;

						if( pickedUpAmount > 0 )
						{
							destinationDeposit.Remove( pickedUpId, pickedUpAmount );
							AudioManager.PlaySound( destinationDeposit.miningSound );
						}
						break; // Only pick up one resource at a time.
					}
				}
			}
		}

		private void OnArrivalInventory( TacticalGoalController controller, InventoryModule inventoryToPickUp )
		{
			string idPickedUp = "";
			int amountPickedUp = 0;

			Dictionary<string, int> resourcesInInventory = inventoryToPickUp.GetAll();

			foreach( var kvp in resourcesInInventory )
			{
				if( kvp.Value == 0 )
				{
					continue;
				}
				// If the goal wants a specific resource - disregard any other resources.
				if( !string.IsNullOrEmpty( this.resourceId ) && kvp.Key != this.resourceId )
				{
					continue;
				}
				if( this.inventory.GetSpaceLeft( kvp.Key ) != 0 )
				{
					amountPickedUp = this.inventory.Add( kvp.Key, kvp.Value );
					idPickedUp = kvp.Key;

					if( amountPickedUp > 0 )
					{
						int amtRemoved = inventoryToPickUp.Remove( idPickedUp, amountPickedUp );
						AudioManager.PlaySound( DefinitionManager.GetResource( idPickedUp ).pickupSound );
					}
				}
			}

			this.navMeshAgent.ResetPath();
			controller.goal = TacticalGoalController.GetDefaultGoal();
		}

		public override void Update( TacticalGoalController controller )
		{
			// If the object was picked up/destroyed/etc. (is no longer on the map), stop the Goal.
			if( this.destinationObject == null )
			{
				this.navMeshAgent.ResetPath();
				controller.goal = TacticalGoalController.GetDefaultGoal();
				return;
			}

			if( this.destinationObject == controller.ssObject )
			{
				Debug.LogWarning( controller.ssObject.definitionId + ": Destination was set to itself." );
				this.navMeshAgent.ResetPath();
				controller.goal = TacticalGoalController.GetDefaultGoal();
				return;
			}
			// If it's not usable - return, don't move.
			if( controller.ssObject is IUsableToggle && !(controller.ssObject as IUsableToggle).IsUsable() )
			{
				return;
			}

			this.UpdatePosition( controller );
			this.UpdateTargeting( controller );

			if( PhysicsDistance.OverlapInRange( controller.transform, this.destinationObject.transform, 0.75f ) )
			{
				// If the agent has travelled to the destination - reset the path (but keep the goal)
				if( this.navMeshAgent.hasPath )
				{
					this.navMeshAgent.ResetPath();
				}

				ResourceDepositModule[] deposits = this.destinationObject.GetModules<ResourceDepositModule>();
				InventoryModule[] inventories = this.destinationObject.GetModules<InventoryModule>();
				if( deposits.Length > 0 )
				{
					ResourceDepositModule depositToCollect = deposits[0];

					this.OnArrivalDeposit( controller, depositToCollect );
				}
				else
				{
					InventoryModule inventoryToPickUp = inventories[0];

					this.OnArrivalInventory( controller, inventoryToPickUp );
				}
			}
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override TacticalGoalData GetData()
		{
			return new TacticalPickUpGoalData()
			{
				resourceId = this.resourceId,
				destinationObjectGuid = this.destinationObject.guid,
				isHostile = this.isHostile
			};
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalPickUpGoalData data = (TacticalPickUpGoalData)_data;

			this.resourceId = data.resourceId;
			this.destinationObject = SSObject.Find( data.destinationObjectGuid );
			this.isHostile = data.isHostile;
		}
	}
}