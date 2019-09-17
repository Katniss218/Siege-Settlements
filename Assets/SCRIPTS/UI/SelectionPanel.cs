using SS.Content;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	public class SelectionPanel : MonoBehaviour
	{
		/// <summary>
		/// The current mode that the Selection Panel is in.
		/// </summary>
		public static SelectionPanelMode mode { get; private set; }
		
		/// <summary>
		/// Switches between the Object and List modes.
		/// </summary>
		public static void SwitchMode()
		{
			if( mode == SelectionPanelMode.List )
			{
				SetModeObject();
			}
			else
			{
				SetModeList();
			}
		}

		private static void SetModeObject()
		{
			mode = SelectionPanelMode.Object;

			switcherImage.sprite = AssetManager.GetSprite( AssetManager.RESOURCE_ID + "Textures/obj_lst" );
			listTransform.gameObject.SetActive( false );
			objectTransform.gameObject.SetActive( true );
		}

		private static void SetModeList()
		{
			mode = SelectionPanelMode.List;

			switcherImage.sprite = AssetManager.GetSprite( AssetManager.RESOURCE_ID + "Textures/lst_obj" );
			listTransform.gameObject.SetActive( true );
			objectTransform.gameObject.SetActive( false );
		}

		/// <summary>
		/// Switches between the Object and List modes.
		/// </summary>
		public void _SwitchMode()
		{
			SwitchMode();
		}

		private static RectTransform panelTransform;

		public static RectTransform objectTransform { get; private set; }
		public static RectTransform listTransform { get; private set; }
		private static Image switcherImage;


		private void Awake()
		{
			panelTransform = this.GetComponent<RectTransform>();
			objectTransform = this.transform.Find( "Object" ).GetComponent<RectTransform>();
			listTransform = this.transform.Find( "List" ).GetComponent<RectTransform>();
			switcherImage = this.transform.Find( "Switcher Button" ).GetComponent<Image>();

			SetModeObject();
		}
		
		public static class List
		{
			private static Dictionary<Selectable, GameObject> icons = new Dictionary<Selectable, GameObject>();
			/// <summary>
			/// Adds an icon and binds it to the specified object.
			/// </summary>
			/// <param name="obj">The object to associate the icon with.</param>
			/// <param name="icon">The icon to display.</param>
			public static void AddIcon( Selectable obj, Sprite icon )
			{
				if( obj == null )
				{
					Debug.LogWarning( "ListAddIcon: The object was null." );
					return;
				}
				GameObject gameObject = new GameObject( "Icon" );
				RectTransform trans = gameObject.AddComponent<RectTransform>();
				trans.SetParent( listTransform );

				Image image = gameObject.AddComponent<Image>();
				image.sprite = icon;

				icons.Add( obj, gameObject );
			}

			/// <summary>
			/// Removes an icon associated with the specified object. Also, un-associates the object with any icon.
			/// </summary>
			/// <param name="obj">The object whose icon to remove.</param>
			public static void RemoveIcon( Selectable obj )
			{
				if( obj == null )
				{
					Debug.LogWarning( "ListRemoveIcon: The object was null." );
					return;
				}

				GameObject gameObject = null;
				if( icons.TryGetValue( obj, out gameObject ) )
				{
					Destroy( gameObject );
					icons.Remove( obj );
				}
				else
				{
					Debug.LogWarning( "ListRemoveIcon: The object was not associated with any icon." );
				}
			}

			/// <summary>
			/// Clears the icons' list.
			/// </summary>
			public static void Clear()
			{
				foreach( var icon in icons )
				{
					Destroy( icon.Value );
				}
				icons.Clear();
			}
		}

		public static class Object
		{
			/// <summary>
			/// Clears every UI element that belongs to highlighted objects.
			/// </summary>
			public static void Clear()
			{
				for( int i = 0; i < objectTransform.childCount; i++ )
				{
					Destroy( objectTransform.GetChild( i ).gameObject );
				}
			}
		}
	}
}