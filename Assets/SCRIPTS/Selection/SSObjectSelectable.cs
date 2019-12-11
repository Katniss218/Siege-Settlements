using UnityEngine;
using UnityEngine.Events;

namespace SS.Objects
{
	/// <summary>
	/// Add this to any object to make it selectable by the player.
	/// </summary>
	public abstract class SSObjectSelectable : SSObject, ISelectDisplayHandler
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
		/// Is called when the object gets highlighted.
		/// </summary>
		public UnityEvent onHighlight = new UnityEvent();

		/// <summary>
		/// Is called when the object gets deselected.
		/// </summary>
		public UnityEvent onDeselect = new UnityEvent();

		public abstract void OnDisplay();
		public abstract void OnHide();
	}
}