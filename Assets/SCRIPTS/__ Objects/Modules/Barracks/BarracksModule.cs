using SS.Objects.Buildings;
using SS.Content;
using SS.Diplomacy;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.ResourceSystem;
using SS.ResourceSystem.Payment;
using SS.Technologies;
using SS.UI;
using SS.Objects.Units;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using SS.Objects;

namespace SS.Objects.Modules
{
	[RequireComponent( typeof( FactionMember ) )]
	public class BarracksModule : SSModule, ISelectDisplayHandler, IPaymentReceiver
	{
		public const string KFF_TYPEID = "barracks";

		public UnityEvent onTrainingBegin = new UnityEvent();

		public UnityEvent onTrainingProgress = new UnityEvent();

		public UnityEvent onTrainingEnd = new UnityEvent();

		public UnityEvent onPaymentReceived { get; private set; } = new UnityEvent();
		
		/// <summary>
		/// Contains every unit that can be created in the barracks.
		/// </summary>
		public UnitDefinition[] trainableUnits { get; set; }

		/// <summary>
		/// Contains the multiplier used as the construction speed.
		/// </summary>
		public float trainSpeed { get; set; } = 1.0f;

		

		public UnitDefinition trainedUnit { get; private set; }

		/// <summary>
		/// Contains the local-space position, the units move towards, after creation.
		/// </summary>
		public Vector3 rallyPoint { get; set; }

		
		private FactionMember __factionMember = null;
		public FactionMember factionMember
		{
			get
			{
				if( this.__factionMember == null )
				{
					this.__factionMember = this.GetComponent<FactionMember>();
				}
				return this.__factionMember;
			}
		}


		private Dictionary<string, int> resourcesRemaining = new Dictionary<string, int>();

		private float trainProgressRemaining = 0.0f;

		private Vector3 spawnPosition = Vector3.zero;


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		void Awake()
		{
			Building ssObjectBuilding = this.ssObject as Building;
			if( ssObjectBuilding != null )
			{
				if( ssObjectBuilding.entrance == null )
				{
					Debug.LogWarning( "Barracks assigned to Building with no entrance: '" + ssObjectBuilding.definitionId + "'." );
				}
				else
				{
					this.spawnPosition = ssObjectBuilding.entrance.Value;
				}
			}
			
			LevelDataManager.onTechStateChanged.AddListener( this.OnTechStateChanged );
		}
		
		void Update()
		{
			if( this.IsPaymentDone() )
			{
				if( this.trainedUnit == null )
				{
					return;
				}
				this.ProgressTraining();
			}
		}

