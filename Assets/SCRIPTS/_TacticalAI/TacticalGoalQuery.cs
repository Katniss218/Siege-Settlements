using SS.AI.Goals;
using SS.Content;
using SS.Levels;
using SS.Objects;
using SS.Objects.Modules;
using SS.ResourceSystem.Payment;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace SS.AI
{
	public static class TacticalGoalQuery
	{
		public static void InputQuery( Ray viewRay )
		{
			if( EventSystem.current.IsPointerOverGameObject() )
			{
				return;
			}

			RaycastHit[] raycastHits = Physics.RaycastAll( viewRay, float.MaxValue, ObjectLayer.ALL_MASK );

			Vector3? terrainHitPos = null;

			SSObjectDFS hitInventorySSObject = null;
			InventoryModule hitInventory = null;

			SSObject hitDepositSSObject = null;
			ResourceDepositModule hitDeposit = null;

			SSObjectDFS hitReceiverSSObject = null;
			IPaymentReceiver[] hitPaymentReceivers = null;

			SSObjectDFS hitDamageable = null;

			SSObjectDFS hitInteriorDFS = null;
			InteriorModule hitInterior = null;

			for( int i = 0; i < raycastHits.Length; i++ )
			{
				if( raycastHits[i].collider.gameObject.layer == ObjectLayer.TERRAIN )
				{
					terrainHitPos = raycastHits[i].point;
				}
				else
				{
					ResourceDepositModule deposit = raycastHits[i].collider.GetComponent<ResourceDepositModule>();
					if( deposit != null && hitDepositSSObject == null )
					{
						hitDepositSSObject = raycastHits[i].collider.GetComponent<SSObject>();
						hitDeposit = deposit;
					}

					InventoryModule inventory = raycastHits[i].collider.GetComponent<InventoryModule>();
					if( inventory != null && hitInventorySSObject == null )
					{
						hitInventorySSObject = raycastHits[i].collider.GetComponent<SSObjectDFS>();
						hitInventory = inventory;
					}

					IPaymentReceiver[] receivers = raycastHits[i].collider.GetComponents<IPaymentReceiver>();
					if( receivers.Length > 0 && hitReceiverSSObject == null )
					{
						hitReceiverSSObject = raycastHits[i].collider.GetComponent<SSObjectDFS>();
						hitPaymentReceivers = receivers;
					}

					SSObjectDFS damageable = raycastHits[i].collider.GetComponent<SSObjectDFS>();
					if( damageable != null && hitDamageable == null )
					{
						hitDamageable = damageable;
					}

					InteriorModule interior = raycastHits[i].collider.GetComponent<InteriorModule>();
					if( interior != null && hitInterior == null )
					{
						hitInteriorDFS = interior.ssObject as SSObjectDFS;
						hitInterior = interior;
					}
				}
			}

			if( hitDeposit == null && hitInventory == null && hitReceiverSSObject == null && hitDamageable == null && terrainHitPos.HasValue )
			{
				AssignMoveToGoal( terrainHitPos.Value, Selection.GetSelectedObjects() );
				return;
			}

			if( hitReceiverSSObject != null && (hitReceiverSSObject.factionId == LevelDataManager.PLAYER_FAC) )
			{
				AssignMakePaymentGoal( hitReceiverSSObject, hitPaymentReceivers, Selection.GetSelectedObjects() );
				return;
			}
			if( hitInterior != null && (hitInteriorDFS.factionId == LevelDataManager.PLAYER_FAC) )
			{
				AssignMoveToInteriorOrObjGoal( null, hitInterior, Selection.GetSelectedObjects() );
				return;
			}
			if( hitDeposit != null )
			{
				AssignPickupDepositGoal( hitDepositSSObject, hitDeposit, Selection.GetSelectedObjects() );
				return;
			}
			if( hitInventory != null && !hitInventory.isEmpty && (hitInventorySSObject == null || hitInventorySSObject.factionId == LevelDataManager.PLAYER_FAC) )
			{
				AssignPickupInventoryGoal( hitInventorySSObject, hitInventory, Selection.GetSelectedObjects() );
				return;
			}
			if( hitDamageable != null && (hitDamageable.factionId != LevelDataManager.PLAYER_FAC) )
			{
				AssignAttackGoal( hitDamageable, Selection.GetSelectedObjects() );
				return;
			}
			if( hitDamageable != null && (hitDamageable.factionId == LevelDataManager.PLAYER_FAC) )
			{
				AssignMoveToInteriorOrObjGoal( hitDamageable, null, Selection.GetSelectedObjects() );
				return;
			}
		}
		



		//
		//
		//


		private static void AssignAttackGoal( SSObjectDFS target, SSObjectDFS[] selected )
		{
			List<SSObjectDFS> filteredObjects = new List<SSObjectDFS>();
			
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			for( int i = 0; i < selected.Length; i++ )
			{
				if( selected[i].factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}
				IAttackModule[] targeters = selected[i].GetComponents<IAttackModule>();
				if( targeters == null || targeters.Length == 0 )
				{
					continue;
				}
				
				bool canTarget = false;
				for( int j = 0; j < targeters.Length; j++ )
				{
					if( target.CanTargetAnother( selected[i] ) )
					{
						canTarget = true;
						break;
					}
				}

				if( canTarget )
				{
					filteredObjects.Add( selected[i] );
				}
			}

			if( filteredObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
			}
			for( int i = 0; i < filteredObjects.Count; i++ )
			{
				TacticalGoalController goalController = filteredObjects[i].GetComponent<TacticalGoalController>();
				TacticalTargetGoal goal = new TacticalTargetGoal();
				goal.target = target;
				goal.targetForced = true;
				goalController.SetGoals( goal );
			}
		}

		internal static void AssignDropoffToInventoryGoal( RaycastHit hitInfo, InventoryModule hitInventory, SSObjectDFS[] selected )
		{
			List<GameObject> movableWithInvGameObjects = new List<GameObject>();
			
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			for( int i = 0; i < selected.Length; i++ )
			{
				bool suitable = true;

				if( selected[i].factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}
				InventoryModule inv = selected[i].GetComponent<InventoryModule>();
				if( inv == null )
				{
					continue;
				}
				if( inv.isEmpty )
				{
					continue;
				}
				if( selected[i].GetComponent<NavMeshAgent>() == null )
				{
					continue;
				}

				Dictionary<string, int> inventoryItems = inv.GetAll();

				foreach( var kvp in inventoryItems )
				{
					if( hitInventory.GetSpaceLeft( kvp.Key ) == 0 )
					{
						suitable = false;
						break;
					}
					// don't move if the deposit doesn't have space to leave resource (inv full of that specific resource).
					if( hitInventory.GetSpaceLeft( kvp.Key ) == hitInventory.Get( kvp.Key ) )
					{
						suitable = false;
						break;
					}
				}

				if( suitable )
				{
					movableWithInvGameObjects.Add( selected[i].gameObject );
				}
			}

#warning if the unit is an employed civilian or civilian on auto duty, lock the interaction.
			if( movableWithInvGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
			}
			for( int i = 0; i < movableWithInvGameObjects.Count; i++ )
			{
				TacticalGoalController goalController = movableWithInvGameObjects[i].GetComponent<TacticalGoalController>();
				TacticalDropOffGoal goal = new TacticalDropOffGoal();
				goal.isHostile = false;
				goal.SetDestination( hitInventory );
				goalController.SetGoals( goal );
			}
		}
		private static void AssignMoveToGoal( Vector3 terrainHitPos, SSObjectDFS[] selected )
		{
			const float GRID_MARGIN = 0.125f;

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<SSObject> movableGameObjects = new List<SSObject>();

			float biggestRadius = float.MinValue;

			for( int i = 0; i < selected.Length; i++ )
			{
				if( selected[i].factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}
				NavMeshAgent navMeshAgent = selected[i].GetComponent<NavMeshAgent>();
				if( navMeshAgent == null )
				{
					continue;
				}

				// Calculate how big is the biggest unit/hero/etc. to be used when specifying movement grid size.
				movableGameObjects.Add( selected[i] );
				if( navMeshAgent.radius > biggestRadius )
				{
					biggestRadius = navMeshAgent.radius;
				}
			}

			//Calculate the grid position.
			
			MovementGridInfo gridInfo = new MovementGridInfo( movableGameObjects );


			if( gridInfo.positions.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
			}
			Quaternion gridRotation = Quaternion.Euler( 0, Main.cameraPivot.rotation.eulerAngles.y, 0 );
			foreach( var kvp in gridInfo.positions )
			{
				Vector3 gridPositionWorld = gridInfo.GridPosToWorld( kvp.Value, gridRotation, terrainHitPos, biggestRadius * 2 + GRID_MARGIN );

				RaycastHit gridHit;
				Ray r = new Ray( gridPositionWorld + new Vector3( 0.0f, 50.0f, 0.0f ), Vector3.down );
				if( Physics.Raycast( r, out gridHit, 100.0f, ObjectLayer.TERRAIN_MASK ) )
				{
					TacticalGoalController goalController = kvp.Key.GetComponent<TacticalGoalController>();
					TacticalMoveToGoal goal = new TacticalMoveToGoal();
					goal.isHostile = false;
					goal.SetDestination( gridPositionWorld );
					goalController.SetGoals( goal );
				}
				else
				{
					Debug.LogWarning( "Movement Grid position " + gridPositionWorld + " was outside of the map." );
				}
			}
		}

		private static void AssignMoveToInteriorOrObjGoal( SSObject obj, InteriorModule interior, SSObjectDFS[] selected )
		{

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<SSObject> movableGameObjects = new List<SSObject>();

			float biggestRadius = float.MinValue;

			for( int i = 0; i < selected.Length; i++ )
			{
				if( selected[i].factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}
				NavMeshAgent navMeshAgent = selected[i].GetComponent<NavMeshAgent>();
				if( navMeshAgent == null )
				{
					continue;
				}

				// Calculate how big is the biggest unit/hero/etc. to be used when specifying movement grid size.
				movableGameObjects.Add( selected[i] );
				if( navMeshAgent.radius > biggestRadius )
				{
					biggestRadius = navMeshAgent.radius;
				}
			}
			
			if( movableGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
			}
			for( int i = 0; i < movableGameObjects.Count; i++ )
			{
				TacticalGoalController goalController = movableGameObjects[i].GetComponent<TacticalGoalController>();
				TacticalMoveToGoal goal = new TacticalMoveToGoal();
				goal.isHostile = false;
				if( interior == null )
				{
					goal.SetDestination( obj );
				}
				else
				{
					if( movableGameObjects[i].definitionId == "unit.civilian" )
					{
#warning change back after testing.
						goal.SetDestination( interior, InteriorModule.SlotType.Worker );
					}
					else
					{

						goal.SetDestination( interior, InteriorModule.SlotType.Generic );
					}
				}
				goalController.SetGoals( goal );
			}
		}

		private static void AssignPickupInventoryGoal( SSObject hitSSObject, InventoryModule hitInventory, SSObjectDFS[] selected )
		{
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<GameObject> movableWithInvGameObjects = new List<GameObject>();

			// Go pick up if the inventory can hold any of the resources in the deposit.
			Dictionary<string, int> resourcesInDeposit = hitInventory.GetAll();

			for( int i = 0; i < selected.Length; i++ )
			{
				if( selected[i].factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}
				if( selected[i].GetComponent<NavMeshAgent>() == null )
				{
					continue;
				}
				InventoryModule inv = selected[i].GetComponent<InventoryModule>();
				if( inv == null )
				{
					continue;
				}

				foreach( var kvp in resourcesInDeposit )
				{
					// if can pick up && has empty space for it.
					if( inv.GetSpaceLeft( kvp.Key ) > 0 )
					{
						movableWithInvGameObjects.Add( selected[i].gameObject );
						break;
					}
				}
			}


			if( movableWithInvGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
			}
			for( int i = 0; i < movableWithInvGameObjects.Count; i++ )
			{
				TacticalGoalController goalController = movableWithInvGameObjects[i].GetComponent<TacticalGoalController>();
				TacticalPickUpGoal goal = new TacticalPickUpGoal();
				goal.isHostile = false;
				goal.SetDestination( hitInventory );
				goalController.SetGoals( goal );
			}
		}

		private static void AssignPickupDepositGoal( SSObject hitSSObject, ResourceDepositModule hitDeposit, SSObjectDFS[] selected )
		{
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<GameObject> movableWithInvGameObjects = new List<GameObject>();

			// Go pick up if the inventory can hold any of the resources in the deposit.
			Dictionary<string, int> resourcesInDeposit = hitDeposit.GetAll();

			for( int i = 0; i < selected.Length; i++ )
			{
				if( selected[i].factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}
				if( selected[i].GetComponent<NavMeshAgent>() == null )
				{
					continue;
				}
				InventoryModule inv = selected[i].GetComponent<InventoryModule>();
				if( inv == null )
				{
					continue;
				}
				bool canPickupAny = false;
				foreach( var kvp in resourcesInDeposit )
				{
					// if can pick up && has empty space for it.
					if( inv.GetSpaceLeft( kvp.Key ) > 0 )
					{
						canPickupAny = true;
						break;
					}
				}

				if( canPickupAny )
				{
					movableWithInvGameObjects.Add( selected[i].gameObject );
				}
			}


			if( movableWithInvGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
			}
			for( int i = 0; i < movableWithInvGameObjects.Count; i++ )
			{
				TacticalGoalController goalController = movableWithInvGameObjects[i].GetComponent<TacticalGoalController>();
				TacticalPickUpGoal goal = new TacticalPickUpGoal();
				goal.isHostile = false;
				goal.SetDestination( hitDeposit );
				goalController.SetGoals( goal );
			}
		}

		private static void AssignMakePaymentGoal( SSObjectDFS paymentReceiverSSObject, IPaymentReceiver[] paymentReceivers, SSObjectDFS[] selected )
		{
			if( paymentReceiverSSObject != null )
			{
				// Don't assign make payment to non player.
				if( paymentReceiverSSObject.factionId != LevelDataManager.PLAYER_FAC )
				{
					return;
				}
			}


			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<SSObjectDFS> toBeAssignedGameObjects = new List<SSObjectDFS>();

			for( int i = 0; i < selected.Length; i++ )
			{
				if( selected[i].factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}
				if( selected[i].GetComponent<NavMeshAgent>() == null )
				{
					continue;
				}
				InventoryModule inv = selected[i].GetComponent<InventoryModule>();
				if( inv == null )
				{
					continue;
				}
				// If the inventory doesn't have any resources that can be left at the payment receiver.
				if( inv.isEmpty )
				{
					continue;
				}

				// loop over every receiver and check if any of them wants resources that are in the selected obj's inventory.
				for( int j = 0; j < paymentReceivers.Length; j++ )
				{
					Dictionary<string, int> wantedRes = paymentReceivers[j].GetWantedResources();
					bool hasWantedItem_s = false;
					foreach( var kvp in wantedRes )
					{
						//Debug.Log( paymentReceiverSSObject.displayName + " wants: " + kvp.Value + "x " + kvp.Key );
						if( inv.Get( kvp.Key ) > 0 )
						{
							hasWantedItem_s = true;
							break;
						}
					}

					if( hasWantedItem_s )
					{
						toBeAssignedGameObjects.Add( selected[i] );
						break;
					}
					// if this receiver is not compatible - check the next one.
				}
			}


			if( toBeAssignedGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
			}
			for( int i = 0; i < toBeAssignedGameObjects.Count; i++ )
			{
				TacticalGoalController goalController = toBeAssignedGameObjects[i].GetComponent<TacticalGoalController>();
				TacticalDropOffGoal goal = new TacticalDropOffGoal();
				goal.isHostile = false;
#warning flawed way. need to search for the one that best fits.
				goal.SetDestination( paymentReceivers[0] );
				goalController.SetGoals( goal );
			}
		}
	}
}