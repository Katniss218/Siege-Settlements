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
				// Shift - add to the current selection.
				if( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) )
				{
					Selectable obj = GetSelectableAtCursor();
					if( obj != null )
					{
						if( !SelectionManager.IsSelected( obj ) )
						{
							SelectionManager.Select( obj );
						}
					}
				}
				// No Shift - deselect all and select at cursor.
				else
				{
					SelectionManager.DeselectAll();

					Selectable obj = GetSelectableAtCursor();
					if( obj != null )
					{
						SelectionManager.Select( obj );
					}
				}
			}
		}


		private static Selectable GetSelectableAtCursor()
		{
			if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out RaycastHit hitInfo ) )
			{
				return hitInfo.collider.GetComponent<Selectable>(); // Return null if not present.
			}
			return null;
		}

	}
}