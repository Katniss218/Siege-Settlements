
using KFF;

namespace SS.DataStructures
{
	/// <summary>
	/// An abstract class that holds a piece of data, of some object.
	/// </summary>
	public abstract class Data : IKFFSerializable
	{
		protected Data() { }

		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );
	}
}