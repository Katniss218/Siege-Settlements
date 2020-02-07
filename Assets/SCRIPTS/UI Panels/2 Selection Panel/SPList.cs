using SS.Objects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	[DisallowMultipleComponent]
	public class SPList : MonoBehaviour
	{
		private Dictionary<SSObject, GameObject> icons = new Dictionary<SSObject, GameObject>();

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
		public void AddIcon( SSObject obj, Sprite icon )
		{
			if( obj == null )
			{
				Debug.LogWarning( "ListAddIcon: The object was null." );
				return;
			}
			GameObject gameObject = new GameObject( "Icon" );
			RectTransform trans = gameObject.AddComponent<RectTransform>();
			trans.SetParent( this.transform );

			Image image = gameObject.AddComponent<Image>();
			image.sprite = icon;

			icons.Add( obj, gameObject );
		}

		/// <summary>
		/// Removes an icon associated with the specified object. Also, un-associates the object with any icon.
		/// </summary>
		/// <param name="obj">The object whose icon to remove.</param>
		public void RemoveIcon( SSObject obj )
		{
			if( obj == null )
			{
				Debug.LogWarning( "ListRemoveIcon: The object was null." );
				return;
			}

			GameObject gameObject = null;
			if( icons.TryGetValue( obj, out gameObject ) )
			{
				Object.Destroy( gameObject );
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
		public void Clear()
		{
			foreach( var icon in icons )
			{
				Object.Destroy( icon.Value );
			}
			icons.Clear();
		}
	}
}