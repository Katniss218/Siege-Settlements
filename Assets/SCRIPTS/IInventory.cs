using SS.ResourceSystem;
using System.Collections.Generic;
using UnityEngine.Events;

namespace SS
{
	public interface IInventory
	{
		bool isEmpty { get; }

		int Get( string id );
		List<ResourceStack> GetAll();
		bool Has( string id, int amount ); // returns true if the inv has more or equal to
		bool CanHold( string id, int amount ); // returns true if the inv can have added to it more or equal to
		int Add( string id, int amountPref ); // returns the actual amt aadded.
		int Remove( string id, int amountPref ); // returns the actual amt removed.

		void Clear();

		_UnityEvent_string_int onAdd { get; }
		_UnityEvent_string_int onRemove { get; }
	}

	public class _UnityEvent_string_int : UnityEvent<string, int> { }
}