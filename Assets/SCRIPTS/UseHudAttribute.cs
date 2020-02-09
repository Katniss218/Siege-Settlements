using System;

namespace SS
{
	/// <summary>
	/// Add this attribute to an SSObject or an SSModule to make it automatically spawn & assign a HUD object.
	/// </summary>
	public class UseHudAttribute : Attribute
	{
		/// <summary>
		/// The type of the HUD class.
		/// </summary>
		public Type hudType { get; set; }

		/// <summary>
		/// The name of the field to assign the spawned HUD to. Must be a field (get/set).
		/// </summary>
		public string fieldName { get; set; }



		public UseHudAttribute( Type type, string fieldName )
		{
			hudType = type;
			this.fieldName = fieldName;
		}
	}
}