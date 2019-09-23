using KFF;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Contains every information to successfully round-trip a projectile, to and from file.
	/// </summary>
	public class ProjectileData : IKFFSerializable
	{
		public Vector3 position { get; set; }
		public Vector3 velocity { get; set; }

		public int factionId { get; set; }

		public DamageType damageTypeOverride { get; set; }
		public float damageOverride { get; set; }
		public float armorPenetrationOverride { get; set; }
		

#warning incomplete - lacks fields.
#warning incomplete - lacks modules.

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