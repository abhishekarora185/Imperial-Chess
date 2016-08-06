using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
public class AI
{
	public static bool terminateAllThreads;

	public Chessboard chessboardStateBeforeAIMove;

	public Thread AIThread;

	public bool doComputation;

	private bool isComputationDone;

	private Move bestMove;

	private int currentDepth;

	private int maxAllowableDepth;

	private int cutoffScore;

	public AI()
	{
		terminateAllThreads = false;

		this.AIThread = new Thread(this.AIComputation);
		this.AIThread.IsBackground = true;

		this.isComputationDone = false;
		this.doComputation = false;
		this.currentDepth = 0;
		this.maxAllowableDepth = Constants.AI.maxAllowedDepth;
		this.cutoffScore = 2;

		this.AIThread.Start();
	}

	public void AIComputation()
	{
		// Keep spinning till allowed to compute the next move
		Debug.Log("Thread " + Thread.CurrentThread.ManagedThreadId + " waiting to compute!");
		while (!terminateAllThreads && !this.doComputation);
		Debug.Log("Thread " + Thread.CurrentThread.ManagedThreadId + " starting computation.");

		if (terminateAllThreads)
		{
			// When the application is closed, make sure all threads die
			return;
		}

		this.isComputationDone = false;

		this.bestMove = this.computeBestMove(chessboardStateBeforeAIMove.CurrentMovingSide(), chessboardStateBeforeAIMove, 0);
		Debug.Log("Thread " + Thread.CurrentThread.ManagedThreadId + " finished computation.");

		this.isComputationDone = true;
		this.doComputation = false;

		this.AIComputation();
	}

	public void PostAIComputation()
	{
		// Make sure this is done on the main thread
		// Since we've duplicated the chessboard, the piece associate with the move needs to be set to the actual game chessboard's instance
		this.isComputationDone = false;
		Move moveToMake = new Move(GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard.GetPieceAtPosition(bestMove.getPiece().GetCurrentPosition()), bestMove.getPosition());
		MoveActions.standardMoveActions(moveToMake);
	}

	public bool isAIComputationDone()
	{
		return this.isComputationDone;
	}

	private Move computeBestMove(Side side, Chessboard chessboard, int presentCumulativeScore)
	{
		currentDepth++;

		// First, get the set of available moves
		List<Move> moves = new List<Move>();
		Move bestMoveForIteration = new Move(null, null);

		bool isPlayingSide = chessboard.CurrentMovingSide() == side;

		foreach (AbstractPiece piece in chessboard.getActivePieces())
		{
			if (piece.side == chessboard.CurrentMovingSide())
			{
				foreach (Position movePosition in piece.GetSafeMovesForCurrentPosition().GetPositions())
				{
					Move newMove = MoveEvaluator.EvaluateMove(new Move(piece, movePosition), isPlayingSide);
					moves = InsertionSortMove(moves, newMove);
				}
			}
		}

		if (moves.Count > 0)
		{
			bestMoveForIteration = moves.ToArray()[0];
			if (currentDepth < maxAllowableDepth)
			{
				foreach (Move move in moves)
				{
					Chessboard copyChessboard = Chessboard.MakeCopyOfChessboard(chessboard);
					copyChessboard.MoveTo(copyChessboard.GetPieceAtPosition(move.getPiece().GetCurrentPosition()), move.getPosition());
					copyChessboard.ChangeMovingSide();
					int followUpScore = computeBestMove(side, copyChessboard, presentCumulativeScore + move.getScore()).getScore();

					if (move.getScore() + followUpScore > bestMoveForIteration.getScore())
					{
						bestMoveForIteration = move;
					}
				}
			}
		}

		currentDepth--;

		return bestMoveForIteration;
	}

	// Use this function while evaluating one turn's moves, as only absolute values are used for evaluation
	private static List<Move> InsertionSortMove(List<Move> moves, Move newMove)
	{
		int index = 0;
		foreach (Move move in moves)
		{
			if (Mathf.Abs(newMove.getScore()) > Mathf.Abs(move.getScore()))
			{
				break;
			}
			index++;
		}

		moves.Insert(index, newMove);
		return moves;
	}

}

public class MoveEvaluator
{
	public enum MoveType {
		NormalMove,
		CapturePiece,
		PromotePawn,
		CheckOpponent,
		CheckmateOpponent
	};

	public static Dictionary<MoveType, int> worthOfMoves = new Dictionary<MoveType, int>
	{
		{MoveType.NormalMove, 1},
		{MoveType.CapturePiece, 2},
		{MoveType.PromotePawn, 11},
		{MoveType.CheckOpponent, 13},
		{MoveType.CheckmateOpponent, 20}
	};