		void OnDestroy()
		{
			LevelDataManager.onTechStateChanged.RemoveListener( this.OnTechStateChanged );
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		private bool IsPaymentDone()
		{
			if( this.resourcesRemaining == null )
			{
				return true;
			}
			foreach( var kvp in this.resourcesRemaining )
			{
				if( kvp.Value > 0 )
				{
					return false;
				}
			}
			return true;
		}
		
		public void ReceivePayment( string id, int amount )
		{
			if( this.resourcesRemaining == null )
			{
				Debug.LogWarning( "The payment of " + amount + "x '" + id + "' was not wanted." );
				return;
			}
			if( this.resourcesRemaining.ContainsKey( id ) )
			{
				this.resourcesRemaining[id] -= amount;
				if( this.resourcesRemaining[id] < 0 )
				{
					this.resourcesRemaining[id] = 0;
				}
			}
			this.PaymentReceived_UI();
			this.onPaymentReceived?.Invoke();
		}

		public Dictionary<string, int> GetWantedResources()
		{
			Dictionary<string, int> ret = new Dictionary<string, int>();
			if( this.resourcesRemaining == null )
			{
				return ret;
			}
			foreach( var kvp in this.resourcesRemaining )
			{
				if( kvp.Value > 0 )
				{
					ret.Add( kvp.Key, kvp.Value );
				}
			}
			return ret;
		}



		public void BeginTraining( UnitDefinition def )
		{
			if( this.trainedUnit != null )
			{
				throw new Exception( "You can't start training unit, when another one is already being trained." );
			}

			this.trainedUnit = def;
			this.trainProgressRemaining = def.buildTime;

			this.resourcesRemaining = new Dictionary<string, int>( this.trainedUnit.cost );
			this.TrainingBegin_UI();
			this.onTrainingBegin?.Invoke();
		}

		private void ProgressTraining()
		{
			this.trainProgressRemaining -= this.trainSpeed * Time.deltaTime;

			if( this.trainProgressRemaining <= 0 )
			{
				this.EndTraining( true );
			}
			else
			{
				this.TrainingProgress_UI();
				this.onTrainingProgress?.Invoke();
			}
		}

		private void SpawnTrainedUnit()
		{
			// Calculate world-space spawn position.
			Matrix4x4 toWorld = this.transform.localToWorldMatrix;
			Vector3 spawnPos = toWorld.MultiplyVector( this.spawnPosition ) + this.transform.position;
			
			UnitData data = new UnitData();
			data.guid = Guid.NewGuid();
			data.position = spawnPos;
			data.rotation = Quaternion.identity;
			data.factionId = this.factionMember.factionId;
			data.health = this.trainedUnit.healthMax;
			GameObject obj = UnitCreator.Create( this.trainedUnit, data );
			// Move the newly spawned unit to the rally position.
			Vector3 rallyPointWorld = toWorld.MultiplyVector( this.rallyPoint ) + this.transform.position;
			TAIGoal.MoveTo.AssignTAIGoal( obj, rallyPointWorld );
		}

		public void EndTraining( bool isSuccess )
		{
			if( this.trainedUnit == null )
			{
				throw new Exception( "You can't end training, when there's no unit being trained." );
			}

			if( isSuccess )
			{
				this.SpawnTrainedUnit();
			}

			this.trainedUnit = null;
			this.trainProgressRemaining = 0.0f;
			this.resourcesRemaining = null;
			this.TrainingEnd_UI();
			this.onTrainingEnd?.Invoke();
		}
		

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		/// <summary>
		/// Creates a new BarracksModuleSaveState from a GameObject.
		/// </summary>
		/// <param name="unit">The GameObject to extract the save state from.</param>
		public override ModuleData GetData()
		{
			BarracksModuleData saveState = new BarracksModuleData();

			saveState.resourcesRemaining = this.resourcesRemaining;
			if( this.trainedUnit == null )
			{
				saveState.trainedUnitId = "";
			}
			else
			{
				saveState.trainedUnitId = this.trainedUnit.id;
			}
			saveState.trainProgress = this.trainProgressRemaining;

			return saveState;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		public override void SetDefData( ModuleDefinition _def, ModuleData _data )
		{
			if( !(_def is BarracksModuleDefinition) )
			{
				throw new Exception( "Provided definition is not of the correct type." );
			}
			if( _def == null )
			{
				throw new Exception( "Provided definition is null." );
			}

			if( !(_data is BarracksModuleData) )
			{
				throw new Exception( "Provided data is not of the correct type." );
			}
			if( _data == null )
			{
				throw new Exception( "Provided data is null." );
			}
			
			BarracksModuleDefinition def = (BarracksModuleDefinition)_def;
			BarracksModuleData data = (BarracksModuleData)_data;

			this.icon = def.icon;
			this.trainSpeed = def.trainSpeed;
			this.trainableUnits = new UnitDefinition[def.trainableUnits.Length];
			for( int i = 0; i < this.trainableUnits.Length; i++ )
			{
				this.trainableUnits[i] = DefinitionManager.GetUnit( def.trainableUnits[i] );
			}
			
			// ------          DATA

			this.resourcesRemaining = data.resourcesRemaining;
			if( data.trainedUnitId == "" )
			{
				this.trainedUnit = null;
			}
			else
			{
				this.trainedUnit = DefinitionManager.GetUnit( data.trainedUnitId );
			}
			this.trainProgressRemaining = data.trainProgress;
			this.rallyPoint = data.rallyPoint;
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		//		UI INTEGRATION


		private string GetStatusString()
		{
			StringBuilder sb = new StringBuilder();

			if( this.resourcesRemaining == null )
			{
				return "null";
			}
			foreach( var kvp in this.resourcesRemaining )
			{
				if( kvp.Value != 0 )
				{
					ResourceDefinition resDef = DefinitionManager.GetResource( kvp.Key );
					sb.Append( kvp.Value + "x " + resDef.displayName );
				}
				sb.Append( ", " );
			}

			return sb.ToString();
		}

		private void ShowList()
		{
			GameObject[] gridElements = new GameObject[this.trainableUnits.Length];
			// Initialize the grid elements' GameObjects.
			for( int i = 0; i < this.trainableUnits.Length; i++ )
			{
				UnitDefinition unitDef = this.trainableUnits[i];

				// If the unit's techs required have not been researched yet, add unclickable button, otherwise, add normal button.
				if( TechLock.CheckLocked( unitDef, LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].GetAllTechs() ) )
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon, null );
				}
				else
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon, () =>
					{
						this.BeginTraining( unitDef );
					} );
				}
				ToolTipUIHandler toolTipUIhandler = gridElements[i].AddComponent<ToolTipUIHandler>();
				toolTipUIhandler.constructToolTip = () =>
				{
					ToolTip.Create( 450.0f, unitDef.displayName );

					ToolTip.AddText( "Health: " + unitDef.healthMax );
					ToolTip.Style.SetPadding( 100, 100 );
					foreach( var kvp in unitDef.cost )
					{
						ResourceDefinition resourceDef = DefinitionManager.GetResource( kvp.Key );
						ToolTip.AddText( resourceDef.icon, kvp.Value.ToString() );
					}
				};
			}

