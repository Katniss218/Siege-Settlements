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

namespace SS
{
	/// <summary>
	/// The main class, think of it like a Game Manager class.
	/// </summary>
	public class Main : MonoBehaviour
	{
		private static GameObject __particleSystemInstance = null;
		new public static GameObject particleSystem
		{
			get
			{
				if( __particleSystemInstance == null ) { __particleSystemInstance = Instantiate( AssetManager.GetPrefab( "resource:Prefabs/Particle System" ) ); }
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
		/*
		// TODO ----- Change this to class with ShaderType & materials.
		
		private static Material __materialFactionColored = null;
		public static Material materialFactionColored
		{
			get
			{
				if( __materialFactionColored == null ) { __materialFactionColored = AssetManager.GetMaterial( "resource:Materials/FactionColored" ); }
				return __materialFactionColored;
			}
		}

		private static Material __materialFactionColoredDestroyable = null;
		public static Material materialFactionColoredDestroyable
		{
			get
			{
				if( __materialFactionColoredDestroyable == null ) { __materialFactionColoredDestroyable = AssetManager.GetMaterial( "resource:Materials/FactionColoredDestroyable" ); }
				return __materialFactionColoredDestroyable;
			}
		}

		private static Material __materialFactionColoredConstructible = null;
		public static Material materialFactionColoredConstructible
		{
			get
			{
				if( __materialFactionColoredConstructible == null ) { __materialFactionColoredConstructible = AssetManager.GetMaterial( "resource:Materials/FCConstructible" ); }
				return __materialFactionColoredConstructible;
			}
		}

		private static Material __materialSolid = null;
		public static Material materialSolid
		{
			get
			{
				if( __materialSolid == null ) { __materialSolid = AssetManager.GetMaterial( "resource:Materials/Solid" ); }
				return __materialSolid;
			}
		}

		private static Material __materialPlantTransparent = null;
		public static Material materialPlantTransparent
		{
			get
			{
				if( __materialPlantTransparent == null ) { __materialPlantTransparent = AssetManager.GetMaterial( "resource:Materials/PlantTransparent" ); }
				return __materialPlantTransparent;
			}
		}

		private static Material __materialPlantSolid = null;
		public static Material materialPlantSolid
		{
			get
			{
				if( __materialPlantSolid == null ) { __materialPlantSolid = AssetManager.GetMaterial( "resource:Materials/PlantSolid" ); }
				return __materialPlantSolid;
			}
		}
		*/
		private static Material __materialParticle = null;
		public static Material materialParticle
		{
			get
			{
				if( __materialParticle == null ) { __materialParticle = AssetManager.GetMaterial( "resource:Materials/Particle" ); }
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


					
					Selectable[] selected = SelectionManager.selectedObjects;

					if( hitDeposit == null && hitPayment == null && terrainHitPos.HasValue )
					{
						//
						// MOVE.
						//

						// Get the selected object as array of GameObjects.
						GameObject[] selectedGameObjects = new GameObject[selected.Length];
						float biggestRadius = float.MinValue;

						for( int i = 0; i < selected.Length; i++ )
						{
							selectedGameObjects[i] = selected[i].gameObject;


							NavMeshAgent navMeshAgent = selected[i].GetComponent<NavMeshAgent>();
							if( navMeshAgent != null )
							{
								if( navMeshAgent.radius > biggestRadius )
								{
									biggestRadius = navMeshAgent.radius;
								}
							}
						}

						//Calculate the grid position.
						TAIGoal.MoveTo.GridPositionInfo grid = TAIGoal.MoveTo.GetGridPositions( selectedGameObjects );

						foreach( var kvp in grid.positions )
						{
							const float GRID_SPACING = 0.125f;

							Vector3 newV = TAIGoal.MoveTo.GridToWorld( kvp.Value, terrainHitPos.Value, biggestRadius * 2 + GRID_SPACING );
														
							RaycastHit gridHit;
							Ray r = new Ray( newV + new Vector3( 0, 50, 0 ), Vector3.down );
							if( Physics.Raycast( r, out gridHit, 100, ObjectLayer.TERRAIN_MASK ) )
							{
								TAIGoal.MoveTo.AssignTAIGoal( kvp.Key, newV );
								AudioManager.PlayNew( AssetManager.GetAudioClip( "resource:Sounds/ai_response" ) );
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

							for( int i = 0; i < selected.Length; i++ )
							{
								IInventory inv = selected[i].gameObject.GetComponent<IInventory>();
								if( inv != null )
								{
									if( inv.CanHold( hitDeposit.resourceId ) )
									{
										TAIGoal.PickupDeposit.AssignTAIGoal( selected[i].gameObject, hitDeposit );
										AudioManager.PlayNew( AssetManager.GetAudioClip( "resource:Sounds/ai_response" ) );
									}
								}
							}
						}
						
						if( hitPayment != null )
						{
							//
							// PAY.
							//

							for( int i = 0; i < selected.Length; i++ )
							{
								IInventory inv = selected[i].gameObject.GetComponent<IInventory>();
								if( inv != null )
								{
									// Assign the makePayment TAIGoal only if the selected object contains wanted resource in the inv.
									if( hitPayment.ContainsWantedResource( inv.GetAll() ) )
									{
										TAIGoal.MakePayment.AssignTAIGoal( selected[i].gameObject, hitPayment );
										AudioManager.PlayNew( AssetManager.GetAudioClip( "resource:Sounds/ai_response" ) );
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
						if( hitInfo.collider.gameObject.layer != ObjectLayer.BUILDINGS )
						{
							return;
						}
						if( Building.IsRepairable( hitInfo.collider.GetComponent<Damageable>() ) )
						{
							return;
						}
						// If it is a building, start repair.
						ConstructionSite.BeginConstructionOrRepair( hitInfo.collider.gameObject );
						AudioManager.PlayNew( AssetManager.GetAudioClip( "resource:Sounds/ai_response" ) );
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
						PaymentReceiver pr = hitInfo.collider.GetComponent<PaymentReceiver>();
						if( pr != null )
						{
							// If it is a building, start repair.
							List<ResourceDefinition> ress = Content.DataManager.GetAllOfType<ResourceDefinition>();

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
						Selectable[] selected = SelectionManager.selectedObjects;
						for( int i = 0; i < selected.Length; i++ )
						{
							IInventory inv = selected[i].GetComponent<IInventory>();
							if( inv != null )
							{
								TAIGoal.DropOffDeposit.AssignTAIGoal( selected[i].gameObject, hitInfo.point );
								AudioManager.PlayNew( AssetManager.GetAudioClip( "resource:Sounds/ai_response" ) );
							}
						}
					}
				}
			}
		}
	}
}