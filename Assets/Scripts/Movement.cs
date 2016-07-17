using System;
using System.Collections.Generic;
using UnityEngine;

public class Position : IEquatable<Position>
{
	private int row;

	private int column;

	public static int min = 1;

	public static int max = 8;

	public Position(int column, int row)
	{
		this.row = row;
		this.column = column;
	}

	public int GetRow()
	{
		return this.row;
	}

	public void SetRow(int row)
	{
		this.row = row;
	}

	public int GetColumn()
	{
		return this.column;
	}

	public void SetColumn(int column)
	{
		this.column = column;
	}

	public override string ToString()
	{
		return ((char)(64 + this.GetColumn()) + string.Empty + this.GetRow());
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as Position);
	}

	public bool Equals(Position position)
	{
		return (position != null && this.GetColumn() == position.GetColumn() && this.GetRow() == position.GetRow());
	}

	public override int GetHashCode()
	{
		return (19 + this.GetRow()) * 19 + this.GetColumn();
	}
}

public class Move
{
	private Position toPosition;

	private AbstractPiece pieceToMove;

	private int score;

	public Move(AbstractPiece pieceToMove, Position moveToPosition)
	{
		this.pieceToMove = pieceToMove;
		this.toPosition = moveToPosition;
	}

	public Position getPosition()
	{
		return this.toPosition;
	}

	public AbstractPiece getPiece()
	{
		return this.pieceToMove;
	}

	public int getScore()
	{
		return this.score;
	}

	public void setScore(int score)
	{
		this.score = score;
	}
}

public class MoveActions
{
	public static void standardMoveActions(Move move)
	{
		TryPlayDeathAnimation(move);

		if (move.getPiece().GetType().Name == Constants.PieceClassNames.Pawn)
		{
			TryAnimateEnPassantCapture(move);
		}

		if (move.getPiece().GetType().Name == Constants.PieceClassNames.King)
		{
			TryAnimateRookCastling(move);
		}

		GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard.MoveTo(move.getPiece(), move.getPosition());
		move.getPiece().gameObject.GetComponent<PieceBehaviour>().isInAnimationState = true;
		move.getPiece().PostMoveActions();
	}

	private static void TryPlayDeathAnimation(Move move)
	{
		AbstractPiece pieceAtMovePosition = GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard.GetPieceAtPosition(move.getPosition());

		if (pieceAtMovePosition != null)
		// Switch to action cam for some awesomeness
		{
			GameObject.Find(Constants.ActionCameraObject).GetComponent<ActionCamera>().EnableActionCamera(move.getPiece(), pieceAtMovePosition);
			GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().DestroyPiece(pieceAtMovePosition);
		}
	}

	private static void TryAnimateEnPassantCapture(Move move)
	{
		Position enPassantPosition;
		if (move.getPiece().side == Side.Black)
		{
			enPassantPosition = new Position(move.getPosition().GetColumn(), move.getPosition().GetRow() + 1);
		}
		else
		{
			enPassantPosition = new Position(move.getPosition().GetColumn(), move.getPosition().GetRow() - 1);
		}

		AbstractPiece pieceAtEnPassantPosition = GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard.GetPieceAtPosition(enPassantPosition);

		Pawn dyingPawn = null;

		if (pieceAtEnPassantPosition != null && pieceAtEnPassantPosition.GetType().Name == Constants.PieceClassNames.Pawn && pieceAtEnPassantPosition.side != move.getPiece().side && ((Pawn)pieceAtEnPassantPosition).allowEnPassantCapture)
		{
			dyingPawn = (Pawn)pieceAtEnPassantPosition;
		}

		if (dyingPawn != null)
		{
			GameObject.Find(Constants.ActionCameraObject).GetComponent<ActionCamera>().EnableActionCamera(move.getPiece(), dyingPawn);
			GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().DestroyPiece(dyingPawn);
		}
	}

	private static void TryAnimateRookCastling(Move move)
	{
		if (move.getPosition().GetRow() == move.getPiece().GetCurrentPosition().GetRow() &&
			Mathf.Abs(move.getPosition().GetColumn() - move.getPiece().GetCurrentPosition().GetColumn()) == 2)
		{
			// Castling is happening!
			if (move.getPosition().GetColumn() == 2)
			{
				// King side
				GameObject rookGameObject = null;

				foreach (GameObject piece in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
				{
					if (piece.GetComponent<AbstractPiece>().side == move.getPiece().side && piece.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Rook && piece.GetComponent<AbstractPiece>().GetCurrentPosition().GetColumn() < move.getPosition().GetColumn())
					{
						rookGameObject = piece;
						break;
					}
				}
				if (rookGameObject != null)
				{
					rookGameObject.GetComponent<PieceBehaviour>().isInAnimationState = true;
				}
			}
			else if (move.getPosition().GetColumn() == 6)
			{
				// Queen side
				GameObject rookGameObject = null;

				foreach (GameObject piece in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
				{
					if (piece.GetComponent<AbstractPiece>().side == move.getPiece().side && piece.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Rook && piece.GetComponent<AbstractPiece>().GetCurrentPosition().GetColumn() > move.getPosition().GetColumn())
					{
						rookGameObject = piece;
						break;
					}
				}
				if (rookGameObject != null)
				{
					rookGameObject.GetComponent<PieceBehaviour>().isInAnimationState = true;
				}
			}
		}
	}
}