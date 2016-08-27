/*
 * Author: Abhishek Arora
 * The Chess Engine class that controls Rooks
 * */

using System.Collections.Generic;
using UnityEngine;

public class Rook : AbstractPiece
{
	// If the Rook can take part in castling or not
	public bool canCastle;

	public Rook()
	{
		this.InitializationActions();
	}

	public Rook(Position position)
	{
		this.InitializationActions();
		this.SetCurrentPosition(position);
	}

	// Use this for initialization
	void Start()
	{
		this.InitializationActions();
	}

	public override void PostMoveActions()
	{
		// If the rook has moved, it can no longer take part in castling
		if (this.canCastle)
		{
			this.canCastle = false;
		}
	}

	public override void PerTurnProcessing()
	{
		// No per-turn processing for Rooks
	}

	protected override void ComputeMoves()
	{
		// Compute universal moves for rooks
		this.moves = new Dictionary<Position, Bitboard>();

		int column, row;

		for (column = Position.min; column <= Position.max; column++)
		{
			for (row = Position.min; row <= Position.max; row++)
			{
				Position position = new Position(column, row);
				Bitboard positionBitboard = new Bitboard();

				int positionRow, positionColumn;
				for (positionColumn = Position.min; positionColumn <= Position.max; positionColumn++)
				{
					if (positionColumn != column)
					{
						positionBitboard.FlipPosition(new Position(positionColumn, row));
					}
				}
				for (positionRow = Position.min; positionRow <= Position.max; positionRow++)
				{
					if (positionRow != row)
					{
						positionBitboard.FlipPosition(new Position(column, positionRow));
					}
				}

				this.moves[position] = positionBitboard;
			}
		}
	}

	// Only update static calculations with the current board state
	protected override Bitboard AdditionalMoveProcessing(Bitboard movesForCurrentPosition)
	{
		if (this.side == Side.Black)
		{
			movesForCurrentPosition = movesForCurrentPosition.ComputeRayIntersections(this.chessBoard.GetPieceLocations(Side.White), this.GetCurrentPosition(), true);
		}
		else
		{
			movesForCurrentPosition = movesForCurrentPosition.ComputeRayIntersections(this.chessBoard.GetPieceLocations(Side.Black), this.GetCurrentPosition(), true);
		}
		movesForCurrentPosition = movesForCurrentPosition.ComputeRayIntersections(this.chessBoard.GetPieceLocations(this.side), this.GetCurrentPosition(), false);

		return movesForCurrentPosition;
	}

	public override AbstractPiece CopyPiece(AbstractPiece pieceToCopy)
	{
		pieceToCopy = base.CopyPiece(pieceToCopy);

		Rook rookToCopy = (Rook)pieceToCopy;

		rookToCopy.canCastle = this.canCastle;

		return rookToCopy;
	}

	protected override void InitializationActions()
	{
		this.ComputeMoves();
		this.canCastle = true;
	}

}
