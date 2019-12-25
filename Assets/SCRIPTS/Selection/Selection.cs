﻿using SS.Objects.Modules;
using SS.Objects;
using SS.UI;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public class Selection
	{
		private class DisplayedObjectData
		{
			public SSObjectDFS obj { get; private set; } = null;
			public ISelectDisplayHandler module { get; private set; } = null;
			public bool isGroup { get; private set; } = false;

			public static DisplayedObjectData NewGroup()
			{
				return new DisplayedObjectData() { obj = null, module = null, isGroup = true };
			}

			public static DisplayedObjectData NewObject( SSObjectDFS obj )
			{
				return new DisplayedObjectData() { obj = obj, module = null, isGroup = false };
			}

			public static DisplayedObjectData NewObject( SSObjectDFS obj, SSModule module )
			{
				if( !(module is ISelectDisplayHandler) )
				{
					throw new System.Exception( "Module isn't displayable." );
				}
				return new DisplayedObjectData() { obj = obj, module = ((ISelectDisplayHandler)module), isGroup = false };
			}
		}

		private static List<SSObjectDFS> selected = new List<SSObjectDFS>();

		/// <summary>
		/// Returns a copy of the selected objects.
		/// </summary>
		public static SSObjectDFS[] selectedObjects
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
		// When module is not null, object also can't be null.
		private static DisplayedObjectData displayedObjectData = null;

		public static SSObjectDFS displayedObject
		{
			get
			{
				if( displayedObjectData == null )
				{
					return null;
				}
				return displayedObjectData.obj;
			}
		}

		public static SSModule displayedModule
		{
			get
			{
				if( displayedObjectData == null )
				{
					return null;
				}
				return (SSModule)displayedObjectData.module;
			}
		}

		private static ISelectDisplayHandler GetDisplayedThing()
		{
			if( displayedObjectData == null )
			{
				return null;
			}
			if( displayedObjectData.module != null )
			{
				return displayedObjectData.module;
			}
			if( displayedObjectData.obj != null )
			{
				return displayedObjectData.obj;
			}
			return null;
		}

		public static bool IsDisplayed( SSObjectDFS obj )
		{
			if( displayedObjectData == null )
			{
				return false;
			}
			return displayedObjectData.obj == obj;
		}

		public static bool IsDisplayedGroup()
		{
			return displayedObjectData != null && displayedObjectData.isGroup;
		}

		public static bool IsDisplayedModule( SSModule module )
		{
			if( !(module is ISelectDisplayHandler) )
			{
				throw new System.Exception( "This module can't be displayed" );
			}
			if( displayedObjectData == null )
			{
				return false;
			}
			ISelectDisplayHandler moduleS = (ISelectDisplayHandler)module;
			return displayedObjectData.module == moduleS;
		}

		/// <summary>
		/// Displays a group, based on objects that are selected.
		/// </summary>
		public static void DisplayGroupSelected()
		{
			displayedObjectData = DisplayedObjectData.NewGroup();

			SelectionPanel.instance.obj.displayNameText.text = "Group: " + selected.Count;
			float healthTotal = 0.0f;
			float healthMaxTotal = 0.0f;
			for( int i = 0; i < selected.Count; i++ )
			{
				healthTotal += selected[i].health;
				healthMaxTotal += selected[i].healthMax;
			}
			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), "Total Health: " + (int)healthTotal + "/" + (int)healthMaxTotal );
			SelectionPanel.instance.obj.RegisterElement( "group.health", healthUI.transform );
		}

		/// <summary>
		/// Displays an object.
		/// </summary>
		public static void DisplayObject( SSObjectDFS obj )
		{
			if( obj == null )
			{
				throw new System.Exception( "Object can't be null." );
			}
			displayedObjectData = DisplayedObjectData.NewObject( obj );

			if( !(obj is IUsableToggle) || ((IUsableToggle)obj).IsUsable() )
			{
				SSModule[] modules = ((SSObject)obj).GetModules();

				for( int i = 0; i < modules.Length; i++ )
				{
					if( !(modules[i] is ISelectDisplayHandler) )
					{
						continue;
					}
					SelectionPanel.instance.obj.CreateModuleButton( modules[i] );
				}
			}

			obj.OnDisplay();
			SelectionPanel.instance.obj.HighlightIcon();
		}

		/// <summary>
		/// Displays a module on a specified object.
		/// </summary>
		public static void DisplayModule( SSObjectDFS obj, SSModule module )
		{
			if( !(module is ISelectDisplayHandler) )
			{
				throw new System.Exception( "This module can't be displayed" );
			}
			if( !IsDisplayed( obj ) )
			{
				throw new System.Exception( "Object needs to be displayed to display it's module." );
			}
			GetDisplayedThing()?.OnHide();
			SelectionPanel.instance.obj.ClearAllElements();
			ActionPanel.instance.ClearAll();
			displayedObjectData = DisplayedObjectData.NewObject( obj, module );
			(module as ISelectDisplayHandler).OnDisplay();
			SelectionPanel.instance.obj.HighlightIcon( module );
		}

		/// <summary>
		/// Stops displaying anything.
		/// </summary>
		public static void StopDisplaying()
		{
			GetDisplayedThing()?.OnHide();
			displayedObjectData = null;
			SelectionPanel.instance.obj.ClearAllElements();
			SelectionPanel.instance.obj.ClearAllModules();
			SelectionPanel.instance.obj.ClearIcon();
			SelectionPanel.instance.obj.displayNameText.text = "";

			ActionPanel.instance.ClearAll();
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		/// <summary>
		/// Checks if the object is currently selected.
		/// </summary>
		/// <param name="obj">The object to check.</param>
		public static bool IsSelected( SSObjectDFS obj )
		{
			return selected.Contains( obj );
		}

		public static int TrySelect( SSObjectDFS[] objs )
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
				StopDisplaying();

				DisplayObject( objs[0] );
			}
			else if( selected.Count == 0 )
			{
				SelectionPanel.instance.gameObject.SetActive( false );
				ActionPanel.instance.gameObject.SetActive( false );
			}
			else
			{
				StopDisplaying();

				DisplayGroupSelected();
			}

			return numSelected;
		}

		/// <summary>
		/// Deselects an object.
		/// </summary>
		/// <param name="obj">The object to deselect.</param>
		public static void Deselect( SSObjectDFS obj )
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

			// If the deselected object is displayed - stop displaying it.
			if( IsDisplayed( obj ) || (displayedObjectData != null && displayedObjectData.isGroup) )
			{
				StopDisplaying();
			}
			// If the selection contains only 1 object - display it.
			if( selected.Count == 1 )
			{
				DisplayObject( selected[0] );
			}
			else if( selected.Count == 0 )
			{
				SelectionPanel.instance.gameObject.SetActive( false );
				ActionPanel.instance.gameObject.SetActive( false );
			}
			else
			{
				DisplayGroupSelected();
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

		public static void Clear()
		{
			selected.Clear();
		}
	}
}