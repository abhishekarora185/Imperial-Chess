using System;
using System.Collections.Generic;
public class AI
{
	private static int currentDepth = 0;

	public static Move computeBestMove(Side side, Chessboard chessboard)
	{
		// First, get the set of available moves
		List<Move> moves = new List<Move>();
		Move bestMove = new Move(null, null);

		foreach (AbstractPiece piece in chessboard.getActivePieces())
		{
			if (piece.side == side)
			{
				foreach (Position movePosition in piece.GetSafeMovesForCurrentPosition().GetPositions())
				{
					Move newMove = new Move(piece, movePosition);
					moves.Add(newMove);
					bestMove = newMove;
				}
			}
		}

		return bestMove;
	}


}

