using SS.Content;
using UnityEngine;

namespace SS
{
	/// <summary>
	/// Manages font assets, font styles, standard colors, etc.
	/// </summary>
	public static class FontManager
	{
		public const string MAIN_FONT_PATH = AssetManager.BUILTIN_ASSET_IDENTIFIER + "Chomsky SDF";
		public const string UI_FONT_PATH = AssetManager.BUILTIN_ASSET_IDENTIFIER + "Chomsky - UI";

		public static Color lightColor
		{
			get { return new Color( 1.0f, 0.9803922f, 0.7843137f ); }
		}

		public static Color darkColor
		{
			get { return new Color( 0.2f, 0.1882353f, 0.1082353f ); }
		}
	}
}