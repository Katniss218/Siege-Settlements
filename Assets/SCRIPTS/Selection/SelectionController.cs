using SS.Content;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SS
{
	public class SelectionController : MonoBehaviour
	{
		void Update()
		{
			// if the click was over UI element, return.
			if( EventSystem.current.IsPointerOverGameObject() )
			{
				return;
			}

			// If the left mouse button was pressed.
			if( Input.GetMouseButtonDown( 0 ) )
			{
				HandleSelecting();
			}
		}

		internal static void HandleSelecting()
		{
			Selectable obj = GetSelectableAtCursor();

			// Shift - add to the current selection.
			// If the object is already selected, but is not highlighted, highlight it.
			// - (allows for switching of highlighted object within the pool of already selected objects).
			if( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) )
			{
				if( obj != null )
				{
					FactionMember factionOfSelectable = obj.GetComponent<FactionMember>();
					if( factionOfSelectable != null )// && factionOfSelectable.factionId == FactionManager.PLAYER )
					{
						if( SelectionManager.IsSelected( obj ) )
						{
							if( !SelectionManager.IsHighlighted( obj ) )
							{
								SelectionManager.HighlightSelected( obj );
								AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/select" ) );
							}
						}
						else
						{
							SelectionManager.SelectAndHighlight( obj );
							AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/select" ) );
						}
					}
				}
			}
			// No Shift - deselect all and select at cursor (if possible).
			else
			{
				int numSelected = SelectionManager.selectedObjects.Length;
				if( obj == null )
				{
					if( numSelected > 0 )
					{
						SelectionManager.DeselectAll();
						AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/deselect" ) );
					}
				}
				else
				{
					bool flag = SelectionManager.IsSelected( obj );
					SelectionManager.DeselectAll();
					
					FactionMember factionOfSelectable = obj.GetComponent<FactionMember>();
					if( factionOfSelectable != null ) //&& factionOfSelectable.factionId == FactionManager.PLAYER )
					{
						SelectionManager.SelectAndHighlight( obj );
						if( !flag ) // If was selected before clearing, don't play the selecting sound, since in the end, nothing changes.
						{
							AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/select" ) );
						}
					}
					else
					{
						AudioManager.PlayNew( AssetManager.GetAudioClip( AssetManager.RESOURCE_ID + "Sounds/deselect" ) );
					}
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

	}
}