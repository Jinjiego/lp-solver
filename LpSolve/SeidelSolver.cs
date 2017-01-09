﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LpSolve.Elements;
using LpSolve.Interface;

namespace LpSolve
{
	public class SeidelSolver
	{
		private List<HalfSpace> _halfSpaces;
		private Vector _vector;

		private List<HalfSpace> _resultPolyhedron;
		private Point _resultPoint;
		private Random _random;
		private SeidelResultEnum _resultType;

		public SeidelResultEnum ResultType { get { return this._resultType; } }
		public Point ResultPoint { get { return this._resultPoint; } }

		public SeidelSolver(List<HalfSpace> halfSpaces, Vector vector)
		{
			var vectorSize = vector.GetDimension();
			if (!halfSpaces.TrueForAll(x => x.Plane.GetDimension() == vectorSize))
			{
				throw new ArgumentException("Not all planes match the vector dimension");
			}

			if (!halfSpaces.Any())
			{
				throw new ArgumentException("halfSpaces");
			}

			this._halfSpaces = halfSpaces;
			this._vector = vector;

			this._random = new Random(DateTime.Now.Millisecond);

			var coordinates = new double[vectorSize];
			for (int i = 0; i < vectorSize; i++)
			{
				coordinates[i] = vector.GetAt(i);
			}

			this._resultPoint = new Point(coordinates);
			this._resultPolyhedron = new List<HalfSpace>();
		}

		public void Run()
		{
			while (this._halfSpaces.Any() && this._resultType != SeidelResultEnum.Infeasible)
			{
				var space = this._halfSpaces[_random.Next(this._halfSpaces.Count)];

				this._halfSpaces.Remove(space);
				Iterate(space);
			}

			if (this._resultType == SeidelResultEnum.Ambigous && this._vector.GetDimension() > 1)
			{
				//if the result point is still ambigous it is required to iterate through vertices of polyhedron
				//firstly required to find vertices

				var resultVertices = new List<Point>();

				foreach (var item in this._resultPolyhedron)
				{
					foreach (var innerItem in this._resultPolyhedron)
					{
						var resultVertex = item.Plane.Intersect(innerItem.Plane);
						if (resultVertex != null && resultVertex is Point)
						{
							resultVertices.Add((Point)resultVertex);
						}
					}
				}

				if (resultVertices.Any())
				{
					foreach (var item in this._resultPolyhedron)
					{
						for (int i = 0; i < resultVertices.Count; i++)
						{
							if (!item.Contains(resultVertices[i]))
							{
								resultVertices.RemoveAt(i);
								i = i - 1;
							}
						}
					}

					var minimumPoint = resultVertices.First();

					for (int i = 0; i < this._vector.GetDimension(); i++)
					{
						if (this._vector.GetAt(i) > 0)
						{
							foreach (var item in resultVertices)
							{
								if (item.GetAt(i) < minimumPoint.GetAt(i))
								{
									minimumPoint = item;
								}
							}
						}
						else //if this._vector.GetAt(i) < 0
						{
							foreach (var item in resultVertices)
							{
								if (item.GetAt(i) > minimumPoint.GetAt(i))
								{
									minimumPoint = item;
								}
							}
						}
					}

					this._resultPoint = minimumPoint;
					this._resultType = SeidelResultEnum.Minimum;
				}
			}
		}

		private void Iterate(HalfSpace halfSpace)
		{
			this._resultPolyhedron.Add(halfSpace);

			if (this._resultPolyhedron.Count == 1)
			{
				this._resultType = SeidelResultEnum.Ambigous;
				this._resultPoint = halfSpace.Plane.Point;
				return;
			}

			//simply check if current result satisfies the new constraint
			if (this._resultType == SeidelResultEnum.Minimum)
			{
				if (!halfSpace.Contains(this._resultPoint))
				{
					return;
				}

				this._resultType = SeidelResultEnum.Ambigous;
			}

			if (this._resultType == SeidelResultEnum.Ambigous)
			{
				if (halfSpace.GetDimension() == 1)
				{
					var sections = new List<HalfSpace>();
					var processed = 0;
					foreach (var item in this._halfSpaces)
					{
						if (!sections.TrueForAll(x => x.Contains(item.Plane.Point)))
						{
							this._resultType = SeidelResultEnum.Infeasible;
							this._resultPoint = null;
							return;
						}

						sections.Add(item);
						processed++;

						var result = sections.First().Plane.Point;
						foreach (var section in sections)
						{
							if (this._vector.X > 0)
							{
								if (section.Plane.Point.X < result.X)
								{
									result = section.Plane.Point;
								}
							}
							else
							{
								if (section.Plane.Point.X > result.X)
								{
									result = section.Plane.Point;
								}
							}
						}

						this._resultPoint = result;
						this._resultType = SeidelResultEnum.Minimum;
					}
				}
				else
				{
					//move one dimension lower
					var passItems = new List<HalfSpace>();
					foreach (var item in this._halfSpaces)
					{
						passItems.Add(item.MoveDown());
					}

					passItems = passItems.Where(x => x.Plane.Exists).ToList();

					if (passItems.Count == 0)
					{
						return;
					}

					var innerSolver = new SeidelSolver(passItems, this._vector.MoveDown());

					innerSolver.Run();

					if (innerSolver.ResultType == SeidelResultEnum.Infeasible)
					{
						this._resultType = SeidelResultEnum.Infeasible;
						this._resultPoint = null;
						return;
					}

					this._resultType = innerSolver.ResultType;

					//searching point on current plane
					var plane = halfSpace.Plane;
					var planeNorm = plane.Vector;

					var result = 0.0;
					for (int i = 0; i < planeNorm.GetDimension(); i++)
					{
						if (i != planeNorm.GetDimension() - 1)
						{
							result -= planeNorm.GetAt(i);
						}
					}

					result += plane.D;
					result /= planeNorm.GetAt(planeNorm.GetDimension() - 1);

					var resultCoeffs = new double[planeNorm.GetDimension()];
					for (int i = 0; i < innerSolver.ResultPoint.GetDimension(); i++)
					{
						resultCoeffs[i] = innerSolver.ResultPoint.GetAt(i);
					}

					resultCoeffs[resultCoeffs.Length - 1] = result;

					this._resultPoint = new Point(resultCoeffs);
				}
			}
		}
	}
}
