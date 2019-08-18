using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	/// <summary>
	/// Add this to any object to make it selectable by the player.
	/// </summary>
	public class Selectable : MonoBehaviour
	{
		/// <summary>
		/// The icon that is shown on the list of all selected objects.
		/// </summary>
		public Sprite icon;

		/// <summary>
		/// Is called when the object gets selected.
		/// </summary>
		public UnityEvent onSelect = new UnityEvent();

		/// <summary>
		/// Is called when the object gets highlighted. Use it to create UI elements on the SelectionPanel.Object.
		/// </summary>
		public UnityEvent onHighlight = new UnityEvent();

		/// <summary>
		/// Is called when the object gets deselected.
		/// </summary>
		public UnityEvent onDeselect = new UnityEvent();
	}
}