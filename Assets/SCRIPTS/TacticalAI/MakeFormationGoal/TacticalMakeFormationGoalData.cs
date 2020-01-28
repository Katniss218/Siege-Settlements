using KFF;
using System;
using UnityEngine;

namespace SS.AI.Goals
{
	public class TacticalMakeFormationGoalData : TacticalGoalData
	{
		public Guid? beaconGuid { get; set; }

		public bool isHostile { get; set; }


		public override TacticalGoal GetGoal()
		{
			TacticalMoveToGoal goal = new TacticalMoveToGoal();
			goal.SetData( this );
			return goal;
		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.beaconGuid = serializer.ReadGuid( "BeaconGuid" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'BeaconGuid' (" + serializer.file.fileName + ")." );
			}


			try
			{
				this.isHostile = serializer.ReadBool( "IsHostile" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'HostileMode' (" + serializer.file.fileName + ")." );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteGuid( "", "BeaconGuid", this.beaconGuid.Value );
			
			serializer.WriteBool( "", "IsHostile", this.isHostile );
		}
	}
}