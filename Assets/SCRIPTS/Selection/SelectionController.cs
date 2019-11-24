using SS.Content;
using SS.Diplomacy;
using SS.InputSystem;
using SS.Objects;
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
			GameObject obj = Instantiate( (GameObject)AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Selection Rect (UI)" ), Main.canvas.transform );
			
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
			Main.cameraController.isMovementLocked = true;

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
			Main.cameraController.isMovementLocked = false;

			selectionRect.gameObject.SetActive( false );
		}

		private static void HandleSelecting( SSObjectSelectable[] uniqueObjs, SelectionMode selectionMode )
		{
			if( selectionMode == SelectionMode.Add )
			{
				if( uniqueObjs != null )
				{
					for( int i = 0; i < uniqueObjs.Length; i++ )
					{
						if( uniqueObjs[i] == null )
						{
							continue;
						}

						FactionMember factionOfSelectable = uniqueObjs[i].GetComponent<FactionMember>();
						if( factionOfSelectable == null )
						{
							continue;
						}
						if( !Selection.IsSelected( uniqueObjs[i] ) )
						{
							Selection.Select( uniqueObjs[i] );
							AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/select" ) );
						}
					}
				}
				return;
			}
			// No Shift - deselect all and select at cursor (if possible).
			if( selectionMode == SelectionMode.Replace )
			{
				if( uniqueObjs == null )
				{
					int numSelected = Selection.selectedObjects.Length;
					if( numSelected > 0 )
					{
						Selection.DeselectAll();
						AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/deselect" ) );
					}
				}
				else
				{
					Selection.DeselectAll();

					for( int i = 0; i < uniqueObjs.Length; i++ )
					{
						if( uniqueObjs[i] == null )
						{
							continue;
						}
						
						FactionMember factionOfSelectable = uniqueObjs[i].GetComponent<FactionMember>();
						if( factionOfSelectable != null )
						{
							Selection.Select( uniqueObjs[i] );
							
							AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/select" ) );
							
						}
						else
						{
							AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/deselect" ) );
						}
					}
				}
				return;
			}

			throw new System.Exception( "Invalid selection mode" );
		}

		private void OnPress( InputQueue self )
		{
			this.pressOriginatedOnUI = EventSystem.current.IsPointerOverGameObject();
			
			oldMousePos = Input.mousePosition;
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
				if( Mathf.Abs( oldMousePos.x - Input.mousePosition.x ) > XY_THRESHOLD && Mathf.Abs( oldMousePos.y - Input.mousePosition.y ) > XY_THRESHOLD ||
				Vector3.Distance( oldMousePos, Input.mousePosition ) > MAGN_THRESHOLD )
				{
					BeginDrag();
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
				SSObjectSelectable[] overlap = GetSelectablesInDragArea();

				HandleSelecting( overlap, (Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift )) ? SelectionMode.Add : SelectionMode.Replace );


				EndDrag();
			}
			else
			{
				// Don't process clicks that began over UI elements.
				if( pressOriginatedOnUI )
				{
					return;
				}
				SSObjectSelectable atCursor = GetSelectableAtCursor();
				SSObjectSelectable[] array = atCursor == null ? null : new SSObjectSelectable[] { atCursor };
				HandleSelecting( array, (Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift )) ? SelectionMode.Add : SelectionMode.Replace );
			}
		}


		void OnEnable()
		{
			Main.mouseInput.RegisterOnPress( MouseCode.LeftMouseButton, 50.0f, OnPress, true );
			Main.mouseInput.RegisterOnHold( MouseCode.LeftMouseButton, 50.0f, OnHold, true );
			Main.mouseInput.RegisterOnRelease( MouseCode.LeftMouseButton, 50.0f, OnRelease, true );
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
		
		private static SSObjectSelectable GetSelectableAtCursor()
		{
			if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out RaycastHit hitInfo ) )
			{
				// Returns null if the mouse is over non-selectable object.
				return hitInfo.collider.GetComponent<SSObjectSelectable>();
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

		private static SSObjectSelectable[] GetSelectablesInDragArea()
		{
			SSObjectSelectable[] selectables = SSObject.GetAllSelectables();

			List<SSObjectSelectable> ret = new List<SSObjectSelectable>();

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