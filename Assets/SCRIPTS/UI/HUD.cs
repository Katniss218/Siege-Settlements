using UnityEngine;

namespace SS.UI
{
	/// <summary>
	/// Represents any type of HUD.
	/// </summary>
	public abstract class HUD : MonoBehaviour
	{
		public abstract void SetColor( Color color );
		public abstract void SetHealthBarFill( float percentHealth );
	}
}