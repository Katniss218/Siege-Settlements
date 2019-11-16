using UnityEngine;
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
using SS.InputSystem;
using SS.Modules;

namespace SS
{
	/// <summary>
	/// The main class, think of it like a Game Manager class.
	/// </summary>
	public class Main : MonoBehaviour
	{
		public const float DEFAULT_NAVMESH_BASE_OFFSET = -0.075f;
		
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
				if( __particleSystemInstance == null ) { __particleSystemInstance = Instantiate( (GameObject)AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Particle System" ) ); }
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

		private static QueuedKeyboardInput __keyboardInput = null;
		public static QueuedKeyboardInput keyboardInput
		{
			get
			{
				if( __keyboardInput == null ) { __keyboardInput = FindObjectOfType<QueuedKeyboardInput>(); }
				return __keyboardInput;
			}
		}

		private static QueuedMouseInput __mouseInput = null;
		public static QueuedMouseInput mouseInput
		{
			get
			{
				if( __mouseInput == null ) { __mouseInput = FindObjectOfType<QueuedMouseInput>(); }
				return __mouseInput;
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

		public bool IsControllableByPlayer( GameObject gameObject, int playerId )
		{
			// Being controllable not necessarily means that you need to be selectable.

			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			if( factionMember == null )
			{
				return true;
			}
			return factionMember.factionId == playerId;
		}

		private void Inp_Right( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				RaycastHit[] raycastHits = Physics.RaycastAll( Main.camera.ScreenPointToRay( Input.mousePosition ) );

				Vector3? terrainHitPos = null;

				GameObject hitInventoryGameObject = null;
				IInventory hitInventory = null;

				GameObject hitDepositGameObject = null;
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
						ResourceDepositModule deposit = raycastHits[i].collider.GetComponent<ResourceDepositModule>();
						if( deposit != null && hitDepositGameObject == null )
						{
							hitDepositGameObject = raycastHits[i].collider.gameObject;
							hitDeposit = deposit;
						}

						IInventory inventory = raycastHits[i].collider.GetComponent<IInventory>();
						if( inventory != null && hitInventoryGameObject == null )
						{
							hitInventoryGameObject = raycastHits[i].collider.gameObject;
							hitInventory = inventory;
						}

						IPaymentReceiver[] receivers = raycastHits[i].collider.GetComponents<IPaymentReceiver>();
						FactionMember recFactionMember = raycastHits[i].collider.GetComponent<FactionMember>();
						if( receivers.Length > 0 && recFactionMember != null && hitReceiverTransform == null )
						{
							hitReceiverTransform = raycastHits[i].collider.transform;
							hitPaymentReceivers = receivers;
							hitReceiverFactionMember = recFactionMember;
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

				else if( hitReceiverTransform != null && hitReceiverFactionMember.factionId == LevelDataManager.PLAYER_FAC )
				{
					AssignMakePaymentGoal( hitReceiverTransform, hitPaymentReceivers, Selection.selectedObjects );
				}
				else if( hitDeposit != null )
				{
					AssignPickupDepositGoal( hitDepositGameObject, hitDeposit, Selection.selectedObjects );
				}
				else if( hitInventory != null )
				{
					AssignPickupInventoryGoal( hitInventoryGameObject, hitInventory, Selection.selectedObjects );
				}
				else if( hitDamageable != null )
				{
					AssignAttackGoal( hitDamageable, Selection.selectedObjects );
				}
			}
		}

		private void Inp_L( InputQueue self )
		{// Try repair mouseovered building.
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
					AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ) );
				}
			}
		}

		private void Inp_K( InputQueue self )
		{// Temporary resource payment speedup (every payment receiver to full).
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				RaycastHit hitInfo;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
				{
					GameObject gameObject = hitInfo.collider.gameObject;
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

		private void Inp_O( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				RaycastHit hitInfo;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
				{
					IInventory hitInventory = hitInfo.collider.GetComponent<IInventory>();

					if( hitInventory != null )
					{

						AssignDropoffToInventoryGoal( hitInfo, hitInventory, Selection.selectedObjects );
					}
				}
			}
		}
		
		private void Inp_Y( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				RaycastHit hitInfo;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
				{
					Damageable damageable = hitInfo.collider.GetComponent<Damageable>();

					if( damageable != null )
					{
						damageable.TakeDamageUnscaled( 99999999.99f );
					}
				}
			}
		}

