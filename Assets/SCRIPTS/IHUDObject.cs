using UnityEngine;

namespace SS
{
	/// <summary>
	/// Represents an object that has a HUD attached to it.
	/// </summary>
	public interface IHUDObject
	{
		GameObject hud { get; }
	}
}