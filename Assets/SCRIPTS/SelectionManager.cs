﻿using SS.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
			// if the click was over UI element, return.
			if( EventSystem.current.IsPointerOverGameObject() )
			{
				return;
			}
			if( Input.GetMouseButtonDown( 0 ) )
			{
				if( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) )
				{
					ISelectable obj = RaycastSelect();
					if( obj != null )
					{
						if( !selected.Contains( obj ) )
						{
							Select( obj );
						}
					}
				}
				else
				{
					DeselectAll();

					ISelectable obj = RaycastSelect();
					if( obj != null )
					{
						Select( obj );
					}
				}
			}
		}

		private static ISelectable RaycastSelect()
		{
			RaycastHit hitInfo;
			if( Physics.Raycast( Camera.main.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
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
			SelectionPanel.ListAddIcon( obj, Main.toolTipBackground );
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
			SelectionPanel.ListRemoveIcon( obj );
		}

		public static void DeselectAll()
		{
			selected.Clear();

			SelectionPanel.ListClear();
		}
	}
}