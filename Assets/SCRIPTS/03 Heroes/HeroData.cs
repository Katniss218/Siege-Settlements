using KFF;
using System;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Contains every information to successfully round-trip a hero, to and from file.
	/// </summary>
	public class HeroData : IKFFSerializable
	{
		public Guid guid { get; set; }

		public Vector3 position { get; set; }
		public Quaternion rotation { get; set; }

		public int factionId { get; set; }

		public float health { get; set; }

		public TAIGoalData taiGoalData { get; set; }


		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.guid = Guid.ParseExact( serializer.ReadString( "Guid" ), "D" );

			this.position = serializer.ReadVector3( "Position" );
			this.rotation = serializer.ReadQuaternion( "Rotation" );

			this.factionId = serializer.ReadInt( "FactionId" );
			this.health = serializer.ReadFloat( "Health" );

			TAIGoalType goalType = (TAIGoalType)serializer.ReadByte( "TAIGoalType" );
			if( goalType != TAIGoalType.None )
			{

				switch( goalType )
				{
					case TAIGoalType.DropoffToInventory:
						this.taiGoalData = new DropoffToInventoryData();
						serializer.Deserialize( "TAIGoalData", this.taiGoalData );
						break;

					case TAIGoalType.DropoffToNew:
						this.taiGoalData = new DropoffToNewData();
						serializer.Deserialize( "TAIGoalData", this.taiGoalData );
						break;

					case TAIGoalType.MakePayment:
						this.taiGoalData = new MakePaymentData();
						serializer.Deserialize( "TAIGoalData", this.taiGoalData );
						break;

					case TAIGoalType.MoveTo:
						this.taiGoalData = new MoveToData();
						serializer.Deserialize( "TAIGoalData", this.taiGoalData );
						break;

					case TAIGoalType.PickupDeposit:
						this.taiGoalData = new PickupDepositData();
						serializer.Deserialize( "TAIGoalData", this.taiGoalData );
						break;
				}
			}
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Guid", this.guid.ToString( "D" ) );

			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteQuaternion( "", "Rotation", this.rotation );

			serializer.WriteInt( "", "FactionId", this.factionId );
			serializer.WriteFloat( "", "Health", this.health );
			
			if( this.taiGoalData != null )
			{
				if( this.taiGoalData is DropoffToInventoryData )
				{
					serializer.WriteByte( "", "TAIGoalType", (byte)TAIGoalType.DropoffToInventory );
					serializer.Serialize( "", "TAIGoalData", this.taiGoalData );
				}
				else if( this.taiGoalData is DropoffToNewData )
				{
					serializer.WriteByte( "", "TAIGoalType", (byte)TAIGoalType.DropoffToNew );
					serializer.Serialize( "", "TAIGoalData", this.taiGoalData );
				}
				else if( this.taiGoalData is MakePaymentData )
				{
					serializer.WriteByte( "", "TAIGoalType", (byte)TAIGoalType.MakePayment );
					serializer.Serialize( "", "TAIGoalData", this.taiGoalData );
				}
				else if( this.taiGoalData is MoveToData )
				{
					serializer.WriteByte( "", "TAIGoalType", (byte)TAIGoalType.MoveTo );
					serializer.Serialize( "", "TAIGoalData", this.taiGoalData );
				}
				else if( this.taiGoalData is PickupDepositData )
				{
					serializer.WriteByte( "", "TAIGoalType", (byte)TAIGoalType.PickupDeposit );
					serializer.Serialize( "", "TAIGoalData", this.taiGoalData );
				}
			}
		}
	}
}