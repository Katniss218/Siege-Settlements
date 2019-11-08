using KFF;
using SS.Modules;
using SS.Modules.Inventories;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Content
{
	public abstract class ObjectDefinition : AddressableDefinition
	{
		private struct ModuleCacheItem
		{
			public ModuleDefinition module { get; set; }
			public Type moduleType { get; set; }
			public Guid moduleId { get; set; }

			public ModuleCacheItem( Guid moduleId, ModuleDefinition module )
			{
				this.moduleId = moduleId;
				this.module = module;
				this.moduleType = module.GetType();
			}
		}
		
		private List<ModuleCacheItem> moduleCache;
		

		protected ObjectDefinition( string id ) : base( id )
		{
			this.moduleCache = new List<ModuleCacheItem>();
		}


		// Checks if a module can be added to this obj, given the obj type and the modules added before this one.
		private bool CanAddModuleType( ModuleDefinition mod )
		{
			if( !mod.CheckTypeDefConstraints( this.GetType() ) )
			{
				return false;
			}
			List<Type> moduleTypes = new List<Type>();
			for( int i = 0; i < this.moduleCache.Count; i++ )
			{
				moduleTypes.Add( this.moduleCache[i].moduleType );
			}
			if( !mod.CheckModuleDefConstraints( moduleTypes ) )
			{
				return false;
			}
			return true;
		}


		/// <summary>
		/// Gets a single module of type T (if found). Returns null if no module of specified type is present.
		/// </summary>
		public T GetModule<T>() where T : ModuleDefinition
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
		public T[] GetModules<T>() where T : ModuleDefinition
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

		public void GetAllModules( out Guid[] moduleIds, out ModuleDefinition[] defs )
		{
			moduleIds = new Guid[this.moduleCache.Count];
			defs = new ModuleDefinition[this.moduleCache.Count];
			
			for( int i = 0; i < this.moduleCache.Count; i++ )
			{
				moduleIds[i] = this.moduleCache[i].moduleId;
				defs[i] = this.moduleCache[i].module;
			}
		}

		/// <summary>
		/// Adds a single module of type T to the object definition.
		/// </summary>
		public void AddModule<T>( Guid moduleId, T module ) where T : ModuleDefinition
		{
			if( !this.CanAddModuleType( module ) )
			{
				throw new Exception( "This module type can't be added to this object." );
			}

			this.moduleCache.Add( new ModuleCacheItem( moduleId, module ) );
		}
		
		protected void DeserializeModulesKFF( KFFSerializer serializer )
		{
			for( int i = 0; i < serializer.Analyze( "Modules" ).childCount; i++ )
			{
				string moduleTypeString = serializer.ReadString( new Path( "Modules.{0}.TypeId" , i) );
				ModuleDefinition module = null;
				
				if( moduleTypeString == MeleeModuleDefinition.KFF_TYPEID )
				{
					module = new MeleeModuleDefinition();
				}
				else if( moduleTypeString == RangedModuleDefinition.KFF_TYPEID )
				{
					module = new RangedModuleDefinition();
				}
				else if( moduleTypeString == BarracksModuleDefinition.KFF_TYPEID )
				{
					module = new BarracksModuleDefinition();
				}
				else if( moduleTypeString == ResearchModuleDefinition.KFF_TYPEID )
				{
					module = new ResearchModuleDefinition();
				}
				else if( moduleTypeString == InventoryConstrainedDefinition.KFF_TYPEID )
				{
					module = new InventoryConstrainedDefinition();
				}
				else if( moduleTypeString == InventoryUnconstrainedDefinition.KFF_TYPEID )
				{
					module = new InventoryUnconstrainedDefinition();
				}
				else if( moduleTypeString == ResourceDepositModuleDefinition.KFF_TYPEID )
				{
					module = new ResourceDepositModuleDefinition();
				}
				else
				{
					throw new Exception( "Unknown module type '" + moduleTypeString + "'." );
				}

				serializer.Deserialize<IKFFSerializable>( new Path( "Modules.{0}", i ), module );


				Guid guid = Guid.ParseExact( serializer.ReadString( new Path( "Modules.{0}.ModuleId", i ) ), "D" );

				this.AddModule( guid, module );
			}
		}

		protected void SerializeModulesKFF( KFFSerializer serializer )
		{
			ModuleDefinition[] modulesArray;
			Guid[] moduleIdsArray;
			this.GetAllModules( out moduleIdsArray, out modulesArray );

			serializer.SerializeArray<IKFFSerializable>( "", "Modules", modulesArray );

			for( int i = 0; i < modulesArray.Length; i++ )
			{
				string moduleTypeString = null;

				if( modulesArray[i] is MeleeModuleDefinition )
				{
					moduleTypeString = MeleeModuleDefinition.KFF_TYPEID;
				}
				else if( modulesArray[i] is RangedModuleDefinition )
				{
					moduleTypeString = RangedModuleDefinition.KFF_TYPEID;
				}
				else if( modulesArray[i] is BarracksModuleDefinition )
				{
					moduleTypeString = BarracksModuleDefinition.KFF_TYPEID;
				}
				else if( modulesArray[i] is ResearchModuleDefinition )
				{
					moduleTypeString = ResearchModuleDefinition.KFF_TYPEID;
				}
				else if( modulesArray[i] is InventoryConstrainedDefinition )
				{
					moduleTypeString = InventoryConstrainedDefinition.KFF_TYPEID;
				}
				else if( modulesArray[i] is InventoryUnconstrainedDefinition )
				{
					moduleTypeString = InventoryUnconstrainedDefinition.KFF_TYPEID;
				}
				else if( modulesArray[i] is ResourceDepositModuleDefinition )
				{
					moduleTypeString = ResourceDepositModuleDefinition.KFF_TYPEID;
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