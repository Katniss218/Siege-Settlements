using KFF;
using System;

namespace SS.AI.Goals
{
	public abstract class TacticalGoalData : IKFFSerializable
	{
		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );

		public abstract TacticalGoal GetInstance();

		public static TacticalGoalData TypeIdToInstance( string typeId )
		{
			if( typeId == TacticalIdleGoal.KFF_TYPEID )
			{
				return new TacticalIdleGoalData();
			}
			if( typeId == TacticalMoveToGoal.KFF_TYPEID )
			{
				return new TacticalMoveToGoalData();
			}
			if( typeId == TacticalDropOffGoal.KFF_TYPEID )
			{
				return new TacticalDropOffGoalData();
			}
			if( typeId == TacticalPickUpGoal.KFF_TYPEID )
			{
				return new TacticalPickUpGoalData();
			}
			if( typeId == TacticalTargetGoal.KFF_TYPEID )
			{
				return new TacticalTargetGoalData();
			}

			throw new Exception( "Unknown Tactical Goal type '" + typeId + "'." );
		}

		public static string InstanceToTypeId( TacticalGoalData data )
		{
			if( data is TacticalIdleGoalData )
			{
				return TacticalIdleGoal.KFF_TYPEID;
			}
			if( data is TacticalMoveToGoalData )
			{
				return TacticalMoveToGoal.KFF_TYPEID;
			}
			if( data is TacticalDropOffGoalData )
			{
				return TacticalDropOffGoal.KFF_TYPEID;
			}
			if( data is TacticalPickUpGoalData )
			{
				return TacticalPickUpGoal.KFF_TYPEID;
			}
			if( data is TacticalTargetGoalData )
			{
				return TacticalTargetGoal.KFF_TYPEID;
			}

			throw new Exception( "Inknown Tactical Goal type '" + data.GetType().Name + "'." );
		}
	}
}