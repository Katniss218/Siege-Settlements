using System.Collections.Generic;

namespace Katniss.ModifierAffectedValues
{
	public class IntM
	{
		public int baseValue;

		List<string> modifierIds;
		List<float> modifiers;

		public float this[string id]
		{
			get
			{
				for( int i = 0; i < this.modifiers.Count; i++ )
				{
					if( this.modifierIds[i] == id )
					{
						return this.modifiers[i];
					}
				}
				throw new System.Exception( "Unknown id '" + id + "'." );
			}
			set
			{
				for( int i = 0; i < this.modifiers.Count; i++ )
				{
					if( this.modifierIds[i] == id )
					{
						this.modifiers[i] = value;
					}
				}
				this.modifiers.Add( value );
				this.modifierIds.Add( id );
			}
		}

		public IntM( int baseValue )
		{
			this.baseValue = baseValue;
			this.modifierIds = new List<string>();
			this.modifiers = new List<float>();
		}

		public int GetModifiedValue()
		{
#warning precalculate the value, since it's not changing randomly. It's well defined by the base value & modifiers. Only recalc when it's necessary.
			if( this.modifiers.Count == 0 )
			{
				return this.baseValue;
			}
			
			float value = this.baseValue;
			for( int i = 0; i < this.modifiers.Count; i++ )
			{
				value = value * this.modifiers[i];
			}
			return (int)value;
		}
		
		public static explicit operator IntM( int right )
		{
			return new IntM( right );
		}
	}
}