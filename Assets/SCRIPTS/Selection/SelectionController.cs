using SS.Content;
using SS.Diplomacy;
using SS.InputSystem;
using SS.Levels;
using SS.Objects;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SS
{
	public class SelectionController : MonoBehaviour
	{
		// How much the mouse needs to move before the selection is regarded as box-selection (on both axis).
		private const float XY_THRESHOLD = 4f;
		// How much the mouse needs to move before the selection is regarded as box-selection (combined magnitude).
		private const float MAGN_THRESHOLD = 16f;

		private enum SelectionMode : byte
		{
			Add,
			Replace
		}

		/// <summary>
		/// Is the controller currently box-selecting something? (Read Only).
		/// </summary>
		public static bool isDragging { get; private set; }

		private bool pressOriginatedOnUI = false;

		private static Vector2 beginDragPos;

		private static RectTransform selectionRect = null;

		private static Vector3 oldMousePos;

		
		private static void InitRect()
		{
			GameObject obj = Instantiate( (GameObject)AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Selection Rect (UI)" ), Main.objectHUDCanvas );
			
			selectionRect = obj.GetComponent<RectTransform>();
		}

		private static void BeginDrag()
		{
			if( selectionRect == null )
			{
				InitRect();
			}

			beginDragPos = Input.mousePosition;
			isDragging = true;
			CameraController.instance.isMovementLocked = true;

			selectionRect.gameObject.SetActive( true );
		}

		private static void UpdateDrag()
		{
			if( selectionRect == null )
			{
				throw new System.Exception( "UpdateDrag called before BeginDrag" );
			}
			selectionRect.sizeDelta = new Vector2( Mathf.Abs( beginDragPos.x - Input.mousePosition.x ), Mathf.Abs( beginDragPos.y - Input.mousePosition.y ) );
			selectionRect.anchoredPosition = new Vector2( Mathf.Min( Input.mousePosition.x, beginDragPos.x ), Mathf.Min( Input.mousePosition.y, beginDragPos.y ) );
		}

		private static void EndDrag()
		{
			isDragging = false;
			CameraController.instance.isMovementLocked = false;

			selectionRect.gameObject.SetActive( false );
		}

		private static void HandleSelecting( SSObjectDFS[] uniqueSelectables, SelectionMode selectionMode )
		{
			// Select selectables on the list (if not selected).
			if( selectionMode == SelectionMode.Add )
			{
				if( uniqueSelectables != null )
				{
					int numSelected = Selection.TrySelect( uniqueSelectables );

					if( numSelected > 0 )
					{
						AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/select" ) );
					}
				}
				return;
			}
			// Deselect already selected that are not present on the list.
			// Select selectables on the list (if not selected).
			if( selectionMode == SelectionMode.Replace )
			{
				if( uniqueSelectables == null )
				{
					if( Selection.GetSelectedObjects().Length == 0 )
					{
						return;
					}

					Selection.DeselectAll();
					AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/deselect" ) );
				}
				else
				{
					SSObjectDFS[] selectedObjs = Selection.GetSelectedObjects();
					bool playDeselect = false;

					for( int i = 0; i < selectedObjs.Length; i++ )
					{
						bool isToBeSelected = false;
						for( int j = 0; j < uniqueSelectables.Length; j++ )
						{
							if( selectedObjs[i] == uniqueSelectables[j] )
							{
								isToBeSelected = true;
							}
						}
						if( !isToBeSelected )
						{
							Selection.Deselect( selectedObjs[i] );
							playDeselect = true;
						}
					}
					int numSelected = Selection.TrySelect( uniqueSelectables );

					if( numSelected > 0 )
					{
						AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/select" ) );
					}
					if( playDeselect )
					{
						AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/deselect" ) );
					}
				}
				return;
			}

			throw new Exception( "Invalid selection mode" );
		}

		private static bool isDoublePress = false;

		private void OnPress( InputQueue self )
		{
			if( self.pressCount == 1 )
			{
				this.pressOriginatedOnUI = EventSystem.current.IsPointerOverGameObject();

				isDoublePress = false;

				oldMousePos = Input.mousePosition;
			}
			else if( self.pressCount == 2 )
			{
				this.pressOriginatedOnUI = EventSystem.current.IsPointerOverGameObject();

				isDoublePress = true;

				oldMousePos = Input.mousePosition;
			}
		}

		private void OnHold( InputQueue self )
		{
			if( !isDragging )
			{
				// Don't start drags that began over UI elements (required to not bug selection rect).
				if( pressOriginatedOnUI )
				{
					return;
				}


				if( isDoublePress ) // Technically, I'd need to know in advance if the click is going ot be single or double.
				{
					return;
				}
				else
				{
					if( Mathf.Abs( oldMousePos.x - Input.mousePosition.x ) > XY_THRESHOLD && Mathf.Abs( oldMousePos.y - Input.mousePosition.y ) > XY_THRESHOLD ||
					Vector3.Distance( oldMousePos, Input.mousePosition ) > MAGN_THRESHOLD )
					{
						BeginDrag();
					}
				}
			}
			if( isDragging )
			{
				UpdateDrag();
			}
		}

		private void OnRelease( InputQueue self )
		{
			if( isDragging )
			{
				// Don't process drags that began over UI elements.
				if( pressOriginatedOnUI )
				{
					return;
				}


				if( isDoublePress )
				{
					return;
				}
				else
				{
					SSObjectDFS[] overlap = GetSelectablesInDragArea();

					HandleSelecting( overlap, (Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift )) ? SelectionMode.Add : SelectionMode.Replace );


					EndDrag();
				}
			}
			else
			{
				// Don't process clicks that began over UI elements.
				if( pressOriginatedOnUI )
				{
					return;
				}


				if( isDoublePress )
				{
					SSObjectDFS atCursor = GetSelectableAtCursor();

					if( atCursor != null && atCursor.factionId == LevelDataManager.PLAYER_FAC )
					{
						SelectTheSame( atCursor.definitionId );
					}
				}
				else
				{
					SSObjectDFS atCursor = GetSelectableAtCursor();
					SSObjectDFS[] array = atCursor == null ? null : new SSObjectDFS[] { atCursor };
					HandleSelecting( array, (Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift )) ? SelectionMode.Add : SelectionMode.Replace );
				}
			}
		}

		private void SelectTheSame( string definitionId )
		{
			SSObjectDFS[] selectables = SSObjectDFS.GetAllSelectables();

			List<SSObjectDFS> sameIdAndWithinView = new List<SSObjectDFS>();
			for( int i = 0; i < selectables.Length; i++ )
			{
				if( selectables[i].definitionId != definitionId )
				{
					continue;
				}

				if( selectables[i].factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}

				Vector3 viewportPos = Main.camera.WorldToViewportPoint( selectables[i].transform.position );
				if( (viewportPos.x < 0 || viewportPos.x > 1) || (viewportPos.y < 0 || viewportPos.y > 1) )
				{
					continue;
				}

				sameIdAndWithinView.Add( selectables[i] );
			}

			SSObjectDFS[] array = sameIdAndWithinView.ToArray();
			HandleSelecting( array, (Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift )) ? SelectionMode.Add : SelectionMode.Replace );
		}

		void OnEnable()
		{
			Main.mouseInput.RegisterOnPress( MouseCode.LeftMouseButton, 50.0f, OnPress );
			Main.mouseInput.RegisterOnHold( MouseCode.LeftMouseButton, 50.0f, OnHold );
			Main.mouseInput.RegisterOnRelease( MouseCode.LeftMouseButton, 50.0f, OnRelease );
		}

		void OnDisable()
		{
			if( Main.mouseInput != null )
			{
				Main.mouseInput.ClearOnPress( MouseCode.LeftMouseButton, OnPress );
				Main.mouseInput.ClearOnHold( MouseCode.LeftMouseButton, OnHold );
				Main.mouseInput.ClearOnRelease( MouseCode.LeftMouseButton, OnRelease );
			}
		}
		
		private static SSObjectDFS GetSelectableAtCursor()
		{
			if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out RaycastHit hitInfo ) )
			{
				// Returns null if the mouse is over non-selectable object.
				return hitInfo.collider.GetComponent<SSObjectDFS>();
			}
			return null;
		}

		private static Bounds GetViewportBounds( Camera camera, Vector3 corner1, Vector3 corner2 )
		{
			Vector3 c1View = camera.ScreenToViewportPoint( corner1 );
			Vector3 c2View = camera.ScreenToViewportPoint( corner2 );

			Vector3 min = Vector3.Min( c1View, c2View );
			min.z = camera.nearClipPlane;

			Vector3 max = Vector3.Max( c1View, c2View );
			max.z = camera.farClipPlane;

			Bounds ret = new Bounds();
			ret.SetMinMax( min, max );

			return ret;
		}

		private static SSObjectDFS[] GetSelectablesInDragArea()
		{
			SSObjectDFS[] selectables = SSObject.GetAllSelectables();

			List<SSObjectDFS> ret = new List<SSObjectDFS>();

			Bounds viewportBounds = GetViewportBounds( Main.camera, new Vector3( beginDragPos.x, beginDragPos.y, 0 ), Input.mousePosition );

			for( int i = 0; i < selectables.Length; i++ )
			{
				if( viewportBounds.Contains( Main.camera.WorldToViewportPoint( selectables[i].gameObject.transform.position ) ) )
				{
					ret.Add( selectables[i] );
				}
			}
			if( ret.Count == 0 )
			{
				return null;
			}
			return ret.ToArray();
		}
	}
}