/*
 * Author: Abhishek Arora
 * The Chess Engine class that defines the Bitboard, a representation of a chess board state that allows for fast computations
 * Bitwise operations are carried out by this class, while high-level operations are exposed as methods for other chess engine classes to easily use
 * */

using System.Collections.Generic;
using UnityEngine;
public class Bitboard {

	// Each byte is a row, starting from the -ve X-axis (white side)
	// There will, obviously, always be 8 bytes stored, given the dimensions of a standard chessBoard
	private byte[] bitboard;

	public Bitboard()
	{
		this.bitboard = new byte[8];
	}

	public byte[] GetBitboard()
	{
		return this.bitboard;
	}

	public int ValueAtPosition(Position position)
	{
		int valueAtPosition = this.bitboard[position.GetRow() - 1] & (byte)(Mathf.Pow(2, 7 - (position.GetColumn() - 1)));
		return valueAtPosition;
	}

	public int FlipPosition(Position position)
	{
		// Flip the bit corresponding to a particular position on the board
		int valueAtPosition = this.bitboard[position.GetRow() - 1] & (byte)(Mathf.Pow(2, 7 - (position.GetColumn() - 1)));
		this.bitboard[position.GetRow() - 1] ^= (byte)(Mathf.Pow(2, 7 - (position.GetColumn() - 1)));
		return valueAtPosition;
	}

	public List<Position> GetPositions()
	{
		// Return all Positions corresponding to set bits in the bitboard
		List<Position> setPositions = new List<Position>();

		int row;
		for (row = 0; row < this.bitboard.Length; row++)
		{
			int column;
			for (column = Position.min; column <= Position.max; column++)
			{
				if ((this.bitboard[row] & (byte)(Mathf.Pow(2, 7 - (column - 1)))) > 0)
				{
					setPositions.Add(new Position(column, row + 1));
				}
			}
		}

		return setPositions;
	}

	public Bitboard ComplementBitboard(Bitboard bitboardToComplement)
	{
		// Flips all the bits of the passed bitboard and returns the result
		Bitboard newBitboard = new Bitboard();
		int row;

		for (row = 0; row < this.bitboard.Length; row++)
		{
			newBitboard.GetBitboard()[row] = (byte) ~ bitboardToComplement.GetBitboard()[row];
		}

		return newBitboard;
	}

	public Bitboard IntersectBitboard(Bitboard bitboardToIntersect)
	{
		Bitboard newBitboard = new Bitboard();
		int row;
		// Get all intersections of this bitboard with the other
		for (row = 0; row < this.bitboard.Length; row++)
		{
			newBitboard.GetBitboard()[row] = (byte)((this.bitboard[row] ^ bitboardToIntersect.GetBitboard()[row]) & this.bitboard[row]);
		}
		return newBitboard;
	}

	public Bitboard ComputeRayIntersections(Bitboard bitboardToIntersect, Position movingPiecePosition, bool includeIntersectionPosition)
	{
		// Recompute this bitboard after stopping all rays from the moving piece that are intersecting the passed bitboard
		Bitboard newBitboard = new Bitboard();
		// Compute a bitboard containing all intersection points
		Bitboard intersectBitboard = this.ComplementBitboard(this.IntersectBitboard(bitboardToIntersect));
		int row;
		for (row = 0; row < this.bitboard.Length; row++)
		{
			newBitboard.GetBitboard()[row] = (byte) this.GetBitboard()[row];
			intersectBitboard.GetBitboard()[row] = (byte) (intersectBitboard.GetBitboard()[row] & newBitboard.GetBitboard()[row]);
		}

		// Now, for each intersect position, go along the unit vector connecting the moving position to this position and wipe out all points till the edge of the bitboard
		// These "vectors" can only be horizontal, vertical or diagonal
		foreach (Position intersectionPosition in intersectBitboard.GetPositions())
		{
			Vector2 positionVector = new Vector2(intersectionPosition.GetColumn() - movingPiecePosition.GetColumn(),
				intersectionPosition.GetRow() - movingPiecePosition.GetRow());
			if (positionVector.x > 1 || positionVector.x < -1)
			{
				positionVector.x = positionVector.x / Mathf.Abs(positionVector.x);
			}
			if (positionVector.y > 1 || positionVector.y < -1)
			{
				positionVector.y = positionVector.y / Mathf.Abs(positionVector.y);
			}

			Position clearPosition = new Position(intersectionPosition.GetColumn(), intersectionPosition.GetRow());
			if (includeIntersectionPosition)
			{
				// Don't process the intersection position
				clearPosition.SetColumn(clearPosition.GetColumn() + (int)positionVector.x);
				clearPosition.SetRow(clearPosition.GetRow() + (int)positionVector.y);
			}

			while (clearPosition.GetColumn() >= Position.min && clearPosition.GetColumn() <= Position.max
				&& clearPosition.GetRow() >= Position.min && clearPosition.GetRow() <= Position.max)
			{
				if (newBitboard.ValueAtPosition(clearPosition) > 0)
				{
					newBitboard.FlipPosition(clearPosition);
				}
				else
				{
					break;
				}
				clearPosition.SetColumn(clearPosition.GetColumn() + (int) positionVector.x);
				clearPosition.SetRow(clearPosition.GetRow() + (int)positionVector.y);
			}
		}

		return newBitboard;
	}

	public void PrintBitboard()
	{
		// For debugging purposes only
		int row;
		for (row = 0; row < this.bitboard.Length; row++)
		{
			string rowBits = string.Empty;
			int column;
			for (column = Position.min; column <= Position.max; column++)
			{
				string appendValue = "0";
				if ((this.bitboard[row] & (byte)(Mathf.Pow(2, 7 - (column - 1)))) > 0)
				{
					appendValue = "1";
				}
				rowBits += appendValue + " ";
			}

			Debug.Log(rowBits);
		}
	}
}
