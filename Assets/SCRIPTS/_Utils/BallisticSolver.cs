// Changelog 
// - 1.0  (01-05-2019) - Initial Release.
//
// Author
// - Katniss
//
// API
// - static float GetMaxRange( float speed, float gravity, float initialHeight )
// - static int Solve( Vector3 projectilePos, float projectileSpeed, Vector3 targetPos, float gravity, out Vector3 low, out Vector3 high )
// - static int Solve( Vector3 projectilePos, float projectileSpeed, Vector3 targetPos, Vector3 targetVelocity, float gravity, out Vector3 solution0, out Vector3 solution1 )


using UnityEngine;
using System;

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

			double p, q, D;

			/* normal form: x^2 + px + q = 0 */
			p = b / (2 * a);
			q = c / a;

			D = p * p - q;

			if( IsZero( D ) )
			{
				solution0 = -p;
				return 1;
			}
			else if( D < 0 )
			{
				return 0;
			}
			else /* if (D > 0) */
			{
				double sqrt_D = Math.Sqrt( D );

				solution0 = sqrt_D - p;
				solution1 = -sqrt_D - p;
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

			int num;
			double sub;
			double A, B, C;
			double sq_A, p, q;
			double cb_p, D;

			/* normal form: x^3 + Ax^2 + Bx + C = 0 */
			A = b / a;
			B = c / a;
			C = d / a;

			/*  substitute x = y - A/3 to eliminate quadric term:  x^3 +px + q = 0 */
			sq_A = A * A;
			p = 1.0 / 3 * (-1.0 / 3 * sq_A + B);
			q = 1.0 / 2 * (2.0 / 27 * A * sq_A - 1.0 / 3 * A * B + C);

			/* use Cardano's formula */
			cb_p = p * p * p;
			D = q * q + cb_p;

			if( IsZero( D ) )
			{
				if( IsZero( q ) ) /* one triple solution */
				{
					solution0 = 0;
					num = 1;
				}
				else /* one single and one double solution */
				{
					double u = Math.Pow( -q, 1.0 / 3.0 );
					solution0 = 2 * u;
					solution1 = -u;
					num = 2;
				}
			}
			else if( D < 0 ) /* Casus irreducibilis: three real solutions */
			{
				double phi = 1.0 / 3 * Math.Acos( -q / Math.Sqrt( -cb_p ) );
				double t = 2 * Math.Sqrt( -p );

				solution0 = t * Math.Cos( phi );
				solution1 = -t * Math.Cos( phi + Math.PI / 3 );
				solution2 = -t * Math.Cos( phi - Math.PI / 3 );
				num = 3;
			}
			else /* one real solution */
			{
				double sqrt_D = Math.Sqrt( D );
				double u = Math.Pow( sqrt_D - q, 1.0 / 3.0 );
				double v = -Math.Pow( sqrt_D + q, 1.0 / 3.0 );

				solution0 = u + v;
				num = 1;
			}

			/* resubstitute */
			sub = 1.0 / 3 * A;

			if( num > 0 )
			{
				solution0 -= sub;
			}

			if( num > 1 )
			{
				solution1 -= sub;
			}

			if( num > 2 )
			{
				solution2 -= sub;
			}

			return num;
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
			double z, u, v, sub;
			double A, B, C, D;
			double sq_A, p, q, r;
			int num;

			/* normal form: x^4 + Ax^3 + Bx^2 + Cx + D = 0 */
			A = b / a;
			B = c / a;
			C = d / a;
			D = e / a;

			/*  substitute x = y - A/4 to eliminate cubic term: x^4 + px^2 + qx + r = 0 */
			sq_A = A * A;
			p = -3.0 / 8 * sq_A + B;
			q = 1.0 / 8 * sq_A * A - 1.0 / 2 * A * B + C;
			r = -3.0 / 256 * sq_A * sq_A + 1.0 / 16 * sq_A * B - 1.0 / 4 * A * C + D;

			if( IsZero( r ) )
			{
				/* no absolute term: y(y^3 + py + q) = 0 */

				coeffs[3] = q;
				coeffs[2] = p;
				coeffs[1] = 0;
				coeffs[0] = 1;

				num = SolveCubic( coeffs[0], coeffs[1], coeffs[2], coeffs[3], out solution0, out solution1, out solution2 );
			}
			else
			{
				/* solve the resolvent cubic ... */
				coeffs[3] = 1.0 / 2 * r * p - 1.0 / 8 * q * q;
				coeffs[2] = -r;
				coeffs[1] = -1.0 / 2 * p;
				coeffs[0] = 1;

				SolveCubic( coeffs[0], coeffs[1], coeffs[2], coeffs[3], out solution0, out solution1, out solution2 );

				/* ... and take the one real solution ... */
				z = solution0;

				/* ... to build two quadratic equations */
				u = z * z - r;
				v = 2 * z - p;

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

				num = SolveQuadratic( coeffs[0], coeffs[1], coeffs[2], out solution0, out solution1 );

				coeffs[2] = z + u;
				coeffs[1] = q < 0 ? v : -v;
				coeffs[0] = 1;

				if( num == 0 )
				{
					num += SolveQuadratic( coeffs[0], coeffs[1], coeffs[2], out solution0, out solution1 );
				}

				if( num == 1 )
				{
					num += SolveQuadratic( coeffs[0], coeffs[1], coeffs[2], out solution1, out solution2 );
				}

				if( num == 2 )
				{
					num += SolveQuadratic( coeffs[0], coeffs[1], coeffs[2], out solution2, out solution3 );
				}
			}

			/* resubstitute */
			sub = 1.0 / 4 * A;

			if( num > 0 )
			{
				solution0 -= sub;
			}

			if( num > 1 )
			{
				solution1 -= sub;
			}

			if( num > 2 )
			{
				solution2 -= sub;
			}

			if( num > 3 )
			{
				solution3 -= sub;
			}

			return num;
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
			if( speed <= 0 || gravity <= 0 || initialHeight < 0 )
			{
				throw new Exception( "BallisticSolver.GetMaxRange called with invalid argument." );
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
				throw new Exception( "BallisticSolver.SolveBallisticArc called with invalid argument. Projectile's position can't be the same as target's position." );
			}
			if( projectileSpeed <= 0 || gravity < 0 )
			{
				throw new Exception( "BallisticSolver.SolveBallisticArc called with invalid argument(s). Projectile's speed or gravity can't be less than 0." );
			}

			// C# requires out variables be set
			low = Vector3.zero;
			high = Vector3.zero;
			
			Vector3 diff = targetPos - projectilePos;
			Vector3 diffXZ = new Vector3( diff.x, 0f, diff.z );
			float groundDist = diffXZ.magnitude;

			float speed2 = projectileSpeed * projectileSpeed;
			float speed4 = projectileSpeed * projectileSpeed * projectileSpeed * projectileSpeed;
			float y = diff.y;
			float x = groundDist;
			float gx = gravity * x;

			float root = speed4 - gravity * (gravity * x * x + 2 * y * speed2);

			// No solution
			if( root < 0 )
			{
				return 0;
			}

			root = Mathf.Sqrt( root );

			float lowAng = Mathf.Atan2( speed2 - root, gx );
			float highAng = Mathf.Atan2( speed2 + root, gx );
			int numSolutions = lowAng != highAng ? 2 : 1;

			Vector3 groundDir = diffXZ.normalized;
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
		/// <param name="projectilePos">Point the projectile will be fired from.</param>
		/// <param name="projectileSpeed">Speed of the projectile (scalar value).</param>
		/// <param name="targetPos">Point the projectile is trying to hit.</param>
		/// <param name="targetVelocity"></param>
		/// <param name="gravity">Force of gravity, positive is down (in unity, negative is down).</param>
		/// <param name="solution0">Firing solution (fastest time to impact).</param>
		/// <param name="solution1">Firing solution (next time to impact).</param>
		/// <returns>Number of unique solutions found, can be: 0, 1, 2, 3 or 4.</returns>
		public static int Solve( Vector3 projectilePos, float projectileSpeed, Vector3 targetPos, Vector3 targetVelocity, float gravity, out Vector3 solution0, out Vector3 solution1 )
		{
			// Handling these cases is up to your project's coding standards
			if( projectilePos == targetPos )
			{
				throw new Exception( "BallisticSolver.SolveBallisticArc called with invalid argument. Projectile's position can't be the same as target's position." );
			}
			if( projectileSpeed <= 0 || gravity < 0 )
			{
				throw new Exception( "BallisticSolver.SolveBallisticArc called with invalid argument(s). Projectile's speed or gravity can't be less than 0." );
			}

			// Initialize output parameters
			solution0 = Vector3.zero;
			solution1 = Vector3.zero;

			double G = gravity;

			double A = projectilePos.x;
			double B = projectilePos.y;
			double C = projectilePos.z;
			double M = targetPos.x;
			double N = targetPos.y;
			double O = targetPos.z;
			double P = targetVelocity.x;
			double Q = targetVelocity.y;
			double R = targetVelocity.z;
			double S = projectileSpeed;

			double H = M - A;
			double J = O - C;
			double K = N - B;
			double L = -.5f * G;

			// Quartic Coeffecients
			double c0 = L * L;
			double c1 = 2 * Q * L;
			double c2 = Q * Q + 2 * K * L - S * S + P * P + R * R;
			double c3 = 2 * K * Q + 2 * H * P + 2 * J * R;
			double c4 = K * K + H * H + J * J;

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
				solutions[numSolutions].x = (float)((H + P * t) / t);
				solutions[numSolutions].y = (float)((K + Q * t - L * t * t) / t);
				solutions[numSolutions].z = (float)((J + R * t) / t);
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