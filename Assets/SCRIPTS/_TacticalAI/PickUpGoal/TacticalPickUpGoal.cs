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

		
		private bool IsOnValidObject( SSObject ssObject )
		{
			return ssObject is INavMeshAgent && ssObject.GetModules<InventoryModule>().Length > 0;
		}

		public override void Start( TacticalGoalController controller )
		{
			if( !IsOnValidObject( controller.ssObject ) )
			{
				throw new System.Exception( this.GetType().Name + "Was added to an invalid object " + controller.ssObject.GetType().Name );
			}
			this.inventory = controller.ssObject.GetModules<InventoryModule>()[0];
			this.navMeshAgent = (controller.ssObject as INavMeshAgent).navMeshAgent;
			this.attackModules = controller.GetComponents<IAttackModule>();
		}


		private void UpdatePosition( TacticalGoalController controller )
		{
			Vector3 currDestPos = this.destinationObject.transform.position;
			if( this.oldDestination != currDestPos )
			{
				this.navMeshAgent.SetDestination( currDestPos );
			}
			
			// If the agent has travelled to the destination - reset the path (but keep the goal)
			if( this.navMeshAgent.hasPath )
			{
				if( Vector3.Distance( this.navMeshAgent.pathEndPosition, controller.transform.position ) <= Main.DEFAULT_NAVMESH_STOPPING_DIST_CUSTOM )
				{
					this.navMeshAgent.ResetPath();
				}
			}

			this.oldDestination = currDestPos;
		}

		private void UpdateTargeting( TacticalGoalController controller )
		{
			if( this.isHostile )
			{
				IFactionMember fac = controller.GetComponent<IFactionMember>();
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( !Targeter.CanTarget( controller.transform.position, this.attackModules[i].targeter.searchRange, this.attackModules[i].targeter.target, fac.factionMember ) )
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
							this.attackModules[i].targeter.TrySetTarget( controller.transform.position, Targeter.TargetingMode.CLOSEST );
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

		private void OnArrivalDeposit( TacticalGoalController controller, ResourceDepositModule depositToPickUp )
		{
			string idPickedUp = "";
			int amountPickedUp = 0;

			this.amountCollectedDeposit += ResourceDepositModule.MINING_SPEED * Time.deltaTime;
			int amountCollectedFloored = Mathf.FloorToInt( amountCollectedDeposit );
			if( amountCollectedFloored >= 1 )
			{
				Dictionary<string, int> resourcesInDeposit = depositToPickUp.GetAll();

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
						amountPickedUp = this.inventory.Add( kvp.Key, amountCollectedFloored );
						idPickedUp = kvp.Key;
						this.amountCollectedDeposit -= amountCollectedFloored;

						if( amountPickedUp > 0 )
						{
							depositToPickUp.Remove( idPickedUp, amountPickedUp );
							AudioManager.PlaySound( depositToPickUp.miningSound );
						}
						break; // Only pick up one resource at a time.
					}
				}
			}

#warning TODO! - Make sure to disable pickup goal after inventory is full (can no longer pick up).
			/*if( amountPickedUp == 0 ) doesn't work
			{
				controller.goal = TacticalGoalController.GetDefaultGoal();
			}*/
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

					Debug.Log( amountPickedUp + "x " + idPickedUp );
					if( amountPickedUp > 0 )
					{
						int amtRemoved = inventoryToPickUp.Remove( idPickedUp, amountPickedUp );
						Debug.Log( "Taken: " + amtRemoved + "x " + idPickedUp );
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
				destinationObjectGuid = this.destinationObject.guid.Value,
				isHostile = this.isHostile
			};
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalPickUpGoalData data = (TacticalPickUpGoalData)_data;

			this.resourceId = data.resourceId;
			this.destinationObject = Main.GetSSObject( data.destinationObjectGuid );
			this.isHostile = data.isHostile;
		}
	}
}