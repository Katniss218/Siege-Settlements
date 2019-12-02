using UnityEngine;

namespace SS
{
	/// <summary>
	/// Represents an object that has a HUD attached to it.
	/// </summary>
	public interface IHUDHolder
	{
		GameObject hud { get; }
	}
}