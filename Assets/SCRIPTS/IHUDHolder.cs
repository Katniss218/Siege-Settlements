using SS.UI;

namespace SS
{
	/// <summary>
	/// Represents an object that has a HUD attached to it.
	/// </summary>
	public interface IHUDHolder
	{
		HUD hud { get; }
	}
}