using SS.AI;
using SS.AI.Goals;
using SS.Content;
using SS.InputSystem;
using SS.Levels;
using SS.Objects;
using SS.Objects.Modules;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace SS
{
	public static class InputOverridePickUpQuery
	{
		// block mouse inputs.

		// left - assigns the employment.

		// right - cancels the blocking & custom input.



		private static void Inp_Cancel( InputQueue self )
		{
			DisableInput();
			self.StopExecution();
		}

		private static void Inp_Query( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				RaycastHit hitInfo;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
				{
					SSObject ssObject = hitInfo.collider.GetComponent<SSObject>();

					if( ssObject != null )
					{

						InventoryModule[] inventories = ssObject.GetModules<InventoryModule>();
						if( inventories.Length > 0 )
						{
							AssignPickupInventoryGoal( ssObject, inventories[0], Selection.GetSelectedObjects() );
						}
						else
						{
							ResourceDepositModule[] deposits = ssObject.GetModules<ResourceDepositModule>();
							if( deposits.Length > 0 )
							{
								AssignPickupDepositGoal( ssObject, deposits[0], Selection.GetSelectedObjects() );
							}
						}
					}
				}
			}


			DisableInput();
			self.StopExecution();
		}

		private static void Inp_BlockSelectionOverride( InputQueue self )
		{
			self.StopExecution();
		}

		public static void EnableInput()
		{
			// Need to override all 3 channels of mouse input, since selection uses all 3 of them (so all 3 need to be blocked to block selection).
			Main.mouseInput.RegisterOnPress( MouseCode.RightMouseButton, 9.0f, Inp_BlockSelectionOverride, true ); // left
			Main.mouseInput.RegisterOnHold( MouseCode.RightMouseButton, 9.0f, Inp_BlockSelectionOverride, true );
			Main.mouseInput.RegisterOnRelease( MouseCode.RightMouseButton, 9.0f, Inp_Cancel, true );

			Main.mouseInput.RegisterOnPress( MouseCode.LeftMouseButton, 9.0f, Inp_Query, true ); // right
			Main.mouseInput.RegisterOnHold( MouseCode.LeftMouseButton, 9.0f, Inp_BlockSelectionOverride, true );
			Main.mouseInput.RegisterOnRelease( MouseCode.LeftMouseButton, 9.0f, Inp_BlockSelectionOverride, true );
		}

		public static void DisableInput()
		{
			if( Main.mouseInput != null )
			{
				// The action is assigned to on release to block selection controller deselecting the object when placed
				//    (building get's placed on press, then preview removes itself from the input, and later on release it deselects).
				Main.mouseInput.ClearOnPress( MouseCode.RightMouseButton, Inp_BlockSelectionOverride ); // left
				Main.mouseInput.ClearOnHold( MouseCode.RightMouseButton, Inp_BlockSelectionOverride );
				Main.mouseInput.ClearOnRelease( MouseCode.RightMouseButton, Inp_Cancel );

				Main.mouseInput.ClearOnPress( MouseCode.LeftMouseButton, Inp_Query ); // right
				Main.mouseInput.ClearOnHold( MouseCode.LeftMouseButton, Inp_BlockSelectionOverride );
				Main.mouseInput.ClearOnRelease( MouseCode.LeftMouseButton, Inp_BlockSelectionOverride );
			}
		}

		private static void AssignPickupInventoryGoal( SSObject hitSSObject, InventoryModule hitInventory, SSObjectDFSC[] selected )
		{
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<SSObjectDFSC> movableWithInvGameObjects = new List<SSObjectDFSC>();

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
						movableWithInvGameObjects.Add( selected[i] );
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
				goal1.SetDestination( hitSSObject );

				TacticalPickUpGoal goal2 = new TacticalPickUpGoal();
				goal2.isHostile = false;
				goal2.SetDestination( hitInventory );
				goalController.SetGoals( TacticalGoalQuery.TAG_CUSTOM, goal1, goal2 );
			}
		}

		private static void AssignPickupDepositGoal( SSObject hitSSObject, ResourceDepositModule hitDeposit, SSObjectDFSC[] selected )
		{
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<SSObjectDFSC> movableWithInvGameObjects = new List<SSObjectDFSC>();

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
					movableWithInvGameObjects.Add( selected[i] );
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
				goal1.SetDestination( hitSSObject );

				TacticalPickUpGoal goal2 = new TacticalPickUpGoal();
				goal2.isHostile = false;
				goal2.SetDestination( hitDeposit );
				goalController.SetGoals( TacticalGoalQuery.TAG_CUSTOM, goal1, goal2 );
			}
		}

	}
}