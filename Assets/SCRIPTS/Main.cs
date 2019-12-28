using SS.Content;
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
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;
using SS.AI;
using SS.AI.Goals;
using SS.Objects.Units;

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

		public static bool isHudForcedVisible { get; private set; }

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

		private void Inp_Right( InputQueue self )
		{
			TacticalGoalQuery.InputQuery( Main.camera.ScreenPointToRay( Input.mousePosition ) );
		}

		private void Inp_L( InputQueue self )
		{// Try repair mouseovered building.
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				RaycastHit hitInfo;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo, ObjectLayer.BUILDINGS_MASK ) )
				{
					Building building = hitInfo.collider.GetComponent<Building>();
					if( building == null )
					{
						SSObjectDFS damageable = hitInfo.collider.GetComponent<SSObjectDFS>();
						if( damageable == null )
						{
							return;
						}

						damageable.healthPercent += 0.1f;
					}
					else
					{
						/*if( building.factionId != LevelDataManager.PLAYER_FAC )
						{
							return;
						}*/
						if( !Building.IsRepairable( building ) )
						{
							return;
						}

						// If it is a building, start repair.
						// Empty ConstructionSiteData (no resources present).
						ConstructionSiteData constructionSiteData = new ConstructionSiteData();

						ConstructionSite.BeginConstructionOrRepair( building, constructionSiteData );
					}
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
						TacticalGoalQuery.AssignDropoffToInventoryGoal( hitInfo, hitInventory, Selection.selectedObjects );
					}
				}
			}
		}

		private void Inp_I( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				SSObjectDFS[] selected = Selection.selectedObjects;
				for( int i = 0; i < selected.Length; i++ )
				{
					RaycastHit hitInfo;
					if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
					{
						Unit unitRay = hitInfo.collider.GetComponent<Unit>();
						if( unitRay == null )
						{
							return;
						}
						/*
						TacticalGoalController goalController = selected[i].GetComponent<TacticalGoalController>();
						TacticalMoveToGoal goal = new TacticalMoveToGoal();
						goal.isHostile = false;
						goal.SetDestination( ssObject );
						goalController.goal = goal;
						*/

						TacticalGoalController goalControllerBeacon = unitRay.GetComponent<TacticalGoalController>();
						TacticalMakeFormationGoal goal = new TacticalMakeFormationGoal();
						goal.isHostile = false;
						goal.beacon = unitRay;
						goalControllerBeacon.goal = goal;

						TacticalGoalController goalController = selected[i].GetComponent<TacticalGoalController>();
						goal = new TacticalMakeFormationGoal();
						goal.isHostile = false;
						goal.beacon = unitRay;
						goalController.goal = goal;
					}
				}
			}
		}

		private void Inp_U( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				SSObjectDFS[] selected = Selection.selectedObjects;
				for( int i = 0; i < selected.Length; i++ )
				{
					Unit u = (Unit)selected[i];
					if( !u.CanChangePopulation() )
					{
						continue;
					}

					Unit.Split( u, PopulationSize.x1 );
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
					/*TacticalGoalController goalController = hitInfo.collider.GetComponent<TacticalGoalController>();
					if( goalController == null )
					{
						return;
					}
					TacticalTargetGoal goal = new TacticalTargetGoal();
					goalController.goal = goal;*/
					IPopulationScaler popScaler = hitInfo.collider.GetComponent<IPopulationScaler>();
					if( popScaler == null )
					{
						return;
					}
					if( popScaler is Unit )
					{
						if( !((Unit)popScaler).CanChangePopulation() )
						{
							return;
						}
					}
					switch( popScaler.population )
					{
						case PopulationSize.x1:
							popScaler.population = PopulationSize.x2;
							break;
						case PopulationSize.x2:
							popScaler.population = PopulationSize.x4;
							break;
						case PopulationSize.x4:
							popScaler.population = PopulationSize.x8;
							break;
						case PopulationSize.x8:
							popScaler.population = PopulationSize.x1;
							break;
					}
				}
			}
		}

		private void Inp_Y( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				RaycastHit hitInfo;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo, float.MaxValue, ObjectLayer.OBJECTS_MASK ) )
				{
					IDamageable damageable = hitInfo.collider.GetComponent<IDamageable>();

					if( damageable == null )
					{
						SSObject ssObject = hitInfo.collider.GetComponent<SSObject>();
						if( ssObject == null )
						{
							return;
						}
						ssObject.Destroy();
					}
					else
					{
						damageable.Die();
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
					SSObjectDFS sel = hitInfo.collider.GetComponent<SSObjectDFS>();
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
						Selection.TrySelect( new SSObjectDFS[] { sel } );
					}
				}
			}
		}

		private void Inp_Tab( InputQueue self )
		{
			isHudForcedVisible = !isHudForcedVisible;

			onHudLockChange?.Invoke( isHudForcedVisible );
		}

		private static void SetFactionSelected( int fac )
		{
			SSObjectDFS[] selected = Selection.selectedObjects;
			for( int i = 0; i < selected.Length; i++ )
			{
				IFactionMember faction = selected[i].GetComponent<IFactionMember>();

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

					GameObject extra = ExtraCreator.Create( def, data.guid );
					ExtraCreator.SetData( extra, data );

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

						GameObject hero = HeroCreator.Create( def, data.guid );
						HeroCreator.SetData( hero, data );
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

						GameObject extra = ExtraCreator.Create( def, data.guid );
						ExtraCreator.SetData( extra, data );
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
			SSObjectDFS.onHealthChangeAny.AddListener( ( SSObjectDFS obj, float deltaHealth ) =>
			{
				if( Selection.IsDisplayedGroup() )
				{
					SSObjectDFS ssObjectSelectable = obj.GetComponent<SSObjectDFS>();
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
				Main.keyboardInput.RegisterOnPress( KeyCode.I, 60.0f, Inp_I, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.U, 60.0f, Inp_U, true );
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
				Main.keyboardInput.ClearOnPress( KeyCode.I, Inp_I );
				Main.keyboardInput.ClearOnPress( KeyCode.U, Inp_U );
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


		public static bool IsInRange( Vector3 pos1, Vector3 pos2, float threshold )
		{
			return (pos1 - pos2).sqrMagnitude <= threshold * threshold;
		}
	}
}