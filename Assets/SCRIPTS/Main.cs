using UnityEngine;
using SS.Buildings;
using UnityEngine.EventSystems;
using SS.UI;
using SS.Extras;
using SS.Inventories;
using SS.ResourceSystem.Payment;
using System.Collections.Generic;
using UnityEngine.AI;
using SS.ResourceSystem;
using SS.Content;
using UnityEngine.Events;

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
				if( __particleSystemInstance == null ) { __particleSystemInstance = Instantiate( AssetManager.GetPrefab( AssetManager.RESOURCE_ID + "Prefabs/Particle System" ) ); }
				return __particleSystemInstance;
			}
		}


		private static Transform __worldUIs = null;
		public static Transform worldUIs
		{
			get
			{
				if( __worldUIs == null ) { __worldUIs = FindObjectOfType<Canvas>().transform.Find( "_WorldUIs" ); }
				return __worldUIs;
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

		private static Transform __main_transform = null;
		public static Transform main_transform
		{
			get
			{
				if( __main_transform == null )
				{
					__main_transform = FindObjectOfType<Main>().transform;
				}
				return __main_transform;
			}
		}

		private static ResourcePanel __resourcePanel = null;
		public static ResourcePanel resourcePanel
		{
			get
			{
				if( __resourcePanel == null )
				{
					__resourcePanel = FindObjectOfType<ResourcePanel>();
				}
				return __resourcePanel;
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
					PaymentReceiver hitPayment = null;

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
							PaymentReceiver payment = raycastHits[i].collider.GetComponent<PaymentReceiver>();
							if( payment != null )
							{
								hitPayment = payment;
							}

						}
					}


					if( hitDeposit == null && hitPayment == null && terrainHitPos.HasValue )
					{
						AssignMoveToGoal( terrainHitPos, Selection.selectedObjects );
					}

					else if( hitPayment != null )
					{
						AssignMakePaymentGoal( hitPayment, Selection.selectedObjects );
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
						if( !IsControllableByPlayer( gameObject, FactionManager.PLAYER ) )
						{
							return;
						}
						if( !Building.IsRepairable( gameObject.GetComponent<Damageable>() ) )
						{
							return;
						}
						// If it is a building, start repair.
						ConstructionSite.BeginConstructionOrRepair( gameObject );
						AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/ai_response" ) );
					}
				}
			}

			// Temporary resource payment speedup.
			if( Input.GetKeyDown( KeyCode.K ) )
			{
				if( !EventSystem.current.IsPointerOverGameObject() )
				{
					RaycastHit hitInfo;
					if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
					{
						GameObject gameObject = hitInfo.collider.gameObject;
						if( !IsControllableByPlayer( gameObject, FactionManager.PLAYER ) )
						{
							return;
						}
						PaymentReceiver pr = gameObject.GetComponent<PaymentReceiver>();
						if( pr != null )
						{
							// If it is a building, start repair.
							List<ResourceDefinition> ress = DataManager.GetAllOfType<ResourceDefinition>();

							foreach( var res in ress )
							{
								int amt = pr.GetWantedAmount( res.id );
								if( amt != 0 )
								{
									pr.ReceivePayment( res.id, amt );
								}
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
				if( !IsControllableByPlayer( selected[i].gameObject, FactionManager.PLAYER ) )
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
				AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/ai_response" ) );
			}
		}

		private void AssignDropoffToInventoryGoal( RaycastHit hitInfo, ResourceDeposit hitDeposit, Selectable[] selected )
		{
			List<GameObject> movableWithInvGameObjects = new List<GameObject>();

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			for( int i = 0; i < selected.Length; i++ )
			{
				bool suitable = true;

				if( !IsControllableByPlayer( selected[i].gameObject, FactionManager.PLAYER ) )
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
				TAIGoal.DropoffToInventory.AssignTAIGoal( movableWithInvGameObjects[i], hitInfo.collider.transform );
				AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/ai_response" ) );
			}
		}

		private void AssignMoveToGoal( Vector3? terrainHitPos, Selectable[] selected )
		{
			const float GRID_MARGIN = 0.125f;

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<GameObject> movableGameObjects = new List<GameObject>();

			float biggestRadius = float.MinValue;

			for( int i = 0; i < selected.Length; i++ )
			{
				if( !IsControllableByPlayer( selected[i].gameObject, FactionManager.PLAYER ) )
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
				Vector3 gridPositionWorld = TAIGoal.MoveTo.GridToWorld( kvp.Value, terrainHitPos.Value, biggestRadius * 2 + GRID_MARGIN );

				RaycastHit gridHit;
				Ray r = new Ray( gridPositionWorld + new Vector3( 0.0f, 50.0f, 0.0f ), Vector3.down );
				if( Physics.Raycast( r, out gridHit, 100.0f, ObjectLayer.TERRAIN_MASK ) )
				{
					TAIGoal.MoveTo.AssignTAIGoal( kvp.Key, gridPositionWorld );
					AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/ai_response" ) );
				}
				else
				{
					Debug.LogWarning( "Movement Grid position " + gridPositionWorld + " was outside of the map." );
				}
			}
		}

		private void AssignPickupDepositGoal( ResourceDeposit hitDeposit, Selectable[] selected )
		{
			// TODO ----- move this to separate method, propably to the specific tai goal class.

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<GameObject> movableWithInvGameObjects = new List<GameObject>();

			// Go pick up if the inventory can hold any of the resources in the deposit.
			Dictionary<string, int> resourcesInDeposit = hitDeposit.inventory.GetAll();

			for( int i = 0; i < selected.Length; i++ )
			{
				bool suitable = true;
				if( !IsControllableByPlayer( selected[i].gameObject, FactionManager.PLAYER ) )
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
				foreach( var kvp in resourcesInDeposit )
				{
					if( inv.GetMaxCapacity( kvp.Key ) == 0 )
					{
						suitable = false;
						break;
					}
					// don't move if can't pick up (inv full of that specific resource).
					if( inv.GetMaxCapacity( kvp.Key ) == inv.Get( kvp.Key ) )
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
				IInventory inv = movableWithInvGameObjects[i].GetComponent<IInventory>();

				TAIGoal.PickupDeposit.AssignTAIGoal( movableWithInvGameObjects[i], hitDeposit );
				AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/ai_response" ) );
			}
		}

		private void AssignMakePaymentGoal( PaymentReceiver hitPayment, Selectable[] selected )
		{
			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<GameObject> movableWithInvGameObjects = new List<GameObject>();

			for( int i = 0; i < selected.Length; i++ )
			{
				if( !IsControllableByPlayer( selected[i].gameObject, FactionManager.PLAYER ) )
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
				if( !hitPayment.ContainsWantedResource( inv.GetAll() ) )
				{
					continue;
				}

				movableWithInvGameObjects.Add( selected[i].gameObject );
			}


			for( int i = 0; i < movableWithInvGameObjects.Count; i++ )
			{
				TAIGoal.MakePayment.AssignTAIGoal( movableWithInvGameObjects[i], hitPayment );
				AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/ai_response" ) );
			}
		}
	}
}