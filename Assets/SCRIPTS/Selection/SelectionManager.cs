﻿using SS.UI;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public class SelectionManager
	{
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

		private static List<Selectable> selected = new List<Selectable>();
		private static Selectable highlighted = null;
		
		private static void __Highlight( Selectable obj )
		{
			// Clear the previous object's UI elements.
			if( highlighted != null )
			{
				SelectionPanel.Object.Clear();
			}
			// Highlight the new object.
			highlighted = obj;
			// Allow the newly highlighted object to create it's own UI elements.
			obj.onHighlight?.Invoke();
		}

		private static void __Select( Selectable obj )
		{
			// Add the selected object's icon to the SelectionPanel.List.
			SelectionPanel.List.AddIcon( obj, obj.icon );
			// Select the object.
			selected.Add( obj );
			// Notify the object that's being selected.
			obj.onSelect?.Invoke();
		}

		private static void __Deselect( Selectable obj )
		{
			// Remove the deselected object's icon from the SelectionPanel.List.
			SelectionPanel.List.RemoveIcon( obj );
			// If the object is highlighted:
			// - Un-highlight it.
			if( IsHighlighted( obj ) )
			{
				SelectionPanel.Object.Clear();
				highlighted = null;
			}
			// Notify the object that's being deselected.
			obj.onDeselect?.Invoke();
		}


		/// <summary>
		/// Checks if the object is currently highlighted.
		/// </summary>
		/// <param name="obj">The object to check.</param>
		public static bool IsHighlighted( Selectable obj )
		{
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
				Debug.LogError( "Attempted to highlight object that is NOT selected." );
			}
			else
			{
				__Highlight( obj );
			}
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
			// Clear the SelectionPanel.Object, and SelectionPanel.List.
			SelectionPanel.Object.Clear();
			SelectionPanel.List.Clear();

			// Notify every object that's being deselected of the fact.
			for( int i = 0; i < selected.Count; i++ )
			{
				selected[i].onDeselect?.Invoke();
			}

			// Un-highlight the highlighted object.
			highlighted = null;
			// Deselect every object.
			selected.Clear();
		}
	}
}