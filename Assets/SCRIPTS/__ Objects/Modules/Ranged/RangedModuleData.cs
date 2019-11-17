using KFF;
using SS.Modules;
using System;

namespace SS.Levels.SaveStates
{
	public class RangedModuleData : ModuleData
	{
		public const string KFF_TYPEID = "ranged";

		public Guid? targetGuid { get; set; }
		

		public RangedModuleData()
		{
			this.targetGuid = null;
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			if( serializer.Analyze( "TargetGuid" ).isSuccess )
			{
				this.targetGuid = Guid.ParseExact( serializer.ReadString( "TargetGuid" ), "D" );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			if( this.targetGuid != null )
			{
				serializer.WriteString( "", "TargetGuid", this.targetGuid.Value.ToString( "D" ) );
			}
		}
	}
}