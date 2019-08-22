using UnityEngine;

namespace SS.UI
{
	public struct GenericUIData
	{
		public Vector2 position;
		public Vector2 size;
		public Vector2 pivot;
		public Vector2 anchorMin;
		public Vector2 anchorMax;

		public GenericUIData( Vector2 position, Vector2 size )
		{
			this.position = position;
			this.size = size;
			this.pivot = Vector2.zero;
			this.anchorMin = Vector2.zero;
			this.anchorMax = Vector2.zero;
		}

		public GenericUIData( Vector2 position, Vector2 size, Vector2 pivot, Vector2 anchor )
		{
			this.position = position;
			this.size = size;
			this.pivot = pivot;
			this.anchorMin = anchor;
			this.anchorMax = anchor;
		}

		public GenericUIData( Vector2 position, Vector2 size, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax )
		{
			this.position = position;
			this.size = size;
			this.pivot = pivot;
			this.anchorMin = anchorMin;
			this.anchorMax = anchorMax;
		}

	}

	public static class RectTransformExtensions
	{
		/// <summary>
		/// Use this to apply positional data to UI elements.
		/// </summary>
		public static void ApplyUIData( this RectTransform transform, GenericUIData data )
		{
			transform.anchorMin = data.anchorMin;
			transform.anchorMax = data.anchorMax;
			transform.pivot = data.pivot;
			transform.anchoredPosition = data.position;
			transform.sizeDelta = data.size;
		}
	}
}