using KFF;
using SS.Objects.Modules;

namespace SS.Levels.SaveStates
{
	public class ResourceCollectorWorkplaceModuleData : ModuleData
	{
		public AreaOfInfluence aoi { get; set; }


		public ResourceCollectorWorkplaceModuleData()
		{

		}



		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.aoi = new AreaOfInfluence();
			serializer.Deserialize( "AreaOfInfluence", this.aoi );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.Serialize( "", "AreaOfInfluence", this.aoi );
		}
	}
}