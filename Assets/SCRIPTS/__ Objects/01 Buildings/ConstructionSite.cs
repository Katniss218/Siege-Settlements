using System;
using System.Collections.Generic;
using UnityEngine;
using SS.Content;
using SS.ResourceSystem.Payment;
using Object = UnityEngine.Object;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Diplomacy;
using SS.UI;
using System.Text;
using SS.ResourceSystem;
using UnityEngine.Events;

namespace SS.Objects.Buildings
{
	/// <summary>
	/// Represents a building that's being constructed.
	/// </summary>
	[RequireComponent( typeof( Building ) )]
	public class ConstructionSite : MonoBehaviour, IPaymentReceiver
	{
		public class ResourceInfo
		{
			public int initialResource { get; set; }
			public float remaining { get; set; }
			public float healthToResource { get; set; }
		}

		private Dictionary<string, ResourceInfo> resourceInfo;

		public UnityEvent onPaymentReceived { get; private set; }
		
		private Building building;
		private MeshRenderer[] renderers;


		float buildingHeight = 0.0f;

		private bool IsDone()
		{
			foreach( var kvp in resourceInfo )
			{
				int roundedAmount = RoundExact( kvp.Value.remaining );

				if( roundedAmount != 0 )
				{
					return false;
				}
			}
			return true;
		}

		public void ReceivePayment( string id, int amount )
		{
			foreach( var kvp in this.resourceInfo )
			{
				// Skip to the matching resource.
				if( kvp.Key != id )
				{
					continue;
				}

				int roundedRemaining = RoundExact( kvp.Value.remaining );
				if( roundedRemaining == 0 )
				{
					throw new Exception( "Received resource wasn't wanted." );
				}
				// Received more than was needed (invalid behavior).
				if( roundedRemaining < amount )
				{
					throw new Exception( "Received amount of '" + id + "' (" + amount + ") was more than the required amount (" + roundedRemaining + ")." );
				}


				kvp.Value.remaining -= amount;
				if( kvp.Value.remaining < 0 )
				{
					kvp.Value.remaining = 0;
				}

				Main.particleSystem.transform.position = this.gameObject.transform.position + new Vector3( 0, 0.125f, 0 );
				ParticleSystem.ShapeModule shape = Main.particleSystem.GetComponent<ParticleSystem>().shape;

				BoxCollider col = this.GetComponent<BoxCollider>();
				shape.scale = new Vector3( col.size.x, 0.25f, col.size.z );
				shape.position = Vector3.zero;
				Main.particleSystem.GetComponent<ParticleSystem>().Emit( 36 );

				AudioManager.PlaySound( this.building.buildSoundEffect );
				

				bool isDone = this.IsDone();
				if( isDone )
				{
					// Remove onHealthChange_whenConstructing, so the damageable doesn't call listener, that doesn't exist (cause the construction ended).
					this.building.onHealthChange.RemoveListener( this.OnHealthChange );
					this.building.onFactionChange.RemoveListener( this.OnFactionChange );

					for( int i = 0; i < this.renderers.Length; i++ )
					{
						this.renderers[i].material.SetFloat( "_YOffset", 0.0f );
					}
					this.building.health = this.building.healthMax.value;

					Object.Destroy( this.transform.Find( "construction_site_graphics" ).gameObject );
					Object.DestroyImmediate( this ); // Use 'DestroyImmediate()', so that the redraw doesn't detect the construction site, that'd still present if we used 'Destroy()'.

					this.ConstructionComplete_UI();
				}

				float healAmt = ((this.building.healthMax.value * (1 - 0.1f)) / kvp.Value.initialResource) * kvp.Value.healthToResource * amount;

				// If it would be healed above the max health (due to rounding up the actual resource amount received), heal it just to the max health.
				// Otherwise, heal it normally.
				if( this.building.healthMax.value + healAmt > this.building.healthMax.value )
				{
					this.building.health = this.building.healthMax.value;
				}
				else
				{
					this.building.health += healAmt;
				}


				this.onPaymentReceived?.Invoke();
				this.paymentReceived_UI();

				return;
			}
		}

