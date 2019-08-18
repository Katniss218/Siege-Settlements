using UnityEngine;
using UnityEngine.UI;

namespace SS.UI.Elements
{
	public struct GridData
	{
		public float cellSize;

		public GridData( float cellSize )
		{
			this.cellSize = cellSize;
		}

		public void ApplyTo( GridLayoutGroup obj )
		{
			obj.cellSize = new Vector2( this.cellSize, this.cellSize );
		}
	}
}