using KFF;
using SS.Content;
using UnityEngine;

namespace SS.Projectiles
{
	public class ProjectileDefinition : ObjectDefinition, IKFFSerializable
	{
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

			this.hitSoundEffect = serializer.ReadAudioClipFromAssets( "HitSound" );
			this.missSoundEffect = serializer.ReadAudioClipFromAssets( "MissSound" );

			this.DeserializeModulesKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );

			serializer.WriteString( "", "HitSound", (string)this.hitSoundEffect );
			serializer.WriteString( "", "MissSound", (string)this.missSoundEffect );
			
			this.SerializeModulesKFF( serializer );
		}
	}
}