		private void ConstructionComplete_UI()
		{
			if( !Selection.IsDisplayed( this.building ) )
			{
				return;
			}
			
			SelectionPanel.instance.obj.TryClearElement( "building.construction_status" );
		}

		internal string GetStatusString()
		{
			StringBuilder sb = new StringBuilder();
			
			foreach( var kvp in this.GetWantedResources() )
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

		private void paymentReceived_UI()
		{
			Transform statusUI = SelectionPanel.instance.obj.GetElement( "building.construction_status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Waiting for resources... " + this.GetStatusString() );
			}
		}

		public Dictionary<string, int> GetWantedResources()
		{
			Dictionary<string, int> ret = new Dictionary<string, int>();

			foreach( var kvp in this.resourceInfo )
			{
				int amtRounded = RoundExact( kvp.Value.remaining );
				if( amtRounded != 0 )
				{
					ret.Add( kvp.Key, amtRounded );
				}
			}
			return ret;
		}

		/// <summary>
		/// Assigns a cost (in resources) to fully construct the building. Resets any progression in resources.
		/// </summary>
		public void SetRequiredResources( Dictionary<string, int> requiredResources )
		{
			this.resourceInfo = new Dictionary<string, ResourceInfo>( requiredResources.Count );

			float totalResourcesNeeded = 0;
			//int i = 0;
			foreach( var id in requiredResources.Keys )
			{
				int amount = requiredResources[id];
				this.resourceInfo.Add( id, new ResourceInfo() { initialResource = amount, remaining = amount } );

				totalResourcesNeeded += amount;

				//i++;
			}

			// Once we have our total, calculate how much each resource contributes to the total.
			foreach( var kvp in this.resourceInfo )
			{
				kvp.Value.healthToResource = kvp.Value.remaining / totalResourcesNeeded;
			}
		}

		void Awake()
		{
			this.onPaymentReceived = new UnityEvent();
		}

		// Rounds down when the decimal is <=0.5, rounds up when >0.5
		private static int RoundExact( float remainingResAmt )
		{
			return Mathf.CeilToInt( remainingResAmt );
			int floored = Mathf.FloorToInt( remainingResAmt );
			if( remainingResAmt - floored <= 0.5f )
			{
				return floored;
			}
			return floored + 1;
		}


		/// <summary>
		/// Creates a new ConstructionSiteData from a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to extract the save state from. Must be a building, must be under construction.</param>
		public ConstructionSiteData GetSaveState()
		{
			ConstructionSiteData data = new ConstructionSiteData();

			data.resourcesRemaining = new Dictionary<string, float>();
			foreach( var kvp in this.resourceInfo )
			{
				data.resourcesRemaining.Add( kvp.Key, kvp.Value.remaining );
			}

			return data;
		}

