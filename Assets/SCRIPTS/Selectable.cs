using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	public class Selectable : MonoBehaviour
	{
		public class _UnityEvent_Selectable : UnityEvent<Selectable> { }

		public Sprite icon;

		public _UnityEvent_Selectable onSelect = new _UnityEvent_Selectable();
		public _UnityEvent_Selectable onDeselect = new _UnityEvent_Selectable();
	}
}