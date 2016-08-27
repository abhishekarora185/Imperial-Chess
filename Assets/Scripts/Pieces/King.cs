/*
 * Author: Abhishek Arora
 * The Chess Engine class that controls Kings
 * */

using System.Collections.Generic;
using UnityEngine;

public class King : AbstractPiece
{
	// If the King is currently allowed to castle or not
	public bool canCastle;

	public King()
	{
		this.InitializationActions();
	}

	public King(Position position)
	{
		this.InitializationActions();
		this.SetCurrentPosition(position);
	}

	// Use this for initialization
	void Start()
	{
		this.InitializationActions();
	}

	// If the move was a castle, move the corresponding Rook as well, and disable further castling
	public override void PostMoveActions()
	{
		if (this.canCastle)
		{
			// Move the Rook
			// If this move was deemed valid in the first place, the Rook should present in the specified (below) position
			if (this.GetCurrentPosition().GetColumn() == 2)
			{
				// King side castling
				Rook kingSideRook = (Rook)this.chessBoard.GetPieceAtPosition(new Position(1, this.GetCurrentPosition().GetRow()));
				this.chessBoard.MoveTo(kingSideRook, new Position(3, kingSideRook.GetCurrentPosition().GetRow()));
			}
			else if (this.GetCurrentPosition().GetColumn() == 6)
			{
				// Queen side castling
				Rook queenSideRook = (Rook)this.chessBoard.GetPieceAtPosition(new Position(8, this.GetCurrentPosition().GetRow()));
				this.chessBoard.MoveTo(queenSideRook, new Position(5, queenSideRook.GetCurrentPosition().GetRow()));
			}
			// If the King has moved, it can no longer castle
			this.canCastle = false;
		}
	}

	public override void PerTurnProcessing()
	{
		// No per-turn processing for Kings
	}

	protected override void ComputeMoves()
	{
		// Compute universal moves for kings
		this.moves = new Dictionary<Position, Bitboard>();

		int column, row;

		for (column = Position.min; column <= Position.max; column++)
		{
			for (row = Position.min; row <= Position.max; row++)
			{
				Position position = new Position(column, row);
				Bitboard positionBitboard = new Bitboard();

				int positionRow, positionColumn;

				for (positionColumn = column - 1;
					positionColumn <= column + 1;
					positionColumn++)
				{
					for (positionRow = row - 1;
						positionRow <= row + 1;
						positionRow++)
					{
						if (positionColumn >= Position.min && positionColumn <=Position.max && positionRow >= Position.min && positionRow <= Position.max)
						{
							positionBitboard.FlipPosition(new Position(positionColumn, positionRow));
						}
					}
				}

				this.moves[position] = positionBitboard;
			}
		}
	}

	protected override Bitboard AdditionalMoveProcessing(Bitboard movesForCurrentPosition)
	{
		// Update static calculations based on the positions of pieces
		if (this.side == Side.Black)
		{
			movesForCurrentPosition = movesForCurrentPosition.ComputeRayIntersections(this.chessBoard.GetPieceLocations(Side.White), this.GetCurrentPosition(), true);
		}
		else
		{
			movesForCurrentPosition = movesForCurrentPosition.ComputeRayIntersections(this.chessBoard.GetPieceLocations(Side.Black), this.GetCurrentPosition(), true);
		}
		movesForCurrentPosition = movesForCurrentPosition.ComputeRayIntersections(this.chessBoard.GetPieceLocations(this.side), this.GetCurrentPosition(), false);

		// The current moving side check is needed since we don't want to compute castling as a move when we're trying to see if the king is checked
		// Castling
		if (this.chessBoard.CurrentMovingSide() == this.side &&
			this.canCastle && 
			!this.chessBoard.IsKingInCheck(this.side)
			)
		{
			// King side castling
			if (this.chessBoard.GetPieceAtPosition(new Position(1, this.GetCurrentPosition().GetRow())) != null &&
				typeof(Rook).IsInstanceOfType(this.chessBoard.GetPieceAtPosition(new Position(1, this.GetCurrentPosition().GetRow()))))
			{
				Rook kingSideRook = (Rook)this.chessBoard.GetPieceAtPosition(new Position(1, this.GetCurrentPosition().GetRow()));
				// If both move positions are unoccupied
				if (kingSideRook.canCastle &&
					this.chessBoard.GetPieceAtPosition(new Position(2, this.GetCurrentPosition().GetRow())) == null &&
					this.chessBoard.GetPieceAtPosition(new Position(3, this.GetCurrentPosition().GetRow())) == null
					)
				{
					// The final move will be filtered if the King's target position leaves it in check, so the last check here should be the intermediate position, or where the Rook will be after the move
					if (this.IsMoveSafe(new Position(3, this.GetCurrentPosition().GetRow())))
					{
						movesForCurrentPosition.FlipPosition(new Position(2, this.GetCurrentPosition().GetRow()));
					}
				}
			}

			// Queen side castling
			if (this.chessBoard.GetPieceAtPosition(new Position(8, this.GetCurrentPosition().GetRow())) != null &&
				typeof(Rook).IsInstanceOfType(this.chessBoard.GetPieceAtPosition(new Position(8, this.GetCurrentPosition().GetRow()))))
			{
				Rook queenSideRook = (Rook)this.chessBoard.GetPieceAtPosition(new Position(8, this.GetCurrentPosition().GetRow()));
				if (queenSideRook.canCastle &&
					this.chessBoard.GetPieceAtPosition(new Position(5, this.GetCurrentPosition().GetRow())) == null &&
					this.chessBoard.GetPieceAtPosition(new Position(6, this.GetCurrentPosition().GetRow())) == null
					)
				{
					// The final move will be filtered if the King's target position leaves it in check, so the last check here should be the intermediate position, or where the Rook will be after the move
					if (this.IsMoveSafe(new Position(5, this.GetCurrentPosition().GetRow())))
					{
						movesForCurrentPosition.FlipPosition(new Position(6, this.GetCurrentPosition().GetRow()));
					}
				}
			}
		}

		return movesForCurrentPosition;
	}

	public override AbstractPiece CopyPiece(AbstractPiece pieceToCopy)
	{
		pieceToCopy = base.CopyPiece(pieceToCopy);

		King kingToCopy = (King)pieceToCopy;

		kingToCopy.canCastle = this.canCastle;

		return kingToCopy;
	}

	protected override void InitializationActions()
	{
		this.ComputeMoves();
		this.canCastle = true;
	}

}
