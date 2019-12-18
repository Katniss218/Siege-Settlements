using System;
using System.Collections.Generic;

namespace Katniss.ModifierAffectedValues
{
	public class IntM
	{
		private int __baseValue;
		public int baseValue
		{
			get
			{
				return this.__baseValue;
			}
			set
			{
				this.__baseValue = value;
				this.CalculateModifiedValue();
			}
		}

		public Action onAnyChangeCallback;

		private List<Modifier> modifiers;

		public int value { get; private set; }


		public float this[string id]
		{
			get
			{
				for( int i = 0; i < this.modifiers.Count; i++ )
				{
					if( this.modifiers[i].id == id )
					{
						return this.modifiers[i].value;
					}
				}
				throw new Exception( "Unknown id '" + id + "'." );
			}
			set
			{
				for( int i = 0; i < this.modifiers.Count; i++ )
				{
					if( this.modifiers[i].id == id )
					{
						this.modifiers[i].Set( value );
						this.CalculateModifiedValue();
						return;
					}
				}
				this.modifiers.Add( new Modifier( id, value ) );
				this.CalculateModifiedValue();
			}
		}



		public IntM()
		{
			this.baseValue = 0;
			this.modifiers = new List<Modifier>();
			this.value = 0;
		}



		public Modifier[] GetModifiers()
		{
			return this.modifiers.ToArray();
		}

		private void CalculateModifiedValue()
		{
			if( this.modifiers == null || this.modifiers.Count == 0 )
			{
				this.value = this.baseValue;
				this.onAnyChangeCallback?.Invoke();
				return;
			}

			float value = this.baseValue;
			for( int i = 0; i < this.modifiers.Count; i++ )
			{
				value *= this.modifiers[i].value;
			}
			this.value = (int)value;
			this.onAnyChangeCallback?.Invoke();
		}
	}
}