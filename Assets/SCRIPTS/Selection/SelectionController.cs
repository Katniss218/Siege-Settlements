using SS.Content;
using SS.InputSystem;
using SS.Levels;
using SS.Objects;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SS
{
	public enum SelectionMode : byte
	{
		Add,
		Replace
	}

	public class SelectionController : MonoBehaviour
	{
		// How much the mouse needs to move before the selection is regarded as box-selection (on both axis).
		private const float XY_THRESHOLD = 4f;
		// How much the mouse needs to move before the selection is regarded as box-selection (combined magnitude).
		private const float MAGN_THRESHOLD = 16f;


		/// <summary>
		/// Is the controller currently box-selecting something? (Read Only).
		/// </summary>
		public static bool isDragging { get; private set; }

		private bool pressOriginatedOnUI = false;
		private bool pressOriginatedDoubleClick = false;

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

		


		private void Inp_Press( InputQueue self )
		{
			if( self.pressCount == 1 )
			{
				this.pressOriginatedOnUI = EventSystem.current.IsPointerOverGameObject();

				this.pressOriginatedDoubleClick = false;

				oldMousePos = Input.mousePosition;
			}
			else if( self.pressCount == 2 )
			{
				this.pressOriginatedOnUI = EventSystem.current.IsPointerOverGameObject();

				this.pressOriginatedDoubleClick = true;

				oldMousePos = Input.mousePosition;
			}
		}

		private void Inp_Hold( InputQueue self )
		{
			if( !isDragging )
			{
				// Don't start drags that began over UI elements (required to not bug selection rect).
				if( this.pressOriginatedOnUI )
				{
					return;
				}


				// Don't begin drags that originated via a double-click.
				if( this.pressOriginatedDoubleClick ) // Technically, I'd need to know in advance if the click is going to be single or double.
				{
					return;
				}

				// Only drag-select once the mouse had moved enough in both directions or in magnitude.
				if( (Mathf.Abs( oldMousePos.x - Input.mousePosition.x ) > XY_THRESHOLD && Mathf.Abs( oldMousePos.y - Input.mousePosition.y ) > XY_THRESHOLD) || (Vector3.Distance( oldMousePos, Input.mousePosition ) > MAGN_THRESHOLD) )
				{
					BeginDrag();
				}
			}

			// if was dragging or just began dragging...
			if( isDragging )
			{
				UpdateDrag();
			}
		}

		private void Inp_Release( InputQueue self )
		{
			// Don't process clicks or drags that began over UI elements.
			if( this.pressOriginatedOnUI )
			{
				return;
			}


			if( isDragging )
			{
				SelectionUtils.SelectOnScreen( beginDragPos, Input.mousePosition, (Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift )) ? SelectionMode.Add : SelectionMode.Replace );
				EndDrag();
			}
			else
			{
				// Double-Click - select the same as the clicked on (if player).
				if( this.pressOriginatedDoubleClick )
				{
					SSObject atCursor = GetSelectableAtCursor();

					if( atCursor == null )
					{
						return;
					}

					if( atCursor is IFactionMember && ((IFactionMember)atCursor).factionId != LevelDataManager.PLAYER_FAC )
					{
						return;
					}

					SelectionUtils.SelectTheSame( atCursor.definitionId, (Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift )) ? SelectionMode.Add : SelectionMode.Replace );
				}
				// Single-Click - select the clicked on.
				else
				{
					SSObject atCursor = GetSelectableAtCursor();
					SSObject[] array = atCursor == null ? null : new SSObject[] { atCursor };
					SelectionUtils.Select( array, (Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift )) ? SelectionMode.Add : SelectionMode.Replace );
				}
			}
		}

		void Group( byte index )
		{
#if UNITY_EDITOR
			if( Input.GetKey( KeyCode.LeftShift ) )
#else
			if( Input.GetKey( KeyCode.LeftControl ) )
#endif
			{
				Selection.SetGroup( index, Selection.GetSelectedObjects() );
			}
			else
			{
				SelectionUtils.Select( Selection.GetGroup( index ), SelectionMode.Replace );
			}
		}

		void Inp_1( InputQueue self )
		{
			Group( 0 );
			self.StopExecution();
		}

		void Inp_2( InputQueue self )
		{
			Group( 1 );
			self.StopExecution();
		}

		void Inp_3( InputQueue self )
		{
			Group( 2 );
			self.StopExecution();
		}

		void Inp_4( InputQueue self )
		{
			Group( 3 );
			self.StopExecution();
		}

		void Inp_5( InputQueue self )
		{
			Group( 4 );
			self.StopExecution();
		}

		void Inp_6( InputQueue self )
		{
			Group( 5 );
			self.StopExecution();
		}

		void Inp_7( InputQueue self )
		{
			Group( 6 );
			self.StopExecution();
		}

		void Inp_8( InputQueue self )
		{
			Group( 7 );
			self.StopExecution();
		}

		void Inp_9( InputQueue self )
		{
			Group( 8 );
			self.StopExecution();
		}

		void Inp_0( InputQueue self )
		{
			Group( 9 );
			self.StopExecution();
		}

		void OnEnable()
		{
			if( Main.mouseInput != null )
			{
				Main.mouseInput.RegisterOnPress( MouseCode.LeftMouseButton, 50.0f, Inp_Press );
				Main.mouseInput.RegisterOnHold( MouseCode.LeftMouseButton, 50.0f, Inp_Hold );
				Main.mouseInput.RegisterOnRelease( MouseCode.LeftMouseButton, 50.0f, Inp_Release );
			}
			if( Main.keyboardInput != null )
			{
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha1, 20.0f, Inp_1 );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha2, 20.0f, Inp_2 );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha3, 20.0f, Inp_3 );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha4, 20.0f, Inp_4 );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha5, 20.0f, Inp_5 );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha6, 20.0f, Inp_6 );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha7, 20.0f, Inp_7 );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha8, 20.0f, Inp_8 );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha9, 20.0f, Inp_9 );
				Main.keyboardInput.RegisterOnPress( KeyCode.Alpha0, 20.0f, Inp_0 );
			}
		}

		void OnDisable()
		{
			if( Main.mouseInput != null )
			{
				Main.mouseInput.ClearOnPress( MouseCode.LeftMouseButton, Inp_Press );
				Main.mouseInput.ClearOnHold( MouseCode.LeftMouseButton, Inp_Hold );
				Main.mouseInput.ClearOnRelease( MouseCode.LeftMouseButton, Inp_Release );
			}
			if( Main.keyboardInput != null )
			{
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha1, Inp_1 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha2, Inp_2 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha3, Inp_3 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha4, Inp_4 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha5, Inp_5 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha6, Inp_6 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha7, Inp_7 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha8, Inp_8 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha9, Inp_9 );
				Main.keyboardInput.ClearOnPress( KeyCode.Alpha0, Inp_0 );
			}
		}
		
		private static SSObject GetSelectableAtCursor()
		{
			if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out RaycastHit hitInfo, ObjectLayer.POTENTIALLY_INTERACTIBLE_MASK ) )
			{
				// Returns null if the mouse is over non-selectable object.
				return hitInfo.collider.GetComponent<SSObject>();
			}
			return null;
		}
	}
}