		private void Inp_Tab( InputQueue self )
		{
			isHudLocked = !isHudLocked;

			onHudLockChange?.Invoke( isHudLocked );
		}

		private static void SetFactionSelected( int fac )
		{
			Selectable[] selected = Selection.selectedObjects;
			for( int i = 0; i < selected.Length; i++ )
			{
				FactionMember faction = selected[i].GetComponent<FactionMember>();

				if( faction == null )
				{
					continue;
				}
				faction.factionId = fac;
			}
		}

		private void Inp_A1( InputQueue self )
		{
			SetFactionSelected( 0 );
		}

		private void Inp_A2( InputQueue self )
		{
			SetFactionSelected( 1 );
		}

		private void Inp_A3( InputQueue self )
		{
			SetFactionSelected( 2 );
		}

		private static void CreateDepositRaycast( string id )
		{
			RaycastHit hitInfo;
			if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
			{
				if( hitInfo.collider.gameObject.layer == ObjectLayer.TERRAIN )
				{
					ExtraDefinition def = DefinitionManager.GetExtra( id );
					ExtraData data = new ExtraData();
					data.guid = Guid.NewGuid();
					data.position = hitInfo.point;
					data.rotation = Quaternion.Euler( 0, UnityEngine.Random.Range( -180.0f, 180.0f ), 0 );

					GameObject extra = ExtraCreator.Create( def, data );
					ResourceDepositModule resDepo = extra.GetComponent<ResourceDepositModule>();
					foreach( var slot in def.GetModule<ResourceDepositModuleDefinition>().slots )
					{
						resDepo.Add( slot.resourceId, slot.capacity );
					}
				}
			}
		}

		private void Inp_A4( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				RaycastHit hitInfo;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
				{
					if( hitInfo.collider.gameObject.layer == ObjectLayer.TERRAIN )
					{
						HeroDefinition def = DefinitionManager.GetHero( "hero.cerberos" );
						HeroData data = new HeroData();
						data.guid = Guid.NewGuid();
						data.position = hitInfo.point;
						data.rotation = Quaternion.Euler( 0, UnityEngine.Random.Range( -180.0f, 180.0f ), 0 );
						data.factionId = 0;
						data.health = def.healthMax;
						HeroCreator.Create( def, data );
					}
				}
			}
		}

