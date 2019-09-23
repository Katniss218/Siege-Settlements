using KFF;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Contains every information to (with a definition) successfully round-trip a unit, to and from file.
	/// </summary>
	public class UnitData : IKFFSerializable
	{
		public Vector3 position { get; set; }
		public Quaternion rotation { get; set; }

		public int factionId { get; set; }
		
		public float health { get; set; }

		
#warning incomplete - lacks modules.
		// when a module save state is present on the save state, and it's not present on the definition - throw an exception.
		// not every module needs save state, but every save state needs module.

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