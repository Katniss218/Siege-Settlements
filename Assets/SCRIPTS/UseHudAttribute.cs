using System;

namespace SS
{
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