using Katniss.Utils;
using SS.Content;
using SS.Factions;
using SS.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SS
{
	public class SelectionController : MonoBehaviour
	{
		private const float XY_THRESHOLD = 4f;
		private const float MAGN_THRESHOLD = 16f;

		private enum SelectionMode : byte
		{
			Additive,
			Exclusive
		}

		public static bool isDragging { get; private set; }

		private static Vector2 beginDragPos;

		private static RectTransform selectionRect = null;

		private static Vector3 oldMousePos;

		private static void InitRect()
		{
			GameObject obj;
			RectTransform t;

			GameObjectUtils.RectTransform( Main.canvas.transform, "SelectionRect", new GenericUIData( Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero ), out obj, out t );

			obj.AddImageSliced( AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Textures/selection_rect" ), false );

			selectionRect = t;
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

		private static void HandleSelecting( Selectable[] uniqueObjs, SelectionMode selectionMode )
		{
			if( selectionMode == SelectionMode.Additive )
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
						if( factionOfSelectable != null )
						{
							if( Selection.IsSelected( uniqueObjs[i] ) )
							{
								if( !Selection.IsHighlighted( uniqueObjs[i] ) )
								{
									Selection.HighlightSelected( uniqueObjs[i] );
									AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Sounds/select" ) );
								}
							}
							else
							{
								Selection.SelectAndHighlight( uniqueObjs[i] );
								AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Sounds/select" ) );
							}
						}
					}
				}
				return;
			}
			// No Shift - deselect all and select at cursor (if possible).
			if( selectionMode == SelectionMode.Exclusive )
			{
				if( uniqueObjs == null )
				{
					int numSelected = Selection.selectedObjects.Length;
					if( numSelected > 0 )
					{
						Selection.DeselectAll();
						AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Sounds/deselect" ) );
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
							Selection.SelectAndHighlight( uniqueObjs[i] );
							
							AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Sounds/select" ) );
							
						}
						else
						{
							AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Sounds/deselect" ) );
						}
					}
				}
				return;
			}

			throw new System.Exception( "Invalid selection mode" );
		}


		void Update()
		{
			// If the left mouse button was pressed.
			if( Input.GetMouseButtonDown( 0 ) )
			{
				oldMousePos = Input.mousePosition;
			}
			// If the left mouse button was pressed.
			if( Input.GetMouseButton( 0 ) )
			{
				if( !isDragging )
				{
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
			// If the left mouse button was released.
			if( Input.GetMouseButtonUp( 0 ) )
			{
				if( isDragging )
				{
					Selectable[] overlap = GetSelectablesInDragArea();

					HandleSelecting( overlap, (Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift )) ? SelectionMode.Additive : SelectionMode.Exclusive );


					EndDrag();
				}
				else
				{
					// if the click was over UI element, return.
					if( EventSystem.current.IsPointerOverGameObject() )
					{
						return;
					}
					Selectable atCursor = GetSelectableAtCursor();
					Selectable[] array = atCursor == null ? null : new Selectable[] { atCursor };
					HandleSelecting( array, (Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift )) ? SelectionMode.Additive : SelectionMode.Exclusive );
				}
			}
		}


		private static Selectable GetSelectableAtCursor()
		{
			if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out RaycastHit hitInfo ) )
			{
				// Returns null if the mouse is over non-selectable object.
				return hitInfo.collider.GetComponent<Selectable>();
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

		private static Selectable[] GetSelectablesInDragArea()
		{
			Selectable[] selectables = Selectable.GetAllInScene();

			List<Selectable> ret = new List<Selectable>();

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