	public static Dictionary<Type, int> worthOfPieces = new Dictionary<Type, int>
	{
		{typeof(Pawn), 1},
		{typeof(Bishop), 2},
		{typeof(Rook), 3},
		{typeof(Knight), 4},
		{typeof(Queen), 5},
		{typeof(King), 10}
	};

	public static Move EvaluateMove(Move move, bool isPlayingSide)
	{
		int score = worthOfMoves[MoveType.NormalMove];

		Type capturedPieceType = isMoveAnOpponentCapture(move);
		if (capturedPieceType != typeof(AbstractPiece))
		{
			score += worthOfMoves[MoveType.CapturePiece] * worthOfPieces[capturedPieceType];
		}

		if (isMovePawnPromotion(move))
		{
			score += worthOfMoves[MoveType.PromotePawn];
		}
        /*
		if (willMoveCheckOpponent(move))
		{
			score += worthOfMoves[MoveType.CheckOpponent];
		}

		if (willMoveCheckmateOpponent(move))
		{
			score += worthOfMoves[MoveType.CheckmateOpponent];
		}
        */
		// If we're currently computing a move's value for the AI's opponent, the score computed should be negative
		if (!isPlayingSide)
		{
			score = -score;
		}
		move.setScore(score);
		return move;
	}

	private static Type isMoveAnOpponentCapture(Move move)
	{
		Type capturedPieceType = typeof(AbstractPiece);

		Chessboard gameState = move.getPiece().GetChessboard();
		AbstractPiece pieceAtMovePosition = gameState.GetPieceAtPosition(move.getPosition());

		//TODO: Make it easier to get en passant positions
		Position enPassantPosition;
		AbstractPiece pieceAtEnPassantPosition = null;
		
		if (move.getPiece().GetType() == typeof(Pawn))
		{
			if (move.getPiece().side == Side.Black)
			{
				enPassantPosition = new Position(move.getPosition().GetColumn(), move.getPosition().GetRow() + 1);
			}
			else
			{
				enPassantPosition = new Position(move.getPosition().GetColumn(), move.getPosition().GetRow() - 1);
			}
			pieceAtEnPassantPosition = gameState.GetPieceAtPosition(enPassantPosition);

			if (pieceAtEnPassantPosition != null && (pieceAtEnPassantPosition.GetType() != typeof(Pawn) || pieceAtEnPassantPosition.side == move.getPiece().side))
			{
				pieceAtEnPassantPosition = null;
			}
		}

		// Normal captures
		if(pieceAtMovePosition != null && pieceAtMovePosition.side != move.getPiece().side)
		{
			capturedPieceType = pieceAtMovePosition.GetType();
		}
		//En passant capture
		else if (pieceAtEnPassantPosition != null && ((Pawn)pieceAtEnPassantPosition).allowEnPassantCapture)
		{
			capturedPieceType = pieceAtEnPassantPosition.GetType();
		}

		return capturedPieceType;
	}

	private static bool isMovePawnPromotion(Move move)
	{
		bool isPawnPromotion = false;
		if (move.getPiece().GetType() == typeof(Pawn))
		{
			if (move.getPiece().side == Side.Black && move.getPosition().GetRow() == Position.min ||
				move.getPiece().side == Side.White && move.getPosition().GetRow() == Position.max)
			{
				isPawnPromotion = true;
			}
		}
		return isPawnPromotion;
	}

	private static bool willMoveCheckOpponent(Move move)
	{
		bool willCheckOpponent = false;
		Chessboard gameStateAfterMove = Chessboard.MakeCopyOfChessboard(move.getPiece().GetChessboard());
		AbstractPiece correspondingMovingPiece = gameStateAfterMove.GetPieceAtPosition(move.getPiece().GetCurrentPosition());
		gameStateAfterMove.MoveTo(correspondingMovingPiece, move.getPosition());

		if (gameStateAfterMove.IsKingInCheck(gameStateAfterMove.CurrentMovingSide()))
		{
			willCheckOpponent = true;
		}

		return willCheckOpponent;
	}

	private static bool willMoveCheckmateOpponent(Move move)
	{
		bool willCheckmateOpponent = false;
		Chessboard gameStateAfterMove = Chessboard.MakeCopyOfChessboard(move.getPiece().GetChessboard());
		AbstractPiece correspondingMovingPiece = gameStateAfterMove.GetPieceAtPosition(move.getPiece().GetCurrentPosition());
		gameStateAfterMove.MoveTo(correspondingMovingPiece, move.getPosition());

		if (gameStateAfterMove.IsKingInCheckmate(gameStateAfterMove.CurrentMovingSide()))
		{
			willCheckmateOpponent = true;
		}

		return willCheckmateOpponent;
	}

}

