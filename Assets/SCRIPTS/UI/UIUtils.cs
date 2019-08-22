﻿using Katniss.Utils;
using SS.UI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SS.UI
{
	/// <summary>
	/// Class containing methods for creating standardized Siege Settlements' UI elements at runtime.
	/// </summary>
	public static class UIUtils
	{
		// this is class used for creating user-customizable UI elements. e.g. Selection Panel elements, Resource Panel elements, etc.

		// TODO ----- move tooltip into this.
		private static GameObject __CreateUIElement( Transform parent, string name, GenericUIData basicData )
		{
			GameObject container;
			RectTransform rectTransform;
			GameObjectUtils.RectTransform( parent, name, basicData.position, basicData.size, basicData.pivot, basicData.anchorMin, basicData.anchorMax, out container, out rectTransform );

			return container;
		}


		
		// Creates a text label.
		public static GameObject CreateText( Transform parent, GenericUIData basicData, string text, TextData specificData )
		{
			GameObject container = __CreateUIElement( parent, "Text", basicData );

			TextMeshProUGUI textObj = container.AddComponent<TextMeshProUGUI>();
			textObj.text = text;
			specificData.ApplyTo( textObj );
			container.GetComponent<RectTransform>().sizeDelta = basicData.size;

			return container;
		}

		// Creates an icon label.
		public static GameObject CreateIcon( Transform parent, GenericUIData basicData, Sprite icon, IconData specificData )
		{
			GameObject container = __CreateUIElement( parent, "Icon", basicData );

			Image imageObj = container.AddComponent<Image>();
			imageObj.sprite = icon;
			specificData.ApplyTo( imageObj );

			return container;
		}

		// Creates a generic button.
		public static GameObject CreateButton( Transform parent, GenericUIData basicData, string text, TextData specificData, UnityAction onClick )
		{
			GameObject container = __CreateUIElement( parent, "Button - Text", basicData );

			Image imageObj = container.AddComponent<Image>();
			imageObj.sprite = null; // replace with the real sprite.

			Button buttonObj = container.AddComponent<Button>();
			buttonObj.onClick.AddListener( onClick );


			GameObject textOnButton = __CreateUIElement( container.transform, "Text", new GenericUIData( Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one ) );

			TextMeshProUGUI textObj = textOnButton.AddComponent<TextMeshProUGUI>();
			textObj.text = text;
			specificData.ApplyTo( textObj );
			
			return container;
		}

		// Creates an icon label.
		public static GameObject CreateButton( Transform parent, GenericUIData basicData, Sprite icon, IconData specificData, UnityAction onClick )
		{
			GameObject container = __CreateUIElement( parent, "Button - Icon", basicData );

			Image imageObj = container.AddComponent<Image>();
			imageObj.sprite = icon;
			specificData.ApplyTo( imageObj );

			Button buttonObj = container.AddComponent<Button>();
			buttonObj.onClick.AddListener( onClick );


			return container;
		}

		public static GameObject CreateScrollableGrid( Transform parent, GenericUIData basicData, GridData specificData, GameObject[] gridContents )
		{
			const float HANDLE_WIDTH = 19.0f;
			// scrolling top-bottom

			GameObject container = __CreateUIElement( parent, "Scrollable Grid", basicData );

			RectMask2D mask = container.AddComponent<RectMask2D>();

			GameObject gridGameObject = __CreateUIElement( container.transform, "Grid", new GenericUIData( Vector2.zero, new Vector2( -HANDLE_WIDTH, 0 ), Vector2.up, Vector2.up, Vector2.one ) );

			GridLayoutGroup layoutGroup = gridGameObject.AddComponent<GridLayoutGroup>();
			layoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
			layoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
			specificData.ApplyTo( layoutGroup );

			ContentSizeFitter fitter = gridGameObject.AddComponent<ContentSizeFitter>();
			fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;



			GameObject scrollbarGameObject = __CreateUIElement( container.transform, "Scrollbar - Vert", new GenericUIData( Vector2.zero, new Vector2( HANDLE_WIDTH, 0 ), new Vector2( 1.0f, 0.5f ), Vector2.right, Vector2.one ) );

			GameObject slidingAreaGameObject = __CreateUIElement( scrollbarGameObject.transform, "Sliding Area", new GenericUIData( Vector2.zero, new Vector2( -8, -8 ), new Vector2( 0.5f, 0.5f ), Vector2.zero, Vector2.one ) );

			GameObject handleGameObject = __CreateUIElement( slidingAreaGameObject.transform, "Handle", new GenericUIData( Vector2.zero, new Vector2( 8, 8 ), new Vector2( 0.5f, 0.5f ), new Vector2( 0.0f, 0.5f ), new Vector2( 1.0f, 0.5f ) ) );

			Image scrollbarImg = slidingAreaGameObject.AddComponent<Image>();
			scrollbarImg.sprite = Main.uiScrollArea;
			scrollbarImg.color = Color.white;
			scrollbarImg.type = Image.Type.Sliced;

			Image handleImg = handleGameObject.AddComponent<Image>();
			handleImg.sprite = Main.uiScrollHandle;
			handleImg.color = Color.white;
			handleImg.type = Image.Type.Sliced;

			Scrollbar scrollbar = scrollbarGameObject.AddComponent<Scrollbar>();
			scrollbar.handleRect = handleGameObject.GetComponent<RectTransform>();
			scrollbar.direction = Scrollbar.Direction.BottomToTop;
			scrollbar.targetGraphic = scrollbarImg;

			ScrollRect scrollRect = gridGameObject.AddComponent<ScrollRect>();
			scrollRect.viewport = container.GetComponent<RectTransform>();
			scrollRect.content = gridGameObject.GetComponent<RectTransform>();
			scrollRect.verticalScrollbar = scrollbar;
			scrollRect.vertical = true;
			scrollRect.horizontal = false;
			scrollRect.scrollSensitivity = 15f;
			scrollRect.movementType = ScrollRect.MovementType.Clamped;


			if( gridContents == null )
			{
				return container;
			}
			for( int i = 0; i < gridContents.Length; i++ )
			{
				gridContents[i].transform.SetParent( gridGameObject.transform );
			}
			return container;
		}

		public static GameObject CreateScrollableList( Transform parent, GenericUIData basicData, GameObject[] listContents )
		{
			const float HANDLE_WIDTH = 30.0f;
			// scrolling top-bottom

			GameObject container = __CreateUIElement( parent, "Scrollable List", basicData );

			RectMask2D mask = container.AddComponent<RectMask2D>();

			GameObject listGameObject = __CreateUIElement( container.transform, "List", new GenericUIData( Vector2.zero, new Vector2( -HANDLE_WIDTH, 0 ), Vector2.up, Vector2.up, Vector2.one ) );

			VerticalLayoutGroup layoutGroup = listGameObject.AddComponent<VerticalLayoutGroup>();
			layoutGroup.childForceExpandWidth = false;//
			layoutGroup.childForceExpandHeight = false;
			layoutGroup.childControlWidth = false;
			layoutGroup.childControlHeight = false;//= GridLayoutGroup.Corner.UpperLeft;
			//layoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
			//specificData.ApplyTo( layoutGroup );

			ContentSizeFitter fitter = listGameObject.AddComponent<ContentSizeFitter>();
			fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;


			// Add the scrolling
			GameObject scrollbarGameObject = __CreateUIElement( container.transform, "Scrollbar - Vert", new GenericUIData( Vector2.zero, new Vector2( 30, 0 ), new Vector2( 1.0f, 0.5f ), Vector2.right, Vector2.one ) );

			GameObject slidingAreaGameObject = __CreateUIElement( scrollbarGameObject.transform, "Sliding Area", new GenericUIData( Vector2.zero, new Vector2( -20, -20 ), new Vector2( 0.5f, 0.5f ), Vector2.zero, Vector2.one ) );

			GameObject handleGameObject = __CreateUIElement( slidingAreaGameObject.transform, "Handle", new GenericUIData( Vector2.zero, new Vector2( 20, 20 ), new Vector2( 0.5f, 0.5f ), new Vector2( 0.0f, 0.5f ), new Vector2( 1.0f, 0.5f ) ) );

			Image scrollbarImg = slidingAreaGameObject.AddComponent<Image>();
			scrollbarImg.sprite = null;
			scrollbarImg.color = Color.gray;

			Image handleImg = handleGameObject.AddComponent<Image>();
			handleImg.sprite = null;
			handleImg.color = Color.white;

			Scrollbar scrollbar = scrollbarGameObject.AddComponent<Scrollbar>();
			scrollbar.handleRect = handleGameObject.GetComponent<RectTransform>();
			scrollbar.direction = Scrollbar.Direction.BottomToTop;

			ScrollRect scrollRect = listGameObject.AddComponent<ScrollRect>();
			scrollRect.viewport = container.GetComponent<RectTransform>();
			scrollRect.content = listGameObject.GetComponent<RectTransform>();
			scrollRect.verticalScrollbar = scrollbar;
			scrollRect.vertical = true;
			scrollRect.horizontal = false;
			scrollRect.scrollSensitivity = 15f;
			scrollRect.movementType = ScrollRect.MovementType.Clamped;


			if( listContents == null )
			{
				return container;
			}
			for( int i = 0; i < listContents.Length; i++ )
			{
				listContents[i].transform.SetParent( listGameObject.transform );
			}
			return container;
		}



		//### Text
		//### Icon
		//### Generic Button (resizable rectangle/square, generic texture, with text)
		//### Icon button (resizable rectangle/square, custom texture)

		//### Scrollable Grid (grid layout)

		// Scrollable list
	}
}