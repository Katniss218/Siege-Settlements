using KFF;
using SS.AI.Goals;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	public class BuildingData : SSObjectData
	{
		public Guid guid { get; set; }

		public Vector3 position { get; set; }
		public Quaternion rotation { get; set; }

		private int __factionId = 0;
		public int factionId
		{
			get
			{
				return this.__factionId;
			}
			set
			{
				if( value < 0 )
				{
					throw new Exception( "Can't set faction to outside of acceptable values." );
				}
				this.__factionId = value;
			}
		}

		public float health { get; set; }
		
		public ConstructionSiteData constructionSaveState { get; set; }

		public TacticalGoalData[] tacticalGoalData { get; set; }
		public int tacticalGoalTag { get; set; }

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.guid = serializer.ReadGuid( "Guid" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Guid' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.position = serializer.ReadVector3( "Position" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Position' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.rotation = serializer.ReadQuaternion( "Rotation" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Rotation' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.factionId = serializer.ReadInt( "FactionId" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'FactionId' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.health = serializer.ReadFloat( "Health" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Health' (" + serializer.file.fileName + ")." );
			}

			if( serializer.Analyze( "ConstructionSaveState" ).isSuccess )
			{
				this.constructionSaveState = new ConstructionSiteData();
				serializer.Deserialize( "ConstructionSaveState", this.constructionSaveState );
			}

			int tag;
			this.tacticalGoalData = SSObjectData.DeserializeTacticalGoalKFF( serializer, out tag );
			this.tacticalGoalTag = tag;

			this.DeserializeModulesKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteGuid( "", "Guid", this.guid );

			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteQuaternion( "", "Rotation", this.rotation );

			serializer.WriteInt( "", "FactionId", this.factionId );
			serializer.WriteFloat( "", "Health", this.health );

			if( this.constructionSaveState != null )
			{
				serializer.Serialize( "", "ConstructionSaveState", this.constructionSaveState );
			}

			SSObjectData.SerializeTacticalGoalKFF( serializer, this.tacticalGoalData, this.tacticalGoalTag );

			this.SerializeModulesKFF( serializer );
		}
	}
}