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
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SS.Objects
{
    /// <summary>
    /// The most basal class for ALL the types of objects that exist in the game world.
    /// </summary>
    /// <remarks>
    /// If it exists on the map, it's an SSObject.
    /// </remarks>
    [DisallowMultipleComponent]
    public class SSObject : MonoBehaviour
    {
        private static List<SSObject> allSSObjects = new List<SSObject>();

        private static List<SSObjectDFC> allSelectables = new List<SSObjectDFC>();

        private static List<Unit> allUnits = new List<Unit>();
        private static List<Building> allBuildings = new List<Building>();
        private static List<Projectile> allProjectiles = new List<Projectile>();
        private static List<Hero> allHeroes = new List<Hero>();
        private static List<Extra> allExtras = new List<Extra>();

        public static SSObject[] GetAll()
        {
            return allSSObjects.ToArray();
        }

        public static SSObjectDFC[] GetAllDFC()
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



        //
        //
        //



        public bool isSelectable { get; set; }

        /// <summary>
        /// The icon that is shown on the list of all selected objects.
        /// </summary>
        public Sprite icon { get; set; }

        public UnityEvent onSelect { get; private set; } = new UnityEvent();

        public UnityEvent onHighlight { get; private set; } = new UnityEvent();

        public UnityEvent onDeselect { get; private set; } = new UnityEvent();

        /// <summary>
        /// Returns true if the object should display its parameters on the Selection Panel. By default always true for non-faction objects.
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
                    throw new InvalidOperationException( "Guid hasn't been assigned yet." );
                }
                return this.__guid.Value;
            }
            set
            {
                if( this.__guid != null )
                {
                    throw new InvalidOperationException( $"Tried to re-assign guid to '{gameObject.name}'. A guid is already assigned." );
                }
                this.__guid = value;
            }
        }

        /// <summary>
        /// Gets or sets the definition ID of the object (CAN'T be re-assigned after setting it once).
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
                    throw new InvalidOperationException( $"Tried to re-assign definition to '{gameObject.name}'. A definition is already assigned." );
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

        /// <summary>
        /// The HUD assigned to this specific object.
        /// </summary>
        internal HudContainer hudBase = null;


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

        // Module caches.
        public bool hasInventoryModule { get; private set; }
        public bool hasPaymentReceiverModule { get; private set; }


        //
        //
        //				PAYMENTS



        public bool HasUsablePaymentReceivers()
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

        /// <summary>
        /// Returns every payment receiver that wants to receive payment.
        /// </summary>
        public IPaymentReceiver[] GetAvailablePaymentReceivers()
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
        //			MODULES


        /// <summary>
        /// Gets a specific module attached to this SSObject. Null if not found.
        /// </summary>
        public SSModule GetModule( Guid moduleId )
        {
            if( !this.modulesSealed )
            {
                throw new InvalidOperationException( "Can't get Module - Modules haven't been sealed yet." );
            }

            foreach( var module in this.modules )
            {
                if( module.moduleId == moduleId )
                {
                    return module;
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
                throw new InvalidOperationException( "Can't get Modules - Modules haven't been sealed yet." );
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
                throw new InvalidOperationException( "Can't get Module - Modules haven't been sealed yet." );
            }

            foreach( var module in this.modules )
            {
                if( module.moduleId == moduleId )
                {
                    if( module is T )
                    {
                        return (T)module;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all modules of specified type T, assigned to this SSobject.
        /// </summary>
        public T[] GetModules<T>() where T : SSModule
#warning TODO - interfaces? (<T>)
        {
            if( !this.modulesSealed )
            {
                throw new InvalidOperationException( "Can't get Modules - Modules haven't been sealed yet." );
            }

            List<T> ret = new List<T>();

            foreach( var module in this.modules )
            {
                if( module is T )
                {
                    ret.Add( (T)module );
                }
            }

            return ret.ToArray();
        }

        /// <summary>
        /// Adds a module of specified type T to this SSobject.
        /// </summary>
        internal T AddModule<T>( Guid moduleId ) where T : SSModule
        {
            if( this.modulesSealed )
            {
                throw new InvalidOperationException( "Can't add Module - Modules have been sealed already." );
            }

            for( int i = 0; i < this.modulesTemp.Count; i++ )
            {
                if( this.modulesTemp[i].moduleId == moduleId )
                {
                    throw new InvalidOperationException( "Can't add another module with the same module ID." );
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
        internal void SealModules()
        {
            this.modules = new SSModule[this.modulesTemp.Count];
            for( int i = 0; i < this.modulesTemp.Count; i++ )
            {
                this.modules[i] = this.modulesTemp[i];

                // cache.
                if( !this.hasPaymentReceiverModule && (this.modulesTemp[i] is IPaymentReceiver) )
                {
                    this.hasPaymentReceiverModule = true;
                }
                if( !this.hasInventoryModule && (this.modulesTemp[i] is InventoryModule) )
                {
                    this.hasInventoryModule = true;
                }
            }

            this.modulesTemp = null;
        }



        //
        //
        //			SUB-OBJECTS



        /// <summary>
        /// Gets a SubObject with specified Id. Returns null if none are present.
        /// </summary>
        public SubObject GetSubObject( Guid subObjectId )
        {
            if( !this.subObjectsSealed )
            {
                throw new InvalidOperationException( "Can't get Sub-Objects - Sub-Objects haven't been sealed yet." );
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
                throw new InvalidOperationException( "Can't get Sub-Objects - Sub-Objects haven't been sealed yet." );
            }

            return this.subObjects;
        }

        /// <summary>
        /// Gets all SubObjects assigned to this SSObject.
        /// </summary>
        public T[] GetSubObjects<T>() where T : SubObject
        {
            if( !this.subObjectsSealed )
            {
                throw new InvalidOperationException( "Can't get Sub-Objects - Sub-Objects haven't been sealed yet." );
            }

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
        /// Adds a Sub-Object of specified type T to this SSObject.
        /// </summary>
        internal (GameObject go, T sub) AddSubObject<T>( Guid subObjectId ) where T : SubObject
        {
            if( this.subObjectsSealed )
            {
                throw new InvalidOperationException( "Can't add Sub-Object - Sub-Objects have been sealed already." );
            }

            foreach( var subObj in this.subObjectsTemp )
            {
                if( subObj.subObjectId == subObjectId )
                {
                    throw new InvalidOperationException( "Can't add another sub-object with the same sub-object ID." );
                }
            }

            GameObject child = new GameObject( "Sub-Object [" + typeof( T ).Name + "] '" + subObjectId.ToString( "D" ) + "'" );
            child.transform.SetParent( this.transform );

            T subObject = child.AddComponent<T>();
            subObject.subObjectId = subObjectId;

            this.subObjectsTemp.Add( subObject );

            return (child, subObject);
        }

        /// <summary>
        /// Blocks the ability to add modules to this object. Calculates all of the caches for modules. Call this when you finish preparing the object.
        /// </summary>
        internal void SealSubObjects()
        {
            this.subObjects = this.subObjectsTemp.ToArray();
            this.subObjectsTemp = null;
        }



        //
        //
        //



        protected virtual void OnEnable()
        {
            allSSObjects.Add( this );
            if( this is SSObjectDFC )
            {
                allSelectables.Add( this as SSObjectDFC );
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
            if( this is SSObjectDFC )
            {
                allSelectables.Remove( this as SSObjectDFC );
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

        /// <summary>
        /// Finds and returns an object with the specified guid. Returns null if nothing was found.
        /// </summary>
        public static SSObject Find( Guid guid )
        {
            foreach( var obj in GetAll() )
            {
                if( obj.guid == guid )
                {
                    return obj;
                }
            }

            return null;
        }



        //
        //
        //


        /// <summary>
        /// Runs once when the object is created.
        /// </summary>
        protected virtual void OnObjSpawn() { }

        /// <summary>
        /// Runs once when the object is destroyed.
        /// </summary>
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