using KFF;
using SS.Content;
using System;
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
			try
			{
				this.id = serializer.ReadString( "Id" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Id' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.displayName = serializer.ReadString( "DisplayName" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DisplayName' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.canGetStuck = serializer.ReadBool( "CanGetStuck" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'CanGetStuck' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.blastRadius = serializer.ReadFloat( "BlastRadius" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'BlastRadius' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.lifetime = serializer.ReadFloat( "Lifetime" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Lifetime' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.hitSoundEffect = serializer.ReadAudioClipFromAssets( "HitSound" );
			}
			catch( KFFException )
			{
				throw new Exception( "Missing 'HitSound' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.missSoundEffect = serializer.ReadAudioClipFromAssets( "MissSound" );
			}
			catch( KFFException )
			{
				throw new Exception( "Missing 'MissSound' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

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