			GameObject list = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 30.0f, 5.0f ), new Vector2( -60.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
			SelectionPanel.instance.obj.RegisterElement( "barracks.list", list.transform );
		}

		private void OnTechStateChanged( int factionId, string id, TechnologyResearchProgress newProgress )
		{
			if( factionId != this.factionMember.factionId )
			{
				return;
			}
			if( !Selection.IsDisplayedModule( this ) )
			{
				return;
			}

			if( this.IsPaymentDone() )
			{
				if( this.trainedUnit == null )
				{
					if( SelectionPanel.instance.obj.GetElement( "barracks.list" ) != null )
					{
						SelectionPanel.instance.obj.ClearElement( "barracks.list" );
					}
					this.ShowList();
				}
			}
		}

		private void PaymentReceived_UI()
		{
			if( !Selection.IsDisplayedModule( this ) )
			{
				return;
			}

			Transform statusUI = SelectionPanel.instance.obj.GetElement( "barracks.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Waiting for resources... ('" + this.trainedUnit.displayName + "'): " + this.GetStatusString() );
			}
		}
		
		private void TrainingBegin_UI()
		{
			if( !Selection.IsDisplayedModule( this ) )
			{
				return;
			}

			if( SelectionPanel.instance.obj.GetElement( "barracks.list" ) != null )
			{
				SelectionPanel.instance.obj.ClearElement( "barracks.list" );
			}
			if( !this.IsPaymentDone() )
			{
				Transform statusUI = SelectionPanel.instance.obj.GetElement( "barracks.status" );
				if( statusUI != null )
				{
					UIUtils.EditText( statusUI.gameObject, "Waiting for resources... ('" + this.trainedUnit.displayName + "'): " + this.GetStatusString() );
				}
				ActionPanel.instance.CreateButton( "barracks.ap.cancel", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/cancel" ), () =>
				{
					this.EndTraining( false );
				} );
			}
		}

		private void TrainingProgress_UI()
		{
			if( !Selection.IsDisplayedModule( this ) )
			{
				return;
			}

			Transform statusUI = SelectionPanel.instance.obj.GetElement( "barracks.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Training... '" + this.trainedUnit.displayName + "' - " + (int)this.trainProgressRemaining + " s." );
			}
		}
		
		private void TrainingEnd_UI()
		{
			if( !Selection.IsDisplayedModule( this ) )
			{
				return;
			}

			Transform statusUI = SelectionPanel.instance.obj.GetElement( "barracks.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Select unit to make..." );
			}

			ActionPanel.instance.Clear( "barracks.ap.cancel" );

			this.ShowList();
		}

		public void OnDisplay()
		{
			// If it's not usable - return, don't train anything.
			if( this.ssObject is IUsableToggle && !(this.ssObject as IUsableToggle).IsUsable() )
			{
				return;
			}

			if( this.factionMember.factionId != LevelDataManager.PLAYER_FAC )
			{
				return;
			}
			if( this.IsPaymentDone() )
			{
				if( this.trainedUnit != null )
				{
					GameObject status = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Training... '" + this.trainedUnit.displayName + "' - " + (int)this.trainProgressRemaining + " s." );
					SelectionPanel.instance.obj.RegisterElement( "barracks.status", status.transform );

					ActionPanel.instance.CreateButton( "barracks.ap.cancel", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/cancel" ), () =>
					{
						this.EndTraining( false );
					} );
				}
				else
				{
					this.ShowList();

					GameObject status = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Select unit to make..." );
					SelectionPanel.instance.obj.RegisterElement( "barracks.status", status.transform );
				}
			}
			else
			{
				GameObject status = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Waiting for resources... ('" + this.trainedUnit.displayName + "'): " + GetStatusString() );
				SelectionPanel.instance.obj.RegisterElement( "barracks.status", status.transform );
				
				ActionPanel.instance.CreateButton( "barracks.ap.cancel", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/cancel" ), () =>
				{
					this.EndTraining( false );
				} );
			}
		}

#if UNITY_EDITOR

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.blue;

			Matrix4x4 toWorld = this.transform.localToWorldMatrix;
			Vector3 spawnPos = toWorld.MultiplyVector( this.spawnPosition ) + this.transform.position;

			Gizmos.DrawSphere( spawnPos, 0.1f );

			Vector3 rallyPos = toWorld.MultiplyVector( this.rallyPoint ) + this.transform.position;

			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere( rallyPos, 0.15f );
			Gizmos.DrawLine( spawnPos, rallyPos );
		}
#endif
	}
}