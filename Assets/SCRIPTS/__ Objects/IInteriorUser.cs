using UnityEngine;

namespace SS.Objects.Modules
{
	/// <summary>
	/// Represents a thing that can go inside interiors.
	/// </summary>
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