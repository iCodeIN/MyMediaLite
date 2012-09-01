// Copyright (C) 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
//
// This file is part of MyMediaLite.
//
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using MyMediaLite.DataType;

namespace MyMediaLite.Correlation
{
	/// <summary>Class for storing and 'bi-directional' computing conditional probabilities</summary>
	/// <remarks>
	/// TODO LIT
	/// </remarks>
	///
	public sealed class BidirectionalConditionalProbability : AsymmetricCorrelationMatrix, IBinaryDataCorrelationMatrix
	{
		float Alpha { get; set; } // TODO check value

		/// <summary>Creates an object of type BidirectionalConditionalProbability</summary>
		/// <param name="num_entities">the number of entities</param>
		/// <param name="alpha">alpha parameter</param>
		public BidirectionalConditionalProbability(int num_entities, float alpha) : base(num_entities)
		{
			Alpha = alpha;
		}

		/// <summary>Creates conditional probability matrix from given data</summary>
		/// <param name="vectors">the boolean data</param>
		/// <param name="alpha">alpha parameter</param>
		/// <returns>the similarity matrix based on the data</returns>
		static public BidirectionalConditionalProbability Create(IBooleanMatrix vectors, float alpha)
		{
			BidirectionalConditionalProbability cm;
			int num_entities = vectors.NumberOfRows;
			try
			{
				cm = new BidirectionalConditionalProbability(num_entities, alpha);
			}
			catch (OverflowException)
			{
				Console.Error.WriteLine("Too many entities: " + num_entities);
				throw;
			}
			cm.ComputeCorrelations(vectors);
			return cm;
		}

		///
		public void ComputeCorrelations(IBooleanMatrix entity_data)
		{
			var transpose = entity_data.Transpose() as IBooleanMatrix;

			var overlap = new SymmetricMatrix<int>(entity_data.NumberOfRows);

			// go over all (other) entities
			for (int row_id = 0; row_id < transpose.NumberOfRows; row_id++)
			{
				var row = transpose.GetEntriesByRow(row_id);
				for (int i = 0; i < row.Count; i++)
				{
					int x = row[i];
					for (int j = 0; j < row.Count; j++)
					{
						int y = row[j];
						overlap[x, y]++;
					}
				}
			}

			// the diagonal of the correlation matrix
			for (int i = 0; i < num_entities; i++)
				this[i, i] = 1;

			// compute conditional probabilities
			float alpha = Alpha;
			float one_minus_alpha = 1 - alpha;
			for (int x = 0; x < num_entities; x++)
				for (int y = 0; y < x; y++)
				{
					double x_given_y = (double) (overlap[x, y] / entity_data.NumEntriesByRow(x));
					double y_given_x = (double) (overlap[x, y] / entity_data.NumEntriesByRow(y));

					this[x, y] = (float) ( Math.Pow(x_given_y, alpha) * Math.Pow(y_given_x, one_minus_alpha) );
					this[y, x] = (float) ( Math.Pow(y_given_x, alpha) * Math.Pow(x_given_y, one_minus_alpha) );
				}
		}

		///
		public float ComputeCorrelation(ICollection<int> vector_i, ICollection<int> vector_j)
		{
			int cntr = 0;
			foreach (int k in vector_j)
				if (vector_i.Contains(k))
					cntr++;
			return (float) ( Math.Pow(cntr / vector_i.Count, Alpha) * Math.Pow(cntr / vector_j.Count, 1 - Alpha) );
		}
	}
}
