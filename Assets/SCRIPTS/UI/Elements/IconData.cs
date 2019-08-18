using UnityEngine;
using UnityEngine.UI;

namespace SS.UI.Elements
{
	public struct IconData
	{
		public Color tint;
		
		public IconData( Color tint )
		{
			this.tint = tint;
		}

		public void ApplyTo( Image obj )
		{
			obj.color = this.tint;
		}
	}
}