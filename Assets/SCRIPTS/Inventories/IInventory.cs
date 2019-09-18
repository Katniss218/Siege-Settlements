using System.Collections.Generic;
using UnityEngine.Events;

namespace SS.Inventories
{
	/// <summary>
	/// Represents any type of inventory.
	/// </summary>
	public interface IInventory
	{
		/// <summary>
		/// Returns true, if the inventory is currently holding a resource (Read Only).
		/// </summary>
		bool isEmpty { get; }

		/// <summary>
		/// Returns the number of slots of the inventory.
		/// </summary>
		int slotCount { get; }

		/// <summary>
		/// Fires after resource is added to the inventory. Fires once per resource id.
		/// </summary>
		_UnityEvent_string_int onAdd { get; }
		/// <summary>
		/// Fires after resource is removed from the inventory. Fires once per resource id.
		/// </summary>
		_UnityEvent_string_int onRemove { get; }

		/// <summary>
		/// Returns every resource in the inventory.
		/// </summary>
		Dictionary<string, int> GetAll();

		/// <summary>
		/// Checks if the inventory contains specified amount of specified resource.
		/// </summary>
		bool Has( string id, int amount );

		/// <summary>
		/// Checks if the inventory has slots that can hold specified resource.
		/// </summary>
		bool CanHold( string id );
		/// <summary>
		/// Checks if the inventory can hold specified amount of specified resource.
		/// </summary>
		bool CanHold( string id, int amount );

		/// <summary>
		/// Adds the specified resource to the inventory. Returns actual the amount of resource added.
		/// </summary>
		/// <param name="id">The id of the resource to add.</param>
		/// <param name="amountMax">The maximum amount of resource that will be added.</param>
		int Add( string id, int amountMax );
		/// <summary>
		/// Removes the specified resource from the inventory. Returns actual the amount of resource removed.
		/// </summary>
		/// <param name="id">The id of the resource to remove.</param>
		/// <param name="amountMax">The maximum amount of resource that will be removed.</param>
		int Remove( string id, int amountMax );

		/// <summary>
		/// Clears the inventory.
		/// </summary>
		void Clear();
	}

	public class _UnityEvent_string_int : UnityEvent<string, int> { }
}