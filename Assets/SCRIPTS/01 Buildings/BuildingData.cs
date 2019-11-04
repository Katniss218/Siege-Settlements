using KFF;
using SS.Content;
using SS.Modules;
using System;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Contains every information to successfully round-trip a building, to and from file.
	/// </summary>
	public class BuildingData : ObjectData
	{
		public Guid guid { get; set; }

		public Vector3 position { get; set; }
		public Quaternion rotation { get; set; }

		public int factionId { get; set; }

		public float health { get; set; }
		
		public ConstructionSiteData constructionSaveState { get; set; }

		//public ModuleData barracksSaveState { get; set; }
		//public ModuleData researchSaveState { get; set; }
		

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.guid = Guid.ParseExact( serializer.ReadString( "Guid" ), "D" );

			this.position = serializer.ReadVector3( "Position" );
			this.rotation = serializer.ReadQuaternion( "Rotation" );

			this.factionId = serializer.ReadInt( "FactionId" );
			this.health = serializer.ReadFloat( "Health" );

			if( serializer.Analyze( "ConstructionSaveState" ).isSuccess )
			{
				this.constructionSaveState = new ConstructionSiteData();
				serializer.Deserialize( "ConstructionSaveState", this.constructionSaveState );
			}
			
			this.DeserializeModulesKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Guid", this.guid.ToString( "D" ) );

			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteQuaternion( "", "Rotation", this.rotation );

			serializer.WriteInt( "", "FactionId", this.factionId );
			serializer.WriteFloat( "", "Health", this.health );

			if( this.constructionSaveState != null )
			{
				serializer.Serialize( "", "ConstructionSaveState", this.constructionSaveState );
			}

			this.SerializeModulesKFF( serializer );
		}
	}
}