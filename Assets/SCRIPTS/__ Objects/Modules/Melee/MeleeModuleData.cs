using KFF;
using SS.Objects.Modules;
using System;

namespace SS.Levels.SaveStates
{
	public class MeleeModuleData : ModuleData
	{
		public Guid? targetGuid { get; set; }
		public float? damageOverride { get; set; }
		

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
					this.targetGuid = serializer.ReadGuid( "TargetGuid" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'TargetGuid' (" + serializer.file.fileName + ")." );
				}
			}

			if( serializer.Analyze( "DamageOverride" ).isSuccess )
			{
				try
				{
					this.damageOverride = serializer.ReadFloat( "DamageOverride" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'DamageOverride' (" + serializer.file.fileName + ")." );
				}
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			if( this.targetGuid != null )
			{
				serializer.WriteGuid( "", "TargetGuid", this.targetGuid.Value );
			}
			if( this.damageOverride != null )
			{
				serializer.WriteFloat( "", "DamageOverride", this.damageOverride.Value );
			}
		}
	}
}