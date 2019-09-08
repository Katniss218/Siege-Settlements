using UnityEngine;
using TMPro;

namespace SS
{
	/// <summary>
	/// Manages font assets, font styles, standard colors, etc.
	/// </summary>
	public static class FontManager
	{
		public static Color lightColor
		{
			get { return new Color( 1.0f, 0.9803922f, 0.7843137f ); }
		}

		public static Color darkColor
		{
			get { return new Color( 0.2f, 0.1882353f, 0.1082353f ); }
		}



		private static TMP_FontAsset __mainFont = null;
		public static TMP_FontAsset mainFont
		{
			get
			{
				if( __mainFont == null )
				{
					__mainFont = Resources.Load<TMP_FontAsset>( "Chomsky SDF" );
				}
				return __mainFont;
			}
		}

		private static TMP_FontAsset __uiFont = null;
		public static TMP_FontAsset uiFont
		{
			get
			{
				if( __uiFont == null )
				{
					__uiFont = Resources.Load<TMP_FontAsset>( "Chomsky - UI" );
				}
				return __uiFont;
			}
		}
	}
}