using KFF;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	public class ProjectileData : SSObjectData
	{
		public Guid guid { get; set; }

		public Vector3 position { get; set; }

		public bool isStuck { get; set; }
		public Quaternion stuckRotation { get; set; }
		public Vector3 velocity { get; set; }
		
		public DamageType damageTypeOverride { get; set; }
		public float damageOverride { get; set; }
		public float armorPenetrationOverride { get; set; }
		
		public float? originY { get; set; }

		public int ownerFactionIdCache { get; set; }
		public Tuple<Guid,Guid> owner { get; set; }

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
				this.isStuck = serializer.ReadBool( "IsStuck" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'IsStuck' (" + serializer.file.fileName + ")." );
			}

			if( this.isStuck )
			{
				try
				{
					this.stuckRotation = serializer.ReadQuaternion( "StuckRotation" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'StuckRotation' (" + serializer.file.fileName + ")." );
				}
			}
			else
			{
				try
				{
					this.velocity = serializer.ReadVector3( "Velocity" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'Velocity' (" + serializer.file.fileName + ")." );
				}
			}

			try
			{
				this.damageTypeOverride = (DamageType)serializer.ReadByte( "DamageTypeOverride" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DamageTypeOverride' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.damageOverride = serializer.ReadFloat( "DamageOverride" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DamageOverride' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.armorPenetrationOverride = serializer.ReadFloat( "ArmorPenetrationOverride" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'ArmorPenetrationOverride' (" + serializer.file.fileName + ")." );
			}

			if( serializer.Analyze( "OriginY" ).isSuccess )
			{
				try
				{
					this.originY = serializer.ReadFloat( "OriginY" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'OriginY' (" + serializer.file.fileName + ")." );
				}
			}

			try
			{
				this.ownerFactionIdCache = serializer.ReadInt( "FactionId" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'FactionId' (" + serializer.file.fileName + ")." );
			}

			if( serializer.Analyze( "Owner" ).isSuccess )
			{
				Guid ownerGuid;
				Guid ownerModuleId;

				try
				{
					ownerGuid = serializer.ReadGuid( "Owner.Guid" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'ArmorPenetrationOverride' (" + serializer.file.fileName + ")." );
				}
				try
				{
					ownerModuleId = serializer.ReadGuid( "Owner.ModuleId" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'ArmorPenetrationOverride' (" + serializer.file.fileName + ")." );
				}

				this.owner = new Tuple<Guid, Guid>( ownerGuid, ownerModuleId );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteGuid( "", "Guid", this.guid );

			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteBool( "", "IsStuck", this.isStuck );
			if( this.isStuck )
			{
				serializer.WriteQuaternion( "", "StuckRotation", this.stuckRotation );
			}
			else
			{
				serializer.WriteVector3( "", "Velocity", this.velocity );
			}


			serializer.WriteByte( "", "DamageTypeOverride", (byte)this.damageTypeOverride );
			serializer.WriteFloat( "", "DamageOverride", this.damageOverride );
			serializer.WriteFloat( "", "ArmorPenetrationOverride", this.armorPenetrationOverride );

			if( this.originY != null )
			{
				serializer.WriteFloat( "", "OriginY", this.originY.Value );
			}

			serializer.WriteInt( "", "FactionId", this.ownerFactionIdCache );

			if( this.owner != null )
			{
				serializer.WriteClass( "", "Owner" );
				serializer.WriteGuid( "Owner", "Guid", this.owner.Item1 );
				serializer.WriteGuid( "Owner", "ModuleId", this.owner.Item2 );
			}
		}
	}
}