using SS.Objects.Modules;
using SS.Objects;
using SS.UI;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SS
{
	public class Selection
	{
		private static Type[] moduleTypeSorted = new Type[]
		{
			typeof( ConstructorModule ),
			typeof( BarracksModule ),
			typeof( ResearchModule ),
			typeof( InteriorModule ),
			typeof( InventoryModule ),
		};

		private class DisplayedObjectData
		{
			public ISelectDisplayHandler obj { get; private set; } = null;
			public ISelectDisplayHandler module { get; private set; } = null;
			public bool isGroup { get; private set; } = false;

			public static DisplayedObjectData NewGroup()
			{
				return new DisplayedObjectData() { obj = null, module = null, isGroup = true };
			}

			public static DisplayedObjectData NewObject( ISelectDisplayHandler obj )
			{
				return new DisplayedObjectData() { obj = obj, module = null, isGroup = false };
			}

			public static DisplayedObjectData NewObject( ISelectDisplayHandler obj, ISelectDisplayHandler module )
			{
				return new DisplayedObjectData() { obj = obj, module = module, isGroup = false };
			}
		}

		private static List<SSObject> selected = new List<SSObject>();

		private static List<SSObject>[] groups = new List<SSObject>[10]
		{
			new List<SSObject>(),
			new List<SSObject>(),
			new List<SSObject>(),
			new List<SSObject>(),
			new List<SSObject>(),
			new List<SSObject>(),
			new List<SSObject>(),
			new List<SSObject>(),
			new List<SSObject>(),
			new List<SSObject>()
		};
			
		public static SSObject[] GetGroup( byte index )
		{
			if( index < 0 || index > 9 )
			{
				throw new Exception( "Invalid index. Can only have groups <0-9>." );
			}

			// Skip all dead objects. They'll get clared when the group is reassigned.
			List<SSObject> sel = new List<SSObject>();
			for( int i = 0; i < groups[index].Count; i++ )
			{
				if( groups[index][i] == null )
				{
					continue;
				}

				sel.Add( groups[index][i] );
			}
			return sel.ToArray();
		}

		public static void SetGroup( byte index, SSObject[] objects )
		{
			if( index < 0 || index > 9 )
			{
				throw new Exception( "Invalid index. Can only have groups <0-9>." );
			}
			
			for( int i = 0; i < groups[index].Count; i++ )
			{
				if( groups[index][i] == null )
				{
					continue;
				}
				
				if( groups[index][i] is SSObjectDFC )
				{
					((SSObjectDFC)groups[index][i]).hudDFSC.SetSelectionGroup( null );
				}
			}
			groups[index].Clear();
			groups[index].AddRange( objects );

			for( int i = 0; i < objects.Length; i++ )
			{
				if( groups[index][i] is SSObjectDFC )
				{
					((SSObjectDFC)groups[index][i]).hudDFSC.SetSelectionGroup( index );
				}
			}
		}

		/// <summary>
		/// Returns a copy of the selected objects.
		/// </summary>
		public static SSObject[] GetSelectedObjects()
		{
			return selected.ToArray();
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		// When object is null, module also must be null.
		// When module is not null, object also can't be null.
		private static DisplayedObjectData displayedObjectData = null;

		public static ISelectDisplayHandler displayedObject
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

		public static ISelectDisplayHandler displayedModule
		{
			get
			{
				if( displayedObjectData == null )
				{
					return null;
				}
				return displayedObjectData.module;
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

		public static bool IsDisplayed( SSObject obj )
		{
			if( displayedObjectData == null )
			{
				return false;
			}
			return (object)displayedObjectData.obj == (object)obj;
		}

		public static bool IsDisplayedGroup()
		{
			return displayedObjectData != null && displayedObjectData.isGroup;
		}

		public static bool IsDisplayedModule( SSModule module )
		{
			if( !(module is ISelectDisplayHandler) )
			{
				throw new Exception( "This module can't be displayed" );
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
				if( selected[i] is IDamageable )
				{
					IDamageable damageable = (IDamageable)selected[i];

					healthTotal += damageable.health;
					healthMaxTotal += damageable.healthMax;
				}
			}
			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), "Total Health: " + (int)healthTotal + "/" + (int)healthMaxTotal );
			SelectionPanel.instance.obj.RegisterElement( "group.health", healthUI.transform );
		}

		/// <summary>
		/// Displays an object.
		/// </summary>
		public static void DisplayObject( SSObject obj )
		{
			if( obj == null )
			{
				throw new Exception( "Object can't be null." );
			}
			if( !(obj is ISelectDisplayHandler) )
			{
				throw new Exception( "Object is not selectable." );
			}

			ISelectDisplayHandler objSel = (ISelectDisplayHandler)obj;
			

			if( !(obj is ISSObjectUsableUnusable) || ((ISSObjectUsableUnusable)obj).isUsable )
			{
				SSModule[] modules = obj.GetModules();
				
				for( int i = 0; i < modules.Length; i++ )
				{
					if( !(modules[i] is ISelectDisplayHandler) )
					{
						continue;
					}
					SelectionPanel.instance.obj.CreateModuleButton( modules[i] );

				}
				if( modules.Length == 0 )
				{
					displayedObjectData = DisplayedObjectData.NewObject( objSel );
					objSel.OnDisplay();
				}
				else
				{
					for( int i = 0; i < moduleTypeSorted.Length; i++ )
					{
						for( int j = 0; j < modules.Length; j++ )
						{
							if( modules[j].GetType() != moduleTypeSorted[i] )
							{
								continue;
							}

							ISelectDisplayHandler moduleSel = (ISelectDisplayHandler)modules[j];
							displayedObjectData = DisplayedObjectData.NewObject( objSel, moduleSel );
							objSel.OnDisplay();
							moduleSel.OnDisplay();
							SelectionPanel.instance.obj.HighlightIcon( modules[j] );

							return;
						}
					}

					// If there is no modules to display on this object (no displayable modules or no modules at all).
					displayedObjectData = DisplayedObjectData.NewObject( objSel );
					objSel.OnDisplay();
				}
			}
			else
			{
				displayedObjectData = DisplayedObjectData.NewObject( objSel );
				objSel.OnDisplay();
			}
		}

		/// <summary>
		/// Displays a module on a specified object.
		/// </summary>
		public static void DisplayModule( SSObject obj, SSModule module )
		{
			if( !(obj is ISelectDisplayHandler) )
			{
				throw new Exception( "This module can't be displayed" );
			}
			if( !(module is ISelectDisplayHandler) )
			{
				throw new Exception( "This module can't be displayed" );
			}
			if( !IsDisplayed( obj ) )
			{
				throw new Exception( "Object needs to be displayed to display it's module." );
			}

			GetDisplayedThing()?.OnHide();
			SelectionPanel.instance.obj.ClearAllElements();
			ActionPanel.instance.ClearAll();

			ISelectDisplayHandler objSel = (ISelectDisplayHandler)obj;
			ISelectDisplayHandler moduleSel = (ISelectDisplayHandler)module;

			displayedObjectData = DisplayedObjectData.NewObject( objSel, moduleSel );

			objSel.OnDisplay();
			moduleSel.OnDisplay();
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
		public static bool IsSelected( SSObject obj )
		{
			return selected.Contains( obj );
		}

		public static int TrySelect( params SSObject[] objs )
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

				if( !objs[i].isSelectable )
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
		public static void Deselect( SSObject obj )
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

		/// <summary>
		/// Clears the selection. Useful for deselecting everything when the scene is unloaded.
		/// </summary>
		public static void Clear()
		{
			selected.Clear();
		}
	}
}