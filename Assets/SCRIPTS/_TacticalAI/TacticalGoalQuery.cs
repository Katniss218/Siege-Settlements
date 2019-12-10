using SS.AI.Goals;
using SS.Content;
using SS.Diplomacy;
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

			SSObject hitInventorySSObject = null;
			InventoryModule hitInventory = null;
			FactionMember hitInventoryFactionMember = null;

			SSObject hitDepositSSObject = null;
			ResourceDepositModule hitDeposit = null;

			Transform hitReceiverTransform = null;
			IPaymentReceiver[] hitPaymentReceivers = null;
			FactionMember hitReceiverFactionMember = null;

			Damageable hitDamageable = null;

			for( int i = 0; i < raycastHits.Length; i++ )
			{
				if( raycastHits[i].collider.gameObject.layer == ObjectLayer.TERRAIN )
				{
					terrainHitPos = raycastHits[i].point;
				}
				else
				{
					FactionMember factionMember = raycastHits[i].collider.GetComponent<FactionMember>();

					ResourceDepositModule deposit = raycastHits[i].collider.GetComponent<ResourceDepositModule>();
					if( deposit != null && hitDepositSSObject == null )
					{
						hitDepositSSObject = raycastHits[i].collider.GetComponent<SSObject>();
						hitDeposit = deposit;
					}

					InventoryModule inventory = raycastHits[i].collider.GetComponent<InventoryModule>();
					if( inventory != null && hitInventorySSObject == null )
					{
						hitInventorySSObject = raycastHits[i].collider.GetComponent<SSObject>();
						hitInventory = inventory;
						hitInventoryFactionMember = factionMember;
					}

					IPaymentReceiver[] receivers = raycastHits[i].collider.GetComponents<IPaymentReceiver>();
					if( receivers.Length > 0 && hitReceiverTransform == null )
					{
						hitReceiverTransform = raycastHits[i].collider.transform;
						hitPaymentReceivers = receivers;
						hitReceiverFactionMember = factionMember;
					}

					Damageable damageable = raycastHits[i].collider.GetComponent<Damageable>();
					if( damageable != null && hitDamageable == null )
					{
						hitDamageable = damageable;
					}
				}
			}

			if( hitDeposit == null && hitInventory == null && hitReceiverTransform == null && hitDamageable == null && terrainHitPos.HasValue )
			{
				AssignMoveToGoal( terrainHitPos.Value, Selection.selectedObjects );
			}

			else if( hitReceiverTransform != null && (hitReceiverFactionMember == null || hitReceiverFactionMember.factionId == LevelDataManager.PLAYER_FAC) )
			{
				AssignMakePaymentGoal( hitReceiverTransform, hitPaymentReceivers, Selection.selectedObjects );
			}
			else if( hitDeposit != null )
			{
				AssignPickupDepositGoal( hitDepositSSObject, hitDeposit, Selection.selectedObjects );
			}
			else if( hitInventory != null && (hitInventoryFactionMember == null || hitInventoryFactionMember.factionId == LevelDataManager.PLAYER_FAC) )
			{
				AssignPickupInventoryGoal( hitInventorySSObject, hitInventory, Selection.selectedObjects );
			}
			else if( hitDamageable != null )
			{
				AssignAttackGoal( hitDamageable, Selection.selectedObjects );
			}
		}
		



		//
		//
		//


		private static void AssignAttackGoal( Damageable target, SSObjectSelectable[] selected )
		{
			List<SSObjectSelectable> filteredObjects = new List<SSObjectSelectable>();

			FactionMember tarFac = target.GetComponent<FactionMember>();

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			for( int i = 0; i < selected.Length; i++ )
			{
				if( !Main.IsControllableByFaction( selected[i], LevelDataManager.PLAYER_FAC ) )
				{
					continue;
				}
				IAttackModule[] targeters = selected[i].GetComponents<IAttackModule>();
				if( targeters == null || targeters.Length == 0 )
				{
					continue;
				}

				FactionMember selFac = selected[i].GetComponent<FactionMember>();

				bool canTarget = false;
				for( int j = 0; j < targeters.Length; j++ )
				{
					if( selFac.CanTargetAnother( tarFac ) )
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
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ) );
			}
			for( int i = 0; i < filteredObjects.Count; i++ )
			{
				TacticalGoalController goalController = filteredObjects[i].GetComponent<TacticalGoalController>();
				TacticalTargetGoal goal = new TacticalTargetGoal();
				goal.target = target;
				goal.targetForced = true;
				goalController.goal = goal;
			}
		}

		internal static void AssignDropoffToInventoryGoal( RaycastHit hitInfo, InventoryModule hitInventory, SSObjectSelectable[] selected )
		{
			List<GameObject> movableWithInvGameObjects = new List<GameObject>();

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			for( int i = 0; i < selected.Length; i++ )
			{
				bool suitable = true;

				if( !Main.IsControllableByFaction( selected[i], LevelDataManager.PLAYER_FAC ) )
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

			if( movableWithInvGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ) );
			}
			for( int i = 0; i < movableWithInvGameObjects.Count; i++ )
			{
				TacticalGoalController goalController = movableWithInvGameObjects[i].GetComponent<TacticalGoalController>();
				TacticalDropOffGoal goal = new TacticalDropOffGoal();
				goal.isHostile = false;
				goal.objectDropOffMode = TacticalDropOffGoal.ObjectDropOffMode.INVENTORY;
				goal.SetDestination( hitInfo.collider.GetComponent<SSObject>() );
				goalController.goal = goal;
			}
		}

		private static void AssignMoveToGoal( Vector3 terrainHitPos, SSObjectSelectable[] selected )
		{
			const float GRID_MARGIN = 0.125f;

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<SSObject> movableGameObjects = new List<SSObject>();

			float biggestRadius = float.MinValue;

			for( int i = 0; i < selected.Length; i++ )
			{
				if( !Main.IsControllableByFaction( selected[i], LevelDataManager.PLAYER_FAC ) )
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






			TacticalMoveToGoal.MovementGridInfo gridInfo = TacticalMoveToGoal.GetGridPositions( movableGameObjects );


			if( gridInfo.positions.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ) );
			}
			foreach( var kvp in gridInfo.positions )
			{
				Vector3 gridPositionWorld = TacticalMoveToGoal.GridToWorld( kvp.Value, gridInfo.sizeX, gridInfo.sizeZ, terrainHitPos, biggestRadius * 2 + GRID_MARGIN );

				RaycastHit gridHit;
				Ray r = new Ray( gridPositionWorld + new Vector3( 0.0f, 50.0f, 0.0f ), Vector3.down );
				if( Physics.Raycast( r, out gridHit, 100.0f, ObjectLayer.TERRAIN_MASK ) )
				{
					TacticalGoalController goalController = kvp.Key.GetComponent<TacticalGoalController>();
					TacticalMoveToGoal goal = new TacticalMoveToGoal();
					goal.isHostile = false;
					goal.SetDestination( gridPositionWorld );
					goalController.goal = goal;
				}
				else
				{
					Debug.LogWarning( "Movement Grid position " + gridPositionWorld + " was outside of the map." );
				}
			}
		}

		private static void AssignPickupInventoryGoal( SSObject hitSSObject, InventoryModule hitInventory, SSObjectSelectable[] selected )
		{
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<GameObject> movableWithInvGameObjects = new List<GameObject>();

			// Go pick up if the inventory can hold any of the resources in the deposit.
			Dictionary<string, int> resourcesInDeposit = hitInventory.GetAll();

			for( int i = 0; i < selected.Length; i++ )
			{
				if( !Main.IsControllableByFaction( selected[i], LevelDataManager.PLAYER_FAC ) )
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
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ) );
			}
			for( int i = 0; i < movableWithInvGameObjects.Count; i++ )
			{
				TacticalGoalController goalController = movableWithInvGameObjects[i].GetComponent<TacticalGoalController>();
				TacticalPickUpGoal goal = new TacticalPickUpGoal();
				goal.isHostile = false;
				goal.destinationObject = hitSSObject;
				goalController.goal = goal;
			}
		}

		private static void AssignPickupDepositGoal( SSObject hitSSObject, ResourceDepositModule hitDeposit, SSObjectSelectable[] selected )
		{
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<GameObject> movableWithInvGameObjects = new List<GameObject>();

			// Go pick up if the inventory can hold any of the resources in the deposit.
			Dictionary<string, int> resourcesInDeposit = hitDeposit.GetAll();

			for( int i = 0; i < selected.Length; i++ )
			{
				if( !Main.IsControllableByFaction( selected[i], LevelDataManager.PLAYER_FAC ) )
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
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ) );
			}
			for( int i = 0; i < movableWithInvGameObjects.Count; i++ )
			{
				TacticalGoalController goalController = movableWithInvGameObjects[i].GetComponent<TacticalGoalController>();
				TacticalPickUpGoal goal = new TacticalPickUpGoal();
				goal.isHostile = false;
				goal.destinationObject = hitSSObject;
				goalController.goal = goal;
			}
		}

		private static void AssignMakePaymentGoal( Transform paymentReceiverTransform, IPaymentReceiver[] paymentReceivers, SSObjectSelectable[] selected )
		{
			FactionMember recFactionMember = paymentReceiverTransform.GetComponent<FactionMember>();
			if( recFactionMember != null )
			{
				// Don't assign make payment to non player.
				if( recFactionMember.factionId != LevelDataManager.PLAYER_FAC )
				{
					return;
				}
			}


			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<GameObject> toBeAssignedGameObjects = new List<GameObject>();

			for( int i = 0; i < selected.Length; i++ )
			{
				if( !Main.IsControllableByFaction( selected[i], LevelDataManager.PLAYER_FAC ) )
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
						Debug.Log( paymentReceiverTransform.gameObject.name + " wants: " + kvp.Value + "x " + kvp.Key );
						if( inv.Get( kvp.Key ) > 0 )
						{
							hasWantedItem_s = true;
							break;
						}
					}

					if( hasWantedItem_s )
					{
						toBeAssignedGameObjects.Add( selected[i].gameObject );
						break;
					}
					// if this receiver is not compatible - check the next one.
				}
			}


			if( toBeAssignedGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ) );
			}
			for( int i = 0; i < toBeAssignedGameObjects.Count; i++ )
			{
				TacticalGoalController goalController = toBeAssignedGameObjects[i].GetComponent<TacticalGoalController>();
				TacticalDropOffGoal goal = new TacticalDropOffGoal();
				goal.isHostile = false;
				goal.objectDropOffMode = TacticalDropOffGoal.ObjectDropOffMode.PAYMENT;
				goal.SetDestination( paymentReceiverTransform.GetComponent<SSObject>() );
				goalController.goal = goal;
			}
		}
	}
}