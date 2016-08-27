/*
 * Author: Abhishek Arora
 * The Chess Engine Abstract class for all pieces that defines some of their functionality, but leaves some to be implemented by them
 * */

using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractPiece
{
	// The side to which the piece belongs
	public Side side;

	// The Chessboard associated with the piece
	protected Chessboard chessBoard;

	// The current position of this piece on the chessBoard associated with it
	protected Position currentPosition;

	// The statically computed moves (rays) for the piece at each position
	protected Dictionary<Position, Bitboard> moves;

	public AbstractPiece()
	{
		this.InitializationActions();
	}

	public Position GetCurrentPosition()
	{
		return this.currentPosition;
	}

	public void SetCurrentPosition(Position position)
	{
		this.currentPosition = position;
	}

	public Chessboard GetChessboard()
	{
		return this.chessBoard;
	}

	public void SetChessboard(Chessboard chessBoard)
	{
		this.chessBoard = chessBoard;
	}

	// Return all the moves from the piece's current position given the additional processing that needs to be carried out for the chessBoard's current state
	public Bitboard GetMovesForCurrentPosition()
	{
		return this.AdditionalMoveProcessing(this.moves[this.currentPosition]);
	}

	// Return all the valid moves from the piece's current position given the additional processing that needs to be carried out for the chessBoard's current state
	public Bitboard GetSafeMovesForCurrentPosition()
	{
		Bitboard availableMoves = this.GetMovesForCurrentPosition();
		foreach (Position position in availableMoves.GetPositions())
		{
			if (!IsMoveSafe(position))
			{
				availableMoves.FlipPosition(position);
			}
		}
		return availableMoves;
	}

	// Processing done at the beginning of each turn
	public abstract void PerTurnProcessing();

	// Processing done after the piece has moved
	public abstract void PostMoveActions();

	// This can be overriden by child classes if they have additional members that need to be copied
	public virtual AbstractPiece CopyPiece(AbstractPiece pieceToCopy)
	{
		pieceToCopy.side = this.side;
		pieceToCopy.currentPosition = this.currentPosition;

		// Don't copy the chessboard as this method is mostly called when a new chessboard has to be created
		pieceToCopy.moves = this.moves;

		return pieceToCopy;
	}

	// Will my King be in check after I make this move?
	protected bool IsMoveSafe(Position movePosition)
	{
		bool safeMove = true;

		Chessboard copyChessboard = Chessboard.MakeCopyOfChessboard(this.chessBoard);
		copyChessboard.MoveTo(copyChessboard.GetPieceAtPosition(this.currentPosition), movePosition);

		if (copyChessboard.IsKingInCheck(this.side))
		{
			safeMove = false;
		}

		return safeMove;
	}

	protected abstract void InitializationActions();

	// Statically compute all moves from all positions on the board for this piece
	protected abstract void ComputeMoves();

	// Dynamic move computation for this piece, for a given position and board state
	protected abstract Bitboard AdditionalMoveProcessing(Bitboard movesForCurrentPosition);

}
