using KFF;
using UnityEngine;

namespace SS
{
	public abstract class TAIGoalData : IKFFSerializable
	{
		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );

		public static TAIGoalData DeserializeUnknownType( KFFSerializer serializer )
		{
			TAIGoalType goalType = (TAIGoalType)serializer.ReadByte( "TAIGoalType" );

			if( goalType == TAIGoalType.None )
			{
				return null;
			}
			else
			{
				TAIGoalData data = null;

				switch( goalType )
				{
					case TAIGoalType.DropoffToInventory:
						data = new DropoffToInventoryData();
						break;

					case TAIGoalType.DropoffToNew:
						data = new DropoffToNewData();
						break;

					case TAIGoalType.MakePayment:
						data = new MakePaymentData();
						break;

					case TAIGoalType.MoveTo:
						data = new MoveToData();
						break;

					case TAIGoalType.PickupDeposit:
						data = new PickupDepositData();
						break;

					case TAIGoalType.Attack:
						data = new AttackData();
						break;
				}
				if( data == null )
				{
					throw new System.Exception( "Unknown TAIGoalType: " + goalType + "." );
				}
				serializer.Deserialize( "TAIGoalData", data );
				return data;
			}
		}

		public static void SerializeUnknownType( KFFSerializer serializer, TAIGoalData data )
		{
			if( data == null )
			{
				serializer.WriteByte( "", "TAIGoalType", (byte)TAIGoalType.None );
			}
			else
			{
				if( data is DropoffToInventoryData )
				{
					serializer.WriteByte( "", "TAIGoalType", (byte)TAIGoalType.DropoffToInventory );
				}
				else if( data is DropoffToNewData )
				{
					serializer.WriteByte( "", "TAIGoalType", (byte)TAIGoalType.DropoffToNew );
				}
				else if( data is MakePaymentData )
				{
					serializer.WriteByte( "", "TAIGoalType", (byte)TAIGoalType.MakePayment );
				}
				else if( data is MoveToData )
				{
					serializer.WriteByte( "", "TAIGoalType", (byte)TAIGoalType.MoveTo );
				}
				else if( data is PickupDepositData )
				{
					serializer.WriteByte( "", "TAIGoalType", (byte)TAIGoalType.PickupDeposit );
				}
				else if( data is AttackData )
				{
					serializer.WriteByte( "", "TAIGoalType", (byte)TAIGoalType.Attack );
				}
				serializer.Serialize( "", "TAIGoalData", data );
			}
		}

		public abstract void AssignTo( GameObject gameObject );
	}
}