		private static GameObject CreateConstructionSiteGraphics( GameObject gameObject, IFactionMember fac )
		{
			BoxCollider collider = gameObject.GetComponent<BoxCollider>();
			
			Color color = fac != null ? LevelDataManager.factions[fac.factionId].color : Color.gray;

			int numX = Mathf.FloorToInt( collider.size.x * 2.0f );
			int numZ = Mathf.FloorToInt( collider.size.z * 2.0f );

			float spacingX = collider.size.x / (numX + 1);
			float spacingZ = collider.size.z / (numZ + 1);

			Mesh corner = AssetManager.GetMesh( AssetManager.EXTERN_ASSET_ID + "Models/ConstructionSites/Corner.ksm" );
			Mesh segment = AssetManager.GetMesh( AssetManager.EXTERN_ASSET_ID + "Models/ConstructionSites/Segment.ksm" );

			Texture2D albedoS = AssetManager.GetTexture2D( AssetManager.EXTERN_ASSET_ID + "Textures/ConstructionSites/segment_albedo.png", TextureType.Color );
			Texture2D normalS = AssetManager.GetTexture2D( AssetManager.EXTERN_ASSET_ID + "Textures/ConstructionSites/segment_normal.png", TextureType.Normal );
			Texture2D albedoC = AssetManager.GetTexture2D( AssetManager.EXTERN_ASSET_ID + "Textures/ConstructionSites/corner_albedo.png", TextureType.Color );
			Texture2D normalC = AssetManager.GetTexture2D( AssetManager.EXTERN_ASSET_ID + "Textures/ConstructionSites/corner_normal.png", TextureType.Normal );

			GameObject constructionSiteGfx = new GameObject( "construction_site_graphics" );
			constructionSiteGfx.transform.SetParent( gameObject.transform );
			constructionSiteGfx.transform.localPosition = new Vector3( -collider.size.x / 2f, 0, -collider.size.z / 2f );
			constructionSiteGfx.transform.localRotation = Quaternion.identity;

			GameObject corner00 = new GameObject( "c00" );
			corner00.transform.SetParent( constructionSiteGfx.transform );
			corner00.transform.localPosition = new Vector3( 0, 0, 0 );
			corner00.transform.localRotation = Quaternion.identity;

			MeshFilter meshFilter = corner00.AddComponent<MeshFilter>();
			meshFilter.mesh = corner;

			MeshRenderer meshRenderer = corner00.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColored( color, albedoC, normalC, null, null, null );

			GameObject corner01 = new GameObject( "c01" );
			corner01.transform.SetParent( constructionSiteGfx.transform );
			corner01.transform.localPosition = new Vector3( 0, 0, collider.size.z );
			corner01.transform.localRotation = Quaternion.identity;

			meshFilter = corner01.AddComponent<MeshFilter>();
			meshFilter.mesh = corner;

			meshRenderer = corner01.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColored( color, albedoC, normalC, null, null, null );

			GameObject corner10 = new GameObject( "c10" );
			corner10.transform.SetParent( constructionSiteGfx.transform );
			corner10.transform.localPosition = new Vector3( collider.size.x, 0, 0 );
			corner10.transform.localRotation = Quaternion.identity;

			meshFilter = corner10.AddComponent<MeshFilter>();
			meshFilter.mesh = corner;

			meshRenderer = corner10.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColored( color, albedoC, normalC, null, null, null );

			GameObject corner11 = new GameObject( "c11" );
			corner11.transform.SetParent( constructionSiteGfx.transform );
			corner11.transform.localPosition = new Vector3( collider.size.x, 0, collider.size.z );
			corner11.transform.localRotation = Quaternion.identity;

			meshFilter = corner11.AddComponent<MeshFilter>();
			meshFilter.mesh = corner;

			meshRenderer = corner11.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColored( color, albedoC, normalC, null, null, null );


			for( int i = 0; i < numX; i++ )
			{
				// 2 rows


				GameObject line1 = new GameObject( "X0-" + i );
				line1.transform.SetParent( constructionSiteGfx.transform );
				line1.transform.localPosition = new Vector3( (i * spacingX) + spacingX, 0, 0 );
				line1.transform.localRotation = Quaternion.identity;

				meshFilter = line1.AddComponent<MeshFilter>();
				meshFilter.mesh = segment;

				meshRenderer = line1.AddComponent<MeshRenderer>();
				meshRenderer.material = MaterialManager.CreateColored( color, albedoS, normalS, null, null, null );

				GameObject line2 = new GameObject( "X1-" + i );
				line2.transform.SetParent( constructionSiteGfx.transform );
				line2.transform.localPosition = new Vector3( (i * spacingX) + spacingX, 0, collider.size.z );
				line2.transform.localRotation = Quaternion.identity;

				meshFilter = line2.AddComponent<MeshFilter>();
				meshFilter.mesh = segment;

				meshRenderer = line2.AddComponent<MeshRenderer>();
				meshRenderer.material = MaterialManager.CreateColored( color, albedoS, normalS, null, null, null );
			}

			for( int i = 0; i < numZ; i++ )
			{
				// 2 rows


				GameObject line1 = new GameObject( "Z0-" + i );
				line1.transform.SetParent( constructionSiteGfx.transform );
				line1.transform.localPosition = new Vector3( 0, 0, (i * spacingZ) + spacingZ );
				line1.transform.localRotation = Quaternion.identity;

				meshFilter = line1.AddComponent<MeshFilter>();
				meshFilter.mesh = segment;

				meshRenderer = line1.AddComponent<MeshRenderer>();
				meshRenderer.material = MaterialManager.CreateColored( color, albedoS, normalS, null, null, null );

				GameObject line2 = new GameObject( "Z1-" + i );
				line2.transform.SetParent( constructionSiteGfx.transform );
				line2.transform.localPosition = new Vector3( collider.size.x, 0, (i * spacingZ) + spacingZ );
				line2.transform.localRotation = Quaternion.identity;

				meshFilter = line2.AddComponent<MeshFilter>();
				meshFilter.mesh = segment;

				meshRenderer = line2.AddComponent<MeshRenderer>();
				meshRenderer.material = MaterialManager.CreateColored( color, albedoS, normalS, null, null, null );
			}

			return constructionSiteGfx;
		}

