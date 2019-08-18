using TMPro;
using UnityEngine;

namespace SS.UI.Elements
{
	public struct TextData
	{
		public TMP_FontAsset font;

		public int fontSize;
		public FontStyles fontStyle;
		public TextAlignmentOptions alignment;
		public Color fontColor;

		public TextData( TMP_FontAsset font, int fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Color fontColor )
		{
			this.font = font;
			this.fontSize = fontSize;
			this.fontStyle = fontStyle;
			this.alignment = alignment;
			this.fontColor = fontColor;
		}

		public void ApplyTo( TextMeshProUGUI obj )
		{
			obj.font = this.font;
			obj.fontSize = this.fontSize;
			obj.fontStyle = this.fontStyle;
			obj.alignment = alignment;
			obj.color = this.fontColor;
		}
	}
}