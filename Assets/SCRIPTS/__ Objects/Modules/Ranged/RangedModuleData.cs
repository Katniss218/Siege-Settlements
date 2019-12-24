using KFF;
using SS.Objects.Modules;
using System;

namespace SS.Levels.SaveStates
{
	public class RangedModuleData : ModuleData
	{
		public Guid? targetGuid { get; set; }
		public int? projectileCountOverride { get; set; }



		public RangedModuleData()
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

			if( serializer.Analyze( "ProjectileCountOverride" ).isSuccess )
			{
				try
				{
					this.projectileCountOverride = serializer.ReadInt( "ProjectileCountOverride" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'ProjectileCountOverride' (" + serializer.file.fileName + ")." );
				}
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			if( this.targetGuid != null )
			{
				serializer.WriteGuid( "", "TargetGuid", this.targetGuid.Value );
			}
			if( this.projectileCountOverride != null )
			{
				serializer.WriteInt( "", "ProjectileCountOverride", this.projectileCountOverride.Value );
			}
		}
	}
}