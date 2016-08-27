/*
 * Author: Abhishek Arora
 * The Chess Engine class that represents a Chessboard
 * A "Chessboard" defined as such is simply a board state, and is not strictly the board seen by the user in the game
 * Several different board states can be used to check if moves are safe, and hence, there is functionality to copy a chessboard
 * */

using UnityEngine;
using System.Collections.Generic;

public class Chessboard {

	// Bitboards marking the positions of pieces of each side; this helps with the dynamic move processing for pieces
	private Bitboard blackPieceLocations;

	private Bitboard whitePieceLocations;

	// The side whose turn it currently is
	private Side movingSide;

	// The pieces that are still alive on this board
	private List<AbstractPiece> activePieces;

	public Chessboard() {
		this.blackPieceLocations = new Bitboard();
		this.whitePieceLocations = new Bitboard();
		this.movingSide = Side.Black;
		this.activePieces = new List<AbstractPiece>();
	}

	public Side CurrentMovingSide()
	{
		return this.movingSide;
	}

	public void ChangeMovingSide()
	{
		if (this.movingSide == Side.White)
		{
			this.movingSide = Side.Black;
		}
		else
		{
			this.movingSide = Side.White;
		}

		// Perform Per-turn processing on each piece of the playing side for them to recompute their own statuses
		foreach (AbstractPiece piece in this.activePieces)
		{
			if (piece.side == this.movingSide)
			{
				piece.PerTurnProcessing();
			}
		}
	}

	public List<AbstractPiece> getActivePieces()
	{
		return this.activePieces;
	}

	// Called by pieces associated with this board whenever they need to move
	public void MoveTo(AbstractPiece piece, Position position)
	{
		// First, modify the relevant bitboard(s)
		if (piece.side == Side.Black)
		{
			this.blackPieceLocations.FlipPosition(piece.GetCurrentPosition());
			this.blackPieceLocations.FlipPosition(position);
		}
		else
		{
			this.whitePieceLocations.FlipPosition(piece.GetCurrentPosition());
			this.whitePieceLocations.FlipPosition(position);
		}

		// Kill anything that gets in the way
		this.KillPieceAtPosition(position, piece);

		// Now, actually move the piece
		piece.SetCurrentPosition(position);

		piece.PostMoveActions();
	}

	public Bitboard GetPieceLocations(Side side)
	{
		if (side == Side.Black)
		{
			return this.blackPieceLocations;
		}
		else
		{
			return this.whitePieceLocations;
		}
	}

	public void FlipBitAtPositionOfBitboard(Side side, Position position)
	{
		if (side == Side.Black)
		{
			this.blackPieceLocations.FlipPosition(position);
		}
		else
		{
			this.whitePieceLocations.FlipPosition(position);
		}
	}

	// Adds a new piece to the board
	public void AddPiece(AbstractPiece newPiece)
	{
		AbstractPiece existingPiece = null;

		// Assuming the piece's position is already set...
		foreach (AbstractPiece piece in this.activePieces)
		{
			if (newPiece.GetCurrentPosition() == piece.GetCurrentPosition())
			{
				// This handles pawn promotion
				existingPiece = piece;
			}
		}

		// Only happens during pawn promotion
		if (existingPiece != null)
		{
			this.activePieces.Remove(existingPiece);
		}

		this.activePieces.Add(newPiece);
	}

	public AbstractPiece GetPieceAtPosition(Position position)
	{
		AbstractPiece foundPiece = null;
		foreach (AbstractPiece piece in this.activePieces)
		{
			if (position.Equals(piece.GetCurrentPosition()))
			{
				foundPiece = piece;
			}
		}

		return foundPiece;
	}

	public void KillPieceAtPosition(Position position, AbstractPiece movingPiece)
	{
		AbstractPiece foundPiece = null;

		foreach (AbstractPiece piece in this.activePieces)
		{
			if (position.Equals(piece.GetCurrentPosition()))
			{
				foundPiece = piece;
				Side side = foundPiece.side;

				if(side == Side.Black)
				{
					this.blackPieceLocations.FlipPosition(position);
				}
				else
				{
					this.whitePieceLocations.FlipPosition(position);
				}

				this.activePieces.Remove(piece);

				break;
			}
		}
	}

