using KFF;
using UnityEngine;

namespace SS.Modules
{
	public class ResearchModuleDefinition : ModuleDefinition
	{
		public float researchSpeed { get; set; }



		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.researchSpeed = serializer.ReadFloat( "ResearchSpeed" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteFloat( "", "ResearchSpeed", this.researchSpeed );
		}
	}
}