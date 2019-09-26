using System.Collections.Generic;
using UnityEngine.Events;

namespace SS.Inventories
{
	/// <summary>
	/// Represents any type of inventory.
	/// </summary>
	public interface IInventory
	{
#error only one IInventory component per object.

		/// <summary>
		/// Returns true, if the inventory is currently holding a resource (Read Only).
		/// </summary>
		bool isEmpty { get; }

		/// <summary>
		/// Returns the number of slots of the inventory.
		/// </summary>
		int slotCount { get; }

		/// <summary>
		/// Returns the amount of resource contained in the inventory.
		/// </summary>
		// Returns 0 if the resource is not present.
		int Get( string id );

		/// <summary>
		/// Returns every resource contained in the inventory.
		/// </summary>
		Dictionary<string, int> GetAll();

		/// <summary>
		/// Returns the max amount of specified resource that the inventory can hold.
		/// </summary>
		// Returns 0 if the resource can't be contained.
		int GetMaxCapacity( string id );
		
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


		/// <summary>
		/// Fires after resource is added to the inventory. Fires once per resource id.
		/// </summary>
		_UnityEvent_string_int onAdd { get; }
		/// <summary>
		/// Fires after resource is removed from the inventory. Fires once per resource id.
		/// </summary>
		_UnityEvent_string_int onRemove { get; }
	}

	public class _UnityEvent_string_int : UnityEvent<string, int> { }
}