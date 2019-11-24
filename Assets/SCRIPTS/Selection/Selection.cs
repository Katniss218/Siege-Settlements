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

		/// <summary>
		/// Selects an object, and highlights it.
		/// </summary>
		/// <param name="obj">The object to select.</param>
		public static void Select( SSObjectSelectable obj )
		{
			if( obj == null )
			{
				return;
			}
			if( selected.Contains( obj ) )
			{
				Debug.LogWarning( "Attempted to select object that is already selected." );
				return;
			}
			if( !SelectionPanel.instance.gameObject.activeSelf )
			{
				SelectionPanel.instance.gameObject.SetActive( true );
			}
			if( !ActionPanel.instance.gameObject.activeSelf )
			{
				ActionPanel.instance.gameObject.SetActive( true );
			}

			SelectionPanel.instance.list.AddIcon( obj, obj.icon );
			selected.Add( obj );
			
			if( selected.Count == 1 )
			{
#warning Fix batch-selecting object displaying the first one.

				Display( obj as ISelectDisplayHandler );

				SSModule[] modules = obj.GetModules();

				for( int i = 0; i < modules.Length; i++ )
				{
					if( !(modules[i] is ISelectDisplayHandler) )
					{
						continue;
					}
					SelectionPanel.instance.obj.AddModuleButton( modules[i] );
				}
			}

			obj.onSelect?.Invoke();
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
			{
				SelectionPanel.instance.obj.ClearModules();
			}
			if( selected.Count == 1 )
			{
				Display( selected[0] as ISelectDisplayHandler );
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