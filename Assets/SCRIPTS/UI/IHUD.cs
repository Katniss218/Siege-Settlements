using UnityEngine;

namespace SS.UI
{
	/// <summary>
	/// Represents any HUD.
	/// </summary>
	public interface IHUD
	{
		void SetColor( Color color );
		void SetHealthBarFill( float percentHealth );
	}
}