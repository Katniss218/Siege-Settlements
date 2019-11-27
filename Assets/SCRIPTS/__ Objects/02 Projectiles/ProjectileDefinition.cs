using KFF;
using SS.Content;
using UnityEngine;

namespace SS.Objects.Projectiles
{
	public class ProjectileDefinition : ObjectDefinition, IKFFSerializable
	{
		public bool canGetStuck { get; set; }

		public string displayName { get; set; }

		public float blastRadius { get; set; } // set to 0 for no blast.

		public float lifetime { get; set; }

		public AddressableAsset<AudioClip> hitSoundEffect { get; private set; }
		public AddressableAsset<AudioClip> missSoundEffect { get; private set; }


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public ProjectileDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );

			this.displayName = serializer.ReadString( "DisplayName" );

			this.canGetStuck = serializer.ReadBool( "CanGetStuck" );

			this.blastRadius = serializer.ReadFloat( "BlastRadius" );

			this.lifetime = serializer.ReadFloat( "Lifetime" );

			this.hitSoundEffect = serializer.ReadAudioClipFromAssets( "HitSound" );
			this.missSoundEffect = serializer.ReadAudioClipFromAssets( "MissSound" );

			this.DeserializeModulesAndSubObjectsKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );

			serializer.WriteString( "", "DisplayName", this.displayName );

			serializer.WriteBool( "", "CanGetStuck", this.canGetStuck );

			serializer.WriteFloat( "", "BlastRadius", this.blastRadius );

			serializer.WriteFloat( "", "Lifetime", this.lifetime );

			serializer.WriteString( "", "HitSound", (string)this.hitSoundEffect );
			serializer.WriteString( "", "MissSound", (string)this.missSoundEffect );
			
			this.SerializeModulesAndSubObjectsKFF( serializer );
		}
	}
}