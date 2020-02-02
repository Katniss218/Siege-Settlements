using UnityEngine;

namespace SS.Objects.Modules
{
	public interface IInteriorUser
	{
		Transform transform { get; }

		Sprite icon { get; }


		InteriorModule interior { get; }
		InteriorModule.SlotType slotType { get; }
		int slotIndex { get; }

		bool isInside { get; }
		bool isInsideHidden { get; }


		void SetInside( InteriorModule interior, InteriorModule.SlotType slotType, int slotIndex );
		void SetOutside();
	}
}