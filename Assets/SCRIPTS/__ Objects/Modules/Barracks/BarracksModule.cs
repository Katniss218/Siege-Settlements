using SS.AI;
using SS.AI.Goals;
using SS.Content;
using SS.InputSystem;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Objects.Units;
using SS.ResourceSystem;
using SS.ResourceSystem.Payment;
using SS.Technologies;
using SS.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SS.Objects.Modules
{
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




		/// <summary>
		/// Contains the world-space position, the units move towards, after creation.
		/// </summary>
		public Vector3? rallyPoint { get; set; }

		public Vector3 GetRallyPoint()
		{
			if( this.rallyPoint == null )
			{
				return this.transform.position;
			}
			return this.rallyPoint.Value;
		}


		private List<UnitDefinition> queuedUnits = new List<UnitDefinition>();

		private Dictionary<string, int> resourcesRemaining = new Dictionary<string, int>();

		private float buildTimeRemaining = 0.0f;

		private Vector3 spawnPosition = Vector3.zero;

		private GameObject rallyPointGameObject = null;

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
				throw new Exception( "Unwanted payment was received." );
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

		//
		//
		//

		/// <summary>
		/// Adds the specified unit to the training queue & updates everything accordingly.
		/// </summary>
		public void Enqueue( UnitDefinition unit )
		{
			this.queuedUnits.Add( unit );
			this.RefreshQueue_UI();

			if( this.queuedUnits.Count == 1 ) // if the object is the first one, begin training it.
			{
				this.BeginTraining();
			}
		}

		/// <summary>
		/// Removes the unit from the training queue & updates everything accordingly.
		/// </summary>
		public void Dequeue( bool spawn, int index = 0 )
		{
			// Prevent dequeuing when the queue is empty.
			if( index >= this.queuedUnits.Count )
			{
				return;
			}

			if( index == 0 )
			{
				this.EndTraining( spawn );
			}

			this.queuedUnits.RemoveAt( index );
			this.RefreshQueue_UI();

			if( index == 0 && this.queuedUnits.Count > 0 ) // if the training can continue.
			{
				this.BeginTraining();
			}
		}
		

		private void BeginTraining()
		{
			if( this.queuedUnits.Count == 0 )
			{
				throw new Exception( "Tried to begin training with an empty queue." );
			}
			
			this.buildTimeRemaining = this.queuedUnits[0].buildTime;
			this.resourcesRemaining = new Dictionary<string, int>( this.queuedUnits[0].cost );
			this.TrainingBegin_UI();
			this.onTrainingBegin?.Invoke();
		}
		
		private void EndTraining( bool spawn )
		{
			if( this.queuedUnits.Count == 0 )
			{
				throw new Exception( "Tried to end training with an empty queue." );
			}

			UnitDefinition unit = this.queuedUnits[0];
			if( spawn )
			{
				this.Spawn( unit );
			}
			
			this.buildTimeRemaining = 0.0f;
			this.resourcesRemaining = null;
			this.TrainingEnd_UI();
			this.onTrainingEnd?.Invoke();
		}
	

		private void Spawn( UnitDefinition def )
		{
			// Calculate world-space spawn position.
			Matrix4x4 toWorld = this.transform.localToWorldMatrix;
			Vector3 spawnPos = toWorld.MultiplyVector( this.spawnPosition ) + this.transform.position;

			UnitData data = new UnitData();
			data.guid = Guid.NewGuid();
			data.position = spawnPos;
			data.rotation = Quaternion.identity;
			data.factionId = ((IFactionMember)this.ssObject).factionId;
			data.population = PopulationSize.x4;

			Unit unit = UnitCreator.Create( def, data.guid );
			UnitCreator.SetData( unit, data );

			// Move the newly spawned unit to the rally position.
			TacticalGoalController goalController = unit.controller;
			TacticalMoveToGoal goal = new TacticalMoveToGoal();
			goal.isHostile = false;
			goal.SetDestination( this.GetRallyPoint() );
			goalController.SetGoals( TacticalGoalController.DEFAULT_GOAL_TAG_ASSIGNED, goal );
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		protected override void Awake()
		{
			LevelDataManager.onTechStateChanged.AddListener( this.OnTechStateChanged );

			this.ssObject.isSelectable = true;

			base.Awake();
		}
		
		void Update()
		{
			if( this.queuedUnits.Count == 0 )
			{
				return;
			}

			if( this.IsPaymentDone() )
			{
				// progress training.
				this.buildTimeRemaining -= this.trainSpeed * Time.deltaTime;

				if( this.buildTimeRemaining <= 0 )
				{
					this.Dequeue( true );
				}
				else
				{
					this.TrainingProgress_UI();
					this.onTrainingProgress?.Invoke();
				}
			}
		}

		void OnDestroy()
		{
			LevelDataManager.onTechStateChanged.RemoveListener( this.OnTechStateChanged );
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		/// <summary>
		/// Creates a new BarracksModuleSaveState from a GameObject.
		/// </summary>
		/// <param name="unit">The GameObject to extract the save state from.</param>
		public override SSModuleData GetData()
		{
			BarracksModuleData data = new BarracksModuleData();

			data.resourcesRemaining = this.resourcesRemaining;
			if( this.queuedUnits.Count == 0 )
			{
				data.queuedUnits = null;
			}
			else
			{
				data.queuedUnits = new string[this.queuedUnits.Count];
				for( int i = 0; i < this.queuedUnits.Count; i++ )
				{
					data.queuedUnits[i] = this.queuedUnits[i].id;
				}
			}
			data.buildTimeRemaining = this.buildTimeRemaining;
			data.rallyPoint = this.rallyPoint;

			return data;
		}
				
		public override void SetData( SSModuleData _data )
		{
			BarracksModuleData data = ValidateDataType<BarracksModuleData>( _data );
						
			// ------          DATA

			this.resourcesRemaining = data.resourcesRemaining;
			if( data.queuedUnits != null )
			{
				for( int i = 0; i < data.queuedUnits.Length; i++ )
				{
					this.queuedUnits.Add( DefinitionManager.GetUnit( data.queuedUnits[i] ) );
				}
			}
			this.buildTimeRemaining = data.buildTimeRemaining;
			this.rallyPoint = data.rallyPoint;
		}




		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-

		//		UI INTEGRATION


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
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon, null );
				}
				else
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon, () =>
					{
						this.Enqueue( unitDef );
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

			GameObject list = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 300.0f, 5.0f ), new Vector2( -330.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
			SelectionPanel.instance.obj.RegisterElement( "barracks.list", list.transform );
		}

		private void CreateQueueUI()
		{
			GameObject queue = new GameObject();
			RectTransform t = queue.AddComponent<RectTransform>();
			t.SetParent( SelectionPanel.instance.obj.transform );
			t.ApplyUIData( new GenericUIData( new Vector2( 250.0f, 5.0f ), new Vector2( -500.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ) );

			HorizontalLayoutGroup layout = queue.AddComponent<HorizontalLayoutGroup>();
			layout.childControlWidth = true;
			layout.childControlHeight = true;
			layout.childScaleWidth = true;
			layout.childForceExpandWidth = false;
			layout.childForceExpandHeight = false;

			int i = 0;
			// Make the queue icons remove the unit from queue when clicked.
			foreach( UnitDefinition unitDef in this.queuedUnits )
			{
				int j = i;
				GameObject go = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon, () =>
				{
					this.Dequeue( false, j );
				} );
				go.transform.SetParent( t );
				i++;
			}

			SelectionPanel.instance.obj.RegisterElement( "barracks.queue", queue.transform );
		}

		public void RefreshQueue_UI()
		{
			if( (!Selection.IsDisplayedModule( this )) || (!this.ssObject.IsDisplaySafe()) )
			{
				return;
			}

			SelectionPanel.instance.obj.TryClearElement( "barracks.queue" );
			this.CreateQueueUI();
		}


		private void OnTechStateChanged( int factionId, string id, TechnologyResearchProgress newProgress )
		{
			if( (!Selection.IsDisplayedModule( this )) || (!this.ssObject.IsDisplaySafe()) )
			{
				return;
			}

			SelectionPanel.instance.obj.TryClearElement( "barracks.list" );
			this.ShowList();
		}

		private void PaymentReceived_UI()
		{
			if( (!Selection.IsDisplayedModule( this )) || (!this.ssObject.IsDisplaySafe()) )
			{
				return;
			}

			Transform statusUI = SelectionPanel.instance.obj.GetElement( "barracks.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Waiting for resources... ('" + this.queuedUnits[0].displayName + "'): " + ResourceUtils.ToResourceString( this.resourcesRemaining ) );
			}
		}
		
		private void TrainingBegin_UI()
		{
			if( (!Selection.IsDisplayedModule( this )) || (!this.ssObject.IsDisplaySafe()) )
			{
				return;
			}

			if( !this.IsPaymentDone() )
			{
				Transform statusUI = SelectionPanel.instance.obj.GetElement( "barracks.status" );
				if( statusUI != null )
				{
					UIUtils.EditText( statusUI.gameObject, "Waiting for resources... ('" + this.queuedUnits[0].displayName + "'): " + ResourceUtils.ToResourceString( this.resourcesRemaining ) );
				}

				// clear if the begin was caused by decreasing queue.
				ActionPanel.instance.Clear( "barracks.ap.cancel" );
				DisplayCancelButton();
			}
		}

		private void TrainingProgress_UI()
		{
			if( (!Selection.IsDisplayedModule( this )) || (!this.ssObject.IsDisplaySafe()) )
			{
				return;
			}

			Transform statusUI = SelectionPanel.instance.obj.GetElement( "barracks.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Training... '" + this.queuedUnits[0].displayName + "' - " + (int)this.buildTimeRemaining + " s." );
			}
		}
		
		private void TrainingEnd_UI()
		{
			if( (!Selection.IsDisplayedModule( this )) || (!this.ssObject.IsDisplaySafe()) )
			{
				return;
			}

			Transform statusUI = SelectionPanel.instance.obj.GetElement( "barracks.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Select unit to make..." );
			}
			
			SelectionPanel.instance.obj.TryClearElement( "barracks.queue" );
			this.CreateQueueUI();
			
			if( this.queuedUnits.Count == 0 )
			{
				ActionPanel.instance.Clear( "barracks.ap.cancel" );
			}
		}

		private static GenericUIData GetStatusPos()
		{
			return new GenericUIData( new Vector2( 300.0f, 0.0f ), new Vector2( 200.0f, 25.0f ), Vector2.up, Vector2.up, Vector2.up );
		}

		private void DisplayCancelButton()
		{
			ActionPanel.instance.CreateButton( "barracks.ap.cancel", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/cancel" ), "Cancel", "Click to cancel production...", () =>
			{
				this.Dequeue( false );
			} );
		}

		public void OnDisplay()
		{
			if( !this.ssObject.IsDisplaySafe() )
			{
				return;
			}

			// If it's not usable - return, don't train anything.
			if( this.ssObject is ISSObjectUsableUnusable && !(this.ssObject as ISSObjectUsableUnusable).isUsable )
			{
				return;
			}

			this.ShowList();
			this.CreateQueueUI();

			if( this.IsPaymentDone() )
			{
				if( this.queuedUnits.Count == 0 )
				{
					GameObject status = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, GetStatusPos(), "Select unit to make..." );
					SelectionPanel.instance.obj.RegisterElement( "barracks.status", status.transform );
				}
				else
				{
					GameObject status = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, GetStatusPos(), "Training... '" + this.queuedUnits[0].displayName + "' - " + (int)this.buildTimeRemaining + " s." );
					SelectionPanel.instance.obj.RegisterElement( "barracks.status", status.transform );
					DisplayCancelButton();
				}
			}
			else
			{
				GameObject status = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, GetStatusPos(), "Waiting for resources... ('" + this.queuedUnits[0].displayName + "'): " + ResourceUtils.ToResourceString( this.resourcesRemaining ) );
				SelectionPanel.instance.obj.RegisterElement( "barracks.status", status.transform );
				DisplayCancelButton();
			}

			CreateRallyButton();
			SpawnRallyGameObject( this );
		}

		public void OnHide()
		{
			Object.Destroy( this.rallyPointGameObject );
			Main.mouseInput.ClearOnPress( MouseCode.LeftMouseButton, this.Inp_SetRally ); // Force remove input if hidden.
			Main.mouseInput.ClearOnPress( MouseCode.RightMouseButton, this.Inp_CancelRally ); // Force remove input if hidden.
		}


		//
		//
		//


		private static void SpawnRallyGameObject( BarracksModule barracks )
		{
			GameObject rally = new GameObject();

			MeshFilter mf = rally.AddComponent<MeshFilter>();
			mf.mesh = AssetManager.GetMesh( AssetManager.EXTERN_ASSET_ID + "Models/barracks_rally_point.ksm" );

			rally.transform.position = barracks.GetRallyPoint();

			MeshRenderer mr = rally.AddComponent<MeshRenderer>();

			mr.material = MaterialManager.CreateOpaque(
				AssetManager.GetTexture2D( AssetManager.BUILTIN_ASSET_ID + "Textures/pixel_white", TextureType.Color ),
				null,
				AssetManager.GetTexture2D( AssetManager.BUILTIN_ASSET_ID + "Textures/pixel_white", TextureType.Color ),
				AssetManager.GetTexture2D( AssetManager.BUILTIN_ASSET_ID + "Textures/pixel_black", TextureType.Color ),
				AssetManager.GetTexture2D( AssetManager.BUILTIN_ASSET_ID + "Textures/pixel_black", TextureType.Color )
			);
			barracks.rallyPointGameObject = rally;
		}

		private void CreateRallyButton()
		{
			ActionPanel.instance.CreateButton( "barracks.ap.set_rally", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/rally" ), "Set Rally Point", "Click to set the rally point (units move towards it when trained)...", () =>
			{
				if( Main.mouseInput != null )
				{
					Main.mouseInput.RegisterOnPress( MouseCode.LeftMouseButton, -5, this.Inp_SetRally, true );
					Main.mouseInput.RegisterOnPress( MouseCode.RightMouseButton, -5, this.Inp_CancelRally, true );
				}
				ActionPanel.instance.Clear( "barracks.ap.set_rally" );
				// needs to set the left-mouse action to rally point.
			} );
		}

		private void Inp_SetRally( InputQueue self )
		{
			RaycastHit hitInfo;
			if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo, ObjectLayer.TERRAIN_MASK ) )
			{
				this.rallyPoint = hitInfo.point;
				rallyPointGameObject.transform.position = hitInfo.point;
			}
			Main.mouseInput.ClearOnPress( MouseCode.LeftMouseButton, this.Inp_SetRally ); // One-shot
			Main.mouseInput.ClearOnPress( MouseCode.RightMouseButton, this.Inp_CancelRally ); // Clear complement cancel (can't cancel something that's done).

			// (if still displayed, show the rally button again).
			CreateRallyButton();

			self.StopExecution();
		}

		private void Inp_CancelRally( InputQueue self )
		{
			Main.mouseInput.ClearOnPress( MouseCode.LeftMouseButton, this.Inp_SetRally ); // clear complement set (can't set something that's cancelled).
			Main.mouseInput.ClearOnPress( MouseCode.RightMouseButton, this.Inp_CancelRally ); // clear itself (one-shot).

			CreateRallyButton();

			self.StopExecution(); // stop other inputs (priority).
		}


#if UNITY_EDITOR

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.blue;

			Matrix4x4 toWorld = this.transform.localToWorldMatrix;
			Vector3 spawnPos = toWorld.MultiplyPoint( this.spawnPosition );

			Gizmos.DrawSphere( spawnPos, 0.1f );

			Vector3 rallyPos = this.GetRallyPoint();

			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere( rallyPos, 0.15f );
			Gizmos.DrawLine( spawnPos, rallyPos );
		}
#endif
	}
}