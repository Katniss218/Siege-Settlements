using Katniss.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SS.UI
{
	public class ToolTip
	{
		private static RectTransform toolTip = null;
		private static TextMeshProUGUI titleText = null;
		private static RectTransform lastElement = null; // The container.

		// Global padding of the tooltip. Applies to every element inside of the tooltip.
		private const int leftPadding = 5;
		private const int rightPadding = 5;
		private const int topPadding = 5;
		private const int bottomPadding = 5;

		private const int labelInPadding = 5;  // spacing between label and text (label's side)
		private const int labelOutPadding = 5; // spacing between label and text (text's side)
		// helper fields.
		private const int totalHorizontalPadding = leftPadding + rightPadding;
		private const int totalVerticalPadding = topPadding + bottomPadding;

		// Some defaults.
		private static float defaultLabelWidth
		{
			get
			{
				return width / 3f;
			}
		}

		private const int titleFontSize = 24;
		private const int textFontSize = 14;
		private const FontWeight titleFontWeight = FontWeight.Bold;
		private const FontWeight textFontWeight = FontWeight.Thin;

		/// <summary>
		/// Returns the width of the tooltip (Read Only).
		/// </summary>
		public static float width
		{
			get
			{
				return toolTip.sizeDelta.x;
			}
		}

		/// <summary>
		/// Returns the height of the tooltip (Read Only).
		/// </summary>
		public static float height
		{
			get
			{
				return toolTip.sizeDelta.y;
			}
		}

		/// <summary>
		/// Returns the position of the tooltip's top-left corner (Read Only).
		/// </summary>
		public static Vector2 position
		{
			get
			{
				return toolTip.anchoredPosition;
			}
		}

		private static void Init() // creates the tooptip structure (hidden).
		{
			GameObject gameObject;

			GameObjectUtils.RectTransform( Main.canvas.transform, "ToolTip", Vector2.zero, Main.toolTipBackground.rect.size, new Vector2( 0, 1 ), Vector2.zero, Vector2.zero, out gameObject, out toolTip );

			gameObject.AddComponent<CanvasRenderer>();

			Image img = gameObject.AddImageSliced( Main.toolTipBackground );
			img.raycastTarget = false;

			VerticalLayoutGroup layout = gameObject.AddComponent<VerticalLayoutGroup>();
			layout.childControlWidth = true;
			layout.childControlHeight = false;
			layout.childForceExpandWidth = true;
			layout.childForceExpandHeight = false;
			layout.childAlignment = TextAnchor.UpperCenter;
			layout.padding = new RectOffset( leftPadding, rightPadding, topPadding, bottomPadding );

			ContentSizeFitter fitter = gameObject.AddComponent<ContentSizeFitter>();
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

			GameObject title;
			RectTransform titleTransform;
			// textmesh pro force resets the size to 200x50 WTF??
			GameObjectUtils.RectTransform( toolTip, "Title", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, out title, out titleTransform );
			titleText = title.AddComponent<TextMeshProUGUI>();
			titleText.text = "";
			titleText.font = FontManager.uiFont;
			titleText.color = FontManager.lightColor;
			titleText.fontSize = titleFontSize;
			titleText.fontWeight = titleFontWeight;
			titleText.alignment = TextAlignmentOptions.Center;
			titleText.enableWordWrapping = false;
			titleText.raycastTarget = false;
			titleTransform.sizeDelta = new Vector2( 0, 32 );

			gameObject.SetActive( false );
		}

		/// <summary>
		/// Moves the tooltip to a new position (the origin is at top-left).
		/// </summary>
		/// <param name="screenPos">The new position to move the tooltip to.</param>
		/// <param name="clampToScreen">Should the tooltip be always on the screen, even if the position os outside?</param>
		public static void MoveTo( Vector2 screenPos, bool clampToScreen )
		{
			if( toolTip == null )
				Init();
			// FIXME ----- hidden gameObjects not recalculating? (seems that it's only bugged when it's hidden).
			if( clampToScreen )
			{
				if( screenPos.x + width > Screen.currentResolution.width )
				{
					screenPos.x = Screen.currentResolution.width - width;

					if( screenPos.x < 0 )
					{
						screenPos.x = 0;
					}
				}
				if( screenPos.y - height < 0 )
				{
					screenPos.y = height;

					if( screenPos.y + height > Screen.currentResolution.width )
					{
						screenPos.y = Screen.currentResolution.width - height;
					}
				}
			}
			toolTip.anchoredPosition = screenPos;
		}

		/// <summary>
		/// Sets the visibility of the tooltip to true.
		/// </summary>
		public static void Show()
		{
			if( toolTip == null )
				Init();
			toolTip.gameObject.SetActive( true );
		}

		/// <summary>
		/// Sets the visibility of the tooltip to true, and moves it to the specified screen coordinates.
		/// </summary>
		/// <param name="screenPos">The screen-space position to display the tooltip at.</param>
		public static void ShowAt( Vector2 screenPos )
		{
			if( toolTip == null )
				Init();
			toolTip.gameObject.SetActive( true );
			toolTip.anchoredPosition = screenPos;
		}

		/// <summary>
		/// Sets the visibility of the tooltip to false.
		/// </summary>
		public static void Hide()
		{
			if( toolTip == null )
				Init();
			toolTip.gameObject.SetActive( false );
		}

		/// <summary>
		/// Sets the visibility of the tooltip to false.
		/// </summary>
		public static void Remove()
		{
			if( toolTip == null )
				return;
			Object.Destroy( toolTip.gameObject );
		}

		/// <summary>
		/// Creates a new tooltip. Hidden by default.
		/// </summary>
		/// <param name="newWidth">The width of the new tooltip.</param>
		/// <param name="title">The title of the new tooltip.</param>
		public static void Create( float newWidth, string title )
		{
			if( toolTip == null )
				Init();
			for( int i = 1; i < toolTip.childCount; i++ )
			{
				Object.Destroy( toolTip.GetChild( i ).gameObject );
			}
			lastElement = null;
			titleText.text = title ?? "";
			titleText.color = FontManager.lightColor;
			toolTip.sizeDelta = new Vector2( newWidth, toolTip.sizeDelta.y );
		}
		
		// Adds a container element to the tooltip. container always expands the child to fit to size.
		private static RectTransform AddContainer( string name )
		{
			GameObject gameObject;
			RectTransform transform;
			GameObjectUtils.RectTransform( toolTip, name, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, out gameObject, out transform );

			ContentSizeFitter fitter = gameObject.AddComponent<ContentSizeFitter>();
			fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			HorizontalLayoutGroup layout = gameObject.AddComponent<HorizontalLayoutGroup>();
			layout.childControlWidth = true;
			layout.childControlHeight = true;
			layout.childForceExpandWidth = false;
			layout.childForceExpandHeight = true;

			LayoutRebuilder.ForceRebuildLayoutImmediate( toolTip );

			return transform;
		}

		/// <summary>
		/// Adds text object to the tooltip.
		/// </summary>
		/// <param name="text">The text to display.</param>
		public static void AddText( string text )
		{
			if( toolTip == null )
				Init();

			RectTransform container = AddContainer( "Text" );

			GameObject textGameObject;
			RectTransform textTransform;
			GameObjectUtils.RectTransform( container, "Text", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, out textGameObject, out textTransform );

			TextMeshProUGUI textText = textGameObject.AddComponent<TextMeshProUGUI>();
			textText.raycastTarget = false;
			textText.text = text ?? "";
			textText.font = FontManager.uiFont;
			textText.color = FontManager.lightColor;
			textText.fontSize = textFontSize;
			textText.fontWeight = textFontWeight;
			textText.enableWordWrapping = true;
			textText.alignment = TextAlignmentOptions.Justified;

			ContentSizeFitter fitter = textGameObject.AddComponent<ContentSizeFitter>();
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

			LayoutRebuilder.ForceRebuildLayoutImmediate( toolTip );

			lastElement = container;
		}

		/// <summary>
		/// Adds icon object to the tooltip.
		/// </summary>
		/// <param name="icon">The ison to display.</param>
		public static void AddIcon( Sprite icon )
		{
			if( toolTip == null )
				Init();

			RectTransform container = AddContainer( "Icon" );

			GameObject wrapperGameObject;
			RectTransform wrapperTransform;
			GameObjectUtils.RectTransform( container, "Icon - Wrapper", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, out wrapperGameObject, out wrapperTransform );

			HorizontalLayoutGroup layout = wrapperGameObject.AddComponent<HorizontalLayoutGroup>();
			layout.childControlWidth = false;
			layout.childControlHeight = false;
			layout.childForceExpandWidth = true;
			layout.childForceExpandHeight = false;
			layout.childAlignment = TextAnchor.MiddleCenter;

			GameObject spriteGameObject; // we use wrapper to detach the sprite from layout sizing.
			RectTransform spriteTransform;
			GameObjectUtils.RectTransform( wrapperTransform, "Icon", Vector2.zero, icon.rect.size, Vector2.zero, Vector2.zero, Vector2.zero, out spriteGameObject, out spriteTransform );

			Image iconImage = spriteGameObject.AddComponent<Image>();
			iconImage.sprite = icon;
			iconImage.raycastTarget = false;
			
			LayoutRebuilder.ForceRebuildLayoutImmediate( toolTip );

			lastElement = container;
		}

		/// <summary>
		/// Adds labeled text object to the tooltip.
		/// </summary>
		/// <param name="label">The label (text).</param>
		/// <param name="text">The text to display.</param>
		public static void AddText( string label, string text )
		{
			if( toolTip == null )
				Init();

			RectTransform container = AddContainer( "Labeled Text" );

			GameObject labelGameObject;
			RectTransform labelTransform;
			GameObjectUtils.RectTransform( container, "Label", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, out labelGameObject, out labelTransform );

			TextMeshProUGUI labelText = labelGameObject.AddComponent<TextMeshProUGUI>();
			labelText.text = label ?? "";
			labelText.font = FontManager.uiFont;
			labelText.color = FontManager.lightColor;
			labelText.fontSize = textFontSize;
			labelText.fontWeight = FontWeight.Thin;
			labelText.alignment = TextAlignmentOptions.TopRight;
			labelText.color = new Color( 0.85f, 0.85f, 0.85f );
			labelText.characterSpacing = 0;
			labelText.enableWordWrapping = true;
			labelText.margin = new Vector4( 0, 0, labelInPadding, 0 );
			labelText.raycastTarget = false;
			labelTransform.sizeDelta = new Vector2( defaultLabelWidth, 0 );

			LayoutElement labelLayoutElement = labelGameObject.AddComponent<LayoutElement>();
			labelLayoutElement.minWidth = defaultLabelWidth;
			labelLayoutElement.preferredWidth = defaultLabelWidth;


			GameObject textGameObject;
			RectTransform textTransform;
			GameObjectUtils.RectTransform( container, "Text", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, out textGameObject, out textTransform );

			TextMeshProUGUI textText = textGameObject.AddComponent<TextMeshProUGUI>();
			textText.text = text ?? "";
			textText.font = FontManager.uiFont;
			textText.color = FontManager.lightColor;
			textText.fontSize = textFontSize;
			textText.fontWeight = FontWeight.Thin;
			textText.alignment = TextAlignmentOptions.Justified;
			textText.characterSpacing = 0;
			textText.enableWordWrapping = true;
			textText.margin = new Vector4( labelOutPadding, 0, 0, 0 );
			textText.raycastTarget = false;

			LayoutRebuilder.ForceRebuildLayoutImmediate( toolTip );

			lastElement = container;
		}

		/// <summary>
		/// Adds labeled text object to the tooltip.
		/// </summary>
		/// <param name="label">The label (icon).</param>
		/// <param name="text">The text to display.</param>
		public static void AddText( Sprite label, string text )
		{
			if( toolTip == null )
				Init();

			RectTransform container = AddContainer( "Labeled Text" );

			GameObject labelGameObject;
			RectTransform labelTransform;
			GameObjectUtils.RectTransform( container, "Label - Wrapper", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, out labelGameObject, out labelTransform );

			HorizontalLayoutGroup layoutLabelWrapper = labelGameObject.AddComponent<HorizontalLayoutGroup>();
			layoutLabelWrapper.childControlWidth = false;
			layoutLabelWrapper.childControlHeight = false;
			layoutLabelWrapper.childForceExpandWidth = false;
			layoutLabelWrapper.childForceExpandHeight = true;
			layoutLabelWrapper.childAlignment = TextAnchor.UpperRight;
			layoutLabelWrapper.padding = new RectOffset( 0, labelInPadding, 0, 0 );

			LayoutElement labelLayoutElement = labelGameObject.AddComponent<LayoutElement>();
			labelLayoutElement.minWidth = defaultLabelWidth;
			labelLayoutElement.preferredWidth = defaultLabelWidth;

			GameObject labelRealGO;
			RectTransform labelRealTransform;
			GameObjectUtils.RectTransform( labelTransform, "Label", Vector2.zero, label.rect.size, Vector2.zero, Vector2.zero, Vector2.zero, out labelRealGO, out labelRealTransform );

			Image labelImage = labelRealGO.AddComponent<Image>();
			labelImage.sprite = label;
			labelImage.raycastTarget = false;


			GameObject textGO;
			RectTransform textTransform;
			GameObjectUtils.RectTransform( container, "Text", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, out textGO, out textTransform );

			TextMeshProUGUI textText = textGO.AddComponent<TextMeshProUGUI>();
			textText.text = text ?? "";
			textText.font = FontManager.uiFont;
			textText.color = FontManager.lightColor;
			textText.fontSize = textFontSize;
			textText.fontWeight = FontWeight.Thin;
			textText.alignment = TextAlignmentOptions.Justified;
			textText.enableWordWrapping = true;
			textText.margin = new Vector4( labelOutPadding, 0, 0, 0 );
			textText.raycastTarget = false;

			LayoutRebuilder.ForceRebuildLayoutImmediate( toolTip );

			lastElement = container;
		}

		private static bool IsLabeled( Transform container )
		{
			return container.childCount == 2;
		}

		private static bool IsTextLabel( Transform labelWrapper )
		{
			return labelWrapper.childCount == 0;
		}

		private static bool IsIconLabel( Transform labelWrapper )
		{
			return labelWrapper.childCount == 1;
		}

		/// <summary>
		/// Used to change the appearance of the most recently added element element.
		/// </summary>
		public static class Style
		{
			/// <summary>
			/// Sets the left and right padding for the most recently added element. Applies to every element.
			/// </summary>
			/// <param name="left">The left padding (in px).</param>
			/// <param name="right">The right padding (in px).</param>
			public static void SetPadding( int left, int right )
			{
				if( left < 0 || right < 0 )
				{
					throw new Exception( "Padding can't be less than 0." );
				}
				if( lastElement == null )
				{
					throw new Exception( "The last element is null." );
				}
				lastElement.GetComponent<HorizontalOrVerticalLayoutGroup>().padding = new RectOffset( left, right, 0, 0 );
			}

			/// <summary>
			/// Sets the color of the text-label of the most recently added element. Applies to elements labeled with text.
			/// </summary>
			/// <param name="color">The new label color.</param>
			public static void SetLabelColor( Color color )
			{
				lastElement.GetChild( 0 ).GetComponent<TextMeshProUGUI>().color = color;
			}

			/// <summary>
			/// Sets the style of the text-label of the most recently added element. Applies to elements labeled with text.
			/// </summary>
			/// <param name="style">The new label style.</param>
			public static void SetLabelStyle( FontStyles style )
			{
				lastElement.GetChild( 0 ).GetComponent<TextMeshProUGUI>().fontStyle = style;
			}

			/// <summary>
			/// Sets the color of the text of the most recently added element. Applies to both unlabeled and labeled text elements.
			/// </summary>
			/// <param name="color">The new text color.</param>
			public static void SetTextColor( Color color )
			{
				if( IsLabeled( lastElement ) )
				{
					lastElement.GetChild( 1 ).GetComponent<TextMeshProUGUI>().color = color;
				}
				else
				{
					lastElement.GetChild( 0 ).GetComponent<TextMeshProUGUI>().color = color;
				}
			}

			/// <summary>
			/// Sets the style of the text of the most recently added element. Applies to both unlabeled and labeled text elements.
			/// </summary>
			/// <param name="style">The new text style.</param>
			public static void SetTextStyle( FontStyles style )
			{
				if( IsLabeled( lastElement ) )
				{
					lastElement.GetChild( 1 ).GetComponent<TextMeshProUGUI>().fontStyle = style;
				}
				else
				{
					lastElement.GetChild( 0 ).GetComponent<TextMeshProUGUI>().fontStyle = style;
				}
			}

			/// <summary>
			/// Sets the width of the label (5px is subtracted as the spacer between it and the text).
			/// </summary>
			/// <param name="width">The new width of the label. Padding not included.</param>
			public static void SetLabelWidth( float width )
			{
				if( width < 0 )
				{
					throw new Exception( "Label width can't be less than 0 " );
				}
				LayoutElement e = lastElement.GetChild( 0 ).GetComponent<LayoutElement>();
				e.minWidth = width;
				e.preferredWidth = width;
			}
		}
	}
}