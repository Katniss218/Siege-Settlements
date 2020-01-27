using SS.Content;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.ResourceSystem;
using SS.ResourceSystem.Payment;
using SS.Technologies;
using SS.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SS.Objects.Modules
{
	public class ResearchModule : SSModule, ISelectDisplayHandler, IPaymentReceiver
	{
		public const string KFF_TYPEID = "research";

		public UnityEvent onResearchBegin = new UnityEvent();

		public UnityEvent onResearchProgress = new UnityEvent();

		public UnityEvent onResearchEnd = new UnityEvent();

		public UnityEvent onPaymentReceived { get; private set; } = new UnityEvent();

		public TechnologyDefinition[] researchableTechnologies { get; set; }
		
		/// <summary>
		/// Contains the multiplier used as the construction speed.
		/// </summary>
		public float researchSpeed { get; set; } = 1.0f;

		private List<TechnologyDefinition> queuedTechnologies = new List<TechnologyDefinition>();

		private float researchTimeRemaining = 0.0f;

		private Dictionary<string, int> resourcesRemaining = new Dictionary<string, int>();


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

		public void Enqueue( TechnologyDefinition tech )
		{
			// Prevent enqueuing the same technology multiple times.
			for( int i = 0; i < this.queuedTechnologies.Count; i++ )
			{
				if( this.queuedTechnologies[i].id == tech.id )
				{
					return;
				}
			}
		
			this.queuedTechnologies.Add( tech );
			this.RefreshQueue_UI();

			if( this.queuedTechnologies.Count == 1 ) // if the object is the first one, begin training it.
			{
				this.BeginResearching();
			}
		}
		
		public void Dequeue( bool success, int index = 0 )
		{
			// Prevent dequeuing when the queue is empty.
			if( index >= this.queuedTechnologies.Count )
			{
				return;
			}

			if( index == 0 )
			{
				this.EndResearching( success );
			}

			this.queuedTechnologies.RemoveAt( index );
			this.RefreshQueue_UI();

			if( index == 0 && this.queuedTechnologies.Count > 0 ) // if the training can continue.
			{
				this.BeginResearching();
			}
		}
		

		private void BeginResearching()
		{
			if( this.queuedTechnologies.Count == 0 )
			{
				throw new Exception( "You can't start researching technology, when another one is already being researched." );
			}
			
			this.researchTimeRemaining = this.queuedTechnologies[0].researchTime;
			this.resourcesRemaining = new Dictionary<string, int>( this.queuedTechnologies[0].cost );

			this.ResearchBegin_UI();
			this.onResearchBegin?.Invoke();
		}

		private void EndResearching( bool isSuccess )
		{
			if( this.queuedTechnologies.Count == 0 )
			{
				throw new Exception( "You can't end researching, when there's no technology being researched." );
			}

			if( isSuccess )
			{
				LevelDataManager.SetTech( (this.ssObject as IFactionMember).factionId, this.queuedTechnologies[0].id, TechnologyResearchProgress.Researched );
			}

			this.researchTimeRemaining = 0.0f;
			this.resourcesRemaining = null;
			this.ResearchEnd_UI();
			this.onResearchEnd?.Invoke();
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		void Awake()
		{
			this.onPaymentReceived = new UnityEvent();

			LevelDataManager.onTechStateChanged.AddListener( this.OnTechStateChanged );
		}

		void Update()
		{
			if( this.queuedTechnologies.Count == 0 )
			{
				return;
			}

			if( IsPaymentDone() )
			{
				this.researchTimeRemaining -= this.researchSpeed * Time.deltaTime;
				if( this.researchTimeRemaining <= 0 )
				{
					this.Dequeue( true );
				}
				else
				{
					this.ResearchProgress_UI();
					this.onResearchProgress?.Invoke();
				}
			}
		}

		void OnDestroy()
		{
			LevelDataManager.onTechStateChanged.RemoveListener( this.OnTechStateChanged );
		}
		




		/// <summary>
		/// Creates a new BarracksModuleSaveState from a GameObject.
		/// </summary>
		/// <param name="unit">The GameObject to extract the save state from.</param>
		public override ModuleData GetData()
		{
			ResearchModuleData data = new ResearchModuleData();

			data.resourcesRemaining = this.resourcesRemaining;
			if( this.queuedTechnologies == null )
			{
				data.queuedTechnologies = null;
			}
			else
			{
				data.queuedTechnologies = new string[this.queuedTechnologies.Count];
				for( int i = 0; i < this.queuedTechnologies.Count; i++ )
				{
					data.queuedTechnologies[i] = this.queuedTechnologies[i].id;
				}
			}
			data.researchTimeRemaining = this.researchTimeRemaining;

			return data;
		}
		
		public override void SetData( ModuleData _data )
		{
			ResearchModuleData data = ValidateDataType<ResearchModuleData>( _data );

			// ------          DATA

			this.resourcesRemaining = data.resourcesRemaining;

			if( data.queuedTechnologies != null )
			{
				for( int i = 0; i < data.queuedTechnologies.Length; i++ )
				{
					this.queuedTechnologies.Add( DefinitionManager.GetTechnology( data.queuedTechnologies[i] ) );
				}
			}
			this.researchTimeRemaining = data.researchTimeRemaining;
		}


		//
		//
		//




		private string Status()
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
			GameObject[] gridElements = new GameObject[this.researchableTechnologies.Length];
			// Add every available technology to the list.
			for( int i = 0; i < this.researchableTechnologies.Length; i++ )
			{
				TechnologyDefinition techDef = this.researchableTechnologies[i];
				// If it can be researched, add clickable button, otherwise add unclickable button that represents tech already researched/locked.
				if( LevelDataManager.factionData[(this.ssObject as IFactionMember).factionId].GetTech( techDef.id ) == TechnologyResearchProgress.Available )
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), techDef.icon, () =>
					{
						this.Enqueue( techDef );
					} );
				}
				else
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), techDef.icon, null );
				}
				ToolTipUIHandler toolTipUIhandler = gridElements[i].AddComponent<ToolTipUIHandler>();
				toolTipUIhandler.constructToolTip = () =>
				{
					ToolTip.Create( 450.0f, techDef.displayName );

					foreach( var kvp in techDef.cost )
					{
						ResourceDefinition resourceDef = DefinitionManager.GetResource( kvp.Key );
						ToolTip.AddText( resourceDef.icon, kvp.Value.ToString() );
					}
				};
			}
			// Create the actual UI.
			GameObject listGO = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 300.0f, 5.0f ), new Vector2( -330.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
			SelectionPanel.instance.obj.RegisterElement( "research.list", listGO.transform );
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
			foreach( TechnologyDefinition unitDef in this.queuedTechnologies )
			{
				int j = i;
				GameObject go = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon, () =>
				{
					this.Dequeue( false, j );
				} );
				go.transform.SetParent( t );
				i++;
			}

			SelectionPanel.instance.obj.RegisterElement( "research.queue", queue.transform );
		}

		public void RefreshQueue_UI()
		{
			if( (!Selection.IsDisplayedModule( this )) || (!this.ssObject.IsDisplaySafe()) )
			{
				return;
			}

			SelectionPanel.instance.obj.TryClearElement( "research.queue" );
			this.CreateQueueUI();
		}


		private void OnTechStateChanged( int factionId, string id, TechnologyResearchProgress newProgress )
		{
			if( (!Selection.IsDisplayedModule( this )) || (!this.ssObject.IsDisplaySafe()) )
			{
				return;
			}
			
			SelectionPanel.instance.obj.TryClearElement( "research.list" );
			this.ShowList();
		}

		private void PaymentReceived_UI()
		{
			if( (!Selection.IsDisplayedModule( this )) || (!this.ssObject.IsDisplaySafe()) )
			{
				return;
			}

			Transform statusUI = SelectionPanel.instance.obj.GetElement( "research.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Waiting for resources ('" + this.queuedTechnologies[0].displayName + "'): " + Status() );
			}
		}

		private void ResearchBegin_UI()
		{
			if( (!Selection.IsDisplayedModule( this )) || (!this.ssObject.IsDisplaySafe()) )
			{
				return;
			}

			if( !this.IsPaymentDone() )
			{
				Transform statusUI = SelectionPanel.instance.obj.GetElement( "research.status" );
				if( statusUI != null )
				{
					UIUtils.EditText( statusUI.gameObject, "Waiting for resources ('" + this.queuedTechnologies[0].displayName + "'): " + Status() );
				}

				// clear if the begin was caused by decreasing queue.
				ActionPanel.instance.Clear( "research.ap.cancel" );
				DisplayCancelButton();
			}
		}

		private void ResearchProgress_UI()
		{
			if( (!Selection.IsDisplayedModule( this )) || (!this.ssObject.IsDisplaySafe()) )
			{
				return;
			}

			Transform statusUI = SelectionPanel.instance.obj.GetElement( "research.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Researching...: '" + this.queuedTechnologies[0].displayName + "' - " + (int)this.researchTimeRemaining + " s." );
			}
		}

		private void ResearchEnd_UI()
		{
			if( (!Selection.IsDisplayedModule( this )) || (!this.ssObject.IsDisplaySafe()) )
			{
				return;
			}

			Transform statusUI = SelectionPanel.instance.obj.GetElement( "research.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Select tech to research..." );
			}

			SelectionPanel.instance.obj.TryClearElement( "research.queue" );
			this.CreateQueueUI();

			if( this.queuedTechnologies.Count == 0 )
			{
				ActionPanel.instance.Clear( "research.ap.cancel" );
			}
		}


		private void DisplayCancelButton()
		{
			ActionPanel.instance.CreateButton( "research.ap.cancel", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/cancel" ), "Cancel", "Click to cancel research...", () =>
			{
				this.Dequeue( false );
			} );
		}

		private static GenericUIData GetStatusPos()
		{
			return new GenericUIData( new Vector2( 300.0f, 0.0f ), new Vector2( 200.0f, 25.0f ), Vector2.up, Vector2.up, Vector2.up );
		}

		public void OnDisplay()
		{
			if( !this.ssObject.IsDisplaySafe() )
			{
				return;
			}

			// If it's not usable - return, don't research anything.
			if( this.ssObject is ISSObjectUsableUnusable && !(this.ssObject as ISSObjectUsableUnusable).isUsable )
			{
				return;
			}
			
			this.ShowList();
			this.CreateQueueUI();

			if( this.IsPaymentDone() )
			{
				if( this.queuedTechnologies.Count == 0 )
				{
					GameObject statusGO = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, GetStatusPos(), "Select tech to research..." );
					SelectionPanel.instance.obj.RegisterElement( "research.status", statusGO.transform );
				}
				else
				{
					GameObject statusGO = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, GetStatusPos(), "Researching...: '" + this.queuedTechnologies[0].displayName + "' - " + (int)this.researchTimeRemaining + " s." );
					SelectionPanel.instance.obj.RegisterElement( "research.status", statusGO.transform );
					this.DisplayCancelButton();
				}
			}
			else
			{
				GameObject statusGO = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, GetStatusPos(), "Waiting for resources ('" + this.queuedTechnologies[0].displayName + "'): " + Status() );
				SelectionPanel.instance.obj.RegisterElement( "research.status", statusGO.transform );
				this.DisplayCancelButton();
			}
		}

		public void OnHide()
		{

		}
	}
}