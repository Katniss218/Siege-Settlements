using UnityEngine;

namespace SS.Modules
{
	/// <summary>
	/// Represents any module, that can be added to an object to expand it's functionality. Modules encapsulate the functionality in a single MonoMehaviour.
	/// </summary>
	public abstract class Module : MonoBehaviour
	{
		public abstract ModuleData GetData();
		
		public abstract void SetDefinition( ModuleDefinition def );
		public abstract void SetData( ModuleData data );
	}
}