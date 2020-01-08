using SS.Objects.Modules;
using UnityEngine;

namespace SS.Objects.Units
{
	public interface IInteriorUser
	{
		Transform transform { get; }

		Sprite icon { get; }

		InteriorModule interior { get; }
		int slotIndex { get; }

		bool isInside
		{
			get;
		}
		bool isInsideHidden { get; }

		void SetInside( InteriorModule interior, InteriorModule.SlotType slotType, int slotIndex );
		void SetOutside();
	}
}
 