	public List<AbstractPiece> GetPiecesOfTypeAndSide(System.Type type, Side side)
	{
		List<AbstractPiece> foundPieces = new List<AbstractPiece>();

		foreach (AbstractPiece piece in this.activePieces)
		{
			if (piece.side == side && piece.GetType() == type)
			{
				foundPieces.Add(piece);
			}
		}

		return foundPieces;
	}

	public bool IsKingInCheck(Side sideToCheck)
	{
		// Checks the moves of the opposing side
		bool inCheck = false;

		// Common sense dictates that there must be one and only one king on either side
		King kingToCheck = (King)GetPiecesOfTypeAndSide(typeof(King), sideToCheck).ToArray()[0];

		foreach (AbstractPiece activePiece in this.activePieces)
		{
			if (activePiece.side != sideToCheck)
			{
				Bitboard availableMoves = activePiece.GetMovesForCurrentPosition();

				if (availableMoves.ValueAtPosition(kingToCheck.GetCurrentPosition()) > 0)
				{
					// Special check for Pawns advancing forward - they are of no harm and must not be considered here
					if (typeof(Pawn).IsInstanceOfType(activePiece) && activePiece.GetCurrentPosition().GetColumn() == kingToCheck.GetCurrentPosition().GetColumn())
					{
						// Laugh it off
					}
					else
					{
						// Be very scared
						inCheck = true;
					}
				}
			}
		}

		return inCheck;
	}

	public bool IsKingInCheckmate(Side sideToCheck)
	{
		// Checks the moves of all the pieces on the moving side. If the king is in check and there are no available moves, it's game over!
		bool inCheckmate = false, inCheck = IsKingInCheck(sideToCheck), cannotMove = true;

		foreach (AbstractPiece activePiece in this.activePieces)
		{
			if (activePiece.side == sideToCheck && activePiece.GetSafeMovesForCurrentPosition().GetPositions().Count > 0)
			{
				cannotMove = false;
			}
		}

		if (inCheck && cannotMove)
		{
			inCheckmate = true;
		}

		return inCheckmate;
	}

	// Copies are made for computational decisions like check/checkmate
	public static Chessboard MakeCopyOfChessboard(Chessboard chessboardToCopy)
	{
		Chessboard newChessboard = new Chessboard();
		newChessboard.blackPieceLocations = new Bitboard();
		newChessboard.whitePieceLocations = new Bitboard();
		newChessboard.blackPieceLocations = chessboardToCopy.blackPieceLocations.IntersectBitboard(newChessboard.blackPieceLocations);
		newChessboard.whitePieceLocations = chessboardToCopy.whitePieceLocations.IntersectBitboard(newChessboard.whitePieceLocations);
		newChessboard.movingSide = chessboardToCopy.movingSide;

		foreach (AbstractPiece piece in chessboardToCopy.activePieces)
		{
			System.Type pieceType = piece.GetType();
			AbstractPiece newPiece = null;

			if (pieceType == typeof(Pawn))
			{
				newPiece = new Pawn(piece.GetCurrentPosition());
			}
			else if (pieceType == typeof(Rook))
			{
				newPiece = new Rook(piece.GetCurrentPosition());
			}
			else if (pieceType == typeof(Bishop))
			{
				newPiece = new Bishop(piece.GetCurrentPosition());
			}
			else if (pieceType == typeof(Knight))
			{
				newPiece = new Knight(piece.GetCurrentPosition());
			}
			else if (pieceType == typeof(Queen))
			{
				newPiece = new Queen(piece.GetCurrentPosition());
			}
			else if (pieceType == typeof(King))
			{
				newPiece = new King(piece.GetCurrentPosition());
			}

			if (newPiece != null)
			{
				newPiece = piece.CopyPiece(newPiece);
				newPiece.SetChessboard(newChessboard);
				newChessboard.activePieces.Add(newPiece);
			}
		}

		return newChessboard;
	}
}
