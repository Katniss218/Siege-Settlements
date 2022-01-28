using SS.Content;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Objects.Modules;
using SS.Objects.SubObjects;
using SS.ResourceSystem.Payment;
using SS.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SS.Objects.Buildings
{
	/// <summary>
	/// Represents a building that's being constructed.
	/// </summary>
	[RequireComponent( typeof( Building ) )]
	public class ConstructionSite : MonoBehaviour, IPaymentReceiver
	{
		// if a building costs 10 resource, the cost of 10 is to bring it from the starting health to max.

		private class ResourceInfo
		{
			public int initialAmount { get; set; }
			public float remaining { get; set; }
			public float healthToResource { get; set; }
		}

		// Contains info about resources remaining.
		private Dictionary<string, ResourceInfo> resourcesRemaining = null;

		
		private Building building;
		public SSObject ssObject
		{
			get { return this.building; }
		}

		public UnityEvent onPaymentReceived { get; private set; }
		
		/// <summary>
		/// Checks if the construction (payment) has finished. Construction is finished when there are no more resources needed.
		/// </summary>
		private bool IsDone()
		{
			foreach( var remaining in resourcesRemaining )
			{
				int remainingCeiled = Mathf.CeilToInt( remaining.Value.remaining );

				if( remainingCeiled != 0 )
				{
					return false;
				}
			}

			return true;
		}

		public void ReceivePayment( string id, int amount )
		{
			foreach( var remaining in this.resourcesRemaining )
			{
				// Skip to the matching resource.
				if( remaining.Key != id )
				{
					continue;
				}

				int remainingCeiled = Mathf.CeilToInt( remaining.Value.remaining );
				if( remainingCeiled == 0 )
				{
					throw new InvalidOperationException( "Received resource wasn't wanted." );
				}

				// Received more than was needed (invalid behavior).
				if( remainingCeiled < amount )
				{
					throw new InvalidOperationException( $"ReceivePayment amount of '{id}' ({amount}) was more than the required amount ({remainingCeiled})." );
				}

				remaining.Value.remaining -= amount;

				// VFX, SFX

				Main.particleSystem.transform.position = this.gameObject.transform.position + new Vector3( 0, 0.125f, 0 );
				ParticleSystem.ShapeModule shape = Main.particleSystem.GetComponent<ParticleSystem>().shape;

				BoxCollider col = this.GetComponent<BoxCollider>();
				shape.scale = new Vector3( col.size.x, 0.25f, col.size.z );
				shape.position = Vector3.zero;
				Main.particleSystem.GetComponent<ParticleSystem>().Emit( 36 );

				AudioManager.PlaySound( this.building.buildSoundEffect, this.transform.position );
				
				if( this.IsDone() )
				{
					// Remove onHealthChange_whenConstructing, so the damageable doesn't call listener, that doesn't exist (cause the construction ended).
					this.building.constructionSite = null;
					this.building.isUsable = true;
					this.building.onHealthChange.RemoveListener( this.OnHealthChange );
					this.building.onFactionChange.RemoveListener( this.OnFactionChange );

					UpdateYOffset( this.building, 0.0f );
					
					this.building.health = this.building.healthMax;

					Object.Destroy( this.transform.Find( "construction_site_graphics" ).gameObject );
					Object.DestroyImmediate( this ); // Use 'DestroyImmediate()', so that the redraw doesn't detect the construction site and say that it's not usable, that'd still present if we used 'Destroy()'.

					// Re-Display to update.
					if( Selection.IsDisplayed( this.building ) )
					{
						Selection.StopDisplaying();
						Selection.DisplayObject( this.building );
					}
				}

				float healthFromStartToEnd = this.building.healthMax * (1 - Building.STARTING_HEALTH_PERCENT);

				float resourceStartToEnd = healthFromStartToEnd / remaining.Value.initialAmount;

				float healAmt = resourceStartToEnd * remaining.Value.healthToResource * amount;

				// Clamp overhealing (due to rounding up of the actual resource amount wanted/received).
				// Otherwise, heal it normally.
				if( this.building.health + healAmt > this.building.healthMax )
				{
					this.building.health = this.building.healthMax;
				}
				else
				{
					this.building.health += healAmt;
				}


				this.onPaymentReceived?.Invoke();
				this.UpdateStatus_UI();

				return;
			}
		}

		private static void UpdateYOffset( SSObject ssObject, float value )
		{
			MeshSubObject[] meshes = ssObject.GetSubObjects<MeshSubObject>();
			MeshPredicatedSubObject[] meshesPredicated = ssObject.GetSubObjects<MeshPredicatedSubObject>();

			foreach( var mesh in meshes )
			{
				mesh.GetMaterial().SetFloat( "_YOffset", value );
			}
			foreach( var mesh in meshesPredicated )
			{
				mesh.GetMaterial().SetFloat( "_YOffset", value );
			}
		}

		public Dictionary<string, int> GetWantedResources()
		{
			Dictionary<string, int> wanted = new Dictionary<string, int>();

			foreach( var remaining in this.resourcesRemaining )
			{
				int remainingCeiled = Mathf.CeilToInt( remaining.Value.remaining );
				if( remainingCeiled != 0 )
				{
					wanted.Add( remaining.Key, remainingCeiled );
				}
			}

			return wanted;
		}

		/// <summary>
		/// Assigns a cost (in resources) to fully construct the building. Resets any progression in resources.
		/// </summary>
		private void SetRequiredResources( Dictionary<string, int> requiredResources )
		{
			this.resourcesRemaining = new Dictionary<string, ResourceInfo>( requiredResources.Count );

			float totalResourceAmount = 0;

			foreach( var required in requiredResources )
			{
				ResourceInfo requiredResInfo = new ResourceInfo()
				{
					initialAmount = required.Value,
					remaining = required.Value
				};

				this.resourcesRemaining.Add( required.Key, requiredResInfo );

				totalResourceAmount += required.Value;
			}

			// Calculate how much each resource contributes to the total.
			foreach( var remaining in this.resourcesRemaining )
			{
				remaining.Value.healthToResource = remaining.Value.remaining / totalResourceAmount;
			}
		}

		void Awake()
		{
			this.onPaymentReceived = new UnityEvent();
		}
		
		/// <summary>
		/// Creates a new ConstructionSiteData from a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to extract the save state from. Must be a building, must be under construction.</param>
		public ConstructionSiteData GetSaveState()
		{
			ConstructionSiteData data = new ConstructionSiteData();

			data.resourcesRemaining = new Dictionary<string, float>();
			foreach( var remaining in this.resourcesRemaining )
			{
				data.resourcesRemaining.Add( remaining.Key, remaining.Value.remaining );
			}

			return data;
		}

		/// <summary>
		/// Starts the construction / repair of the specified building.
		/// </summary>
		public static void BeginConstructionOrRepair( Building building, ConstructionSiteData data )
		{
			if( !Building.CanStartRepair( building ) )
			{
				throw new Exception($"{building.displayName} - Can't start repair." );
			}

			ConstructionSite constructionSite = building.gameObject.AddComponent<ConstructionSite>();
			constructionSite.building = building;
			constructionSite.SetRequiredResources( building.StartToEndConstructionCost );
			
			if( data.resourcesRemaining == null )
			{
				// Calculate remaining resources from the current health.

				float deltaHP = building.health - building.healthMax;
				foreach( var remaining in constructionSite.resourcesRemaining )
				{
					float healthFromStartToFinish = building.healthMax * (1 - Building.STARTING_HEALTH_PERCENT);

					float resourceFromStartToFinish = remaining.Value.initialAmount / healthFromStartToFinish;

					float newRemaining = resourceFromStartToFinish * -deltaHP;

					remaining.Value.remaining = newRemaining;

					Debug.Log( $"BeginConstructionOrRepair: {remaining.Key}, {newRemaining}" );
				}
			}
			else
			{
				foreach( var remaining in data.resourcesRemaining )
				{
					constructionSite.resourcesRemaining[remaining.Key].remaining = remaining.Value;
				}
			}

			// Can't be inside building that's under construction.
			foreach( var interior in building.GetModules<InteriorModule>() )
			{
				interior.ExitAll();
			}

			building.constructionSite = constructionSite;
			building.isUsable = false;
			building.onHealthChange.AddListener( constructionSite.OnHealthChange );
			building.onFactionChange.AddListener( constructionSite.OnFactionChange );

			GameObject constructionSiteGfx = CreateConstructionSiteGraphics( building.gameObject, building );
			
			UpdateYOffset( constructionSite.building, Mathf.Lerp( -constructionSite.building.size.y, 0.0f, building.healthPercent ) );

			// Re-Display to update.
			if( Selection.IsDisplayed( building ) )
			{
				Selection.StopDisplaying();
				Selection.DisplayObject( building );
			}
		}

		private void OnHealthChange( float deltaHP )
		{
			this.UpdateStatus_UI();

			UpdateYOffset( this.building, Mathf.Lerp( -this.building.size.y, 0.0f, building.healthPercent ) );
			
			// if the building took damage - recalculate the new cost.
			if( deltaHP < 0 )
			{
				foreach( var resource in this.resourcesRemaining )
				{
					float healthFromStartToEnd = building.healthMax * (1 - Building.STARTING_HEALTH_PERCENT);

					float resourcePerOneHealth = resource.Value.initialAmount / healthFromStartToEnd;

					float resAmountAdded = resourcePerOneHealth * -deltaHP;

					resource.Value.remaining += resAmountAdded;
				}
			}
		}

		private void OnFactionChange( int fromFac, int toFac )
		{
			Transform constr_gfx = this.transform.Find( "construction_site_graphics" );
			Color facColor = LevelDataManager.factions[this.building.factionId].color;

			for( int i = 0; i < constr_gfx.childCount; i++ )
			{
				MeshRenderer meshRenderer = constr_gfx.GetChild( i ).GetComponent<MeshRenderer>();

				meshRenderer.material.SetColor( "_FactionColor", facColor );
			}
		}


		//
		//	UI integration
		//

		
		private void ConstructionComplete_UI()
		{
			if( (!Selection.IsDisplayed( this.building )) || (!this.building.IsDisplaySafe()) )
			{
				return;
			}

			SelectionPanel.instance.obj.TryClearElement( "building.construction_status" );
		}

		private void UpdateStatus_UI()
		{
			if( (!Selection.IsDisplayed( this.building )) || (!this.building.IsDisplaySafe()) )
			{
				return;
			}

			Transform statusUI = SelectionPanel.instance.obj.GetElement( "building.construction_status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Waiting for resources... " + ResourceUtils.ToResourceString( this.GetWantedResources() ) );
			}
		}

		public void Display()
		{
			GameObject status = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 25.0f, -50.0f ), new Vector2( 200.0f, 25.0f ), Vector2.up, Vector2.up, Vector2.up ), "Waiting for resources... " + ResourceUtils.ToResourceString( this.GetWantedResources() ) );
			SelectionPanel.instance.obj.RegisterElement( "building.construction_status", status.transform );
		}


		//
		//
		//


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

			// Corner

			GameObject corner00 = new GameObject( "c00" );
			corner00.transform.SetParent( constructionSiteGfx.transform );
			corner00.transform.localPosition = new Vector3( 0, 0, 0 );
			corner00.transform.localRotation = Quaternion.identity;

			MeshFilter meshFilter = corner00.AddComponent<MeshFilter>();
			meshFilter.mesh = corner;

			MeshRenderer meshRenderer = corner00.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColored( color, albedoC, normalC, null, null, null );

			// Corner

			GameObject corner01 = new GameObject( "c01" );
			corner01.transform.SetParent( constructionSiteGfx.transform );
			corner01.transform.localPosition = new Vector3( 0, 0, collider.size.z );
			corner01.transform.localRotation = Quaternion.identity;

			meshFilter = corner01.AddComponent<MeshFilter>();
			meshFilter.mesh = corner;

			meshRenderer = corner01.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColored( color, albedoC, normalC, null, null, null );

			// Corner

			GameObject corner10 = new GameObject( "c10" );
			corner10.transform.SetParent( constructionSiteGfx.transform );
			corner10.transform.localPosition = new Vector3( collider.size.x, 0, 0 );
			corner10.transform.localRotation = Quaternion.identity;

			meshFilter = corner10.AddComponent<MeshFilter>();
			meshFilter.mesh = corner;

			meshRenderer = corner10.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColored( color, albedoC, normalC, null, null, null );

			// Corner

			GameObject corner11 = new GameObject( "c11" );
			corner11.transform.SetParent( constructionSiteGfx.transform );
			corner11.transform.localPosition = new Vector3( collider.size.x, 0, collider.size.z );
			corner11.transform.localRotation = Quaternion.identity;

			meshFilter = corner11.AddComponent<MeshFilter>();
			meshFilter.mesh = corner;

			meshRenderer = corner11.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColored( color, albedoC, normalC, null, null, null );

			// X -> 2 rows

			for( int i = 0; i < numX; i++ )
			{
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

			// Y -> 2 rows

			for( int i = 0; i < numZ; i++ )
			{
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
	}
}