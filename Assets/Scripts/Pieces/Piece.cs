using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractPiece
{
	public Side side;

	protected Chessboard chessBoard;

	protected Position currentPosition;

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

	public void SetChessboard(Chessboard chessBoard)
	{
		this.chessBoard = chessBoard;
	}

	public Bitboard GetMovesForCurrentPosition()
	{
		return this.AdditionalMoveProcessing(this.moves[this.currentPosition]);
	}

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

	public abstract void PostMoveActions();

	public virtual AbstractPiece CopyPiece(AbstractPiece pieceToCopy)
	{
		pieceToCopy.side = this.side;
		pieceToCopy.currentPosition = this.currentPosition;

		// Don't copy the chessboard as this function is mostly called when a new chessboard has to be created
		pieceToCopy.moves = this.moves;

		return pieceToCopy;
	}


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

	protected abstract void ComputeMoves();

	protected abstract Bitboard AdditionalMoveProcessing(Bitboard movesForCurrentPosition);

}
