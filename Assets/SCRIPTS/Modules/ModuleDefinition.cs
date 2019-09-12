using KFF;
using UnityEngine;

namespace SS.Modules
{
	public abstract class ModuleDefinition : IKFFSerializable
	{
		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );
	}
}