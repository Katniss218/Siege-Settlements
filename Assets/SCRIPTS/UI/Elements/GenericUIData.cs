using UnityEngine;

namespace SS.UI.Elements
{
	public struct GenericUIData
	{
		public Vector2 position;
		public Vector2 size;
		public Vector2 pivot;
		public Vector2 anchorMin;
		public Vector2 anchorMax;

		public GenericUIData( Vector2 position, Vector2 size, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax )
		{
			this.position = position;
			this.size = size;
			this.pivot = pivot;
			this.anchorMin = anchorMin;
			this.anchorMax = anchorMax;
		}
	}
}