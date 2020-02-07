using SS.Objects.Buildings;
using SS.Objects.Extras;
using SS.Objects.Heroes;
using SS.Objects.Modules;
using SS.Objects.Projectiles;
using SS.Objects.SubObjects;
using SS.Objects.Units;
using SS.ResourceSystem.Payment;
using SS.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SS.Objects
{
	[DisallowMultipleComponent]
	public class SSObject : MonoBehaviour
	{
		private static List<SSObject> allSSObjects = new List<SSObject>();

		private static List<SSObjectDFSC> allSelectables = new List<SSObjectDFSC>();
		
		private static List<Unit> allUnits = new List<Unit>();
		private static List<Building> allBuildings = new List<Building>();
		private static List<Projectile> allProjectiles = new List<Projectile>();
		private static List<Hero> allHeroes = new List<Hero>();
		private static List<Extra> allExtras = new List<Extra>();
		
		public static SSObject[] GetAll()
		{
			return allSSObjects.ToArray();
		}
		
		public static SSObjectDFSC[] GetAllDFSC()
		{
			return allSelectables.ToArray();
		}
		
		public static Unit[] GetAllUnits()
		{
			return allUnits.ToArray();
		}
		
		public static Building[] GetAllBuildings()
		{
			return allBuildings.ToArray();
		}
		
		public static Projectile[] GetAllProjectiles()
		{
			return allProjectiles.ToArray();
		}
		
		public static Hero[] GetAllHeroes()
		{
			return allHeroes.ToArray();
		}
		
		public static Extra[] GetAllExtras()
		{
			return allExtras.ToArray();
		}


		/// <summary>
		/// Returns true if the object should display it's parameters on the Selection Panel. By default always true for non-faction objects.
		/// </summary>
		public virtual bool IsDisplaySafe()
		{
			return true;
		}


		private Guid? __guid = null;
		private string __definitionId = null;
		private string __displayName = "<missing>";

		/// <summary>
		/// Gets or sets the unique identifier (Guid) of the object (CAN'T be re-assigned after setting it once).
		/// </summary>
		public Guid guid
		{
			get
			{
				if( this.__guid == null )
				{
					throw new Exception( "Guid hasn't been assigned yet." );
				}
				return this.__guid.Value;
			}
			set
			{
				if( this.__guid != null )
				{
					throw new Exception( "Tried to re-assign guid to '" + gameObject.name + "'. A guid is already assigned." );
				}
				this.__guid = value;
			}
		}

		/// <summary>
		/// Gets or sets the definition ID of the object (CAN"T be re-assigned after setting it once).
		/// </summary>
		public string definitionId
		{
			get
			{
				return this.__definitionId;
			}
			set
			{
				if( this.__definitionId != null )
				{
					throw new Exception( "Tried to re-assign definition to '" + gameObject.name + "'. A definition is already assigned." );
				}
				this.__definitionId = value;
			}
		}

		/// <summary>
		/// Gets or sets the display name of the object.
		/// </summary>
		public virtual string displayName
		{
			get
			{
				return this.__displayName;
			}
			set
			{
				this.__displayName = value;
			}
		}

		internal HudContainer hudBase = null;



		private bool hasPaymentReceiverModule;
		public bool hasInventoryModule { get; private set; } // true if the inventory was ever added to this SSObject.

		private List<SSModule> modulesTemp = new List<SSModule>();
		private SSModule[] modules = null;
		private bool modulesSealed
		{
			get { return this.modules != null; }
		}

		

		private List<SubObject> subObjectsTemp = new List<SubObject>();
		private SubObject[] subObjects = null;
		private bool subObjectsSealed
		{
			get { return this.subObjects != null; }
		}

		
		//
		//
		//


		public bool HasPaymentReceivers()
		{
			if( this.hasPaymentReceiverModule )
			{
				return true;
			}
			if( this is ISSObjectUsableUnusable )
			{
				return true;
			}
			return false;
		}

		public IPaymentReceiver[] GetAvailableReceivers()
		{
			if( this is ISSObjectUsableUnusable )
			{
				ISSObjectUsableUnusable usableUnusable = (ISSObjectUsableUnusable)this;
				if( !usableUnusable.isUsable )
				{
					if( usableUnusable.paymentReceiver == null ) // if repair hasn't started yet - can't pay.
					{
						return new IPaymentReceiver[0];
					}

					return new IPaymentReceiver[] { usableUnusable.paymentReceiver };
				}
			}
			return this.GetComponents<IPaymentReceiver>();
		}


		//
		//
		//


		/// <summary>
		/// Gets a specific module attached to this SSObject.
		/// </summary>
		public SSModule GetModule( Guid moduleId )
		{
			if( !this.modulesSealed )
			{
				throw new Exception( "Modules haven't been sealed yet." );
			}
			for( int i = 0; i < this.modules.Length; i++ )
			{
				if( this.modules[i].moduleId == moduleId )
				{
					return this.modules[i];
				}
			}
			return null;
		}

		/// <summary>
		/// Gets all modules assigned to this SSobject.
		/// </summary>
		public SSModule[] GetModules()
		{
			if( !this.modulesSealed )
			{
				throw new Exception( "Modules haven't been sealed yet." );
			}

			return this.modules;
		}

		/// <summary>
		/// Gets a specific module of a specified type T, attached to this SSObject.
		/// </summary>
		public T GetModule<T>( Guid moduleId ) where T : SSModule
		{
			if( !this.modulesSealed )
			{
				throw new Exception( "Modules haven't been sealed yet." );
			}

			for( int i = 0; i < modules.Length; i++ )
			{
				if( this.modules[i].moduleId == moduleId )
				{
					if( this.modules[i] is T )
					{
						return (T)this.modules[i];
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Gets all modules of specified type T, assigned to this SSobject.
		/// </summary>
		public T[] GetModules<T>() where T : SSModule
		{
			if( !this.modulesSealed )
			{
				throw new Exception( "Modules haven't been sealed yet." );
			}

			List<T> ret = new List<T>();
			for( int i = 0; i < this.modules.Length; i++ )
			{
				if( this.modules[i] is T )
				{
					ret.Add( (T)this.modules[i] );
				}
			}
			return ret.ToArray();
		}

		internal T AddModule<T>( Guid moduleId ) where T : SSModule
		{
			for( int i = 0; i < this.modulesTemp.Count; i++ )
			{
				if( this.modulesTemp[i].moduleId == moduleId )
				{
					throw new Exception( "Can't add another module with the same module ID." );
				}
			}

			T module = this.gameObject.AddComponent<T>();
			module.moduleId = moduleId;

			this.modulesTemp.Add( module );
			return module;
		}

		/// <summary>
		/// Blocks the ability to add modules to this object. Calculates all of the caches for modules. Call this when you finish preparing the object.
		/// </summary>
		public void SealModules()
		{
			this.modules = new SSModule[this.modulesTemp.Count];
			for( int i = 0; i < this.modulesTemp.Count; i++ )
			{
				this.modules[i] = this.modulesTemp[i];

				if( !this.hasPaymentReceiverModule && (this.modulesTemp[i] is IPaymentReceiver) )
				{
					this.hasPaymentReceiverModule = true;
				}
				if( !this.hasInventoryModule && (this.modulesTemp[i] is InventoryModule) )
				{
					this.hasInventoryModule = true;
				}
			}

			this.modulesTemp = null; // Garbage Collect unused data
		}
		

		//
		//
		//


		internal Tuple<GameObject,T> AddSubObject<T>( Guid subObjectId ) where T : SubObject
		{
			if( this.subObjectsSealed )
			{
				throw new Exception( "Sub-Objects have been sealed already." );
			}

			for( int i = 0; i < this.subObjectsTemp.Count; i++ )
			{
				if( this.subObjectsTemp[i].subObjectId == subObjectId )
				{
					throw new Exception( "Can't add another sub-object with the same sub-object ID." );
				}
			}

			GameObject child = new GameObject( "Sub-Object [" + typeof( T ).Name + "] '" + subObjectId.ToString( "D" ) + "'" );
			child.transform.SetParent( this.transform );

			T subObject = child.AddComponent<T>();
			subObject.subObjectId = subObjectId;

			this.subObjectsTemp.Add( subObject );
			return new Tuple<GameObject, T>( child, subObject );
		}

		/// <summary>
		/// Gets a SubObject with specified Id. Returns null if none are present.
		/// </summary>
		public SubObject GetSubObject( Guid subObjectId )
		{
			if( !this.subObjectsSealed )
			{
				throw new Exception( "Sub-Objects haven't been sealed yet." );
			}

			for( int i = 0; i < this.subObjects.Length; i++ )
			{
				if( subObjects[i].subObjectId == subObjectId )
				{
					return subObjects[i];
				}
			}
			return null;
		}

		/// <summary>
		/// Gets all SubObjects assigned to this SSObject.
		/// </summary>
		public SubObject[] GetSubObjects()
		{
			if( !this.subObjectsSealed )
			{
				throw new Exception( "Sub-Objects haven't been sealed yet." );
			}
			return this.subObjects;
		}

		/// <summary>
		/// Gets all SubObjects assigned to this SSObject.
		/// </summary>
		public T[] GetSubObjects<T>() where T : SubObject
		{
			List<T> ret = new List<T>();
			for( int i = 0; i < this.transform.childCount; i++ )
			{
				T subObject = this.transform.GetChild( i ).GetComponent<T>();

				if( subObject == null )
				{
					continue;
				}

				ret.Add( subObject );
			}
			return ret.ToArray();
		}

		/// <summary>
		/// Blocks the ability to add modules to this object. Calculates all of the caches for modules. Call this when you finish preparing the object.
		/// </summary>
		public void SealSubObjects()
		{
			this.subObjects = this.subObjectsTemp.ToArray();
			this.subObjectsTemp = null; // Garbage Collect unused data
		}
		
		
		//
		//
		//


		protected virtual void OnEnable()
		{
			allSSObjects.Add( this );
			if( this is SSObjectDFSC )
			{
				allSelectables.Add( this as SSObjectDFSC );
			}

			if( this is Unit )
			{
				allUnits.Add( this as Unit );
				return;
			}
			if( this is Building )
			{
				allBuildings.Add( this as Building );
				return;
			}
			if( this is Projectile )
			{
				allProjectiles.Add( this as Projectile );
				return;
			}
			if( this is Hero )
			{
				allHeroes.Add( this as Hero );
				return;
			}
			if( this is Extra )
			{
				allExtras.Add( this as Extra );
				return;
			}
		}

		protected virtual void OnDisable()
		{
			allSSObjects.Remove( this );
			if( this is SSObjectDFSC )
			{
				allSelectables.Remove( this as SSObjectDFSC );
			}

			if( this is Unit )
			{
				allUnits.Remove( this as Unit );
				return;
			}
			if( this is Building )
			{
				allBuildings.Remove( this as Building );
				return;
			}
			if( this is Projectile )
			{
				allProjectiles.Remove( this as Projectile );
				return;
			}
			if( this is Hero )
			{
				allHeroes.Remove( this as Hero );
				return;
			}
			if( this is Extra )
			{
				allExtras.Remove( this as Extra );
				return;
			}
		}

		public static SSObject Find( Guid guid )
		{
			SSObject[] ssObjectArray = GetAll();

			for( int i = 0; i < ssObjectArray.Length; i++ )
			{
				if( ssObjectArray[i].guid == guid )
				{
					return ssObjectArray[i];
				}
			}
			return null;
		}



		//
		//
		//


		protected virtual void OnObjSpawn() { }

		protected virtual void OnObjDestroyed() { }

		protected virtual void Awake()
		{
			// Spawn HUDs depending on the base class.
			SSObjectCreator.AnalyzeAttributes( this, this );
		}

		protected virtual void Start()
		{
			this.OnObjSpawn();

			for( int i = 0; i < this.modules.Length; i++ )
			{
				this.modules[i].OnObjSpawn();
			}
		}


		public void Destroy()
		{
			SSModule[] modules = this.GetModules();
			for( int i = 0; i < modules.Length; i++ )
			{
				modules[i].OnObjDestroyed();
			}

			this.OnObjDestroyed();

			Object.Destroy( this.gameObject );
		}
	}
}