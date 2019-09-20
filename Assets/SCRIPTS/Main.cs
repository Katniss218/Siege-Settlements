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

		private static Material __materialParticle = null;
		public static Material materialParticle
		{
			get
			{
				if( __materialParticle == null ) { __materialParticle = AssetManager.GetMaterial(AssetManager.RESOURCE_ID + "Materials/Particle" ); }
				return __materialParticle;
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



					Selectable[] selected = Selection.selectedObjects;

					List<GameObject> controllableGameObjects = new List<GameObject>( selected.Length );

					// Get the selected and controllable objects as array of GameObjects.
					for( int i = 0; i < selected.Length; i++ )
					{
						if( IsControllableByPlayer( selected[i].gameObject, FactionManager.PLAYER ) )
						{
							controllableGameObjects.Add( selected[i].gameObject );
						}
					}


					if( hitDeposit == null && hitPayment == null && terrainHitPos.HasValue )
					{
						//
						// MOVE.
						//

						float biggestRadius = float.MinValue;

						for( int i = 0; i < controllableGameObjects.Count; i++ )
						{
							NavMeshAgent navMeshAgent = controllableGameObjects[i].GetComponent<NavMeshAgent>();
							if( navMeshAgent != null )
							{
								if( navMeshAgent.radius > biggestRadius )
								{
									biggestRadius = navMeshAgent.radius;
								}
							}
						}

						//Calculate the grid position.
						TAIGoal.MoveTo.GridPositionInfo grid = TAIGoal.MoveTo.GetGridPositions( controllableGameObjects );

						foreach( var kvp in grid.positions )
						{
							const float GRID_MARGIN = 0.125f;

							Vector3 newV = TAIGoal.MoveTo.GridToWorld( kvp.Value, terrainHitPos.Value, biggestRadius * 2 + GRID_MARGIN );

							RaycastHit gridHit;
							Ray r = new Ray( newV + new Vector3( 0, 50, 0 ), Vector3.down );
							if( Physics.Raycast( r, out gridHit, 100, ObjectLayer.TERRAIN_MASK ) )
							{
								TAIGoal.MoveTo.AssignTAIGoal( kvp.Key, newV );
								AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/ai_response" ) );
							}
						}
					}
					else
					{
						if( hitDeposit != null )
						{
							//
							// PICKUP DEPOSIT.
							//

							for( int i = 0; i < controllableGameObjects.Count; i++ )
							{
								IInventory inv = controllableGameObjects[i].GetComponent<IInventory>();
								if( inv != null )
								{
									// Go pick up if the inventory can hold any of the resources in the deposit.
									Dictionary<string, int> resourcesInDeposit = hitDeposit.inventory.GetAll();
									foreach( var kvp in resourcesInDeposit )
									{
										if( inv.CanHold( kvp.Key ) )
										{
											TAIGoal.PickupDeposit.AssignTAIGoal( controllableGameObjects[i], hitDeposit );
											AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/ai_response" ) );
											break;
										}
									}
								}
							}
						}

						if( hitPayment != null )
						{
							//
							// PAY.
							//

							for( int i = 0; i < controllableGameObjects.Count; i++ )
							{
								IInventory inv = controllableGameObjects[i].GetComponent<IInventory>();
								if( inv != null )
								{
									// Assign the makePayment TAIGoal only if the selected object contains wanted resource in the inv.
									if( hitPayment.ContainsWantedResource( inv.GetAll() ) )
									{
										TAIGoal.MakePayment.AssignTAIGoal( controllableGameObjects[i], hitPayment );
										AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/ai_response" ) );
									}
								}
							}
						}

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

			if( Input.GetKeyDown( KeyCode.P ) )
			{
				if( !EventSystem.current.IsPointerOverGameObject() )
				{
					RaycastHit hitInfo;
					if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
					{
						ResourceDeposit hitDeposit = hitInfo.collider.GetComponent<ResourceDeposit>();

						Selectable[] selected = Selection.selectedObjects;

						List<GameObject> controllableGameObjects = new List<GameObject>( selected.Length );

						// Get the selected and controllable objects as array of GameObjects.
						for( int i = 0; i < selected.Length; i++ )
						{
							if( IsControllableByPlayer( selected[i].gameObject, FactionManager.PLAYER ) )
							{
								controllableGameObjects.Add( selected[i].gameObject );
							}
						}

						for( int i = 0; i < controllableGameObjects.Count; i++ )
						{
							IInventory inv = controllableGameObjects[i].GetComponent<IInventory>();
							if( inv != null )
							{
								TAIGoal.DropOffDeposit.AssignTAIGoal( controllableGameObjects[i], hitInfo.point );
								AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/ai_response" ) );
							}
						}
					}
				}
			}
			if( Input.GetKeyDown( KeyCode.Tab ) )
			{
				isHudLocked = !isHudLocked;

				onHudLockChange?.Invoke( isHudLocked );
			}
		}
	}
}