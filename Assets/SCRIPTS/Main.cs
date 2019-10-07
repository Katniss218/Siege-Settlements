﻿using UnityEngine;
using SS.Buildings;
using UnityEngine.EventSystems;
using SS.Extras;
using SS.Modules.Inventories;
using SS.ResourceSystem.Payment;
using System.Collections.Generic;
using UnityEngine.AI;
using SS.Content;
using UnityEngine.Events;
using SS.Levels.SaveStates;
using System;
using SS.Projectiles;
using SS.Units;
using SS.Heroes;
using SS.Levels;
using SS.Diplomacy;

namespace SS
{
	/// <summary>
	/// The main class, think of it like a Game Manager class.
	/// </summary>
	public class Main : MonoBehaviour
	{
		public const float DEFAULT_NAVMESH_BASE_OFFSET = -0.075f;

		public const string GRAPHICS_GAMEOBJECT_NAME = "graphics";

		public class _UnityEvent_bool : UnityEvent<bool> { }

		public static bool isHudLocked { get; private set; }

		/// <summary>
		/// Called when the HUD becomes locked / unlocked.
		/// </summary>
		public static _UnityEvent_bool onHudLockChange = new _UnityEvent_bool();


		private static GameObject __particleSystemInstance = null;
		new public static GameObject particleSystem
		{
			get
			{
				if( __particleSystemInstance == null ) { __particleSystemInstance = Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Particle System" ) ); }
				return __particleSystemInstance;
			}
		}


		private static Transform __objectHUDCanvas = null;
		public static Transform objectHUDCanvas
		{
			get
			{
				if( __objectHUDCanvas == null )
				{
					Canvas[] canvases = FindObjectsOfType<Canvas>();
					for( int i = 0; i < canvases.Length;i++ )
					{
						if( canvases[i].gameObject.CompareTag( "Object HUD Canvas" ) )
						{
							__objectHUDCanvas = canvases[i].transform;
							break;
						}
					}
				}
				return __objectHUDCanvas;
			}
		}

		private static Canvas __canvas = null;
		public static Canvas canvas
		{
			get
			{
				if( __canvas == null ) { __canvas = FindObjectOfType<Canvas>(); }
				return __canvas;
			}
		}
		
		private static Transform __cameraPivot = null;
		public static Transform cameraPivot
		{
			get
			{
				if( __cameraPivot == null )
				{
					__cameraPivot = FindObjectOfType<CameraController>().transform;
				}
				return __cameraPivot;
			}
		}

		private static CameraController __cameraController = null;
		public static CameraController cameraController
		{
			get
			{
				if( __cameraController == null )
				{
					__cameraController = cameraPivot.GetComponent<CameraController>();
				}
				return __cameraController;
			}
		}

		private static Camera __camera = null;
		new public static Camera camera
		{
			get
			{
				if( __camera == null )
				{
					__camera = cameraPivot.GetChild( 0 ).GetComponent<Camera>();
				}
				return __camera;
			}
		}

		public bool IsControllableByPlayer( GameObject go, int playerId )
		{
			// Being controllable not necessarily means that you need to be selectable.

			FactionMember factionMember = go.GetComponent<FactionMember>();
			if( factionMember == null )
			{
				return true;
			}
			return factionMember.factionId == playerId;
		}

		void Update()
		{
			if( Input.GetMouseButtonDown( 1 ) )
			{
				if( !EventSystem.current.IsPointerOverGameObject() )
				{
					RaycastHit[] raycastHits = Physics.RaycastAll( Main.camera.ScreenPointToRay( Input.mousePosition ) );

					Vector3? terrainHitPos = null;

					ResourceDeposit hitDeposit = null;
					Transform hitReceiverTransform = null;
					IPaymentReceiver[] hitPaymentReceivers = null;

					for( int i = 0; i < raycastHits.Length; i++ )
					{
						if( raycastHits[i].collider.gameObject.layer == ObjectLayer.TERRAIN )
						{
							terrainHitPos = raycastHits[i].point;
						}
						else
						{
							ResourceDeposit deposit = raycastHits[i].collider.GetComponent<ResourceDeposit>();
							if( deposit != null )
							{
								hitDeposit = deposit;
							}
							IPaymentReceiver[] receivers = raycastHits[i].collider.GetComponents<IPaymentReceiver>();
							if( receivers.Length > 0 )
							{
								hitReceiverTransform = raycastHits[i].collider.transform;
								hitPaymentReceivers = receivers;
							}

						}
					}


					if( hitDeposit == null && hitReceiverTransform == null && terrainHitPos.HasValue )
					{
						AssignMoveToGoal( terrainHitPos.Value, Selection.selectedObjects );
					}

					else if( hitReceiverTransform != null )
					{
						AssignMakePaymentGoal( hitReceiverTransform, hitPaymentReceivers, Selection.selectedObjects );
					}

					else if( hitDeposit != null )
					{
						AssignPickupDepositGoal( hitDeposit, Selection.selectedObjects );
					}
				}
			}

			// Try repair mouseovered building.
			if( Input.GetKeyDown( KeyCode.L ) )
			{
				if( !EventSystem.current.IsPointerOverGameObject() )
				{
					RaycastHit hitInfo;
					if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
					{
						GameObject gameObject = hitInfo.collider.gameObject;
						if( gameObject.layer != ObjectLayer.BUILDINGS )
						{
							return;
						}
						if( !IsControllableByPlayer( gameObject, LevelDataManager.PLAYER_FAC ) )
						{
							return;
						}
						if( !Building.IsRepairable( gameObject.GetComponent<Damageable>() ) )
						{
							return;
						}

						// If it is a building, start repair.
						// Empty ConstructionSiteData (no resources present).
						ConstructionSiteData constructionSiteData = new ConstructionSiteData();

						ConstructionSite.BeginConstructionOrRepair( gameObject, constructionSiteData );
						AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Sounds/ai_response" ) );
					}
				}
			}

			// Temporary resource payment speedup (every payment receiver to full).
			if( Input.GetKeyDown( KeyCode.K ) )
			{
				if( !EventSystem.current.IsPointerOverGameObject() )
				{
					RaycastHit hitInfo;
					if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
					{
						GameObject gameObject = hitInfo.collider.gameObject;
						if( !IsControllableByPlayer( gameObject, LevelDataManager.PLAYER_FAC ) )
						{
							return;
						}
						IPaymentReceiver[] paymentReceivers = gameObject.GetComponents<IPaymentReceiver>();
						for( int i = 0; i < paymentReceivers.Length; i++ )
						{
							Dictionary<string, int> wantedRes = paymentReceivers[i].GetWantedResources();

							foreach( var kvp in wantedRes )
							{
								paymentReceivers[i].ReceivePayment( kvp.Key, kvp.Value );
							}
						}
					}
				}
			}

			if( Input.GetKeyDown( KeyCode.O ) )
			{
				if( !EventSystem.current.IsPointerOverGameObject() )
				{
					RaycastHit hitInfo;
					if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
					{
						ResourceDeposit hitDeposit = hitInfo.collider.GetComponent<ResourceDeposit>();

						if( hitDeposit != null )
						{

							AssignDropoffToInventoryGoal( hitInfo, hitDeposit, Selection.selectedObjects );
						}
					}
				}
			}

			if( Input.GetKeyDown( KeyCode.P ) )
			{
				if( !EventSystem.current.IsPointerOverGameObject() )
				{
					RaycastHit hitInfo;
					if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
					{
						AssignDropoffToNewGoal( hitInfo, Selection.selectedObjects );
					}
				}
			}

			if( Input.GetKeyDown( KeyCode.Tab ) )
			{
				isHudLocked = !isHudLocked;

				onHudLockChange?.Invoke( isHudLocked );
			}

			if( Input.GetKeyDown( KeyCode.Alpha1 ) )
			{
				if( !EventSystem.current.IsPointerOverGameObject() )
				{
					RaycastHit hitInfo;
					if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
					{
						FactionMember fac = hitInfo.collider.GetComponent<FactionMember>();
						if( fac != null )
						{
							fac.factionId = 0;
						}
					}
				}
			}
			if( Input.GetKeyDown( KeyCode.Alpha2 ) )
			{
				if( !EventSystem.current.IsPointerOverGameObject() )
				{
					RaycastHit hitInfo;
					if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
					{
						FactionMember fac = hitInfo.collider.GetComponent<FactionMember>();
						if( fac != null )
						{
							fac.factionId = 1;
						}
					}
				}
			}
			if( Input.GetKeyDown( KeyCode.Alpha9 ) )
			{
				if( !EventSystem.current.IsPointerOverGameObject() )
				{
					RaycastHit hitInfo;
					if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
					{
						ResourceDepositDefinition def = DefinitionManager.GetResourceDeposit( "resource_deposit.tree" );
						ResourceDepositData data = new ResourceDepositData();
						data.guid = Guid.NewGuid();
						data.position = hitInfo.point;
						data.rotation = Quaternion.Euler( 0, UnityEngine.Random.Range( -180.0f, 180.0f ), 0 );
						data.resources = new Dictionary<string, int>();
						data.resources.Add( "resource.wood", 5 );
						ResourceDepositCreator.Create( def, data );
					}
				}
			}
			if( Input.GetKeyDown( KeyCode.Alpha0 ) )
			{
				if( !EventSystem.current.IsPointerOverGameObject() )
				{
					RaycastHit hitInfo;
					if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
					{
						ResourceDepositDefinition def = DefinitionManager.GetResourceDeposit( "resource_deposit.stone" );
						ResourceDepositData data = new ResourceDepositData();
						data.guid = Guid.NewGuid();
						data.position = hitInfo.point;
						data.rotation = Quaternion.Euler( 0, UnityEngine.Random.Range( -180.0f, 180.0f ), 0 );
						data.resources = new Dictionary<string, int>();
						data.resources.Add( "resource.stone", 20 );
						ResourceDepositCreator.Create( def, data );
					}
				}
			}
			if( Input.GetKeyDown( KeyCode.Pause ) )
			{
				if( PauseManager.isPaused )
				{
					PauseManager.Unpause();
				}
				else
				{
					PauseManager.Pause();
				}
			}
		}


		//
		//
		//

		
		private void AssignDropoffToNewGoal( RaycastHit hitInfo, Selectable[] selected )
		{
			List<GameObject> movableWithInvGameObjects = new List<GameObject>();

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			for( int i = 0; i < selected.Length; i++ )
			{
				if( !IsControllableByPlayer( selected[i].gameObject, LevelDataManager.PLAYER_FAC ) )
				{
					continue;
				}
				IInventory inv = selected[i].GetComponent<IInventory>();
				if( inv == null )
				{
					continue;
				}
				if( inv.isEmpty )
				{
					continue;
				}

				movableWithInvGameObjects.Add( selected[i].gameObject );
			}

			for( int i = 0; i < movableWithInvGameObjects.Count; i++ )
			{
				TAIGoal.DropoffToNew.AssignTAIGoal( movableWithInvGameObjects[i], hitInfo.point );
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Sounds/ai_response" ) );
			}
		}

		private void AssignDropoffToInventoryGoal( RaycastHit hitInfo, ResourceDeposit hitDeposit, Selectable[] selected )
		{
			List<GameObject> movableWithInvGameObjects = new List<GameObject>();

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			for( int i = 0; i < selected.Length; i++ )
			{
				bool suitable = true;

				if( !IsControllableByPlayer( selected[i].gameObject, LevelDataManager.PLAYER_FAC ) )
				{
					continue;
				}
				IInventory inv = selected[i].GetComponent<IInventory>();
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
					if( hitDeposit.inventory.GetMaxCapacity( kvp.Key ) == 0 )
					{
						suitable = false;
						break;
					}
					// don't move if the deposit doesn't have space to leave resource (inv full of that specific resource).
					if( hitDeposit.inventory.GetMaxCapacity( kvp.Key ) == hitDeposit.inventory.Get( kvp.Key ) )
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

			for( int i = 0; i < movableWithInvGameObjects.Count; i++ )
			{
				TAIGoal.DropoffToInventory.AssignTAIGoal( movableWithInvGameObjects[i], hitInfo.collider.gameObject );
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Sounds/ai_response" ) );
			}
		}

		private void AssignMoveToGoal( Vector3 terrainHitPos, Selectable[] selected )
		{
			const float GRID_MARGIN = 0.125f;

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<GameObject> movableGameObjects = new List<GameObject>();

			float biggestRadius = float.MinValue;

			for( int i = 0; i < selected.Length; i++ )
			{
				if( !IsControllableByPlayer( selected[i].gameObject, LevelDataManager.PLAYER_FAC ) )
				{
					continue;
				}
				NavMeshAgent navMeshAgent = selected[i].GetComponent<NavMeshAgent>();
				if( navMeshAgent == null )
				{
					continue;
				}

				// Calculate how big is the biggest unit/hero/etc. to be used when specifying movement grid size.
				movableGameObjects.Add( selected[i].gameObject );
				if( navMeshAgent.radius > biggestRadius )
				{
					biggestRadius = navMeshAgent.radius;
				}
			}

			//Calculate the grid position.
			TAIGoal.MoveTo.MovementGridInfo gridInfo = TAIGoal.MoveTo.GetGridPositions( movableGameObjects );


			foreach( var kvp in gridInfo.positions )
			{
				Vector3 gridPositionWorld = TAIGoal.MoveTo.GridToWorld( kvp.Value, terrainHitPos, biggestRadius * 2 + GRID_MARGIN );

				RaycastHit gridHit;
				Ray r = new Ray( gridPositionWorld + new Vector3( 0.0f, 50.0f, 0.0f ), Vector3.down );
				if( Physics.Raycast( r, out gridHit, 100.0f, ObjectLayer.TERRAIN_MASK ) )
				{
					TAIGoal.MoveTo.AssignTAIGoal( kvp.Key, gridPositionWorld );
					AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Sounds/ai_response" ) );
				}
				else
				{
					Debug.LogWarning( "Movement Grid position " + gridPositionWorld + " was outside of the map." );
				}
			}
		}

		private void AssignPickupDepositGoal( ResourceDeposit hitDeposit, Selectable[] selected )
		{
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<GameObject> movableWithInvGameObjects = new List<GameObject>();

			// Go pick up if the inventory can hold any of the resources in the deposit.
			Dictionary<string, int> resourcesInDeposit = hitDeposit.inventory.GetAll();

			for( int i = 0; i < selected.Length; i++ )
			{
				if( !IsControllableByPlayer( selected[i].gameObject, LevelDataManager.PLAYER_FAC ) )
				{
					continue;
				}
				if( selected[i].GetComponent<NavMeshAgent>() == null )
				{
					continue;
				}
				IInventory inv = selected[i].GetComponent<IInventory>();
				if( inv == null )
				{
					continue;
				}
				bool canPickupAny = false;
				foreach( var kvp in resourcesInDeposit )
				{
					// if can pick up && has empty space for it.
					if( inv.GetMaxCapacity( kvp.Key ) > 0 && inv.Get( kvp.Key ) != inv.GetMaxCapacity( kvp.Key ) )
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


			for( int i = 0; i < movableWithInvGameObjects.Count; i++ )
			{
				IInventory inv = movableWithInvGameObjects[i].GetComponent<IInventory>();

				TAIGoal.PickupDeposit.AssignTAIGoal( movableWithInvGameObjects[i], hitDeposit.gameObject );
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Sounds/ai_response" ) );
			}
		}

		private void AssignMakePaymentGoal( Transform paymentReceiverTransform, IPaymentReceiver[] paymentReceivers, Selectable[] selected )
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
				if( !IsControllableByPlayer( selected[i].gameObject, LevelDataManager.PLAYER_FAC ) )
				{
					continue;
				}
				if( selected[i].GetComponent<NavMeshAgent>() == null )
				{
					continue;
				}
				IInventory inv = selected[i].GetComponent<IInventory>();
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
						Debug.Log( "Wanted:" + kvp.Key + ", " + kvp.Value );
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


			for( int i = 0; i < toBeAssignedGameObjects.Count; i++ )
			{
				TAIGoal.MakePayment.AssignTAIGoal( toBeAssignedGameObjects[i], paymentReceiverTransform.gameObject );
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Sounds/ai_response" ) );
			}
		}

		public static Guid GetGuid( GameObject obj )
		{
			// gets guid of object.

			Unit unit = obj.GetComponent<Unit>();
			if( unit != null )
			{
				return unit.guid;
			}
			Building building = obj.GetComponent<Building>();
			if( building != null )
			{
				return building.guid;
			}
			ResourceDeposit deposit = obj.GetComponent<ResourceDeposit>();
			if( deposit != null )
			{
				return deposit.guid;
			}
			Hero hero = obj.GetComponent<Hero>();
			if( hero != null )
			{
				return hero.guid;
			}
			Projectile projectile = obj.GetComponent<Projectile>();
			if( projectile != null )
			{
				return projectile.guid;
			}
			Extra extra = obj.GetComponent<Extra>();
			if( extra != null )
			{
				return extra.guid;
			}
			throw new Exception( "Specified Gameobject is not valid and doesn't have a GUID." );
		}

		public static GameObject GetGameObject( Guid guid )
		{
			Unit[] units = Unit.GetAllUnits();
			for( int i = 0; i < units.Length; i++ )
			{
				if( units[i].guid == guid )
				{
					return units[i].gameObject;
				}
			}

			Building[] buildings = Building.GetAllBuildings();
			for( int i = 0; i < buildings.Length; i++ )
			{
				if( buildings[i].guid == guid )
				{
					return buildings[i].gameObject;
				}
			}

			Hero[] heroes = Hero.GetAllHeroes();
			for( int i = 0; i < heroes.Length; i++ )
			{
				if( heroes[i].guid == guid )
				{
					return heroes[i].gameObject;
				}
			}

			ResourceDeposit[] resourceDeposits = ResourceDeposit.GetAllResourceDeposits();
			for( int i = 0; i < resourceDeposits.Length; i++ )
			{
				if( resourceDeposits[i].guid == guid )
				{
					return resourceDeposits[i].gameObject;
				}
			}

			Projectile[] projectiles = Projectile.GetAllProjectiles();
			for( int i = 0; i < projectiles.Length; i++ )
			{
				if( projectiles[i].guid == guid )
				{
					return projectiles[i].gameObject;
				}
			}

			Extra[] extras = Extra.GetAllExtras();
			for( int i = 0; i < extras.Length; i++ )
			{
				if( extras[i].guid == guid )
				{
					return extras[i].gameObject;
				}
			}
			return null;
		}
	}
}