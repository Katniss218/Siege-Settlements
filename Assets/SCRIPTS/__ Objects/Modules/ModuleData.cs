using KFF;
using SS.Levels.SaveStates;
using System;

namespace SS.Objects.Modules
{
	public abstract class ModuleData : IKFFSerializable
	{
		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );

		public static ModuleData TypeIdToData( string typeId )
		{
			if( typeId == MeleeModule.KFF_TYPEID )
			{
				return new MeleeModuleData();
			}
			if( typeId == RangedModule.KFF_TYPEID )
			{
				return new RangedModuleData();
			}
			if( typeId == BarracksModule.KFF_TYPEID )
			{
				return new BarracksModuleData();
			}
			if( typeId == ResearchModule.KFF_TYPEID )
			{
				return new ResearchModuleData();
			}
			if( typeId == InventoryModule.KFF_TYPEID )
			{
				return new InventoryModuleData();
			}
			if( typeId == ResourceDepositModule.KFF_TYPEID )
			{
				return new ResourceDepositModuleData();
			}
			if( typeId == ConstructorModule.KFF_TYPEID )
			{
				return new ConstructorModuleData();
			}
			if( typeId == InteriorModule.KFF_TYPEID )
			{
				return new InteriorModuleData();
			}
			if( typeId == TavernWorkplaceModule.KFF_TYPEID )
			{
				return new TavernWorkplaceModuleData();
			}
			throw new Exception( "Unknown module type '" + typeId + "' (Type->Data)." );
		}

		public static string DataToTypeId( ModuleData data )
		{
			if( data is MeleeModuleData )
			{
				return MeleeModule.KFF_TYPEID;
			}
			if( data is RangedModuleData )
			{
				return RangedModule.KFF_TYPEID;
			}
			if( data is BarracksModuleData )
			{
				return BarracksModule.KFF_TYPEID;
			}
			if( data is ResearchModuleData )
			{
				return ResearchModule.KFF_TYPEID;
			}
			if( data is InventoryModuleData )
			{
				return InventoryModule.KFF_TYPEID;
			}
			if( data is ResourceDepositModuleData )
			{
				return ResourceDepositModule.KFF_TYPEID;
			}
			if( data is ConstructorModuleData )
			{
				return ConstructorModule.KFF_TYPEID;
			}
			if( data is InteriorModuleData )
			{
				return InteriorModule.KFF_TYPEID;
			}
			if( data is TavernWorkplaceModuleData )
			{
				return TavernWorkplaceModule.KFF_TYPEID;
			}
			throw new Exception( "Inknown module type '" + data.GetType().Name + "' (Data->Type)." );
		}
	}
}