		/// <summary>
		/// Starts the construction / repair of the specified building.
		/// </summary>
		public static void BeginConstructionOrRepair( Building building, ConstructionSiteData data )
		{
			if( !Building.IsRepairable( building ) )
			{
				throw new Exception( building.displayName + " - Building is not repairable." );
			}
			
			ConstructionSite constructionSite = building.gameObject.AddComponent<ConstructionSite>();
			constructionSite.building = building;
			constructionSite.SetRequiredResources( building.StartToEndConstructionCost );
			constructionSite.renderers = building.GetComponentsInChildren<MeshRenderer>();

			constructionSite.buildingHeight = building.GetComponent<BoxCollider>().size.y;

			// If no data about remaining resources is present - calculate them from the current health.
			if( data.resourcesRemaining == null )
			{
				float deltaHP = building.health - building.healthMax.value;
				foreach( var kvp in constructionSite.resourceInfo )
				{
					float resAmt = (kvp.Value.initialResource / (building.healthMax.baseValue * (1 - 0.1f))) * -deltaHP;
					Debug.Log( kvp.Key + ", " + resAmt );
					kvp.Value.remaining = resAmt;
				}
			}
			else
			{
				foreach( var kvp in data.resourcesRemaining )
				{
					constructionSite.resourceInfo[kvp.Key].remaining = kvp.Value;
				}
			}


			building.onHealthChange.AddListener( constructionSite.OnHealthChange );
			building.onFactionChange.AddListener( constructionSite.OnFactionChange );

			GameObject constructionSiteGfx = CreateConstructionSiteGraphics( building.gameObject, building );


			// When the construction starts, set the _Progress attrribute of the material to the current health percent (to make the building appear as being constructed).
			for( int i = 0; i < constructionSite.renderers.Length; i++ )
			{
				constructionSite.renderers[i].material.SetFloat( "_YOffset", Mathf.Lerp( -constructionSite.buildingHeight, 0.0f, building.healthPercent ) );
			}
		}


		private void OnHealthChange( float deltaHP )
		{
			for( int i = 0; i < this.renderers.Length; i++ )
			{
				this.renderers[i].material.SetFloat( "_YOffset", Mathf.Lerp( -this.buildingHeight, 0.0f, building.healthPercent ) );
			}
			if( deltaHP < 0 )
			{
				foreach( var kvp in this.resourceInfo )
				{
					float resAmt = (kvp.Value.initialResource / (building.healthMax.baseValue * (1 - 0.1f))) * -deltaHP;

					kvp.Value.remaining += resAmt;
				}
			}
		}

		private void OnFactionChange()
		{
			Transform constr_gfx = this.transform.Find( "construction_site_graphics" );
			Color facColor = LevelDataManager.factions[this.building.factionId].color;

			for( int i = 0; i < constr_gfx.childCount; i++ )
			{
				MeshRenderer meshRenderer = constr_gfx.GetChild( i ).GetComponent<MeshRenderer>();

				meshRenderer.material.SetColor( "_FactionColor", facColor );
			}
		}
	}
}