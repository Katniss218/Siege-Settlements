using KFF;
using SS.Modules;
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

		public ModuleData meleeData { get; set; }
		public ModuleData rangedData { get; set; }

		public TAIGoalData taiGoalData { get; set; }


		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.guid = Guid.ParseExact( serializer.ReadString( "Guid" ), "D" );

			this.position = serializer.ReadVector3( "Position" );
			this.rotation = serializer.ReadQuaternion( "Rotation" );

			this.factionId = serializer.ReadInt( "FactionId" );
			this.health = serializer.ReadFloat( "Health" );
			
			if( serializer.Analyze( "MeleeModuleData").isSuccess )
			{
				this.meleeData = new MeleeModuleData();
				serializer.Deserialize( "MeleeModuleData", this.meleeData );
			}

			if( serializer.Analyze( "RangedModuleData" ).isSuccess )
			{
				this.rangedData = new RangedModuleData();
				serializer.Deserialize( "RangedModuleData", this.rangedData );
			}
			
			this.taiGoalData = TAIGoalData.DeserializeUnknownType( serializer );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Guid", this.guid.ToString( "D" ) );

			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteQuaternion( "", "Rotation", this.rotation );

			serializer.WriteInt( "", "FactionId", this.factionId );
			serializer.WriteFloat( "", "Health", this.health );

			if( this.meleeData != null )
			{
				serializer.Serialize( "", "MeleeModuleData", this.meleeData );
			}

			if( this.rangedData != null )
			{
				serializer.Serialize( "", "RangedModuleData", this.rangedData );
			}

			TAIGoalData.SerializeUnknownType( serializer, this.taiGoalData );
		}
	}
}