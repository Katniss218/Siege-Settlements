using SS.AI.Goals;
using SS.Content;
using SS.Levels;
using SS.Objects;
using SS.Objects.Modules;
using SS.Objects.Units;
using SS.ResourceSystem.Payment;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace SS.AI
{
	public static class TacticalGoalQuery
	{
		/// <summary>
		/// Added to everything user-input-related.
		/// </summary>
		public const int TAG_CUSTOM = -5;


		public static void QueryAt( Ray viewRay )
		{
			if( EventSystem.current.IsPointerOverGameObject() )
			{
				return;
			}
			
			Vector3? terrainHitPos = null;

			SSObject hitObject = null;
			
			if( Physics.Raycast( viewRay, out RaycastHit hitInfo, float.MaxValue, ObjectLayer.ALL_MASK ) )
			{
				if( hitInfo.collider.gameObject.layer == ObjectLayer.TERRAIN )
				{
					terrainHitPos = hitInfo.point;
				}
				else
				{
					SSObject obj = hitInfo.collider.GetComponent<SSObject>();
					if( obj != null && hitObject == null )
					{
						hitObject = obj; // keep looping since the terrain might not been found yet.
					}
				}
			}

			if( hitObject != null )
			{
				SSObjectDFC dfc = (hitObject as SSObjectDFC);

				InventoryModule[] inventories = hitObject.GetModules<InventoryModule>();

				ResourceDepositModule[] resourceDeposits = hitObject.GetModules<ResourceDepositModule>();

				InteriorModule[] interiors = hitObject.GetModules<InteriorModule>();

				WorkplaceModule[] workplaces = hitObject.GetModules<WorkplaceModule>();

				IPaymentReceiver[] paymentReceivers = hitObject.GetAvailablePaymentReceivers();

				
				if( dfc != null && dfc.factionId != LevelDataManager.PLAYER_FAC )
				{
					// the rest stays
					Assign_Attack( dfc, Selection.GetSelectedObjects() );
					return;
				}

				if( paymentReceivers.Length > 0 )
				{
					// the rest stays
					bool flag = Assign_MakePayment( dfc, paymentReceivers, Selection.GetSelectedObjects() );
					if( flag )
					{
						return;
					}
				}
				
				if( resourceDeposits.Length > 0 )
				{
					// the rest stays
					Assign_PickUp( resourceDeposits[0], Selection.GetSelectedObjects() );
					return;
				}

				if( interiors.Length > 0 )
				{
					// the rest stays.
					Assign_MoveToInterior( interiors[0], Selection.GetSelectedObjects() );
					return;
				}

				if( inventories.Length > 0 )
				{
					// the rest stays.
					Assign_DropOff_PickUp( inventories[0], Selection.GetSelectedObjects() );
				}
			}
			else
			{
				if( terrainHitPos.HasValue )
				{
					AssignMoveToGoal( terrainHitPos.Value, Selection.GetSelectedObjects() );
					return;
				}
			}
		}


		//
		//
		//


		private static void Assign_Attack( SSObjectDFC target, SSObject[] selected )
		{
			// the rest stays put.

			List<SSObjectDFC> filteredObjects = new List<SSObjectDFC>();

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			for( int i = 0; i < selected.Length; i++ )
			{
				if( !(selected[i] is SSObjectDFC) )
				{
					continue;
				}

				SSObjectDFC dfc = (SSObjectDFC)selected[i];

				if( dfc.factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}
				IAttackModule[] targeters = dfc.GetComponents<IAttackModule>();
				if( targeters == null || targeters.Length == 0 )
				{
					continue;
				}

				bool canTarget = false;
				for( int j = 0; j < targeters.Length; j++ )
				{
					if( dfc.CanTargetAnother( target ) )
					{
						canTarget = true;
						break;
					}
				}

				if( (selected[i] is Unit) )
				{
					CivilianUnitExtension cue = ((Unit)selected[i]).civilian;
					if( cue != null && cue.isEmployed )
					{
						continue;
					}
				}

				if( canTarget )
				{
					filteredObjects.Add( dfc );
				}
			}

			if( filteredObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
			}
			for( int i = 0; i < filteredObjects.Count; i++ )
			{
				TacticalGoalController goalController = filteredObjects[i].controller;
				TacticalTargetGoal goal = new TacticalTargetGoal();
				goal.target = target;
				goal.targetForced = true;
				goalController.SetGoals( TAG_CUSTOM, goal );
			}
		}

		

		private static void AssignMoveToGoal( Vector3 terrainHitPos, SSObject[] selected )
		{
			// positional (vector3) moveto.

			const float GRID_MARGIN = 0.125f;

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<SSObjectDFC> filteredObjects = new List<SSObjectDFC>();

			float biggestRadius = float.MinValue;

			for( int i = 0; i < selected.Length; i++ )
			{
				if( !(selected[i] is SSObjectDFC) )
				{
					continue;
				}

				SSObjectDFC dfc = (SSObjectDFC)selected[i];

				if( dfc.factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}

				if( !(dfc is IMovable) )
				{
					continue;
				}

				if( (selected[i] is Unit) )
				{
					CivilianUnitExtension cue = ((Unit)selected[i]).civilian;
					if( cue != null && cue.isEmployed )
					{
						continue;
					}
				}

				// Calculate how big is the biggest unit/hero/etc. to be used when specifying movement grid size.
				filteredObjects.Add( dfc );
				IMovable m = (IMovable)selected[i];
				if( m.navMeshAgent.radius > biggestRadius )
				{
					biggestRadius = m.navMeshAgent.radius;
				}
			}

			//Calculate the grid position.

			MovementGridInfo gridInfo = new MovementGridInfo( filteredObjects );
			
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
					TacticalGoalController goalController = kvp.Key.controller;
					TacticalMoveToGoal goal = new TacticalMoveToGoal();
					goal.isHostile = false;
					goal.SetDestination( gridPositionWorld );
					goalController.SetGoals( TAG_CUSTOM, goal );
				}
				else
				{
					Debug.LogWarning( "Movement Grid position " + gridPositionWorld + " was outside of the map." );
				}
			}
		}

		private static void Assign_MoveToInterior( InteriorModule interior, SSObject[] selected )
		{
			if( interior.ssObject is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)interior.ssObject).isUsable )
			{
				return;
			}
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<SSObjectDFC> filteredObjects = new List<SSObjectDFC>();
			
			for( int i = 0; i < selected.Length; i++ )
			{
				if( !(selected[i] is SSObjectDFC) )
				{
					continue;
				}

				SSObjectDFC dfc = (SSObjectDFC)selected[i];

				if( dfc.factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}

				if( !(dfc is IMovable) )
				{
					continue;
				}

				if( (selected[i] is Unit) )
				{
					CivilianUnitExtension cue = ((Unit)selected[i]).civilian;
					if( cue != null && cue.isEmployed )
					{
						continue;
					}
				}

				filteredObjects.Add( dfc );
			}

			if( filteredObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
			}
			for( int i = 0; i < filteredObjects.Count; i++ )
			{
				TacticalGoalController goalController = filteredObjects[i].controller;
				TacticalMoveToGoal goal = new TacticalMoveToGoal();
				goal.SetDestination( interior, InteriorModule.SlotType.Generic );
				goal.isHostile = false;
				goalController.SetGoals( TAG_CUSTOM, goal );
			}
		}

		private static void Assign_PickUp( ResourceDepositModule hitDeposit, SSObject[] selected )
		{
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<SSObjectDFC> movableWithInvGameObjects = new List<SSObjectDFC>();

			// Go pick up if the inventory can hold any of the resources in the deposit.
			Dictionary<string, int> resourcesInDeposit = hitDeposit.GetAll();

			for( int i = 0; i < selected.Length; i++ )
			{
				if( !(selected[i] is SSObjectDFC) )
				{
					continue;
				}

				SSObjectDFC dfc = (SSObjectDFC)selected[i];

				if( dfc.factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}
				if( !(dfc is IMovable) )
				{
					continue;
				}

				if( (selected[i] is Unit) )
				{
					CivilianUnitExtension cue = ((Unit)selected[i]).civilian;
					if( cue != null && cue.isEmployed )
					{
						continue;
					}
				}

				if( !dfc.hasInventoryModule )
				{
					continue;
				}
				InventoryModule inv = dfc.GetModules<InventoryModule>()[0];

				foreach( var kvp in resourcesInDeposit )
				{
					// if can pick up && has empty space for it.
					if( inv.GetSpaceLeft( kvp.Key ) > 0 )
					{
						movableWithInvGameObjects.Add( dfc );
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
				TacticalGoalController goalController = movableWithInvGameObjects[i].controller;
				TacticalMoveToGoal goal1 = new TacticalMoveToGoal();
				goal1.isHostile = false;
				goal1.SetDestination( hitDeposit.ssObject );

				TacticalPickUpGoal goal2 = new TacticalPickUpGoal();
				goal2.isHostile = false;
				goal2.SetDestination( hitDeposit );
				goalController.SetGoals( TacticalGoalQuery.TAG_CUSTOM, goal1, goal2 );
			}
		}
		
		static void Assign_DropOff_PickUp( InventoryModule hitInventory, SSObject[] selected )
		{
			List<SSObjectDFC> filteredPickUp = new List<SSObjectDFC>();
			List<SSObjectDFC> filteredDropOff = new List<SSObjectDFC>();
			
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			for( int i = 0; i < selected.Length; i++ )
			{
				if( !(selected[i] is SSObjectDFC) )
				{
					continue;
				}

				SSObjectDFC dfc = (SSObjectDFC)selected[i];

				if( dfc.factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}
				if( !(dfc is IMovable) )
				{
					continue;
				}

				if( (selected[i] is Unit) )
				{
					CivilianUnitExtension cue = ((Unit)selected[i]).civilian;
					if( cue != null && cue.isEmployed )
					{
						continue;
					}
				}

				if( !dfc.hasInventoryModule )
				{
					continue;
				}
				InventoryModule inv = dfc.GetModules<InventoryModule>()[0];

				if( inv.isEmpty )
				{
					if( !hitInventory.isEmpty )
					{
						filteredPickUp.Add( dfc );
					}
					continue;
				}

				Dictionary<string, int> inventoryItems = inv.GetAll();

				bool canPickUpAny = true;
				// Check if the storage inventory can hold any of the items.
				foreach( var kvp in inventoryItems )
				{
					canPickUpAny = true;
					if( hitInventory.GetSpaceLeft( kvp.Key ) == 0 )
					{
						canPickUpAny = false;
					}
				}

				if( canPickUpAny )
				{
					filteredDropOff.Add( dfc );
				}
				else
				{
					Dictionary<string, int> storageItems = hitInventory.GetAll();
					foreach( var kvp2 in storageItems ) // for every stored resource - check if the dfc can pick up any of them.
					{
						if( inv.GetSpaceLeft( kvp2.Key ) > 0 )
						{
							filteredPickUp.Add( dfc );
							break;
						}
					}
				}
			}

			if( filteredDropOff.Count > 0 || filteredPickUp.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
			}
			for( int i = 0; i < filteredPickUp.Count; i++ )
			{
				TacticalGoalController goalController = filteredPickUp[i].controller;
				TacticalMoveToGoal goal1 = new TacticalMoveToGoal();
				goal1.isHostile = false;
				goal1.SetDestination( hitInventory.ssObject );

				TacticalPickUpGoal goal2 = new TacticalPickUpGoal();
				goal2.isHostile = false;
				goal2.SetDestination( hitInventory );
				goalController.SetGoals( TacticalGoalQuery.TAG_CUSTOM, goal1, goal2 );
			}
			for( int i = 0; i < filteredDropOff.Count; i++ )
			{
				TacticalGoalController goalController = filteredDropOff[i].controller;
				TacticalMoveToGoal goal1 = new TacticalMoveToGoal();
				goal1.isHostile = false;
				goal1.SetDestination( hitInventory.ssObject );

				TacticalDropOffGoal goal2 = new TacticalDropOffGoal();
				goal2.isHostile = false;
				goal2.SetDestination( hitInventory );
				goalController.SetGoals( TacticalGoalQuery.TAG_CUSTOM, goal1, goal2 );
			}
		}

#warning start automatic duty with that object as priority.
		private static bool Assign_MakePayment( SSObjectDFC paymentReceiverSSObject, IPaymentReceiver[] receiversCache, SSObject[] selected )
		{
			// Assigns payment goal to selected objects.
			// Can assign different receivers for different objects, depending on their inventories & wanted resources.

			// the rest stays

			if( paymentReceiverSSObject != null )
			{
				// Don't assign make payment to non player.
				if( paymentReceiverSSObject.factionId != LevelDataManager.PLAYER_FAC )
				{
					return false;
				}
			}


			// Extract only the objects that can have the goal assigned to them from the selected objects.
			Dictionary<SSObjectDFC, IPaymentReceiver> toBeAssignedGameObjects = new Dictionary<SSObjectDFC, IPaymentReceiver>();
			
			for( int i = 0; i < selected.Length; i++ )
			{
				if( !(selected[i] is SSObjectDFC) )
				{
					continue;
				}

				SSObjectDFC dfc = (SSObjectDFC)selected[i];

				if( dfc.factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}
				if( !(dfc is IMovable) )
				{
					continue;
				}

				if( (selected[i] is Unit) )
				{
					CivilianUnitExtension cue = ((Unit)selected[i]).civilian;
					if( cue != null && cue.isEmployed )
					{
						continue;
					}
				}

				if( !dfc.hasInventoryModule )
				{
					continue;
				}
				InventoryModule inv = dfc.GetModules<InventoryModule>()[0];

				// If the inventory doesn't have any resources that can be left at the payment receiver.
				if( inv.isEmpty )
				{
					continue;
				}

				// loop over every receiver and check if any of them wants resources that are in the selected obj's inventory.
				for( int j = 0; j < receiversCache.Length; j++ )
				{
					Dictionary<string, int> wantedRes = receiversCache[j].GetWantedResources();
					bool hasWantedItem_s = false;
					foreach( var kvp in wantedRes )
					{
						if( inv.Get( kvp.Key ) > 0 )
						{
							hasWantedItem_s = true;
							break;
						}
					}

					if( hasWantedItem_s )
					{
						toBeAssignedGameObjects.Add( dfc, receiversCache[j] );
						break;
					}
					// if this receiver is not compatible - check the next one.
				}
			}


			foreach( var kvp in toBeAssignedGameObjects )
			{
				TacticalGoalController goalController = kvp.Key.controller;
				TacticalMoveToGoal goal1 = new TacticalMoveToGoal();
				goal1.isHostile = false;
				goal1.SetDestination( paymentReceiverSSObject );

				TacticalDropOffGoal goal2 = new TacticalDropOffGoal();
				goal2.isHostile = false;
				goal2.SetDestination( kvp.Value );
				goalController.SetGoals( TacticalGoalQuery.TAG_CUSTOM, goal1, goal2 );
			}
			if( toBeAssignedGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
				return true;
			}
			return false;
		}
	}
}