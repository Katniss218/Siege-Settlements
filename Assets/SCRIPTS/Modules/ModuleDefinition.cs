using KFF;
using UnityEngine;

namespace SS.Modules
{
	public abstract class ModuleDefinition : IKFFSerializable
	{
		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );

		/// <summary>
		/// Called to add and setup the module on a GameObject.
		/// </summary>
		/// <param name="obj">The GameObject to add the module to.</param>
		public abstract void AddTo( GameObject obj );
	}
}