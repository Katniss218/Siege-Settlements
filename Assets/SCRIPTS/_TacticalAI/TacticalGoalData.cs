using KFF;
using System;

namespace SS.AI.Goals
{
	public abstract class TacticalGoalData : IKFFSerializable
	{
		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );

		public abstract TacticalGoal GetGoal();

		public static TacticalGoal[] GetGoalsArray( TacticalGoalData[] goalData )
		{
			TacticalGoal[] ret = new TacticalGoal[goalData.Length];

			for( int i = 0; i < goalData.Length; i++ )
			{
				ret[i] = goalData[i].GetGoal();
			}

			return ret;
		}

		public static TacticalGoalData[] GetGoalDataArray( TacticalGoal[] goals )
		{
			TacticalGoalData[] ret = new TacticalGoalData[goals.Length];

			for( int i = 0; i < goals.Length; i++ )
			{
				ret[i] = goals[i].GetData();
			}

			return ret;
		}

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
			if( typeId == TacticalMakeFormationGoal.KFF_TYPEID )
			{
				return new TacticalMakeFormationGoalData();
			}

			throw new Exception( "Unknown Tactical Goal type '" + typeId + "'." );
		}

		public static string InstanceToTypeId( TacticalGoalData data )
		{
			if( data.GetType() == typeof( TacticalIdleGoalData ) )
			{
				return TacticalIdleGoal.KFF_TYPEID;
			}
			if( data.GetType() == typeof( TacticalMoveToGoalData ) )
			{
				return TacticalMoveToGoal.KFF_TYPEID;
			}
			if( data.GetType() == typeof( TacticalDropOffGoalData ) )
			{
				return TacticalDropOffGoal.KFF_TYPEID;
			}
			if( data.GetType() == typeof( TacticalPickUpGoalData ) )
			{
				return TacticalPickUpGoal.KFF_TYPEID;
			}
			if( data.GetType() == typeof( TacticalTargetGoalData ) )
			{
				return TacticalTargetGoal.KFF_TYPEID;
			}
			if( data.GetType() == typeof( TacticalMakeFormationGoalData ) )
			{
				return TacticalMakeFormationGoal.KFF_TYPEID;
			}

			throw new Exception( "Inknown Tactical Goal type '" + data.GetType().Name + "'." );
		}
	}
}