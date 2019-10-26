using KFF;
using UnityEngine;

namespace SS
{
	public abstract class TAIGoalData : IKFFSerializable
	{
		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );

		public abstract void AssignTo( GameObject gameObject );
	}
}