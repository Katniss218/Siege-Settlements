using System;
using System.Linq;

namespace SS.Diplomacy
{
	/// <summary>
	/// Represents relations between 2 members (indices) of type 'int'.
	/// </summary>
	public class RelationMap<T>
	{
		//  -=- Size = 5    -=- (Count = 10)

		//   horizontal: n1
		//   vertical:   n2
		//   matrix:     indices for the '__values' array of relations ('-' means there's no relation, cuz you can't have a relation with yourself).

		//   | 0 1 2 3 4
		// --------------
		// 0 | - 0 1 3 6
		// 1 | 0 - 2 4 7
		// 2 | 1 2 - 5 8
		// 3 | 3 4 5 - 9
		// 4 | 6 7 8 9 -

		private T[] __values;

		public T this[int i]
		{
			get
			{
				return this.__values[i];
			}
			set
			{
				this.__values[i] = value;
			}
		}

		/// <summary>
		/// Gets or sets the relation between members n1 and n2.
		/// </summary>
		/// <param name="n1">Member no. 1</param>
		/// <param name="n2">Member no. 2</param>
		public T this[int n1, int n2]
		{
			get
			{
				return this.Get( n1, n2 );
			}
			set
			{
				this.Set( n1, n2, value );
			}
		}


		/// <summary>
		/// Returns the number of relations between members.
		/// </summary>
		public int MatrixLength
		{
			get
			{
				return this.__values.Length;
			}
		}

		/// <summary>
		/// Returns the size (width and/or height) of the members matrix.
		/// </summary>
		public int Size { get; private set; }



		public static int GetMatrixLength( int size )
		{
			return ((size - 1) * size) / 2;
		}

		public static int GetSize( int matrixLength )
		{
			float num = 0.5f * (float)(1 + Math.Sqrt( 1 + 8 * matrixLength ));

			if( Math.Floor( num ) != num )
			{
				throw new Exception( "Invalid matrix length " + matrixLength );
			}

			return (int)num;
		}
		
		private static int GetMatrixIndex( int n1, int n2 )
		{
			return Solt( n1 ) + n2;
		}


		/// <summary>
		/// Initializes the RelationMap with given size.
		/// </summary>
		/// <param name="size">Size (width and/or height) of the members matrix.</param>
		public RelationMap( int size )
		{
			this.Size = size;
			__values = new T[GetMatrixLength( size )];
		}

		/// <summary>
		/// Initializes the RelationMap with given size and the default value for relations between it's members.
		/// </summary>
		/// <param name="size">Size (width and/or height) of the members matrix.</param>
		/// <param name="defaultValue">Default value of the relations between members.</param>
		public RelationMap( int size, T defaultValue )
		{
			this.Size = size;
			__values = Enumerable.Repeat( defaultValue, GetMatrixLength( size ) ).ToArray();
		}


		/// <summary>
		/// Gets the relation between specified members n1 and n2.
		/// </summary>
		/// <param name="n1">Member no. 1</param>
		/// <param name="n2">Member no. 2</param>
		/// <returns>The relation between members n1 and n2.</returns>
		public T Get( int n1, int n2 )
		{
			if( n1 >= Size || n2 >= Size )
			{
				throw new ArgumentOutOfRangeException( "'n1' and 'n2' must be less than 'Size'." );
			}
			if( n1 == n2 )
			{
				throw new ArgumentException( "The relation between the same element doesn't make sense." );
			}
			if( n1 < n2 )
			{
				Swap<int>( ref n1, ref n2 );
			}

			return __values[GetMatrixIndex( n1, n2 )];
		}

		/// <summary>
		/// Sets the relation between specified members n1 and n2.
		/// </summary>
		/// <param name="n1">Member no. 1</param>
		/// <param name="n2">Member no. 2</param>
		/// <param name="value">The new relation between members n1 and n2.</param>
		public void Set( int n1, int n2, T value )
		{
			if( n1 >= Size || n2 >= Size )
			{
				throw new ArgumentOutOfRangeException( "'n1' and 'n2' must be less than 'Size'." );
			}
			if( n1 == n2 )
			{
				throw new ArgumentException( "The relation between the same element doesn't make sense." );
			}
			if( n1 < n2 )
			{
				Swap<int>( ref n1, ref n2 );
			}

			__values[GetMatrixIndex( n1, n2 )] = value;
		}

		/// <summary>
		/// Resizes the RelationMap.
		/// </summary>
		/// <param name="newSize">New size of the RelationMap's matrix.</param>
		public void Resize( int newSize )
		{
			this.Size = newSize;
			Array.Resize( ref this.__values, newSize );
		}

		/// <summary>
		/// Resizes the RelationMap and initializes all new possible relations with the specified default value.
		/// </summary>
		/// <param name="newSize">New size of the RelationMap's matrix.</param>
		/// <param name="defaultValue">The default value of the relations between members.</param>
		public void Resize( int newSize, T defaultValue )
		{
			this.Size = newSize;
			int oldSize = this.__values.Length;
			Array.Resize( ref this.__values, newSize );

			if( newSize > oldSize )
			{
				for( int i = oldSize - 1; i < newSize; i++ )
				{
					this.__values[i] = defaultValue;
				}
			}
		}


		private static void Swap<U>( ref U v1, ref U v2 )
		{
			U temp = v1;
			v1 = v2;
			v2 = temp;
		}

		private static int Solt( int n ) // Sum Of Less Than ( n )
		{
			int ret = 0;
			for( int i = 0; i < n; i++ )
			{
				ret += i;
			}

			return ret;
		}
	}
}