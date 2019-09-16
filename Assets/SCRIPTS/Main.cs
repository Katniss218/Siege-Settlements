using UnityEngine;
using SS.Buildings;
using UnityEngine.EventSystems;
using SS.UI;
using SS.Extras;
using SS.Inventories;
using SS.ResourceSystem.Payment;
using System.Collections.Generic;
using UnityEngine.AI;

namespace SS
{
	/// <summary>
	/// The main class, think of it like a Game Manager class.
	/// </summary>
	public class Main : MonoBehaviour
	{
		private static AudioClip __aiResponse = null;
		public static AudioClip aiResponse
		{
			get
			{
				if( __aiResponse == null ) { __aiResponse = Resources.Load<AudioClip>( "Sounds/ai_response" ); }
				return __aiResponse;
			}
		}

		private static AudioClip __selectSound = null;
		public static AudioClip selectSound
		{
			get
			{
				if( __selectSound == null ) { __selectSound = Resources.Load<AudioClip>( "Sounds/select" ); }
				return __selectSound;
			}
		}

		private static AudioClip __deselectSound = null;
		public static AudioClip deselectSound
		{
			get
			{
				if( __deselectSound == null ) { __deselectSound = Resources.Load<AudioClip>( "Sounds/deselect" ); }
				return __deselectSound;
			}
		}

		private static GameObject __unitHUD = null;
		public static GameObject unitHUD
		{
			get
			{
				if( __unitHUD == null ) { __unitHUD = Resources.Load<GameObject>( "Prefabs/unit_hud" ); }
				return __unitHUD;
			}
		}

		private static GameObject __buildingHUD = null;
		public static GameObject buildingHUD
		{
			get
			{
				if( __buildingHUD == null ) { __buildingHUD = Resources.Load<GameObject>( "Prefabs/building_hud" ); }
				return __buildingHUD;
			}
		}

		private static GameObject __heroHUD = null;
		public static GameObject heroHUD
		{
			get
			{
				if( __heroHUD == null ) { __heroHUD = Resources.Load<GameObject>( "Prefabs/hero_hud" ); }
				return __heroHUD;
			}
		}

