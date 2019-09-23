using KFF;
using SS.Modules;
using SS.Technologies;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Used to round-trip modules, to and from file.
	/// </summary>
	public struct ResearchModuleSaveState : IKFFSerializable
	{
#warning incomplete.


		public ResearchModuleDefinition def { get; set; }

		public TechnologyDefinition researchedTechnology { get; set; }
		public float researchProgress { get; set; }
		public Dictionary<string, int> resourcesRemaining { get; set; }

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