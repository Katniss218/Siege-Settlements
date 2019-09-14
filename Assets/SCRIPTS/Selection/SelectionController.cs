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
					if( factionOfSelectable != null && factionOfSelectable.factionId == 0 )
					{
						if( SelectionManager.IsSelected( obj ) )
						{
							if( !SelectionManager.IsHighlighted( obj ) )
							{
								SelectionManager.HighlightSelected( obj );
								AudioManager.PlayNew( Main.selectSound );
							}
						}
						else
						{
							SelectionManager.SelectAndHighlight( obj );
							AudioManager.PlayNew( Main.selectSound );
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
						AudioManager.PlayNew( Main.deselectSound );
					}
				}
				else
				{
					SelectionManager.DeselectAll();
					
					FactionMember factionOfSelectable = obj.GetComponent<FactionMember>();
					if( factionOfSelectable != null && factionOfSelectable.factionId == 0 )
					{
						SelectionManager.SelectAndHighlight( obj );
						AudioManager.PlayNew( Main.selectSound );
					}
					else
					{
						AudioManager.PlayNew( Main.deselectSound );
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