using SS.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SS
{
	public class SelectionManager : MonoBehaviour
	{
		public static List<Selectable> selectedObjs = new List<Selectable>();
		public static Selectable highlightedObj = null;
		
		void Update()
		{
			// if the click was over UI element, return.
			if( EventSystem.current.IsPointerOverGameObject() )
			{
				return;
			}
			if( Input.GetMouseButtonDown( 0 ) )
			{
				if( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) )
				{
					Selectable obj = RaycastSelect();
					if( obj != null )
					{
						if( !selectedObjs.Contains( obj ) )
						{
							Select( obj );
						}
					}
				}
				else
				{
					DeselectAll();

					Selectable obj = RaycastSelect();
					if( obj != null )
					{
						Select( obj );
					}
				}
			}
		}

		private static Selectable RaycastSelect()
		{
			RaycastHit hitInfo;
			if( Physics.Raycast( Camera.main.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
			{
				Selectable sel = hitInfo.collider.GetComponent<Selectable>();
				if( sel == null )
				{
					return null;
				}
				return sel;
			}
			return null;
		}

		public static bool IsHighlighted( Selectable obj )
		{
			return highlightedObj == obj;
		}

		public static bool IsSelected( Selectable obj )
		{
			return selectedObjs.Contains( obj );
		}

		public static void Select( Selectable obj )
		{
			if( obj == null )
			{
				return;
			}
			if( selectedObjs.Contains( obj ) )
			{
				return;
			}
			selectedObjs.Add( obj );
			obj.onSelect?.Invoke( obj );
			SelectionPanel.ListAddIcon( obj, Main.toolTipBackground );
		}

		public static void Deselect( Selectable obj )
		{
			if( obj == null )
			{
				return;
			}
			if( selectedObjs.Contains( obj ) )
			{
				selectedObjs.Remove( obj );
			}
			obj.onDeselect?.Invoke( obj );
			SelectionPanel.ListRemoveIcon( obj );
		}
		
		public static void DeselectAll()
		{
			for( int i = 0; i < selectedObjs.Count; i++ )
			{
				selectedObjs[i].onDeselect?.Invoke( selectedObjs[i] );
			}
			selectedObjs.Clear();

			SelectionPanel.ListClear();
		}
	}
}