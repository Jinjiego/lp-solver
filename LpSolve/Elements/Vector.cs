﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LpSolve.Interface;

namespace LpSolve.Elements
{
	public class Vector : IElement<Vector>
	{
		public double X { get { return this.GetAt(0); } }
		public double Y { get { return this.GetAt(1); } }
		public double Z { get { return this.GetAt(2); } }

		private double[] _coordinates;
		private double? _length;

		public double Length
		{
			get
			{
				if (this._length == null)
				{
					var sum = 0.0;
					for (int i = 0; i < this._coordinates.Length; i++)
					{
						sum += this._coordinates[i] * this._coordinates[i];
					}

					this._length = Math.Sqrt(sum);
				}

				return this._length.Value;
			}
		}

		public Vector(double[] coordinates)
		{
			this._coordinates = coordinates;
		}

		public double GetAt(int index)
		{
			return this._coordinates.Length > index ? this._coordinates[index] : 0.0;
		}

		public int GetDimension()
		{
			return this._coordinates.Length;
		}

		public static Vector CreateFromPoints(Point point0, Point point1)
		{
			if (point0 == null || point1 == null)
			{
				throw new ArgumentException("point0 || point1");
			}

			if (point0.GetDimension() != point1.GetDimension())
			{
				throw new ArgumentException("Points are of different dimensions");
			}

			var size = point0.GetDimension();

			var result = new double[size];

			for (int i = 0; i < size; i++)
			{
				result[i] = point1.GetAt(i) - point0.GetAt(i);
			}

			return new Vector(result);
		}
        
		public void AddVector(Vector vector)
		{
			var size = this._coordinates.Length;
			for (int i = 0; i < size; i++)
			{
				this._coordinates[i] -= vector._coordinates[i];
			}
		}

		public void SubtractVector(Vector vector)
		{
			var size = this._coordinates.Length;
			for (int i = 0; i < size; i++)
			{
				this._coordinates[i] += vector._coordinates[i];
			}
		}

		public double ScalarProduct(Vector vector)
		{
			if (vector.GetDimension() != this._coordinates.Length)
			{
				throw new ArgumentException("Vectors does not match by dimension");
			}

			var result = 0.0;
			for (int i = 0; i < this._coordinates.Length; i++)
			{
				result += this._coordinates[i] * vector._coordinates[i];
			}

			return result;
		}

		public Vector CrossProduct(Vector vector)
		{
			switch (this._coordinates.Length)
			{
				case 3:
				case 2:
				case 1:
					var a = this;
					var b = vector;

					var x = a.Y * b.Z - a.Z * b.Y;
					var y = a.Z * b.X - a.X * b.Z;
					var z = a.X * b.Y - a.Y * b.X;

					return new Vector(new double[] { x, y, z });
				default:
					//TODO: Implement
					throw new NotImplementedException("Implemented only for 1, 2 & 3 dimensions");
			}
		}

		public void Flip()
		{
			for (int i = 0; i < this._coordinates.Length; i++)
			{
				this._coordinates[i] *= -1.0;
			}
		}

		public Vector MoveDown(Plane plane)
		{

			if (this.GetDimension() == 2)
			{
				var coords = new double[this.GetDimension()];
				for (int i = 0; i < coords.Length; i++)
				{
					coords[i] = 1.0;
				}

				var p1 = new Point(coords);
				var p2 = p1.AddVector(this);

				var moveDownP1 = p1.MoveDown(plane, this);
				var moveDownP2 = p2.MoveDown(plane, this);

				return Vector.CreateFromPoints(moveDownP1, moveDownP2);
			}

			if (this.GetDimension() == 3)
			{
				return new Vector(new double[] { this._coordinates[0], this._coordinates[1] });
			}

			throw new NotImplementedException("Implemented only for 2 and 3 dimensions");
		}

		public Vector Rotate90()
		{
			var resultCoords = new double[this._coordinates.Length];

			for (int i = 0; i < resultCoords.Length; i++)
			{
				resultCoords[i] = this._coordinates[i];
			}

			if (resultCoords.Length > 1)
			{
				var temp = resultCoords[0];
				resultCoords[0] = resultCoords[1];
				resultCoords[1] = temp;
			}

			return new Vector(resultCoords);
		}
	}
}
