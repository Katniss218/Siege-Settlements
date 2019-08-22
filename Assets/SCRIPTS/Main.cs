using System;
using TMPro;
using UnityEngine;
using SS.Buildings;
using UnityEngine.AI;
using SS.ResourceSystem;
using Katniss.Utils;
using SS.Data;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SS.UI;
using SS.Extras;

namespace SS
{
	/// <summary>
	/// The main class, think of it like a Game Manager class.
	/// </summary>
	public class Main : MonoBehaviour
	{
		public static Color darkText
		{
			get { return new Color( 0.1f, 0.1f, 0.1f ); }
		}


		// onlevelload - when the level is loaded
		// onpostlevelload - after definitions have been initialized.
		// 
		// TODO ----- Move these fields somewhere else.
		private static GameObject __unitUI = null;
		public static GameObject unitUI
		{
			get
			{
				if( __unitUI == null ) { __unitUI = Resources.Load<GameObject>( "Prefabs/unit_ui" ); }
				return __unitUI;
			}
		}

		private static GameObject __buildingUI = null;
		public static GameObject buildingUI
		{
			get
			{
				if( __buildingUI == null ) { __buildingUI = Resources.Load<GameObject>( "Prefabs/building_ui" ); }
				return __buildingUI;
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
				if( __switcherObj == null ) { __switcherObj = Resources.Load<Sprite>( "Textures/selection_toggle" ); }
				return __switcherObj;
			}
		}

		private static Sprite __switcherList = null;
		public static Sprite switcherList
		{
			get
			{
				if( __switcherList == null ) { __switcherList = Resources.Load<Sprite>( "Textures/selection_toggle_list" ); }
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
				if( __materialParticle == null ) { __materialParticle = Resources.Load<Material>( "ParticleMaterial" ); }
				return __materialParticle;
			}
		}

		private static Camera __camera = null;
		new public static Camera camera
		{
			get
			{
				if( __camera == null )
				{
					__camera = FindObjectOfType<CameraController>().transform.GetChild( 0 ).GetComponent<Camera>();
				}
				return __camera;
			}
		}

		private static TMP_FontAsset __mainFont = null;
		public static TMP_FontAsset mainFont
		{
			get
			{
				if( __mainFont == null )
				{
					__mainFont = Resources.Load<TMP_FontAsset>( "Chomsky SDF" );
				}
				return __mainFont;
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

		private void Start()
		{
		}


		// TODO! - priority-based queue for processing input actions?
		//    maybe you can define them as a list, beforehand, so you can add new inputs anywhere in the list w/o explicitly stating the priority value
		//      (fixes the problem of having no priority space between actions, and having to move everything amove it to make space).

		void Update()
		{
			// When RMB is clicked - Move selected units to the cursor.
			if( Input.GetMouseButtonDown( 1 ) )
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
							if( hitDeposit != null )
							{
								TAIGoal goal = selected[i].GetComponent<TAIGoal>();
								if( goal != null )
								{
									DestroyImmediate( goal );
								}
								TAIGoal.PickUpResource pickUp = selected[i].gameObject.AddComponent<TAIGoal.PickUpResource>();
								pickUp.depositToPickUp = hitDeposit;
							}
							else
							{
								TAIGoal goal = selected[i].GetComponent<TAIGoal>();
								if( goal != null )
								{
									DestroyImmediate( goal );
								}
								TAIGoal.MoveTo moveTo = selected[i].gameObject.AddComponent<TAIGoal.MoveTo>();
								moveTo.destination = hitInfo.point;
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
						if( hitInfo.collider.GetComponent<Damageable>().healthPercent == 1f )
							return;
						// If it is a building, start repair.
						ConstructionSite.StartConstructionOrRepair( hitInfo.collider.gameObject );
					}
				}
			}
		}
	}
}