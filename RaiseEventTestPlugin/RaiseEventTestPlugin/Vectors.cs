using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestPlugin
{
	public class Vector3
	{
		public float x;
		public float y;
		public float z;

		public Vector3()
		{
			x = 0.0f;
			y = 0.0f;
			z = 0.0f;
		}

		public Vector3(float _x, float _y, float _z)
		{
			x = _x;
			y = _y;
			z = _z;
		}

		public float Length()
		{
			return (float)Math.Sqrt(x * x + y * y + z * z);
		}

		//Operator Overloads

		static public Vector3 operator-(Vector3 pointA, Vector3 pointB)
		{
			return new Vector3(pointA.x - pointB.x, pointA.y - pointB.y, pointA.z - pointB.z);
		}

		static public Vector3 operator+(Vector3 pointA, Vector3 pointB)
		{
			return new Vector3(pointA.x + pointB.x, pointA.y + pointB.y, pointA.z + pointB.z);
		}
	}

	public class Quaternion
	{
		public float x;
		public float y;
		public float z;
		public float w;
	}

	public class NumberHelper : Singleton<NumberHelper>
	{
		public double RandomNumberBetweenRange(double minimum, double maximum)
		{
			Thread.Sleep(10);
			Random random = new Random();
			return random.NextDouble() * (maximum - minimum) + minimum;
		}

		public double RandomSecondsBetweenRange(double minimum, double maximum)
		{
			Thread.Sleep(10);
			Random random = new Random();
			return (random.NextDouble() * (maximum - minimum) + minimum) * 1000.0;
		}

		public bool RandomBool()
		{
			Thread.Sleep(10);
			Random random = new Random();
			return (random.NextDouble() <= 0.5);
		}
	}
}
