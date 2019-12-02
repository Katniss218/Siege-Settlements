using KFF;
using SS.Objects.Modules;
using System;

namespace SS.Levels.SaveStates
{
	public class MeleeModuleData : ModuleData
	{
		public Guid? targetGuid { get; set; }
		

		public MeleeModuleData()
		{
			this.targetGuid = null;
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			if( serializer.Analyze( "TargetGuid" ).isSuccess )
			{
				try
				{
					this.targetGuid = Guid.ParseExact( serializer.ReadString( "TargetGuid" ), "D" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'TargetGuid' (" + serializer.file.fileName + ")." );
				}
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