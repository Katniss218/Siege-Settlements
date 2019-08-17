using System;
using UnityEngine;

namespace Katniss.Utils
{
	public class BallisticSolver
	{
		/// <summary>
		/// Utility function used by SolveQuadratic, SolveCubic, and SolveQuartic.
		/// </summary>
		private static bool IsZero( double d )
		{
			const double epsilon = 1e-9;
			return d > -epsilon && d < epsilon;
		}
		
		/// <summary>
		/// Solves quadratic equation: a*x^2 + b*x + c
		/// </summary>
		private static int SolveQuadratic( double a, double b, double c, out double solution0, out double solution1 )
		{
			solution0 = double.NaN;
			solution1 = double.NaN;
			
			// normal form: x^2 + px + q = 0
			double p = b / (2 * a);
			double q = c / a;

			double D = p * p - q;

			if( IsZero( D ) )
			{
				solution0 = -p;
				return 1;
			}
			else if( D < 0 )
			{
				return 0;
			}
			else // if (D > 0)
			{
				double sqrtD = Math.Sqrt( D );

				solution0 = sqrtD - p;
				solution1 = -sqrtD - p;
				return 2;
			}
		}
		
		/// <summary>
		/// Solves cubic equation: a*x^3 + b*x^2 + c*x + d
		/// </summary>
		private static int SolveCubic( double a, double b, double c, double d, out double solution0, out double solution1, out double solution2 )
		{
			solution0 = double.NaN;
			solution1 = double.NaN;
			solution2 = double.NaN;

			int solutionCount;

			// normal form: x^3 + Ax^2 + Bx + C = 0
			double A = b / a;
			double B = c / a;
			double C = d / a;

			//  substitute x = y - A/3 to eliminate quadric term:  x^3 +px + q = 0
			double sqA = A * A;
			double p = 1.0 / 3 * (-1.0 / 3 * sqA + B);
			double q = 1.0 / 2 * (2.0 / 27 * A * sqA - 1.0 / 3 * A * B + C);

			// use Cardano's formula
			double cbp = p * p * p;
			double D = q * q + cbp;

			if( IsZero( D ) )
			{
				if( IsZero( q ) ) // one triple solution
				{
					solution0 = 0;
					solutionCount = 1;
				}
				else // one single and one double solution
				{
					double u = Math.Pow( -q, 1.0 / 3.0 );
					solution0 = 2 * u;
					solution1 = -u;
					solutionCount = 2;
				}
			}
			else if( D < 0 ) // Casus irreducibilis: three real solutions
			{
				double phi = 1.0 / 3 * Math.Acos( -q / Math.Sqrt( -cbp ) );
				double t = 2 * Math.Sqrt( -p );

				solution0 = t * Math.Cos( phi );
				solution1 = -t * Math.Cos( phi + Math.PI / 3 );
				solution2 = -t * Math.Cos( phi - Math.PI / 3 );
				solutionCount = 3;
			}
			else // one real solution
			{
				double sqrt_D = Math.Sqrt( D );
				double u = Math.Pow( sqrt_D - q, 1.0 / 3.0 );
				double v = -Math.Pow( sqrt_D + q, 1.0 / 3.0 );

				solution0 = u + v;
				solutionCount = 1;
			}

			// resubstitute
			double sub = 1.0 / 3 * A;

			if( solutionCount > 0 )
			{
				solution0 -= sub;
			}

			if( solutionCount > 1 )
			{
				solution1 -= sub;
			}

			if( solutionCount > 2 )
			{
				solution2 -= sub;
			}

			return solutionCount;
		}
		
