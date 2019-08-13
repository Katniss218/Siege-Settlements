using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	public class SelectionPanel : MonoBehaviour
	{
		private static Dictionary<ISelectable, GameObject> icons = new Dictionary<ISelectable, GameObject>();

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
				ModeObject();
			}
			else
			{
				ModeList();
			}
		}

		private static void ModeObject()
		{
			mode = SelectionPanelMode.Object;

			listTransform.gameObject.SetActive( false );
			objectTransform.gameObject.SetActive( true );
			switcherImage.sprite = Main.switcherObjA;
		}

		private static void ModeList()
		{
			mode = SelectionPanelMode.List;

			listTransform.gameObject.SetActive( true );
			objectTransform.gameObject.SetActive( false );
			switcherImage.sprite = Main.switcherListA;
		}

		/// <summary>
		/// Switches between the Object and List modes.
		/// </summary>
		public void _SwitchMode()
		{
			SwitchMode();
		}

		private static RectTransform panelTransform;

		private static RectTransform objectTransform;
		private static RectTransform listTransform;
		private static Image switcherImage;


		private void Awake()
		{
			panelTransform = this.GetComponent<RectTransform>();
			objectTransform = this.transform.Find( "Object" ).GetComponent<RectTransform>();
			listTransform = this.transform.Find( "List" ).GetComponent<RectTransform>();
			switcherImage = this.transform.Find( "Button" ).GetComponent<Image>();

			ModeObject();
		}

		void Start()
		{

		}

		void Update()
		{

		}

		/// <summary>
		/// Adds an icon and binds it to the specified object.
		/// </summary>
		/// <param name="obj">The object to associate the icon with.</param>
		/// <param name="icon">The icon to display.</param>
		public static void ListAddIcon( ISelectable obj, Sprite icon )
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
		public static void ListRemoveIcon( ISelectable obj )
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
		public static void ListClear()
		{
			foreach( var icon in icons )
			{
				Destroy( icon.Value );
			}
			icons.Clear();
		}
	}
}