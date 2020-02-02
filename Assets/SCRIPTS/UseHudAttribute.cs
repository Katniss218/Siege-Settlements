using SS.UI;
using System;

namespace SS
{
#warning cleanup comment.
	/*public interface IHUDHolder
	{
		HUD hud { get; set; }
	}
	*/
	public class UseHudAttribute : Attribute
	{
		public Type hudType { get; set; }
		public string fieldName { get; set; }

		public UseHudAttribute( Type type, string fieldName )
		{
			hudType = type;
			this.fieldName = fieldName;
		}
	}
}