		private void Inp_A6( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				CreateDepositRaycast( "resource_deposit.iron_ore_0" );
			}
		}

		private void Inp_A7( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				CreateDepositRaycast( "resource_deposit.sulphur_ore_0" );
			}
		}

		private void Inp_A8( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				CreateDepositRaycast( "resource_deposit.tree" );
			}
		}

		private void Inp_A9( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				CreateDepositRaycast( "resource_deposit.pine" );
			}
		}

		private void Inp_A0( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				CreateDepositRaycast( "resource_deposit.rock_0" );
			}
		}

		private void Inp_Pause( InputQueue self )
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

		private void OnEnable()
		{
			Main.mouseInput.RegisterOnPress( MouseCode.RightMouseButton, 60.0f, Inp_Right, true );
			
			Main.keyboardInput.RegisterOnPress( KeyCode.L, 60.0f, Inp_L, true );
			Main.keyboardInput.RegisterOnPress( KeyCode.K, 60.0f, Inp_K, true );
			Main.keyboardInput.RegisterOnPress( KeyCode.O, 60.0f, Inp_O, true );
			Main.keyboardInput.RegisterOnPress( KeyCode.Y, 60.0f, Inp_Y, true );
			Main.keyboardInput.RegisterOnPress( KeyCode.Tab, 60.0f, Inp_Tab, true );
			Main.keyboardInput.RegisterOnPress( KeyCode.Alpha1, 60.0f, Inp_A1, true );
			Main.keyboardInput.RegisterOnPress( KeyCode.Alpha2, 60.0f, Inp_A2, true );
			Main.keyboardInput.RegisterOnPress( KeyCode.Alpha3, 60.0f, Inp_A3, true );
			Main.keyboardInput.RegisterOnPress( KeyCode.Alpha4, 60.0f, Inp_A4, true );
			Main.keyboardInput.RegisterOnPress( KeyCode.Alpha6, 60.0f, Inp_A6, true );
			Main.keyboardInput.RegisterOnPress( KeyCode.Alpha7, 60.0f, Inp_A7, true );
			Main.keyboardInput.RegisterOnPress( KeyCode.Alpha8, 60.0f, Inp_A8, true );
			Main.keyboardInput.RegisterOnPress( KeyCode.Alpha9, 60.0f, Inp_A9, true );
			Main.keyboardInput.RegisterOnPress( KeyCode.Alpha0, 60.0f, Inp_A0, true );
			Main.keyboardInput.RegisterOnPress( KeyCode.Pause, 60.0f, Inp_Pause, true );
		}

		private void OnDisable()
		{
			if( Main.mouseInput != null )
			{
				Main.mouseInput.ClearOnPress( MouseCode.RightMouseButton, Inp_Right );
			}
			if( Main.keyboardInput != null )
			{
				Main.keyboardInput.ClearOnPress( KeyCode.L, Inp_L );
				Main.keyboardInput.ClearOnPress( KeyCode.K, Inp_K );
				Main.keyboardInput.ClearOnPress( KeyCode.O, Inp_O );
				Main.keyboardInput.ClearOnPress( KeyCode.Y, Inp_Y );
				Main.keyboardInput.ClearOnPress( KeyCode.Tab, Inp_Tab );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha1, Inp_A1 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha2, Inp_A2 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha3, Inp_A3 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha4, Inp_A4 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha6, Inp_A6 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha7, Inp_A7 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha8, Inp_A8 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha9, Inp_A9 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha0, Inp_A0 );
				Main.keyboardInput.ClearOnPress( KeyCode.Pause, Inp_Pause );
			}
		}

		//
		//
		//
		
		private void AssignAttackGoal( Damageable target, Selectable[] selected )
		{
			List<GameObject> movableGameObjects = new List<GameObject>();

			FactionMember tarFac = target.GetComponent<FactionMember>();

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			for( int i = 0; i < selected.Length; i++ )
			{
				if( !IsControllableByPlayer( selected[i].gameObject, LevelDataManager.PLAYER_FAC ) )
				{
					continue;
				}
				ITargetFinder[] targeters = selected[i].GetComponents<ITargetFinder>();
				if( targeters == null || targeters.Length == 0 )
				{
					continue;
				}

				FactionMember selFac = selected[i].GetComponent<FactionMember>();

				bool canTarget = false;
				for( int j = 0; j < targeters.Length; j++ )
				{
					if( FactionMember.CanTargetAnother( selFac, tarFac ) )
					{
						canTarget = true;
						break;
					}
				}

				if( canTarget )
				{
					movableGameObjects.Add( selected[i].gameObject );
				}
			}

			if( movableGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ) );
			}
			for( int i = 0; i < movableGameObjects.Count; i++ )
			{
				TAIGoal.Attack.AssignTAIGoal( movableGameObjects[i], target.gameObject );
			}
		}

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

			if( movableWithInvGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ) );
			}
			for( int i = 0; i < movableWithInvGameObjects.Count; i++ )
			{
				TAIGoal.DropoffToNew.AssignTAIGoal( movableWithInvGameObjects[i], hitInfo.point );
			}
		}

		private void AssignDropoffToInventoryGoal( RaycastHit hitInfo, IInventory hitInventory, Selectable[] selected )
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
					if( hitInventory.GetMaxCapacity( kvp.Key ) == 0 )
					{
						suitable = false;
						break;
					}
					// don't move if the deposit doesn't have space to leave resource (inv full of that specific resource).
					if( hitInventory.GetMaxCapacity( kvp.Key ) == hitInventory.Get( kvp.Key ) )
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
				TAIGoal.DropoffToInventory.AssignTAIGoal( movableWithInvGameObjects[i], hitInfo.collider.gameObject );
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


			if( gridInfo.positions.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ) );
			}
			foreach( var kvp in gridInfo.positions )
			{
				Vector3 gridPositionWorld = TAIGoal.MoveTo.GridToWorld( kvp.Value, gridInfo.sizeX, gridInfo.sizeZ, terrainHitPos, biggestRadius * 2 + GRID_MARGIN );

				RaycastHit gridHit;
				Ray r = new Ray( gridPositionWorld + new Vector3( 0.0f, 50.0f, 0.0f ), Vector3.down );
				if( Physics.Raycast( r, out gridHit, 100.0f, ObjectLayer.TERRAIN_MASK ) )
				{
					TAIGoal.MoveTo.AssignTAIGoal( kvp.Key, gridPositionWorld );
				}
				else
				{
					Debug.LogWarning( "Movement Grid position " + gridPositionWorld + " was outside of the map." );
				}
			}
		}

		private void AssignPickupInventoryGoal( GameObject hitGameObject, IInventory hitInventory, Selectable[] selected )
		{
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<GameObject> movableWithInvGameObjects = new List<GameObject>();

			// Go pick up if the inventory can hold any of the resources in the deposit.
			Dictionary<string, int> resourcesInDeposit = hitInventory.GetAll();

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


			if( movableWithInvGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ) );
			}
			for( int i = 0; i < movableWithInvGameObjects.Count; i++ )
			{
				TAIGoal.PickupInventory.AssignTAIGoal( movableWithInvGameObjects[i], hitGameObject );
			}
		}
		private void AssignPickupDepositGoal( GameObject hitGameObject, ResourceDepositModule hitDeposit, Selectable[] selected )
		{
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<GameObject> movableWithInvGameObjects = new List<GameObject>();

			// Go pick up if the inventory can hold any of the resources in the deposit.
			Dictionary<string, int> resourcesInDeposit = hitDeposit.GetAll();

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


			if( movableWithInvGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ) );
			}
			for( int i = 0; i < movableWithInvGameObjects.Count; i++ )
			{
				TAIGoal.PickupDeposit.AssignTAIGoal( movableWithInvGameObjects[i], hitGameObject );
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


			if( toBeAssignedGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ) );
			}
			for( int i = 0; i < toBeAssignedGameObjects.Count; i++ )
			{
				TAIGoal.MakePayment.AssignTAIGoal( toBeAssignedGameObjects[i], paymentReceiverTransform.gameObject );
			}
		}

		public static Guid GetGuid( GameObject obj )
		{
			// gets guid of object.

			Unit unit = obj.GetComponent<Unit>();
			if( unit != null )
			{
				if( unit.guid == null )
				{
					throw new Exception( "Guid not assigned." );
				}
				return unit.guid.Value;
			}
			Building building = obj.GetComponent<Building>();
			if( building != null )
			{
				if( building.guid == null )
				{
					throw new Exception( "Guid not assigned." );
				}
				return building.guid.Value;
			}
			Hero hero = obj.GetComponent<Hero>();
			if( hero != null )
			{
				if( hero.guid == null )
				{
					throw new Exception( "Guid not assigned." );
				}
				return hero.guid.Value;
			}
			Projectile projectile = obj.GetComponent<Projectile>();
			if( projectile != null )
			{
				if( projectile.guid == null )
				{
					throw new Exception( "Guid not assigned." );
				}
				return projectile.guid.Value;
			}
			Extra extra = obj.GetComponent<Extra>();
			if( extra != null )
			{
				if( extra.guid == null )
				{
					throw new Exception( "Guid not assigned." );
				}
				return extra.guid.Value;
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

			Extra[] extras = Extra.GetAllExtras();
			for( int i = 0; i < extras.Length; i++ )
			{
				if( extras[i].guid == guid )
				{
					return extras[i].gameObject;
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

			return null;
		}
	}
}