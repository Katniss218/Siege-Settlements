using SS.Content;
using SS.Diplomacy;
using SS.InputSystem;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Objects.Modules;
using SS.Objects;
using SS.Objects.Buildings;
using SS.Objects.Extras;
using SS.Objects.Heroes;
using SS.ResourceSystem.Payment;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;
using SS.AI;
using SS.AI.Goals;

namespace SS
{
	/// <summary>
	/// The main class, think of it like a Game Manager class.
	/// </summary>
	public class Main : MonoBehaviour
	{
		public const float DEFAULT_NAVMESH_BASE_OFFSET = -0.075f;
		public const float DEFAULT_NAVMESH_ACCELERATION = 8.0f;
		public const float DEFAULT_NAVMESH_STOPPING_DIST = 0.01f;
		public const float DEFAULT_NAVMESH_STOPPING_DIST_CUSTOM = 0.125f;

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
				if( __particleSystemInstance == null ) { __particleSystemInstance = Object.Instantiate( (GameObject)AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Particle System" ) ); }
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
					for( int i = 0; i < canvases.Length; i++ )
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
				if( __keyboardInput == null ) { __keyboardInput = Object.FindObjectOfType<QueuedKeyboardInput>(); }
				return __keyboardInput;
			}
		}

		private static QueuedMouseInput __mouseInput = null;
		public static QueuedMouseInput mouseInput
		{
			get
			{
				if( __mouseInput == null ) { __mouseInput = Object.FindObjectOfType<QueuedMouseInput>(); }
				return __mouseInput;
			}
		}
		
		private static Transform __cameraPivot = null;
		public static Transform cameraPivot
		{
			get
			{
				if( __cameraPivot == null )
				{
					__cameraPivot = Object.FindObjectOfType<CameraController>().transform;
				}
				return __cameraPivot;
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
					InventoryModule hitInventory = hitInfo.collider.GetComponent<InventoryModule>();


					if( hitInventory != null )
					{
						AssignDropoffToInventoryGoal( hitInfo, hitInventory, Selection.selectedObjects );
					}
				}
			}
		}

