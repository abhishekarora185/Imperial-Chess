using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractPiece : MonoBehaviour
{
	public Side side;

	protected Chessboard chessBoard;

	protected Position currentPosition;

	protected Dictionary<Position, Bitboard> moves;

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

	// Processing done at the beginning of each turn
	public abstract void PerTurnProcessing();

	public abstract void PostMoveActions();

	protected abstract void ComputeMoves();

	protected abstract Bitboard AdditionalMoveProcessing(Bitboard movesForCurrentPosition);

}
