using KFF;
using SS.Content;
using SS.Levels.SaveStates;
using SS.Objects.Extras;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class ResourceDepositModuleDefinition : ModuleDefinition
	{
		public struct Slot : IKFFSerializable
		{
			public string resourceId { get; set; }
			public int capacity { get; set; }


			public void DeserializeKFF( KFFSerializer serializer )
			{
				this.resourceId = serializer.ReadString( "ResourceId" );
				this.capacity = serializer.ReadInt( "Capacity" );
			}

			public void SerializeKFF( KFFSerializer serializer )
			{
				serializer.WriteString( "", "ResourceId", this.resourceId );
				serializer.WriteInt( "", "Capacity", this.capacity );
			}
		}


		public Slot[] slots { get; set; }

		public AddressableAsset<AudioClip> mineSound { get; private set; }


		public override bool CheckTypeDefConstraints( Type objType )
		{
			return
				objType == typeof( ExtraDefinition );
		}

		public override bool CheckModuleDefConstraints( List<Type> modTypes )
		{
			return !(
				modTypes.Contains( typeof( InventoryModuleDefinition ) ) ||
				modTypes.Contains( typeof( ResourceDepositModuleDefinition ) ));
		}

		public override void AddModule( GameObject gameObject, Guid moduleId )
		{
			ResourceDepositModule module = gameObject.AddComponent<ResourceDepositModule>();
			module.moduleId = moduleId;
			module.icon = this.icon;

#warning some sort of method for setting the slots(?)
			module.resources = new ResourceDepositModule.SlotGroup[this.slots.Length];
			for( int i = 0; i < module.slotCount; i++ )
			{
				for( int j = 0; j < this.slots.Length; j++ )
				{
					if( module.resources[j].id == this.slots[i].resourceId )
					{
						throw new Exception( "Can't have multiple slots with the same resource id." ); // because that doesn't make sense, just use bigger slot.
					}
				}

				module.resources[i] = new ResourceDepositModule.SlotGroup( this.slots[i].resourceId, 0, this.slots[i].capacity );
			}

			module.miningSound = this.mineSound;
		}


		public override ModuleData GetIdentityData()
		{
			return new ResourceDepositModuleData();
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "Resources" );
			if( analysisData.isSuccess )
			{
				this.slots = new Slot[analysisData.childCount];
				try
				{
					serializer.DeserializeArray( "Resources", this.slots );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'Resources' (" + serializer.file.fileName + ")." );
				}
			}
			else
			{
				throw new Exception( "Missing 'Resources' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.mineSound = serializer.ReadAudioClipFromAssets( "MineSound" );
			}
			catch( KFFException )
			{
				throw new Exception( "Missing 'MineSound' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.icon = serializer.ReadSpriteFromAssets( "Icon" );
			}
			catch( KFFException )
			{
				throw new Exception( "Missing 'Icon' (" + serializer.file.fileName + ")." );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			// Cost
			serializer.SerializeArray( "", "Resources", this.slots );

			serializer.WriteString( "", "MineSound", (string)this.mineSound );
			serializer.WriteString( "", "Icon", (string)this.icon );
		}
	}
}