		private void Inp_P( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				RaycastHit hitInfo;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
				{
					TacticalGoalController goalController = hitInfo.collider.GetComponent<TacticalGoalController>();
					if( goalController == null )
					{
						return;
					}
					TacticalTargetGoal goal = new TacticalTargetGoal();
					goalController.goal = goal;
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

					//if( damageable == null )
					//{
					//	Object.Destroy( hitInfo.collider.gameObject );
					//}
					//else
					if( damageable != null )
					{
						damageable.TakeDamageUnscaled( 99999999.99f );
					}
				}
			}
		}

		private void Inp_H( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				RaycastHit hitInfo;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
				{
					SSObjectSelectable sel = hitInfo.collider.GetComponent<SSObjectSelectable>();
					if( sel == null )
					{
						return;
					}

					if( Selection.IsSelected( sel ) )
					{
						Selection.Deselect( sel );
					}
					else
					{
						Selection.TrySelect( new SSObjectSelectable[] { sel } );
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
			SSObjectSelectable[] selected = Selection.selectedObjects;
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
			if( PauseManager.isPaused )
			{
				return;
			}
			SetFactionSelected( 0 );
		}

		private void Inp_A2( InputQueue self )
		{
			if( PauseManager.isPaused )
			{
				return;
			}
			SetFactionSelected( 1 );
		}

		private void Inp_A3( InputQueue self )
		{
			if( PauseManager.isPaused )
			{
				return;
			}
			SetFactionSelected( 2 );
		}

		private static void CreateDepositRaycast( string id )
		{
			if( PauseManager.isPaused )
			{
				return;
			}
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
			if( PauseManager.isPaused )
			{
				return;
			}
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

		private void Inp_A5( InputQueue self )
		{
			if( PauseManager.isPaused )
			{
				return;
			}
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				RaycastHit hitInfo;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
				{
					if( hitInfo.collider.gameObject.layer == ObjectLayer.TERRAIN )
					{
						ExtraDefinition def = DefinitionManager.GetExtra( "extra.grass" );
						ExtraData data = new ExtraData();
						data.guid = Guid.NewGuid();
						data.position = hitInfo.point;
						data.rotation = Quaternion.Euler( 0, UnityEngine.Random.Range( -180.0f, 180.0f ), 0 );

						GameObject extra = ExtraCreator.Create( def, data );
					}
				}
			}
		}

		private void Inp_A6( InputQueue self )
		{
			if( PauseManager.isPaused )
			{
				return;
			}
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				CreateDepositRaycast( "resource_deposit.iron_ore_0" );
			}
		}

		private void Inp_A7( InputQueue self )
		{
			if( PauseManager.isPaused )
			{
				return;
			}
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				CreateDepositRaycast( "resource_deposit.sulphur_ore_0" );
			}
		}

		private void Inp_A8( InputQueue self )
		{
			if( PauseManager.isPaused )
			{
				return;
			}
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				CreateDepositRaycast( "resource_deposit.tree" );
			}
		}

		private void Inp_A9( InputQueue self )
		{
			if( PauseManager.isPaused )
			{
				return;
			}
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				CreateDepositRaycast( "resource_deposit.pine" );
			}
		}

		private void Inp_A0( InputQueue self )
		{
			if( PauseManager.isPaused )
			{
				return;
			}
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



		void Start()
		{
			Damageable.onHealthChangeAny.AddListener( ( Damageable obj, float deltaHealth ) =>
			{
				if( Selection.IsDisplayedGroup() )
				{
					SSObjectSelectable ssObjectSelectable = obj.GetComponent<SSObjectSelectable>();
					if( ssObjectSelectable  == null )
					{
						return;
					}
					if( !Selection.IsSelected( ssObjectSelectable ) )
					{
						return;
					}
					Selection.StopDisplaying();
					Selection.DisplayGroupSelected();
				}
			} );
		}

		void OnEnable()
		{
			if( Main.mouseInput != null )
			{
				Main.mouseInput.RegisterOnPress( MouseCode.RightMouseButton, 60.0f, Inp_Right, true );
			}
			if( Main.keyboardInput != null )
			{
				Main.keyboardInput.RegisterOnPress( KeyCode.L, 60.0f, Inp_L, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.K, 60.0f, Inp_K, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.O, 60.0f, Inp_O, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.P, 60.0f, Inp_P, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Y, 60.0f, Inp_Y, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.H, 60.0f, Inp_H, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Tab, 60.0f, Inp_Tab, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha1, 60.0f, Inp_A1, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha2, 60.0f, Inp_A2, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha3, 60.0f, Inp_A3, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha4, 60.0f, Inp_A4, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha5, 60.0f, Inp_A5, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha6, 60.0f, Inp_A6, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha7, 60.0f, Inp_A7, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha8, 60.0f, Inp_A8, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha9, 60.0f, Inp_A9, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha0, 60.0f, Inp_A0, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Pause, 60.0f, Inp_Pause, true );
			}
		}

		void OnDisable()
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
				Main.keyboardInput.ClearOnPress( KeyCode.P, Inp_P );
				Main.keyboardInput.ClearOnPress( KeyCode.Y, Inp_Y );
				Main.keyboardInput.ClearOnPress( KeyCode.H, Inp_H );
				Main.keyboardInput.ClearOnPress( KeyCode.Tab, Inp_Tab );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha1, Inp_A1 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha2, Inp_A2 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha3, Inp_A3 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha4, Inp_A4 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha5, Inp_A5 );
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


		private void AssignAttackGoal( Damageable target, SSObjectSelectable[] selected )
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
					movableGameObjects.Add( selected[i].gameObject );
				}
			}

			if( movableGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ) );
			}
			for( int i = 0; i < movableGameObjects.Count; i++ )
			{
				TacticalGoalController goalController = movableGameObjects[i].GetComponent<TacticalGoalController>();
				TacticalTargetGoal goal = new TacticalTargetGoal();
				goal.target = target;
				goal.targetForced = true;
				goalController.goal = goal;
			}
		}
		
		private void AssignDropoffToInventoryGoal( RaycastHit hitInfo, InventoryModule hitInventory, SSObjectSelectable[] selected )
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

		private void AssignMoveToGoal( Vector3 terrainHitPos, SSObjectSelectable[] selected )
		{
			const float GRID_MARGIN = 0.125f;

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<SSObject> movableGameObjects = new List<SSObject>();

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

		private void AssignPickupInventoryGoal( SSObject hitSSObject, InventoryModule hitInventory, SSObjectSelectable[] selected )
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
		private void AssignPickupDepositGoal( SSObject hitSSObject, ResourceDepositModule hitDeposit, SSObjectSelectable[] selected )
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

		private void AssignMakePaymentGoal( Transform paymentReceiverTransform, IPaymentReceiver[] paymentReceivers, SSObjectSelectable[] selected )
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

		public static bool IsInRange( Vector3 pos1, Vector3 pos2, float threshold )
		{
			return (pos1 - pos2).sqrMagnitude <= threshold * threshold;
		}

		public static SSObject GetSSObject( Guid guid )
		{
			SSObject[] ssObjectArray = SSObject.GetAllSSObjects();

			for( int i = 0; i < ssObjectArray.Length; i++ )
			{
				if( ssObjectArray[i].guid == guid )
				{
					return ssObjectArray[i];
				}
			}
			return null;
		}
	}
}