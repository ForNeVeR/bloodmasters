/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
#if CLIENT
using SharpDX;
#endif

namespace CodeImp.Bloodmasters
{
	public struct Vector3D
	{
		// Members
		public float x;
		public float y;
		public float z;

		// Constructor
		public Vector3D(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		// Constructor
		public Vector3D(Vector2D v)
		{
			this.x = v.x;
			this.y = v.y;
			this.z = 0f;
		}

		#if CLIENT
		// Constructor
		public Vector3D(Vector3 v)
		{
			this.x = v.X;
			this.y = v.Y;
			this.z = v.Z;
		}
		#endif

		// This applies 2D coordinates and preserves the Z coordinate
		public void Apply2D(Vector2D v)
		{
			// Apply 2D coordinates
			x = v.x;
			y = v.y;
		}

		// Conversion to Vector2D
		public static implicit operator Vector2D(Vector3D a)
		{
			return new Vector2D(a);
		}

		#if CLIENT
		// Conversion to Vector3
		public static implicit operator Vector3(Vector3D a)
		{
			return new Vector3(a.x, a.y, a.z);
		}
		#endif

		// This adds two vectors
		public static Vector3D operator+(Vector3D a, Vector3D b)
		{
			return new Vector3D(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		// This subtracts two vectors
		public static Vector3D operator-(Vector3D a, Vector3D b)
		{
			return new Vector3D(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		// This reverses a vector
		public static Vector3D operator-(Vector3D a)
		{
			return new Vector3D(-a.x, -a.y, -a.z);
		}

		// This scales a vector
		public static Vector3D operator*(float s, Vector3D a)
		{
			return new Vector3D(a.x * s, a.y * s, a.z * s);
		}

		// This scales a vector
		public static Vector3D operator*(Vector3D a, float s)
		{
			return new Vector3D(a.x * s, a.y * s, a.z * s);
		}

		// This scales a vector
		public static Vector3D operator/(float s, Vector3D a)
		{
			return new Vector3D(a.x / s, a.y / s, a.z / s);
		}

		// This scales a vector
		public static Vector3D operator/(Vector3D a, float s)
		{
			return new Vector3D(a.x / s, a.y / s, a.z / s);
		}

		// This compares a vector
		public static bool operator==(Vector3D a, Vector3D b)
		{
			return (a.x == b.x) && (a.y == b.y) && (a.z == b.z);
		}

		// This compares a vector
		public static bool operator!=(Vector3D a, Vector3D b)
		{
			return (a.x != b.x) || (a.y != b.y) || (a.z != b.z);
		}

		// This calculates the length
		public float Length()
		{
			// Calculate and return the length
			return (float)Math.Sqrt(x * x + y * y + z * z);
		}

		// This calculates the squared length
		public float LengthSq()
		{
			// Calculate and return the length
			return x * x + y * y + z * z;
		}

		// This normalizes the vector
		public void Normalize()
		{
			float lensq = this.LengthSq();
			if(lensq > 0.0000000001f)
			{
				// Divide each element by the length
				float mul = 1f / (float)Math.Sqrt(lensq);
				x *= mul;
				y *= mul;
				z *= mul;
			}
			else
			{
				x = 0f;
				y = 0f;
				z = 0f;
			}
		}

		// This scales the vector
		public void Scale(float s)
		{
			// Scale the vector
			x *= s;
			y *= s;
			z *= s;
		}

		// This changes the vector length
		public void MakeLength(float l)
		{
			// Normalize, then scale
			this.Normalize();
			this.Scale(l);
		}

		// This calculates the cross product
		public static Vector3D CrossProduct(Vector3D a, Vector3D b)
		{
			Vector3D result = new Vector3D();

			// Calculate and return the dot product
			result.x = a.y * b.z - a.z * b.y;
			result.y = a.z * b.x - a.x * b.z;
			result.z = a.x * b.y - a.y * b.x;
			return result;
		}

		// This calculates the dot product
		public static float DotProduct(Vector3D a, Vector3D b)
		{
			// Calculate and return the dot product
			return a.x * b.x + a.y * b.y + a.z * b.z;
		}

		// This creates a random vector
		public static Vector3D Random(Random rnd)
		{
			if(rnd == null) rnd = new Random();
			Vector3D mv = new Vector3D();

			// Create random vector
			mv.x = (float)rnd.NextDouble() - 0.5f;
			mv.y = (float)rnd.NextDouble() - 0.5f;
			mv.z = (float)rnd.NextDouble();
			return mv;
		}

		// This creates a random vector
		public static Vector3D Random(Random rnd, float sx, float sy, float sz)
		{
			if(rnd == null) rnd = new Random();
			Vector3D mv = new Vector3D();

			// Create random vector
			mv.x = ((float)rnd.NextDouble() - 0.5f) * sx;
			mv.y = ((float)rnd.NextDouble() - 0.5f) * sy;
			mv.z = ((float)rnd.NextDouble()) * sz;
			return mv;
		}

		// This reflects the vector v over mirror m
		// Note that mirror m must be normalized!
		public static Vector3D Reflect(Vector3D v, Vector3D m)
		{
			// Get the dot product of v and m
			float dp = Vector3D.DotProduct(v, m);

			// Make the reflected vector
			Vector3D mv = new Vector3D();
			mv.x = -v.x + 2f * m.x * dp;
			mv.y = -v.y + 2f * m.y * dp;
			mv.z = -v.z + 2f * m.z * dp;

			// Return the reflected vector
			return mv;
		}

		// This returns the reversed vector
		public static Vector3D Reversed(Vector3D v)
		{
			// Return reversed vector
			return new Vector3D(-v.x, -v.y, -v.z);
		}

		// This returns a vector from angle
		public static Vector3D FromAnimationAngle(float angle, float length)
		{
			float ax = (float)Math.Sin(angle) * length;
			float ay = -(float)Math.Cos(angle) * length;

			// Return vector
			return new Vector3D(ax, ay, 0f);
		}

		// This returns a vector from angle
		public static Vector3D FromActorAngle(float angle, float anglez, float length)
		{
			// Adjust angles
			angle += 0.5f * (float)Math.PI;
			anglez += 0.5f * (float)Math.PI;

			// Make vector
			float ax = (float)Math.Sin(angle) * (float)Math.Cos(anglez) * length;
			float ay = -(float)Math.Cos(angle) * (float)Math.Cos(anglez) * length;
			float az = (float)Math.Sin(anglez) * length;

			// Return vector
			return new Vector3D(ax, ay, az);
		}

		// This returns a vector from angle
		public static Vector3D FromMapAngle(float angle, float length)
		{
			float ax = (float)Math.Sin(angle) * length;
			float ay = -(float)Math.Cos(angle) * length;

			// Return vector
			return new Vector3D(ax, ay, 0f);
		}
	}
}
