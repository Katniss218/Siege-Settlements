using SS.AI;
using SS.AI.Goals;
using SS.Content;
using SS.InputSystem;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Objects;
using SS.Objects.Extras;
using SS.Objects.Heroes;
using SS.Objects.Modules;
using SS.Objects.Units;
using SS.ResourceSystem.Payment;
using SS.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

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

		private static Transform __levelGUICanvas = null;
		public static Transform levelGUICanvas
		{
			get
			{
				if( __levelGUICanvas == null )
				{
					Canvas[] canvases = FindObjectsOfType<Canvas>();
					for( int i = 0; i < canvases.Length; i++ )
					{
						if( canvases[i].gameObject.CompareTag( "Level GUI canvas" ) )
						{
							__levelGUICanvas = canvases[i].transform;
							break;
						}
					}
				}
				return __levelGUICanvas;
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

		private void Inp_F1( InputQueue self )
		{
			if( levelGUICanvas.gameObject.activeSelf )
			{
				levelGUICanvas.gameObject.SetActive( false );
				objectHUDCanvas.gameObject.SetActive( false );
				ToolTip.canvas.gameObject.SetActive( false );
			}
			else
			{
				levelGUICanvas.gameObject.SetActive( true );
				objectHUDCanvas.gameObject.SetActive( true );
				ToolTip.canvas.gameObject.SetActive( true );
			}
		}

		private void Inp_Right( InputQueue self )
		{
			TacticalGoalQuery.QueryAt( Main.camera.ScreenPointToRay( Input.mousePosition ) );
		}

		private void Inp_L( InputQueue self )
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
					goalController.SetGoals( TacticalGoalQuery.TAG_CUSTOM, goal );
				}
			}
		}
		
		private void Inp_K( InputQueue self )
		{
			// Temporary resource payment speedup (every payment receiver to full).
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

		private void Inp_SplitUnit( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				SSObject[] selected = Selection.GetSelectedObjects();
				for( int i = 0; i < selected.Length; i++ )
				{
					if( !(selected[i] is Unit) )
					{
						continue;
					}

					Unit unit = (Unit)selected[i];
					if( unit.factionId != LevelDataManager.PLAYER_FAC )
					{
						continue;
					}
					
					if( unit.population == PopulationSize.x1 )
					{
						continue;
					}
					if( !unit.CanChangePopulation() )
					{
						continue;
					}

					PopulationUnitUtils.Split( unit, null );

					AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), unit.transform.position );
				}
			}
		}

		private void Inp_CombineUnit( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				RaycastHit hitInfo;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
				{
					Unit unitRay = hitInfo.collider.GetComponent<Unit>();
					if( unitRay == null )
					{
						return;
					}

					if( unitRay.factionId != LevelDataManager.PLAYER_FAC )
					{
						return;
					}

					if( unitRay.population == PopulationSize.x8 )
					{
						return;
					}

					TacticalGoalController goalControllerBeacon = unitRay.controller;
					TacticalMakeFormationGoal goal = new TacticalMakeFormationGoal();
					goal.isHostile = false;
					goal.beacon = unitRay;
					goalControllerBeacon.SetGoals( TacticalGoalQuery.TAG_CUSTOM, goal );

					SSObject[] selected = Selection.GetSelectedObjects();
					for( int i = 0; i < selected.Length; i++ )
					{
						if( !(selected[i] is Unit) )
						{
							continue;
						}

						Unit unit = (Unit)selected[i];

						if( unit.factionId != LevelDataManager.PLAYER_FAC )
						{
							continue;
						}

						if( unit.population == PopulationSize.x8 )
						{
							continue;
						}
						
						TacticalGoalController goalControllerSelected = unit.controller;
						goal = new TacticalMakeFormationGoal();
						goal.isHostile = false;
						goal.beacon = unitRay;
						goalControllerSelected.SetGoals( TacticalGoalQuery.TAG_CUSTOM, goal );

						AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), unitRay.transform.position );
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
					Unit unit = hitInfo.collider.GetComponent<Unit>();
					if( unit == null )
					{
						return;
					}

					if( !unit.CanChangePopulation() )
					{
						return;
					}
					
					switch( unit.population )
					{
						case PopulationSize.x1:
							unit.SetPopulation( PopulationSize.x2, true, true );
							break;
						case PopulationSize.x2:
							unit.SetPopulation( PopulationSize.x4, true, true );
							break;
						case PopulationSize.x4:
							unit.SetPopulation( PopulationSize.x8, true, true );
							break;
						case PopulationSize.x8:
							unit.SetPopulation( PopulationSize.x1, true, true );
							break;
					}
				}
			}
		}

		private void Inp_O( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				RaycastHit hitInfo;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo, float.MaxValue, ObjectLayer.POTENTIALLY_INTERACTIBLE_MASK ) )
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
					SSObjectDFC sel = hitInfo.collider.GetComponent<SSObjectDFC>();
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
						Selection.TrySelect( new SSObjectDFC[] { sel } );
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
			SSObject[] selected = Selection.GetSelectedObjects();
			for( int i = 0; i < selected.Length; i++ )
			{
				if( !(selected[i] is IFactionMember) )
				{
					return;
				}

				((IFactionMember)selected[i]).factionId = fac;
			}
		}

		private void Inp_F3( InputQueue self )
		{
			SSObjectDFC[] damageables = SSObject.GetAllDFC();

			for( int i = 0; i < damageables.Length; i++ )
			{
				damageables[i].health = damageables[i].healthMax;
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

					Extra extra = ExtraCreator.Create( def, data.guid );
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

						Hero hero = HeroCreator.Create( def, data.guid );
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
						UnitDefinition def = DefinitionManager.GetUnit( "unit.civilian" );
						UnitData data = new UnitData();
						data.guid = Guid.NewGuid();
						data.position = hitInfo.point;
						data.rotation = Quaternion.Euler( 0, UnityEngine.Random.Range( -180.0f, 180.0f ), 0 );
						data.factionId = 0;
						data.health = def.healthMax;
						data.population = PopulationSize.x1;

						Unit unit = UnitCreator.Create( def, data.guid );
						UnitCreator.SetData( unit, data );
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
			SSObjectDFC.onHealthChangeAny.AddListener( ( IDamageable obj, float deltaHealth ) =>
			{
				if( Selection.IsDisplayedGroup() )
				{
					if( !Selection.IsSelected( (SSObject)obj ) )
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
				Main.keyboardInput.RegisterOnPress( KeyCode.F1, 99.0f, Inp_F1, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.F3, 99.0f, Inp_F3, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.L, 60.0f, Inp_L, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.K, 60.0f, Inp_K, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.G, 60.0f, Inp_SplitUnit, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.H, 60.0f, Inp_CombineUnit, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.P, 60.0f, Inp_P, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.O, 60.0f, Inp_O, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Tab, 60.0f, Inp_Tab, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha1, -60.0f, Inp_A1, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha2, -60.0f, Inp_A2, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha3, -60.0f, Inp_A3, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha4, -60.0f, Inp_A4, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha5, -60.0f, Inp_A5, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha6, -60.0f, Inp_A6, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha7, -60.0f, Inp_A7, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha8, -60.0f, Inp_A8, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha9, -60.0f, Inp_A9, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha0, -60.0f, Inp_A0, true );
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
				Main.keyboardInput.ClearOnPress( KeyCode.F1, Inp_F1 );
				Main.keyboardInput.ClearOnPress( KeyCode.F3, Inp_F3 );
				Main.keyboardInput.ClearOnPress( KeyCode.L, Inp_L );
				Main.keyboardInput.ClearOnPress( KeyCode.K, Inp_K );
				Main.keyboardInput.ClearOnPress( KeyCode.G, Inp_SplitUnit );
				Main.keyboardInput.ClearOnPress( KeyCode.H, Inp_CombineUnit );
				Main.keyboardInput.ClearOnPress( KeyCode.P, Inp_P );
				Main.keyboardInput.ClearOnPress( KeyCode.O, Inp_O );
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


		public static float CalculateHitChance( SSObject target, float attackerPosY )
		{
			if( target == null )
            {
				throw new ArgumentNullException( "Target can't be null." );
            }

			const float LINEAR_HIT_GROW_FACTOR = 0.5f; // how much the chance to hit increases per 1 meter offset.

			float perc = 0;

			if( target is IInteriorUser )
			{
				IInteriorUser targetInteriorUser = (IInteriorUser)target;

				if( targetInteriorUser.isInside )
				{
					(InteriorModule.Slot slot, HUDInteriorSlot _) = targetInteriorUser.interior.GetSlot( targetInteriorUser.slotType, targetInteriorUser.slotIndex );

					perc = Mathf.LerpUnclamped( 1, 1 + LINEAR_HIT_GROW_FACTOR, (attackerPosY - targetInteriorUser.interior.transform.position.y) );

					perc -= slot.coverValue;

					return perc;
				}
			}

			perc = Mathf.LerpUnclamped( 1, 1 + LINEAR_HIT_GROW_FACTOR, (attackerPosY - target.transform.position.y) );
			return perc;
		}

		/// <summary>
		/// Random-chance boolean
		/// </summary>
		public static bool IsHit( float hitChancePerc )
		{
			float p = UnityEngine.Random.Range( 0.0f, 1.0f );
			return p <= hitChancePerc;
		}
	}
}