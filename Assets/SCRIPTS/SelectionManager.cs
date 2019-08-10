using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public class SelectionManager : MonoBehaviour
	{
		public static List<ISelectable> selected = new List<ISelectable>();
		public static ISelectable highlighted = null;

		void Start()
		{

		}

		void Update()
		{
			if( Input.GetMouseButtonDown( 0 ) )
			{
				if( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) )
				{
					ISelectable obj = raycastSelect();
					if( obj != null )
					{
						if( selected.Contains( obj ) )
						{
							Deselect( obj );
						}
						else
						{
							Select( obj );
						}
					}
				}
				else
				{
					DeselectAll();

					ISelectable obj = raycastSelect();
					if( obj != null )
					{
						Select( obj );
					}
				}
			}
		}

		private static ISelectable raycastSelect()
		{
			RaycastHit hitInfo;
			if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
			{
				ISelectable sel = hitInfo.collider.GetComponent<ISelectable>();
				if( sel == null )
				{
					return null;
				}
				return sel;
			}
			return null;
		}

		public static void Select( ISelectable obj )
		{
			if( obj == null )
			{
				return;
			}
			if( selected.Contains( obj ) )
			{
				return;
			}
			selected.Add( obj );
		}

		public static void Deselect( ISelectable obj )
		{
			if( obj == null )
			{
				return;
			}
			if( selected.Contains( obj ) )
			{
				selected.Remove( obj );
			}
		}

		public static void DeselectAll()
		{
			selected.Clear();
		}
	}
}