using KFF;
using SS.Modules;
using SS.Units;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Used to round-trip modules, to and from file.
	/// </summary>
	public struct BarracksModuleSaveState : IKFFSerializable
	{
#warning incomplete.
		public BarracksModuleDefinition def { get; set; }

		public UnitDefinition trainedUnit { get; set; }
		public float trainProgress { get; set; }
		public Dictionary<string,int> resourcesRemaining { get; set; }
		
		public Vector3 rallyPoint { get; set; }


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