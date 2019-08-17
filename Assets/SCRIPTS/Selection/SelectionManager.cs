using SS.UI;
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
		/// Selects an object.
		/// </summary>
		/// <param name="obj">The object to select.</param>
		public static void Select( Selectable obj )
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
				selected.Add( obj );
				obj.onSelect?.Invoke();
				SelectionPanel.ListAddIcon( obj, obj.icon );
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
			}
			
			obj.onDeselect?.Invoke();
			SelectionPanel.ListRemoveIcon( obj );
		}
		
		/// <summary>
		/// Deselects all objects.
		/// </summary>
		public static void DeselectAll()
		{
			for( int i = 0; i < selected.Count; i++ )
			{
				selected[i].onDeselect?.Invoke();
			}

			selected.Clear();
			SelectionPanel.ListClear();
		}
	}
}