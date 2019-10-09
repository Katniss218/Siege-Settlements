using SS.UI;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public class Selection
	{
		private static List<Selectable> selected = new List<Selectable>();

		/// <summary>
		/// Returns a copy of the selected objects.
		/// </summary>
		public static Selectable[] selectedObjects
		{
			get
			{
				return selected.ToArray();
			}
		}

		private static Selectable highlighted = null;

		/// <summary>
		/// Contains the object that is currently highlighted.
		/// </summary>
		public static Selectable highlightedObject
		{
			get
			{
				return highlighted;
			}
		}
		
		private static void __Highlight( Selectable obj )
		{
			highlighted = obj;
			obj.onHighlight?.Invoke();

			// Allow the newly highlighted object to create it's own UI elements.
			// Also, this is going to clear the previous object's UI elements.
			//ForceSelectionUIRedraw( obj );
		}

		private static void __Select( Selectable obj )
		{
			SelectionPanel.instance.list.AddIcon( obj, obj.icon );

			selected.Add( obj );
			obj.onSelect?.Invoke();
		}

		private static void __Deselect( Selectable obj )
		{
			SelectionPanel.instance.list.RemoveIcon( obj );

			if( IsHighlighted( obj ) )
			{
				SelectionPanel.instance.obj.ClearAll();
				highlighted = null;
			}
			obj.onDeselect?.Invoke();
		}
		
		/// <summary>
		/// Checks if the object is currently highlighted.
		/// </summary>
		/// <param name="obj">The object to check.</param>
		public static bool IsHighlighted( Selectable obj )
		{
			if( obj == null )
			{
				return false;
			}
			return highlighted == obj;
		}

		/// <summary>
		/// Checks if the object is currently selected.
		/// </summary>
		/// <param name="obj">The object to check.</param>
		public static bool IsSelected( Selectable obj )
		{
			return selected.Contains( obj );
		}

		/// <summary>
		/// Highlights the specified object. OBJECT NEEDS TO BE ALREADY SELECTED.
		/// </summary>
		/// <param name="obj">The selected object to highlight.</param>
		public static void HighlightSelected( Selectable obj )
		{
			if( !selected.Contains( obj ) )
			{
				throw new System.Exception( "Attempted to highlight object that is NOT selected." );
			}
			__Highlight( obj );
		}

		/// <summary>
		/// Selects an object, and highlights it.
		/// </summary>
		/// <param name="obj">The object to select.</param>
		public static void SelectAndHighlight( Selectable obj )
		{
			if( obj == null )
			{
				return;
			}
			if( selected.Contains( obj ) )
			{
				Debug.LogWarning( "Attempted to select object that is selected." );
			}
			else
			{
				if( !SelectionPanel.instance.gameObject.activeSelf )
					SelectionPanel.instance.gameObject.SetActive( true );
				if( !ActionPanel.instance.gameObject.activeSelf )
					ActionPanel.instance.gameObject.SetActive( true );

				__Select( obj );
				__Highlight( obj );
			}
		}

		/// <summary>
		/// Deselects an object.
		/// </summary>
		/// <param name="obj">The object to deselect.</param>
		public static void Deselect( Selectable obj )
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

			__Deselect( obj );
		}
		
		/// <summary>
		/// Deselects all objects.
		/// </summary>
		public static void DeselectAll()
		{
			SelectionPanel.instance.obj.ClearAll();
			SelectionPanel.instance.list.Clear();
			
			for( int i = 0; i < selected.Count; i++ )
			{
				selected[i].onDeselect?.Invoke();
			}
			
			highlighted = null;
			selected.Clear();

			SelectionPanel.instance.gameObject.SetActive( false );
			ActionPanel.instance.gameObject.SetActive( false );
		}

		public static void Purge()
		{
			highlighted = null;
			selected.Clear();
		}
	}
}