using SS.Modules;
using SS.UI;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public class Selection
	{
		private static List<SSObjectSelectable> selected = new List<SSObjectSelectable>();

		/// <summary>
		/// Returns a copy of the selected objects.
		/// </summary>
		public static SSObjectSelectable[] selectedObjects
		{
			get
			{
				return selected.ToArray();
			}
		}
		
		public static ISelectDisplayHandler displayedObject { get; private set; } = null;

		
		public static bool IsDisplayed( ISelectDisplayHandler obj )
		{
			return displayedObject == obj ;
		}

		public static void Display( ISelectDisplayHandler obj )
		{
			if( displayedObject != null ) // clear previously displayed.
			{
				SelectionPanel.instance.obj.ClearAllElements();
			}
			displayedObject = obj;
			if( obj != null )
			{
				obj.OnDisplay();
			}
		}


		/// <summary>
		/// Checks if the object is currently selected.
		/// </summary>
		/// <param name="obj">The object to check.</param>
		public static bool IsSelected( SSObjectSelectable obj )
		{
			return selected.Contains( obj );
		}
		
		public static int TrySelect( SSObjectSelectable[] objs )
		{
			if( objs == null )
			{
				return 0;
			}
			if( objs.Length == 0 )
			{
				return 0;
			}
			
			if( !SelectionPanel.instance.gameObject.activeSelf )
			{
				SelectionPanel.instance.gameObject.SetActive( true );
			}
			if( !ActionPanel.instance.gameObject.activeSelf )
			{
				ActionPanel.instance.gameObject.SetActive( true );
			}
			int numSelected = 0;
			for( int i = 0; i < objs.Length; i++ )
			{
				if( objs[i] == null )
				{
					continue;
				}
				if( selected.Contains( objs[i] ) )
				{
					continue;
				}
				SelectionPanel.instance.list.AddIcon( objs[i], objs[i].icon );
				selected.Add( objs[i] );

				numSelected++;
				objs[i].onSelect?.Invoke();
			}

			if( selected.Count == 1 )
			{
#warning Fix batch-selecting object displaying the first one.

				Display( objs[0] as ISelectDisplayHandler );

				SSModule[] modules = objs[0].GetModules();

				for( int i = 0; i < modules.Length; i++ )
				{
					if( !(modules[i] is ISelectDisplayHandler) )
					{
						continue;
					}
					SelectionPanel.instance.obj.AddModuleButton( modules[i] );
				}
			}
			else
			{
				SelectionPanel.instance.obj.ClearModules();
				SelectionPanel.instance.obj.ClearIcon();
				SelectionPanel.instance.obj.displayNameText.text = "Group";
			}
			return numSelected;
		}

		/// <summary>
		/// Deselects an object.
		/// </summary>
		/// <param name="obj">The object to deselect.</param>
		public static void Deselect( SSObjectSelectable obj )
		{
			if( obj == null )
			{
				return;
			}
			if( !selected.Remove( obj ) )
			{
				Debug.LogWarning( "Attempted to deselect object that is not selected." );
				return;
			}

			SelectionPanel.instance.list.RemoveIcon( obj );

			if( IsDisplayed( obj ) )
#warning what if module is displayed? It needs to deselect.
			{
				SelectionPanel.instance.obj.ClearModules();
				SelectionPanel.instance.obj.ClearIcon();
				SelectionPanel.instance.obj.displayNameText.text = "Group";
			}
			if( selected.Count == 1 )
			{
				Display( selected[0] as ISelectDisplayHandler );
			}
			else if( selected.Count == 0 )
			{
				SelectionPanel.instance.gameObject.SetActive( false );
				ActionPanel.instance.gameObject.SetActive( false );
			}

			obj.onDeselect?.Invoke();
		}
		
		/// <summary>
		/// Deselects all objects.
		/// </summary>
		public static void DeselectAll()
		{
			SelectionPanel.instance.obj.ClearAllElements();
			SelectionPanel.instance.obj.ClearIcon();
			SelectionPanel.instance.obj.ClearModules();
			SelectionPanel.instance.obj.displayNameText.text = "";

			SelectionPanel.instance.list.Clear();

			for( int i = 0; i < selected.Count; i++ )
			{
				selected[i].onDeselect?.Invoke();
			}
			
			selected.Clear();

			SelectionPanel.instance.gameObject.SetActive( false );
			ActionPanel.instance.gameObject.SetActive( false );
		}

		public static void Purge()
		{
			selected.Clear();
		}
	}
}