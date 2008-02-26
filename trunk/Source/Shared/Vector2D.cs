/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using CodeImp;

#if CLIENT
using Microsoft.DirectX;
#endif

namespace CodeImp.Bloodmasters
{
	public struct Vector2D
	{	
		// Members
		public float x;
		public float y;
		
		// Constructor
		public Vector2D(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
		
		// Constructor
		public Vector2D(Vector3D v)
		{
			this.x = v.x;
			this.y = v.y;
		}
		
		// Output
		public override string ToString()
		{
			return x + ", " + y;
		}
		
		#if CLIENT
		// Constructor
		public Vector2D(Vector2 v)
		{
			this.x = v.X;
			this.y = v.Y;
		}
		#endif
		
		// Conversion to Vector3D
		public static implicit operator Vector3D(Vector2D a)
		{
			return new Vector3D(a);
		}
		
		#if CLIENT
		// Conversion to Vector2
		public static implicit operator Vector2(Vector2D a)
		{
			return new Vector2(a.x, a.y);
		}
		#endif
		
		// This adds two vectors
		public static Vector2D operator+(Vector2D a, Vector2D b)
		{
			return new Vector2D(a.x + b.x, a.y + b.y);
		}
		
		// This adds to a vector
		public static Vector2D operator+(float a, Vector2D b)
		{
			return new Vector2D(a + b.x, a + b.y);
		}
		
		// This adds to a vector
		public static Vector2D operator+(Vector2D a, float b)
		{
			return new Vector2D(a.x + b, a.y + b);
		}
		
		// This subtracts two vectors
		public static Vector2D operator-(Vector2D a, Vector2D b)
		{
			return new Vector2D(a.x - b.x, a.y - b.y);
		}
		
		// This subtracts from a vector
		public static Vector2D operator-(Vector2D a, float b)
		{
			return new Vector2D(a.x - b, a.y - b);
		}
		
		// This subtracts from a vector
		public static Vector2D operator-(float a, Vector2D b)
		{
			return new Vector2D(a - b.x, a - b.y);
		}
		
		// This reverses a vector
		public static Vector2D operator-(Vector2D a)
		{
			return new Vector2D(-a.x, -a.y);
		}
		
		// This scales a vector
		public static Vector2D operator*(float s, Vector2D a)
		{
			return new Vector2D(a.x * s, a.y * s);
		}
		
		// This scales a vector
		public static Vector2D operator*(Vector2D a, float s)
		{
			return new Vector2D(a.x * s, a.y * s);
		}
		
		// This scales a vector
		public static Vector2D operator/(float s, Vector2D a)
		{
			return new Vector2D(a.x / s, a.y / s);
		}
		
		// This scales a vector
		public static Vector2D operator/(Vector2D a, float s)
		{
			return new Vector2D(a.x / s, a.y / s);
		}
		
		// This calculates the length
		public float Length()
		{
			// Calculate and return the length
			return (float)Math.Sqrt(x * x + y * y);
		}
		
		// This calculates the square length
		public float LengthSq()
		{
			// Calculate and return the square length
			return x * x + y * y;
		}
		
		// This normalizes the vector
		public void Normalize()
		{
			// Divide each element by the length
			float mul = 1f / this.Length();
			x *= mul;
			y *= mul;
		}
		
		// This scales the vector
		public void Scale(float s)
		{
			// Scale the vector
			x *= s;
			y *= s;
		}
		
		// This changes the vector length
		public void MakeLength(float l)
		{
			// Normalize, then scale
			this.Normalize();
			this.Scale(l);
		}
		
		// This calculates the dot product
		public static float DotProduct(Vector2D a, Vector2D b)
		{
			// Calculate and return the dot product
			return a.x * b.x + a.y * b.y;
		}
		
		// This reflects the vector v over mirror m
		// Note that mirror m must be normalized!
		// R = V - 2 * M * (M dot V)
		public static Vector2D Reflect(Vector2D v, Vector2D m)
		{
			// Get the dot product of v and m
			float dp = Vector2D.DotProduct(m, v);
			
			// Make the reflected vector
			Vector2D mv = new Vector2D();
			mv.x = v.x - (2f * m.x * dp);
			mv.y = v.y - (2f * m.y * dp);
			
			// Return the reflected vector
			return mv;
		}
		
		// This returns the reversed vector
		public static Vector2D Reversed(Vector2D v)
		{
			// Return reversed vector
			return new Vector2D(-v.x, -v.y);
		}
		
		// This returns a vector from angle
		public static Vector2D FromAnimationAngle(float angle, float length)
		{
			float ax = (float)Math.Sin(angle) * length;
			float ay = -(float)Math.Cos(angle) * length;
			
			// Return vector
			return new Vector2D(ax, ay);
		}
		
		// This returns a vector from angle
		public static Vector2D FromMapAngle(float angle, float length)
		{
			float ax = (float)Math.Sin(angle) * length;
			float ay = -(float)Math.Cos(angle) * length;
			
			// Return vector
			return new Vector2D(ax, ay);
		}
	}
}
