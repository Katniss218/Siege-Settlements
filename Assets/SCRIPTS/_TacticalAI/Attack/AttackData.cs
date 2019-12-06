using KFF;
using System;
using UnityEngine;

namespace SS
{
	public class AttackData : TAIGoalData
	{
		public Guid targetGuid { get; set; }

		public override void AssignTo( GameObject gameObject )
		{
			TAIGoal.Attack.AssignTAIGoal( gameObject, Main.GetSSObject( this.targetGuid ) );
		}


		public override void DeserializeKFF( KFFSerializer serializer )
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

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "TargetGuid", this.targetGuid.ToString( "D" ) );
		}
	}
}