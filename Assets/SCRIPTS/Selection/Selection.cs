using SS.Modules;
using SS.Objects;
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


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		// When object is null, module also must be null.
		// When module is not null, module also can't be null.
		static SSObjectSelectable displayedObject { get; set; } = null;
		static ISelectDisplayHandler displayedModule { get; set; } = null;

		
		public static bool IsDisplayed( SSObjectSelectable obj )
		{
			return displayedObject == obj ;
		}

		public static bool IsDisplayedModule( ISelectDisplayHandler module )
		{
			return displayedModule == module;
		}

		public static void DisplayObjectDisplayed()
		{
			DisplayObject( displayedObject );
		}

		/// <summary>
		/// Displays an object.
		/// </summary>
		public static void DisplayObject( SSObjectSelectable obj )
		{
			if( obj == null )
			{
				throw new System.Exception( "Object can't be null." );
			}
			if( displayedObject != null ) // clear previously displayed.
			{
				StopDisplaying();
			}
			displayedObject = obj;

			SSModule[] modules = ((SSObject)obj).GetModules();

			for( int i = 0; i < modules.Length; i++ )
			{
				if( !(modules[i] is ISelectDisplayHandler) )
				{
					continue;
				}
				SelectionPanel.instance.obj.AddModuleButton( modules[i] );
			}
		
			obj.OnDisplay();
		}

		/// <summary>
		/// Displays a module on a specified object.
		/// </summary>
		public static void DisplayModule( SSObjectSelectable obj, ISelectDisplayHandler module )
		{
			if( !IsDisplayed( obj ) )
			{
				throw new System.Exception( "Object needs to be displayed to display it's module." );
			}
			displayedModule = module;
			SelectionPanel.instance.obj.ClearAllElements();
			module.OnDisplay();
		}
		
		/// <summary>
		/// Stops displaying anything.
		/// </summary>
		public static void StopDisplaying()
		{
			displayedModule = null;
			displayedObject = null;
			SelectionPanel.instance.obj.ClearAllElements();
			SelectionPanel.instance.obj.ClearModules();
			SelectionPanel.instance.obj.ClearIcon();
			SelectionPanel.instance.obj.displayNameText.text = "";
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


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
				DisplayObject( objs[0] );
			}
			else
			{
				StopDisplaying();
			}

			if( selected.Count == 0 )
			{
				SelectionPanel.instance.gameObject.SetActive( false );
				ActionPanel.instance.gameObject.SetActive( false );
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
			{
				StopDisplaying();
			}
			if( selected.Count == 1 )
			{
				DisplayObject( selected[0] );
			}
			else
			{
				StopDisplaying();
			}

			if( selected.Count == 0 )
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
			StopDisplaying();

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