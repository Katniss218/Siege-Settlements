using UnityEngine;

namespace SS.UI
{
	public struct GenericUIData
	{
		public Vector2 position { get; set; }
		public Vector2 size { get; set; }
		public Vector2 pivot { get; set; }
		public Vector2 anchorMin { get; set; }
		public Vector2 anchorMax { get; set; }
		
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