using KFF;
using SS.Levels.SaveStates;
using SS.Modules;
using System;
using System.Collections.Generic;

namespace SS.Content
{
	public abstract class ObjectData : IKFFSerializable
	{
		private struct ModuleCacheItem
		{
			public ModuleData module { get; set; }
			public Type moduleType { get; set; }
			public Guid moduleId { get; set; }

			public ModuleCacheItem( Guid moduleId, ModuleData module )
			{
				this.moduleId = moduleId;
				this.module = module;
				this.moduleType = module.GetType();
			}
		}

		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );


		private List<ModuleCacheItem> moduleCache;
		
		protected ObjectData()
		{
			this.moduleCache = new List<ModuleCacheItem>();
		}
		
		/// <summary>
		/// Gets a single module of type T (if found). Returns null if no module of specified type is present.
		/// </summary>
		public T GetModuleData<T>() where T : ModuleData
		{
			Type wantedType = typeof( T );

			for( int i = 0; i < this.moduleCache.Count; i++ )
			{
				if( this.moduleCache[i].moduleType == wantedType )
				{
					return this.moduleCache[i].module as T;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets every module of type T (if found). Returns empty array if no module of specified type is present.
		/// </summary>
		public T[] GetModuleDatas<T>() where T : ModuleData
		{
			Type wantedType = typeof( T );
			List<T> ret = new List<T>();

			for( int i = 0; i < this.moduleCache.Count; i++ )
			{
				if( this.moduleCache[i].moduleType == wantedType )
				{
					ret.Add( this.moduleCache[i].module as T );
				}
			}
			return ret.ToArray();
		}

		public void GetAllModules( out Guid[] moduleIds, out ModuleData[] datas )
		{
			moduleIds = new Guid[this.moduleCache.Count];
			datas = new ModuleData[this.moduleCache.Count];

			for( int i = 0; i < this.moduleCache.Count; i++ )
			{
				moduleIds[i] = this.moduleCache[i].moduleId;
				datas[i] = this.moduleCache[i].module;
			}
		}

		/// <summary>
		/// Adds a single module of type T to the object definition.
		/// </summary>
		public void AddModuleData<T>( Guid moduleId, T module ) where T : ModuleData
		{
			this.moduleCache.Add( new ModuleCacheItem( moduleId, module ) );
		}

		protected void DeserializeModulesKFF( KFFSerializer serializer )
		{
			for( int i = 0; i < serializer.Analyze( "Modules" ).childCount; i++ )
			{
				string moduleTypeString = serializer.ReadString( new Path( "Modules.{0}.TypeId", i ) );
				ModuleData module = null;


				if( moduleTypeString == MeleeModuleData.KFF_TYPEID )
				{
					module = new MeleeModuleData();
				}
				else if( moduleTypeString == RangedModuleData.KFF_TYPEID )
				{
					module = new RangedModuleData();
				}
				else if( moduleTypeString == BarracksModuleData.KFF_TYPEID )
				{
					module = new BarracksModuleData();
				}
				else if( moduleTypeString == ResearchModuleData.KFF_TYPEID )
				{
					module = new ResearchModuleData();
				}
				else if( moduleTypeString == InventoryConstrainedData.KFF_TYPEID )
				{
					module = new InventoryConstrainedData();
				}
				else if( moduleTypeString == InventoryUnconstrainedData.KFF_TYPEID )
				{
					module = new InventoryUnconstrainedData();
				}
				else if( moduleTypeString == ResourceDepositModuleData.KFF_TYPEID )
				{
					module = new ResourceDepositModuleData();
				}
				else if( moduleTypeString == ConstructorModuleData.KFF_TYPEID )
				{
					module = new ConstructorModuleData();
				}
				else
				{
					throw new Exception( "Unknown module type '" + moduleTypeString + "'." );
				}

				serializer.Deserialize<IKFFSerializable>( new Path( "Modules.{0}", i ), module );

				Guid guid = Guid.ParseExact( serializer.ReadString( new Path( "Modules.{0}.ModuleId", i ) ), "D" );

				this.AddModuleData( guid, module );
			}
		}

		protected void SerializeModulesKFF( KFFSerializer serializer )
		{
			ModuleData[] modulesArray;
			Guid[] moduleIdsArray;
			this.GetAllModules( out moduleIdsArray, out modulesArray );

			serializer.SerializeArray<IKFFSerializable>( "", "Modules", modulesArray );

			for( int i = 0; i < modulesArray.Length; i++ )
			{
				string moduleTypeString = null;

				if( modulesArray[i] is MeleeModuleData )
				{
					moduleTypeString = MeleeModuleData.KFF_TYPEID;
				}
				else if( modulesArray[i] is RangedModuleData )
				{
					moduleTypeString = RangedModuleData.KFF_TYPEID;
				}
				else if( modulesArray[i] is BarracksModuleData )
				{
					moduleTypeString = BarracksModuleData.KFF_TYPEID;
				}
				else if( modulesArray[i] is ResearchModuleData )
				{
					moduleTypeString = ResearchModuleData.KFF_TYPEID;
				}
				else if( modulesArray[i] is InventoryConstrainedData )
				{
					moduleTypeString = InventoryConstrainedData.KFF_TYPEID;
				}
				else if( modulesArray[i] is InventoryUnconstrainedData )
				{
					moduleTypeString = InventoryUnconstrainedData.KFF_TYPEID;
				}
				else if( modulesArray[i] is ResourceDepositModuleData )
				{
					moduleTypeString = ResourceDepositModuleData.KFF_TYPEID;
				}
				else if( modulesArray[i] is ConstructorModuleData )
				{
					moduleTypeString = ConstructorModuleData.KFF_TYPEID;
				}
				else
				{
					throw new Exception( "Inknown module type '" + modulesArray[i].GetType().Name + "'." );
				}

				serializer.WriteString( new Path( "Modules.{0}", i ), "TypeId", moduleTypeString );
				serializer.WriteString( new Path( "Modules.{0}", i ), "ModuleId", moduleIdsArray[i].ToString( "D" ) );
			}
		}
	}
}