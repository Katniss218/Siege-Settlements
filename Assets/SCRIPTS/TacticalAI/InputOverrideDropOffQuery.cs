﻿using SS.AI;
using SS.AI.Goals;
using SS.Content;
using SS.InputSystem;
using SS.Levels;
using SS.Objects;
using SS.Objects.Modules;
using SS.Objects.Units;
using SS.ResourceSystem.Payment;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SS
{
	public static class InputOverrideDropOffQuery
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
						if( ssObject.HasUsablePaymentReceivers() )
						{
							if( ssObject is SSObjectDFC )
							{
								bool flag = AssignMakePaymentGoal( (SSObjectDFC)ssObject, Selection.GetSelectedObjects() );
								if( flag )
								{
									return;
								}
							}
						}

						InventoryModule[] inventories = ssObject.GetModules<InventoryModule>();
						if( inventories.Length > 0 )
						{
							AssignDropoffToInventoryGoal( inventories[0], Selection.GetSelectedObjects() );
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

		static void AssignDropoffToInventoryGoal( InventoryModule hitInventory, SSObject[] selected )
		{
			List<SSObjectDFC> filteredObjs = new List<SSObjectDFC>();

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
					continue;
				}

				Dictionary<string, int> inventoryItems = inv.GetAll();

				bool suitable = true;
				// Check if the storage inventory can hold any of the items.
				foreach( var kvp in inventoryItems )
				{
					suitable = true;
					if( hitInventory.GetSpaceLeft( kvp.Key ) == 0 )
					{
						suitable = false;
						continue;
					}
				}

				if( suitable )
				{
					filteredObjs.Add( dfc );
				}
			}

			if( filteredObjs.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
			}
			for( int i = 0; i < filteredObjs.Count; i++ )
			{
				TacticalGoalController goalController = filteredObjs[i].controller;
				TacticalMoveToGoal goal1 = new TacticalMoveToGoal();
				goal1.isHostile = false;
				goal1.SetDestination( hitInventory.ssObject );

				TacticalDropOffGoal goal2 = new TacticalDropOffGoal();
				goal2.isHostile = false;
				goal2.SetDestination( hitInventory );
				goalController.SetGoals( TacticalGoalQuery.TAG_CUSTOM, goal1, goal2 );
			}
		}

		private static bool AssignMakePaymentGoal( SSObjectDFC paymentReceiverSSObject, SSObject[] selected )
		{
			// Assigns payment goal to selected objects.
			// Can assign different receivers for different objects, depending on their inventories & wanted resources.

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

			// this makes sure that if building is under construction - only the construction site receiver is returned.
			IPaymentReceiver[] paymentReceivers = paymentReceiverSSObject.GetAvailablePaymentReceivers();

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
				for( int j = 0; j < paymentReceivers.Length; j++ )
				{
					Dictionary<string, int> wantedRes = paymentReceivers[j].GetWantedResources();
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
						toBeAssignedGameObjects.Add( dfc, paymentReceivers[j] );
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