		/// <summary>
		/// Solves quartic function: a*x^4 + b*x^3 + c*x^2 + d*x + e
		/// </summary>
		private static int SolveQuartic( double a, double b, double c, double d, double e, out double solution0, out double solution1, out double solution2, out double solution3 )
		{
			solution0 = double.NaN;
			solution1 = double.NaN;
			solution2 = double.NaN;
			solution3 = double.NaN;

			double[] coeffs = new double[4];
			int solutionCount;

			// normal form: x^4 + Ax^3 + Bx^2 + Cx + D = 0
			double A = b / a;
			double B = c / a;
			double C = d / a;
			double D = e / a;

			//  substitute x = y - A/4 to eliminate cubic term: x^4 + px^2 + qx + r = 0
			double sqA = A * A;
			double p = -3.0 / 8 * sqA + B;
			double q = 1.0 / 8 * sqA * A - 1.0 / 2 * A * B + C;
			double r = -3.0 / 256 * sqA * sqA + 1.0 / 16 * sqA * B - 1.0 / 4 * A * C + D;

			if( IsZero( r ) )
			{
				// no absolute term: y(y^3 + py + q) = 0

				coeffs[3] = q;
				coeffs[2] = p;
				coeffs[1] = 0;
				coeffs[0] = 1;

				solutionCount = SolveCubic( coeffs[0], coeffs[1], coeffs[2], coeffs[3], out solution0, out solution1, out solution2 );
			}
			else
			{
				// solve the resolvent cubic ...
				coeffs[3] = 1.0 / 2 * r * p - 1.0 / 8 * q * q;
				coeffs[2] = -r;
				coeffs[1] = -1.0 / 2 * p;
				coeffs[0] = 1;

				SolveCubic( coeffs[0], coeffs[1], coeffs[2], coeffs[3], out solution0, out solution1, out solution2 );

				// ... and take the one real solution ...
				double z = solution0;

				// ... to build two quadratic equations
				double u = z * z - r;
				double v = 2 * z - p;

				if( IsZero( u ) )
				{
					u = 0;
				}
				else if( u > 0 )
				{
					u = Math.Sqrt( u );
				}
				else
				{
					return 0;
				}

				if( IsZero( v ) )
				{
					v = 0;
				}
				else if( v > 0 )
				{
					v = Math.Sqrt( v );
				}
				else
				{
					return 0;
				}

				coeffs[2] = z - u;
				coeffs[1] = q < 0 ? -v : v;
				coeffs[0] = 1;

				solutionCount = SolveQuadratic( coeffs[0], coeffs[1], coeffs[2], out solution0, out solution1 );

				coeffs[2] = z + u;
				coeffs[1] = q < 0 ? v : -v;
				coeffs[0] = 1;

				if( solutionCount == 0 )
				{
					solutionCount += SolveQuadratic( coeffs[0], coeffs[1], coeffs[2], out solution0, out solution1 );
				}

				if( solutionCount == 1 )
				{
					solutionCount += SolveQuadratic( coeffs[0], coeffs[1], coeffs[2], out solution1, out solution2 );
				}

				if( solutionCount == 2 )
				{
					solutionCount += SolveQuadratic( coeffs[0], coeffs[1], coeffs[2], out solution2, out solution3 );
				}
			}

			// resubstitute
			double sub = 1.0 / 4 * A;

			if( solutionCount > 0 )
			{
				solution0 -= sub;
			}

			if( solutionCount > 1 )
			{
				solution1 -= sub;
			}

			if( solutionCount > 2 )
			{
				solution2 -= sub;
			}

			if( solutionCount > 3 )
			{
				solution3 -= sub;
			}

			return solutionCount;
		}


		/// <summary>
		/// Calculate the maximum range that a ballistic projectile can be fired on given speed and gravity (and height above ground level).
		/// </summary>
		/// <param name="speed">Speed of the projectile (scalar value).</param>
		/// <param name="gravity">Force of gravity, positive is down (in unity, negative is down).</param>
		/// <param name="initialHeight">Initial distance above the flat ground.</param>
		/// <returns>Calculated maximum range.</returns>
		public static float GetMaxRange( float speed, float gravity, float initialHeight )
		{
			if( speed <= 0  )
			{
				throw new Exception( "GetMaxRange: Speed can't be less than 0." );
			}
			if( gravity <= 0 )
			{
				throw new Exception( "GetMaxRange: Gravity can't be less than or equal to 0." );
			}
			if( initialHeight < 0 )
			{
				throw new Exception( "GetMaxRange: Initial height can't be negative." );
			}

			float angle = 45 * Mathf.Deg2Rad; // no air resistence, so 45 degrees provides maximum range
			float cos = Mathf.Cos( angle );
			float sin = Mathf.Sin( angle );

			float range = (speed * cos / gravity) * (speed * sin + Mathf.Sqrt( speed * speed * sin * sin + 2 * gravity * initialHeight ));
			return range;
		}

