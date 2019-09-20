using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	/// <summary>
	/// Add this to any object to make it selectable by the player.
	/// </summary>
	public class Selectable : MonoBehaviour
	{
		private static List<Selectable> __every = new List<Selectable>();

		/// <summary>
		/// Returns every selectable object in the scene that's currently active and enabled.
		/// </summary>
		public static Selectable[] GetAllInScene()
		{
			return __every.ToArray();
		}

		/// <summary>
		/// The icon that is shown on the list of all selected objects.
		/// </summary>
		public Sprite icon;

		/// <summary>
		/// Is called when the object gets selected.
		/// </summary>
		public UnityEvent onSelect = new UnityEvent();

		/// <summary>
		/// Is called when the object gets highlighted.
		/// </summary>
		public UnityEvent onHighlight = new UnityEvent();

		/// <summary>
		/// Is called when the object gets deselected.
		/// </summary>
		public UnityEvent onDeselect = new UnityEvent();

		/// <summary>
		/// Is called whenever the SelectionPanel.Object needs updating. Use it to create UI elements on the SelectionPanel.Object.
		/// </summary>
		public UnityEvent onSelectionUIRedraw = new UnityEvent();


		// Cache the existing selectables to greatly reduce load (searching objects).
		void OnEnable()
		{
			__every.Add( this );
		}

		void OnDisable()
		{
			__every.Remove( this );
		}
	}
}