using KFF;
using SS.Content;
using UnityEngine;

namespace SS.Projectiles
{
	public class ProjectileDefinition : Definition, IKFFSerializable
	{
		public class TrailData
		{
			private float __amount = 5.0f;
			public float amount
			{
				get { return this.__amount; }
				set
				{
					if( value <= 0 )
					{
						throw new System.Exception( "'Trail.Amount' can't be less than or equal to 0." );
					}
					this.__amount = value;
				}
			}

			private float __startSize = 1.0f;
			public float startSize
			{
				get { return this.__startSize; }
				set
				{
					if( value < 0 )
					{
						throw new System.Exception( "'Trail.StartSize' can't be less than 0." );
					}
					this.__startSize = value;
				}
			}

			private float __endSize = 0.0f;
			public float endSize
			{
				get { return this.__endSize; }
				set
				{
					if( value < 0 )
					{
						throw new System.Exception( "'Trail.EndSize' can't be less than 0." );
					}
					this.__endSize = value;
				}
			}

			private float __lifetime = 1.0f;
			public float lifetime
			{
				get { return this.__lifetime; }
				set
				{
					if( value <= 0 )
					{
						throw new System.Exception( "'Trail.Lifetime' can't be less than or equal to 0." );
					}
					this.__lifetime = value;
				}
			}
			
			public AddressableAsset<Texture2D> texture { get; set; }
		}

		public TrailData trailData { get; set; }


		public AddressableAsset<Mesh> mesh { get; set; }
		public AddressableAsset<Texture2D> albedo { get; set; }
		public AddressableAsset<Texture2D> normal { get; set; }
		public AddressableAsset<AudioClip> hitSoundEffect { get; private set; }
		public AddressableAsset<AudioClip> missSoundEffect { get; private set; }

		public ProjectileDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			if( serializer.Analyze( "TrailData" ).isSuccess )
			{
				this.trailData = new TrailData();
				this.trailData.amount = serializer.ReadFloat( "TrailData.Amount" );
				this.trailData.lifetime = serializer.ReadFloat( "TrailData.Lifetime" );
				this.trailData.startSize = serializer.ReadFloat( "TrailData.StartSize" );
				this.trailData.endSize = serializer.ReadFloat( "TrailData.EndSize" );
				this.trailData.texture = serializer.ReadTexture2DFromAssets( "TrailData.Texture", TextureType.Color );
			}
			this.mesh = serializer.ReadMeshFromAssets( "Mesh" );
			this.albedo = serializer.ReadTexture2DFromAssets( "AlbedoTexture", TextureType.Color );
			this.normal = serializer.ReadTexture2DFromAssets( "NormalTexture", TextureType.Normal );
			this.hitSoundEffect = serializer.ReadAudioClipFromAssets( "HitSound" );
			this.missSoundEffect = serializer.ReadAudioClipFromAssets( "MissSound" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "Mesh", (string)this.mesh );
			serializer.WriteString( "", "AlbedoTexture", (string)this.albedo );
			serializer.WriteString( "", "NormalTexture", (string)this.normal );
			serializer.WriteString( "", "HitSound", (string)this.hitSoundEffect );
			serializer.WriteString( "", "MissSound", (string)this.missSoundEffect );

			if( this.trailData != null )
			{
				serializer.WriteClass( "", "TrailData" );
				serializer.WriteFloat( "TrailData", "Amount", this.trailData.amount );
				serializer.WriteFloat( "TrailData", "Lifetime", this.trailData.lifetime );
				serializer.WriteFloat( "TrailData", "StartSize", this.trailData.startSize );
				serializer.WriteFloat( "TrailData", "EndSize", this.trailData.endSize );
				serializer.WriteString( "TrailData", "Texture", (string)this.trailData.texture );
			}
		}
	}
}