using UnityEngine;

namespace SS
{
	/// <summary>
	/// Rotates the gameObject to face the velocity vector (by default the forward vector is oriented along the velocity vector).
	/// </summary>
	[RequireComponent( typeof( Rigidbody ) )]
	public sealed class RotateAlongVelocity : MonoBehaviour
	{
		/// <summary>
		/// Represents one of 6 directions. You can multiply by -1 to get the other (flipped) direction (e.g. Forward -> Backward).
		/// </summary>
		public enum Direction : sbyte
		{
			Forward = 1,
			Backward = -1,
			Right = 2,
			Left = -2,
			Up = 3,
			Down = -3
		}

		/// <summary>
		/// Which GameObject's direction vector should be oriented along the velocity vector.
		/// </summary>
		public Direction direction = Direction.Forward;

		new private Rigidbody rigidbody;

		void Start()
		{
			this.rigidbody = this.GetComponent<Rigidbody>();
		}

		void Update()
		{
			switch( this.direction )
			{
				case Direction.Forward:
					this.transform.forward = this.rigidbody.velocity.normalized;
					break;
				case Direction.Backward:
					this.transform.forward = -this.rigidbody.velocity.normalized;
					break;
				case Direction.Right:
					this.transform.right = this.rigidbody.velocity.normalized;
					break;
				case Direction.Left:
					this.transform.right = -this.rigidbody.velocity.normalized;
					break;
				case Direction.Up:
					this.transform.up = this.rigidbody.velocity.normalized;
					break;
				case Direction.Down:
					this.transform.up = -this.rigidbody.velocity.normalized;
					break;
			}
		}
	}
}