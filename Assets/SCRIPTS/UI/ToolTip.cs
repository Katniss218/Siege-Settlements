using SS.Content;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SS.UI
{
	/// <summary>
	/// Used to create and manipulate the procedural "ToolTip" info box.
	/// </summary>
	public static class ToolTip
	{
		private static RectTransform toolTipTransform = null;
		private static TextMeshProUGUI titleText = null;
		private static Transform lastElement = null; // The container.
		
		private static float defaultLabelWidth
		{
			get
			{
				return width / 3f;
			}
		}
		
		private static void HideToolTip()
		{
			toolTipTransform.anchoredPosition = new Vector2( -9999.0f, -9999.0f );
		}

		/// <summary>
		/// Returns true if the ToolTip is not visible, false otherwise (Read Only).
		/// </summary>
		public static bool isHidden
		{
			get
			{
				return toolTipTransform.anchoredPosition == new Vector2( -9999.0f, -9999.0f );
			}
		}

		/// <summary>
		/// Returns the width of the tooltip (Read Only).
		/// </summary>
		public static float width
		{
			get
			{
				return toolTipTransform.sizeDelta.x;
			}
		}

		/// <summary>
		/// Returns the height of the tooltip (Read Only).
		/// </summary>
		public static float height
		{
			get
			{
				return toolTipTransform.sizeDelta.y;
			}
		}

		/// <summary>
		/// Returns the position of the tooltip's top-left corner (Read Only).
		/// </summary>
		public static Vector2 position
		{
			get
			{
				return toolTipTransform.anchoredPosition;
			}
		}

		private static void Init() // creates the tooptip structure (hidden).
		{
			GameObject tooltipGameObject = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Tooltip Elements/ToolTip Container" ), Main.canvas.transform );
			
			toolTipTransform = tooltipGameObject.GetComponent<RectTransform>();
			
			GameObject title = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Tooltip Elements/Title" ), toolTipTransform );

			titleText = title.GetComponent<TextMeshProUGUI>();

			toolTipTransform.anchoredPosition = new Vector2( -9999, -9999 );
		}

		/// <summary>
		/// Moves the tooltip to a new position (the origin is at top-left).
		/// </summary>
		/// <param name="screenPos">The new position to move the tooltip to.</param>
		/// <param name="clampToScreen">Should the tooltip be always on the screen, even if the position os outside?</param>
		public static void MoveTo( Vector2 screenPos, bool clampToScreen )
		{
			if( toolTipTransform == null )
			{
				Init();
			}
			if( isHidden )
			{
				return;
			}
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
			toolTipTransform.anchoredPosition = screenPos;
		}

		/// <summary>
		/// Sets the visibility of the tooltip to true, and moves it to the specified screen coordinates.
		/// </summary>
		/// <param name="screenPos">The screen-space position to display the tooltip at.</param>
		public static void ShowAt( Vector2 screenPos )
		{
			if( toolTipTransform == null )
			{
				Init();
			}
			toolTipTransform.anchoredPosition = screenPos;
		}

		/// <summary>
		/// Sets the visibility of the tooltip to false.
		/// </summary>
		public static void Hide()
		{
			if( toolTipTransform == null )
			{
				Init();
			}
			HideToolTip();
		}

		/// <summary>
		/// Sets the visibility of the tooltip to false.
		/// </summary>
		public static void Remove()
		{
			if( toolTipTransform == null )
			{
				return;
			}
			Object.Destroy( toolTipTransform.gameObject );
		}

		/// <summary>
		/// Creates a new tooltip. Hidden by default.
		/// </summary>
		/// <param name="newWidth">The width of the new tooltip.</param>
		/// <param name="title">The title of the new tooltip.</param>
		public static void Create( float newWidth, string title )
		{
			if( toolTipTransform == null )
			{
				Init();
			}
			for( int i = 1; i < toolTipTransform.childCount; i++ )
			{
				Object.Destroy( toolTipTransform.GetChild( i ).gameObject );
			}
			lastElement = null;
			titleText.text = title ?? "";
			toolTipTransform.sizeDelta = new Vector2( newWidth, toolTipTransform.sizeDelta.y );
		}

		/// <summary>
		/// Adds text object to the tooltip.
		/// </summary>
		/// <param name="text">The text to display.</param>
		public static void AddText( string text )
		{
			if( toolTipTransform == null )
			{
				Init();
			}

			GameObject element = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Tooltip Elements/Text" ), toolTipTransform );

			TextMeshProUGUI textText = element.transform.Find( "Text" ).GetComponent<TextMeshProUGUI>();
			textText.text = text ?? "";

			LayoutRebuilder.ForceRebuildLayoutImmediate( toolTipTransform );

			lastElement = element.transform;
		}

		/// <summary>
		/// Adds icon object to the tooltip.
		/// </summary>
		/// <param name="icon">The ison to display.</param>
		public static void AddIcon( Sprite icon )
		{
			if( icon == null )
			{
				throw new Exception( "Icon can't be null." );
			}
			if( toolTipTransform == null )
			{
				Init();
			}

			GameObject element = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Tooltip Elements/Icon" ), toolTipTransform );

			Image iconImage = element.transform.Find( "Icon - Wrapper" ).Find( "Icon" ).GetComponent<Image>();
			iconImage.sprite = icon;

			LayoutRebuilder.ForceRebuildLayoutImmediate( toolTipTransform );

			lastElement = element.transform;
		}

		/// <summary>
		/// Adds labeled text object to the tooltip.
		/// </summary>
		/// <param name="label">The label (text).</param>
		/// <param name="text">The text to display.</param>
		public static void AddText( string label, string text )
		{
			if( toolTipTransform == null )
			{
				Init();
			}

			GameObject element = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Tooltip Elements/Text(Text)" ), toolTipTransform );

			Transform labelTransform = element.transform.Find( "Label" );
			LayoutElement labelLayoutElement = labelTransform.GetComponent<LayoutElement>();
			labelLayoutElement.minWidth = defaultLabelWidth;
			labelLayoutElement.preferredWidth = defaultLabelWidth;


			TextMeshProUGUI labelText = labelTransform.GetComponent<TextMeshProUGUI>();
			labelText.text = label ?? "";


			TextMeshProUGUI textText = element.transform.Find( "Text" ).GetComponent<TextMeshProUGUI>();
			textText.text = text ?? "";

			LayoutRebuilder.ForceRebuildLayoutImmediate( toolTipTransform );

			lastElement = element.transform;
		}

		/// <summary>
		/// Adds labeled text object to the tooltip.
		/// </summary>
		/// <param name="label">The label (icon).</param>
		/// <param name="text">The text to display.</param>
		public static void AddText( Sprite label, string text )
		{
			if( label == null )
			{
				throw new Exception( "Label icon can't be null." );
			}
			if( toolTipTransform == null )
			{
				Init();
			}

			GameObject element = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Tooltip Elements/Text(Icon)" ), toolTipTransform );

			Transform labelTransform = element.transform.Find( "Label - Wrapper" );
			LayoutElement labelLayoutElement = labelTransform.GetComponent<LayoutElement>();
			labelLayoutElement.minWidth = defaultLabelWidth;
			labelLayoutElement.preferredWidth = defaultLabelWidth;


			Image labelImage = labelTransform.Find( "Label" ).GetComponent<Image>();
			labelImage.sprite = label;
			labelImage.GetComponent<RectTransform>().sizeDelta = label.rect.size;


			TextMeshProUGUI textText = element.transform.Find( "Text" ).GetComponent<TextMeshProUGUI>();
			textText.text = text ?? "";
			
			LayoutRebuilder.ForceRebuildLayoutImmediate( toolTipTransform );

			lastElement = element.transform;
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

				LayoutRebuilder.ForceRebuildLayoutImmediate( toolTipTransform );
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

				LayoutRebuilder.ForceRebuildLayoutImmediate( toolTipTransform );
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

				LayoutRebuilder.ForceRebuildLayoutImmediate( toolTipTransform );
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

				LayoutRebuilder.ForceRebuildLayoutImmediate( toolTipTransform );
			}
		}
	}
}