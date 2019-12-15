using SS.AI.Goals;
using SS.Content;
using SS.Diplomacy;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.ResourceSystem;
using SS.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SS.Objects.Modules
{	
	public sealed class InventoryModule : SSModule, ISelectDisplayHandler, IMouseOverHandlerListener
	{
		public const string KFF_TYPEID = "inventory";

		public struct SlotGroup
		{
			public readonly string slotId;
			public string id;
			public int amount;
			public readonly int capacity;

			public bool isConstrained { get { return this.slotId != ""; } }
			public bool isEmpty { get { return this.amount == 0 || this.id == ""; } }

			public SlotGroup( string slotId, int slotCapacity )
			{
				this.slotId = slotId ?? "";
				this.id = "";
				this.capacity = slotCapacity;
				this.amount = 0;
			}
		}

		private SlotGroup[] slotGroups;

		/// <summary>
		/// Returns a copy of every slot in the inventory.
		/// </summary>
		public SlotGroup[] GetSlots()
		{
			SlotGroup[] ret = new SlotGroup[this.slotCount];
			for( int i = 0; i < this.slotCount; i++ )
			{
				ret[i] = this.slotGroups[i];
			}
			return ret;
		}

		/// <summary>
		/// Returns the amount of slots of this inventory.
		/// </summary>
		public int slotCount
		{
			get
			{
				return this.slotGroups.Length;
			}
		}


		public class _UnityEvent_string_int : UnityEvent<string, int> { }

		public _UnityEvent_string_int onAdd { get; private set; } = new _UnityEvent_string_int();
		public _UnityEvent_string_int onRemove { get; private set; } = new _UnityEvent_string_int();


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		private void ShowTooltip()
		{
			IFactionMember ssObjectFactionMember = this.ssObject as IFactionMember;
			if( ssObjectFactionMember != null && ssObjectFactionMember.factionId != LevelDataManager.PLAYER_FAC )
			{
				return;
			}

			ToolTip.Create( 200.0f, this.ssObject.displayName );

			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].isEmpty )
				{
					if( this.slotGroups[i].isConstrained )
					{
						ResourceDefinition resourceDef = DefinitionManager.GetResource( this.slotGroups[i].slotId );
						ToolTip.AddText( resourceDef.icon, this.slotGroups[i].amount + " / " + this.slotGroups[i].capacity );
					}
					else
					{
						ToolTip.AddText( AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/empty_resource" ), this.slotGroups[i].amount + " / " + this.slotGroups[i].capacity );
					}
				}
				else
				{
					ResourceDefinition resourceDef = DefinitionManager.GetResource( this.slotGroups[i].id );
					ToolTip.AddText( resourceDef.icon, this.slotGroups[i].amount + " / " + this.slotGroups[i].capacity );
				}
			}
			ToolTip.ShowAt( Input.mousePosition );
		}

		private void MoveTooltip()
		{
			IFactionMember ssObjectFactionMember = this.ssObject as IFactionMember;
			if( ssObjectFactionMember != null && ssObjectFactionMember.factionId != LevelDataManager.PLAYER_FAC )
			{
				return;
			}

			ToolTip.MoveTo( Input.mousePosition, true );
		}

		private void HideTooltip()
		{
			IFactionMember ssObjectFactionMember = this.ssObject as IFactionMember;
			if( ssObjectFactionMember != null && ssObjectFactionMember.factionId != LevelDataManager.PLAYER_FAC )
			{
				return;
			}

			ToolTip.Hide();
		}

		
		public void OnMouseEnterListener()
		{
			this.ShowTooltip();
		}

		public void OnMouseStayListener()
		{
			this.MoveTooltip();
		}

		public void OnMouseExitListener()
		{
			this.HideTooltip();
		}

		private void RegisterTooltip()
		{
			this.onAdd.AddListener( ( string id, int amount ) =>
			{
				if( MouseOverHandler.currentObjectMouseOver == this.gameObject )
				{
					this.ShowTooltip();
				}
			} );
			this.onRemove.AddListener( ( string id, int amount ) =>
			{
				if( MouseOverHandler.currentObjectMouseOver == this.gameObject )
				{
					this.ShowTooltip();
				}
			} );
		}
		
		private void RegisterHUD()
		{
			// integrate hud.
			IHUDHolder hudObj = (IHUDHolder)this.ssObject;


			Transform hudResourceTransform = hudObj.hud.transform.Find( "Resource" );
			if( hudResourceTransform == null )
			{
				return;
			}

			Transform hudResourceIconTransform = hudResourceTransform.Find( "Icon" );
			if( hudResourceIconTransform == null )
			{
				return;
			}

			Image hudResourceIcon = hudResourceIconTransform.GetComponent<Image>();
			TextMeshProUGUI hudResourceAmount = hudObj.hud.transform.Find( "Amount" ).GetComponent<TextMeshProUGUI>();

			// Make the inventory update the HUD wien resources are added/removed.
			this.onAdd.AddListener( ( string id, int amtAdded ) =>
			{
				for( int i = 0; i < this.slotCount; i++ )
				{
					if( this.slotGroups[i].isEmpty )
					{
						continue;
					}
					hudResourceIcon.sprite = DefinitionManager.GetResource( this.slotGroups[i].id ).icon;
					hudResourceAmount.text = "" + this.slotGroups[i].amount;

					hudResourceIcon.gameObject.SetActive( true );
					hudResourceAmount.gameObject.SetActive( true );
					break;
				}
			} );
			this.onRemove.AddListener( ( string id, int amtRemoved ) =>
			{
				if( this.isEmpty )
				{
					hudResourceIcon.gameObject.SetActive( false );
					hudResourceAmount.gameObject.SetActive( false );
				}
				else
				{
					for( int i = 0; i < this.slotCount; i++ )
					{
						if( this.slotGroups[i].isEmpty )
						{
							continue;
						}
						hudResourceIcon.sprite = DefinitionManager.GetResource( this.slotGroups[i].id ).icon;
						hudResourceAmount.text = "" + this.slotGroups[i].amount;
						break;
					}
				}
			} );
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		

		void Awake()
		{
			// needs to be registered on Awake(), since the resources (data) are assigned before Start() happens.
			this.RegisterTooltip();

			if( this.ssObject is IHUDHolder )
			{
				this.RegisterHUD();
			}
			this.onAdd.AddListener( ( string id, int amount ) =>
			{
				ResourcePanel.instance.UpdateResourceEntryDelta( id, amount );
			} );
			this.onRemove.AddListener( ( string id, int amount ) =>
			{
				ResourcePanel.instance.UpdateResourceEntryDelta( id, -amount );
			} );

			if( this.ssObject is IFactionMember )
			{
				IFactionMember fac = (IFactionMember)this.ssObject;
				fac.onFactionChange.AddListener( () =>
				{
					if( fac.factionId == LevelDataManager.PLAYER_FAC )
					{
						// add to the respanel.
						for( int i = 0; i < this.slotCount; i++ )
						{
							if( !this.slotGroups[i].isEmpty )
							{
								ResourcePanel.instance.UpdateResourceEntryDelta( this.slotGroups[i].id, this.slotGroups[i].amount );
							}
						}
					}
					else
					{
						// remove from the respanel
						for( int i = 0; i < this.slotCount; i++ )
						{
							if( !this.slotGroups[i].isEmpty )
							{
								ResourcePanel.instance.UpdateResourceEntryDelta( this.slotGroups[i].id, -this.slotGroups[i].amount );
							}
						}
					}
				} );
			}
			if( this.ssObject is IDamageable )
			{
				IDamageable d = ((IDamageable)this.ssObject);

				d.onDeath.AddListener( () =>
				{
					TacticalDropOffGoal.ExtractFrom( this.transform.position, this.transform.rotation, this.GetAll() );
				} );
			}
		}

		void OnDestroy()
		{
			if( ToolTip.canvas != null ) // If the tooltip exists (can be non-existent, if the OnDestroy() is called when the editor leaves play mode).
			{
				if( MouseOverHandler.currentObjectMouseOver == this.gameObject )
				{
					this.HideTooltip();
				}
			}
		}



		public bool isEmpty
		{
			get
			{
				// If any of the slots is not empty (i.e. contains something, i.e. slot's amount is >0), then the whole inventory is not empty.
				for( int i = 0; i < this.slotCount; i++ )
				{
					if( !this.slotGroups[i].isEmpty )
					{
						return false;
					}
				}
				return true;
			}
		}

		public int Get( string id )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}

			int total = 0;
			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].id == id )
				{
					total += this.slotGroups[i].amount;
				}
			}
			return total;
		}

		public Dictionary<string, int> GetAll()
		{
			if( this.isEmpty )
			{
				return new Dictionary<string, int>();
			}
			Dictionary<string, int> ret = new Dictionary<string, int>();
			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].isEmpty )
				{
					continue;
				}
				if( ret.ContainsKey( this.slotGroups[i].id ) )
				{
					ret[this.slotGroups[i].id] += this.slotGroups[i].amount;
				}
				else
				{
					ret.Add( this.slotGroups[i].id, this.slotGroups[i].amount );
				}
			}
			return ret;
		}
		
		public int GetSpaceLeft( string id )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}

			int total = 0;
			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].isEmpty )
				{
					if( !this.slotGroups[i].isConstrained || this.slotGroups[i].slotId == id )
					{
						total += this.slotGroups[i].capacity;
					}
				}
				else
				{
					// if it can take any type, but only when there is no invalid type already there. OR if it only takes that valid type (can resource be placed in slot).
					if( (!this.slotGroups[i].isConstrained && this.slotGroups[i].id == id) || this.slotGroups[i].slotId == id )
					{
						total += this.slotGroups[i].capacity - this.slotGroups[i].amount;
					}
				}
			}
			return total;
		}

		public int Add( string id, int amountMax )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}
			if( amountMax < 1 )
			{
				throw new ArgumentOutOfRangeException( "Amount can't be less than 1." );
			}

			// array of indices to the slots. (necessary since we want to fill up non-empty slots before filling up empty ones).
			List<int> indicesEmpty = new List<int>();
			List<int> indicesFull = new List<int>();
			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].isEmpty )
				{
					if( !this.slotGroups[i].isConstrained || this.slotGroups[i].slotId == id )
					{
						indicesEmpty.Add( i );
					}
				}
				else
				{// if it can take any type, but only when there is no invalid type already there. OR if it only takes that valid type (can resource be placed in slot).
					if( (!this.slotGroups[i].isConstrained && this.slotGroups[i].id == id) || this.slotGroups[i].slotId == id )
					{
						indicesFull.Add( i );
					}
				}
			}
			List<int> indices = new List<int>();
			indices.AddRange( indicesFull );
			indices.AddRange( indicesEmpty );

			int amountRemaining = amountMax;
			for( int i = 0; i < indices.Count; i++ )
			{
				int index = indices[i];
				int spaceInSlot = this.slotGroups[index].capacity - this.slotGroups[index].amount;
				int amountAdded = spaceInSlot > amountRemaining ? amountRemaining : spaceInSlot;

				this.slotGroups[index].amount += amountAdded;
				this.slotGroups[index].id = id;
				amountRemaining -= amountAdded;

				if( amountRemaining == 0 )
				{
					this.onAdd?.Invoke( id, amountMax );
					this.TryUpdateList();
					return amountMax;
				}
			}
			this.onAdd?.Invoke( id, amountMax - amountRemaining );
			this.TryUpdateList();
			return amountMax - amountRemaining;
		}

		public int Remove( string id, int amountMax )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}
			if( amountMax < 1 )
			{
				throw new ArgumentOutOfRangeException( "Amount can't be less than 1." );
			}

			int amountLeftToRemove = amountMax;
			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].isEmpty )
				{
					continue;
				}
				if( this.slotGroups[i].id == id )
				{
					int amountRemoved = this.slotGroups[i].amount > amountLeftToRemove ? amountLeftToRemove : this.slotGroups[i].amount;

					this.slotGroups[i].amount -= amountRemoved;
					if( this.slotGroups[i].amount == 0 )
					{
						this.slotGroups[i].id = "";
					}
					amountLeftToRemove -= amountRemoved;
					
					if( amountLeftToRemove == 0 )
					{
						this.onRemove?.Invoke( id, amountMax );
						this.TryUpdateList();
						return amountMax;
					}
				}
			}
			this.onRemove?.Invoke( id, amountMax - amountLeftToRemove );
			this.TryUpdateList();
			return amountMax - amountLeftToRemove;
		}

		public void Clear()
		{
			Dictionary<string, int> res = this.GetAll();

			for( int i = 0; i < this.slotCount; i++ )
			{
				this.slotGroups[i].id = "";
				this.slotGroups[i].amount = 0;
			}

			// Call the event after clearing, once per each type removed.
			foreach( var kvp in res )
			{
				this.onRemove?.Invoke( kvp.Key, kvp.Value ); // be consistent, all methods shall invoke after adding/removing.
			}
			this.TryUpdateList();
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override ModuleData GetData()
		{
			InventoryModuleData data = new InventoryModuleData();

			data.items = new InventoryModuleData.SlotData[this.slotCount];
			for( int i = 0; i < data.items.Length; i++ )
			{
				data.items[i] = new InventoryModuleData.SlotData( this.slotGroups[i] );
			}

			return data;
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override void SetDefData( ModuleDefinition _def, ModuleData _data )
		{
			if( !(_def is InventoryModuleDefinition) )
			{
				throw new Exception( "Provided definition is not of the correct type." );
			}
			if( _def == null )
			{
				throw new Exception( "Provided definition is null." );
			}

			if( !(_data is InventoryModuleData) )
			{
				throw new Exception( "Provided data is not of the correct type." );
			}
			if( _data == null )
			{
				throw new Exception( "Provided data is null." );
			}

			InventoryModuleDefinition def = (InventoryModuleDefinition)_def;
			InventoryModuleData data = (InventoryModuleData)_data;

			this.icon = def.icon;
			this.slotGroups = new SlotGroup[def.slots.Length];
			for( int i = 0; i < this.slotCount; i++ )
			{
				this.slotGroups[i] = new SlotGroup( def.slots[i].slotId, def.slots[i].capacity );
			}

			// ------          DATA

			if( data.items != null )
			{
				if( data.items.Length != def.slots.Length )
				{
					throw new Exception( "Inventory slot count is not the same as data's slot count. Can't match the slots." );
				}
				for( int i = 0; i < data.items.Length; i++ )
				{
					this.slotGroups[i].id = data.items[i].id;
					this.slotGroups[i].amount = data.items[i].amount;




					if( this.ssObject is IFactionMember )
					{
						IFactionMember factionMember = (IFactionMember)this.ssObject;
						if( factionMember.factionId == LevelDataManager.PLAYER_FAC )
						{
#warning TODO! - ugly.
#warning TODO! - update when faction changes.
							if( !this.slotGroups[i].isEmpty )
							{
								ResourcePanel.instance.UpdateResourceEntryDelta( this.slotGroups[i].id, this.slotGroups[i].amount );
							}
						}
					}


				}
			}
		}

		private void TryUpdateList()
		{
			if( Selection.IsDisplayedModule( this ) )
			{
				SelectionPanel.instance.obj.TryClearElement( "inventory.slots" );
				
				this.ShowList();
			}
		}

		private void ShowList()
		{
			GameObject[] gridElements = new GameObject[this.slotCount];
			// Initialize the grid elements' GameObjects.
			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].isEmpty )
				{
					gridElements[i] = UIUtils.InstantiateIcon( SelectionPanel.instance.obj.transform, new GenericUIData( Vector2.zero, new Vector2( 32.0f, 32.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/empty_resource" ) );

					UIUtils.InstantiateText( gridElements[i].transform, new GenericUIData( new Vector2( 32.0f, 0.0f ), new Vector2( 320.0f, 32.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), " - " );
					continue;
				}
				ResourceDefinition resDef = DefinitionManager.GetResource( this.slotGroups[i].id );
				gridElements[i] = UIUtils.InstantiateIcon( SelectionPanel.instance.obj.transform, new GenericUIData( Vector2.zero, new Vector2( 32.0f, 32.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), resDef.icon );

				UIUtils.InstantiateText( gridElements[i].transform, new GenericUIData( new Vector2( 32.0f, 0.0f ), new Vector2( 320.0f, 32.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), this.slotGroups[i].amount + " / " + this.slotGroups[i].capacity );
			}

			GameObject list = UIUtils.InstantiateScrollableList( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 30.0f, 5.0f ), new Vector2( -60.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), gridElements );
			SelectionPanel.instance.obj.RegisterElement( "inventory.slots", list.transform );
		}

		public void OnDisplay()
		{
			this.ShowList();
		}

		public void OnHide()
		{

		}
	}
}