		private static GameObject __particleSystem = null;
		new public static GameObject particleSystem
		{
			get
			{
				if( __particleSystem == null ) { __particleSystem = Instantiate( Resources.Load<GameObject>( "Prefabs/Particle System" ) ); }
				return __particleSystem;
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

		private static Sprite __switcherObj = null;
		public static Sprite switcherObj
		{
			get
			{
				if( __switcherObj == null ) { __switcherObj = Resources.Load<Sprite>( "Textures/obj_lst" ); }
				return __switcherObj;
			}
		}

		private static Sprite __switcherList = null;
		public static Sprite switcherList
		{
			get
			{
				if( __switcherList == null ) { __switcherList = Resources.Load<Sprite>( "Textures/lst_obj" ); }
				return __switcherList;
			}
		}

		private static Sprite __tooltipBackground = null;
		public static Sprite toolTipBackground
		{
			get
			{
				if( __tooltipBackground == null ) { __tooltipBackground = Resources.Load<Sprite>( "Textures/tooltip_background" ); }
				return __tooltipBackground;
			}
		}

		private static Sprite __uiButton = null;
		public static Sprite uiButton
		{
			get
			{
				if( __uiButton == null ) { __uiButton = Resources.Load<Sprite>( "Textures/button" ); }
				return __uiButton;
			}
		}

		private static Sprite __uiKnob = null;
		public static Sprite uiKnob
		{
			get
			{
				if( __uiKnob == null ) { __uiKnob = Resources.Load<Sprite>( "Textures/knob" ); }
				return __uiKnob;
			}
		}

		private static Sprite __uiScrollArea = null;
		public static Sprite uiScrollArea
		{
			get
			{
				if( __uiScrollArea == null ) { __uiScrollArea = Resources.Load<Sprite>( "Textures/scroll_area" ); }
				return __uiScrollArea;
			}
		}

		private static Sprite __uiScrollHandle = null;
		public static Sprite uiScrollHandle
		{
			get
			{
				if( __uiScrollHandle == null ) { __uiScrollHandle = Resources.Load<Sprite>( "Textures/scrollbar_handle" ); }
				return __uiScrollHandle;
			}
		}

		private static Material __materialFactionColored = null;
		public static Material materialFactionColored
		{
			get
			{
				if( __materialFactionColored == null ) { __materialFactionColored = new Material( Resources.Load<Shader>( "Shaders/FactionColored" ) ); }
				return __materialFactionColored;
			}
		}

		private static Material __materialFactionColoredDestroyable = null;
		public static Material materialFactionColoredDestroyable
		{
			get
			{
				if( __materialFactionColoredDestroyable == null ) { __materialFactionColoredDestroyable = new Material( Resources.Load<Shader>( "Shaders/FactionColoredDestroyable" ) ); }
				return __materialFactionColoredDestroyable;
			}
		}

		private static Material __materialFactionColoredConstructible = null;
		public static Material materialFactionColoredConstructible
		{
			get
			{
				if( __materialFactionColoredConstructible == null ) { __materialFactionColoredConstructible = new Material( Resources.Load<Shader>( "Shaders/FCConstructible" ) ); }
				return __materialFactionColoredConstructible;
			}
		}

		private static Material __materialSolid = null;
		public static Material materialSolid
		{
			get
			{
				if( __materialSolid == null ) { __materialSolid = new Material( Resources.Load<Shader>( "Shaders/Solid" ) ); }
				return __materialSolid;
			}
		}

		private static Material __materialPlantTransparent = null;
		public static Material materialPlantTransparent
		{
			get
			{
				if( __materialPlantTransparent == null ) { __materialPlantTransparent = new Material( Resources.Load<Shader>( "Shaders/PlantTransparent" ) ); }
				return __materialPlantTransparent;
			}
		}

		private static Material __materialPlantSolid = null;
		public static Material materialPlantSolid
		{
			get
			{
				if( __materialPlantSolid == null ) { __materialPlantSolid = new Material( Resources.Load<Shader>( "Shaders/PlantSolid" ) ); }
				return __materialPlantSolid;
			}
		}

		private static Material __materialParticle = null;
		public static Material materialParticle
		{
			get
			{
				if( __materialParticle == null ) { __materialParticle = Resources.Load<Material>( "Materials/Particle" ); }
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

		private Vector3 GridToWorld( Vector2Int grid, Vector3 gridCenter, float gridSpacing )
		{
			float camRotY = Main.cameraPivot.rotation.eulerAngles.y;

			Vector3 gridRelativeToCenterLocal = new Vector3( grid.x, 0, grid.y ) - new Vector3( gridSpacing / 2f, 0, gridSpacing / 2f );
			
			Vector3 gridRelativeToCenterLocalRotated = Quaternion.Euler( 0, camRotY, 0 ) * (gridRelativeToCenterLocal);

			Vector3 global = gridRelativeToCenterLocalRotated * gridSpacing + gridCenter;
			
			// calculate each node's world position (add the grid's center to it).

			return global;
		}
		
		void Update()
		{
			// When RMB is clicked - Move selected units to the cursor.
			if( Input.GetMouseButtonDown( 1 ) )
			{
				if( !EventSystem.current.IsPointerOverGameObject() )
				{
					RaycastHit[] raycastHits = Physics.RaycastAll( Main.camera.ScreenPointToRay( Input.mousePosition ) );

					Vector3? terrainHitPos = null;

					ResourceDeposit hitDeposit = null;
					PaymentReceiver hitPayment = null;

					int terrainLayer = LayerMask.NameToLayer( "Terrain" );

					for( int i = 0; i < raycastHits.Length; i++ )
					{
						if( raycastHits[i].collider.gameObject.layer == terrainLayer )
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

							Vector3 newV = GridToWorld( kvp.Value, terrainHitPos.Value, biggestRadius * 2 + GRID_SPACING );
														
							RaycastHit gridHit;
							Ray r = new Ray( newV + new Vector3( 0, 50, 0 ), Vector3.down );
							if( Physics.Raycast( r, out gridHit, 100, 1 << LayerMask.NameToLayer( "Terrain" ) ) )
							{
								TAIGoal.MoveTo.AssignTAIGoal( kvp.Key, newV );
								AudioManager.PlayNew( aiResponse );
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
										AudioManager.PlayNew( aiResponse );
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
										AudioManager.PlayNew( aiResponse );
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
						if( hitInfo.collider.gameObject.layer != LayerMask.NameToLayer( "Buildings" ) )
						{
							return;
						}
						if( hitInfo.collider.GetComponent<Damageable>().healthPercent == 1.0f )
						{
							return;
						}
						// If it is a building, start repair.
						ConstructionSite.StartConstructionOrRepair( hitInfo.collider.gameObject );
						AudioManager.PlayNew( aiResponse );
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
								AudioManager.PlayNew( aiResponse );
							}
						}
					}
				}
			}
		}
	}
}