		/// <summary>
		/// Solve firing angles for a ballistic projectile with speed and gravity to hit a fixed position.
		/// </summary>
		/// <param name="projectilePos">Point the projectile will be fired from.</param>
		/// <param name="projectileSpeed">Speed of the projectile (scalar value).</param>
		/// <param name="targetPos">Point the projectile is trying to hit.</param>
		/// <param name="gravity">Force of gravity, positive is down (in unity, negative is down).</param>
		/// <param name="low">Firing solution (low angle).</param>
		/// <param name="high">Firing solution (high angle).</param>
		/// <returns>Number of unique solutions found, can be: 0, 1 or 2.</returns>
		public static int Solve( Vector3 projectilePos, float projectileSpeed, Vector3 targetPos, float gravity, out Vector3 low, out Vector3 high )
		{
			// Handling these cases is up to your project's coding standards
			if( projectilePos == targetPos )
			{
				throw new Exception( "Solve: called with invalid argument. Projectile's position can't be the same as target's position." );
			}
			if( projectileSpeed <= 0 || gravity < 0 )
			{
				throw new Exception( "Solve: called with invalid argument(s). Projectile's speed or gravity can't be less than 0." );
			}

			// Initialize output parameters
			low = Vector3.zero;
			high = Vector3.zero;
			
			Vector3 diff = targetPos - projectilePos;
			Vector3 diffX0Z = new Vector3( diff.x, 0f, diff.z );
			float groundDist = diffX0Z.magnitude;

			float speedSq = projectileSpeed * projectileSpeed;
			float speedPow4 = projectileSpeed * projectileSpeed * projectileSpeed * projectileSpeed;
			//float y = diff.y;
			//float x = groundDist;
			float groundDistTimesGravity = gravity * groundDist;

			float root = speedPow4 - gravity * (gravity * (groundDist * groundDist) + 2 * diff.y * speedSq);

			// No solution
			if( root < 0 )
			{
				return 0;
			}

			root = Mathf.Sqrt( root );

			float lowAng = Mathf.Atan2( speedSq - root, groundDistTimesGravity );
			float highAng = Mathf.Atan2( speedSq + root, groundDistTimesGravity );
			int numSolutions = lowAng != highAng ? 2 : 1;

			Vector3 groundDir = diffX0Z.normalized;
			low = groundDir * Mathf.Cos( lowAng ) * projectileSpeed + Vector3.up * Mathf.Sin( lowAng ) * projectileSpeed;

			if( numSolutions > 1 )
			{
				high = groundDir * Mathf.Cos( highAng ) * projectileSpeed + Vector3.up * Mathf.Sin( highAng ) * projectileSpeed;
			}
			return numSolutions;
		}

		/// <summary>
		/// Solve firing angles for a ballistic projectile with speed and gravity to hit a target moving with constant, linear velocity.
		/// </summary>
		/// <param name="projPos">Point the projectile will be fired from.</param>
		/// <param name="projSpeed">Speed of the projectile (scalar value).</param>
		/// <param name="targetPos">Point the projectile is trying to hit.</param>
		/// <param name="targetVel">The velocity of the point the projectile is trying to hit.</param>
		/// <param name="gravity">Force of gravity, positive is down (in unity, negative is down).</param>
		/// <param name="solution0">Firing solution (fastest time to impact).</param>
		/// <param name="solution1">Firing solution (next time to impact).</param>
		/// <returns>Number of unique solutions found, can be: 0, 1, 2, 3 or 4.</returns>
		public static int Solve( Vector3 projPos, float projSpeed, Vector3 targetPos, Vector3 targetVel, float gravity, out Vector3 solution0, out Vector3 solution1 )
		{
			// Handling these cases is up to your project's coding standards
			if( projPos == targetPos )
			{
				throw new Exception( "Solve: called with invalid argument. Projectile's position can't be the same as target's position." );
			}
			if( projSpeed <= 0 || gravity < 0 )
			{
				throw new Exception( "Solve: called with invalid argument(s). Projectile's speed or gravity can't be less than 0." );
			}

			// Initialize output parameters
			solution0 = Vector3.zero;
			solution1 = Vector3.zero;
			
			Vector3 toTarget = targetPos - projPos;
			double minusHalfGravity = -0.5f * gravity;

			// Quartic Coeffecients
			double c0 = minusHalfGravity * minusHalfGravity;
			double c1 = 2 * targetVel.y * minusHalfGravity;
			double c2 = (targetVel.y * targetVel.y) + 2 * toTarget.y * minusHalfGravity - (projSpeed * projSpeed) + (targetVel.x * targetVel.x) + (targetVel.z * targetVel.z);
			double c3 = (2 * toTarget.y * targetVel.y) + (2 * toTarget.x * targetVel.x) + (2 * toTarget.z * targetVel.z);
			double c4 = (toTarget.y * toTarget.y) + (toTarget.x * toTarget.x) + (toTarget.z * toTarget.z);

			// Solve quartic
			double[] times = new double[4];
			int numTimes = SolveQuartic( c0, c1, c2, c3, c4, out times[0], out times[1], out times[2], out times[3] );

			// Sort so faster collision is found first
			Array.Sort( times );

			// Plug quartic solutions into base equations
			// There should never be more than 2 positive, real roots.
			Vector3[] solutions = new Vector3[2];
			int numSolutions = 0;

			for( int i = 0; i < numTimes && numSolutions < 2; ++i )
			{
				double t = times[i];
				if( t <= 0 )
				{
					continue;
				}
				solutions[numSolutions].x = (float)((toTarget.x + targetVel.x * t) / t);
				solutions[numSolutions].y = (float)((toTarget.y + targetVel.y * t - minusHalfGravity * t * t) / t);
				solutions[numSolutions].z = (float)((toTarget.z + targetVel.z * t) / t);
				++numSolutions;
			}

			// Write out solutions
			if( numSolutions > 0 )
			{
				solution0 = solutions[0];
			}

			if( numSolutions > 1 )
			{
				solution1 = solutions[1];
			}

			return numSolutions;
		}
	}
}