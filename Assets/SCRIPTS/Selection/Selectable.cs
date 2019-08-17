using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	public class Selectable : MonoBehaviour
	{
		public Sprite icon;

		public UnityEvent onSelect = new UnityEvent();
		public UnityEvent onDeselect = new UnityEvent();
	}
}