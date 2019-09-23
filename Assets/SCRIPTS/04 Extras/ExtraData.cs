using KFF;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Contains every information to successfully round-trip an extra, to and from file.
	/// </summary>
	public class ExtraData : IKFFSerializable
	{
		public Vector3 position { get; set; }
		public Quaternion rotation { get; set; }

#warning incomplete - lacks modules (deposit).

		public void DeserializeKFF( KFFSerializer serializer )
		{
			throw new System.NotImplementedException();
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			throw new System.NotImplementedException();
		}
	}
}