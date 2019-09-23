using KFF;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Contains every information to successfully round-trip a building, to and from file.
	/// </summary>
	public class BuildingData : IKFFSerializable
	{
		public Vector3 position { get; set; }
		public Quaternion rotation { get; set; }

		public int factionId { get; set; }

		public float health { get; set; }
		
		public ConstructionSiteData constructionSaveState { get; set; }

		public BarracksModuleSaveState barracksSaveState { get; set; }
		public ResearchModuleSaveState researchSaveState { get; set; }

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