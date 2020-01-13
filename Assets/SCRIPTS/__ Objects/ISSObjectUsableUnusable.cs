using UnityEngine.Events;

namespace SS.Objects
{
	interface ISSObjectUsableUnusable
	{
		UnityEvent onUsableStateChanged { get; }

		bool isUsable { get; set; }
	}
}