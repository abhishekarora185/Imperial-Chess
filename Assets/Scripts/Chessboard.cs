using UnityEngine;
using System.Collections.Generic;

public class Chessboard {

	public GameObject pieceHolderGameObject;

	private Bitboard blackPieceLocations;

	private Bitboard whitePieceLocations;

	private Side movingSide;

	private List<AbstractPiece> activePieces;

	public Chessboard() {
		this.blackPieceLocations = new Bitboard();
		this.whitePieceLocations = new Bitboard();
		this.movingSide = Side.Black;
		this.activePieces = new List<AbstractPiece>();
	}

	public Chessboard(GameObject pieceHolderGameObject)
	{
		this.blackPieceLocations = new Bitboard();
		this.whitePieceLocations = new Bitboard();
		this.movingSide = Side.Black;
		this.activePieces = new List<AbstractPiece>();
		this.pieceHolderGameObject = pieceHolderGameObject;
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

	// Called by pieces whenever they need to move
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

	public List<AbstractPiece> GetPiecesByTypeAndSide(System.Type type, Side side)
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
		King kingToCheck = (King)GetPiecesByTypeAndSide(typeof(King), sideToCheck).ToArray()[0];

		foreach (AbstractPiece activePiece in this.activePieces)
		{
			if (activePiece.side != sideToCheck)
			{
				Bitboard availableMoves = activePiece.GetMovesForCurrentPosition();

				if (availableMoves.ValueAtPosition(kingToCheck.GetCurrentPosition()) > 0)
				{
					// Special check for Pawns advancing forward - they are of no harm and must not be considered here
					if (activePiece.GetType().Name == Constants.PieceClassNames.Pawn && activePiece.GetCurrentPosition().GetColumn() == kingToCheck.GetCurrentPosition().GetColumn())
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

	public static Chessboard MakeCopyOfChessboard(Chessboard chessboardToCopy)
	{
		// It's the responsibility of whoever creates this to clean up their mess
		GameObject pieceHolder = new GameObject();
		pieceHolder.tag = Constants.PieceHolderTag;

		Chessboard newChessboard = new Chessboard(pieceHolder);
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
				newPiece = pieceHolder.AddComponent<Pawn>();
			}
			else if (pieceType == typeof(Rook))
			{
				newPiece = pieceHolder.AddComponent<Rook>();
			}
			else if (pieceType == typeof(Bishop))
			{
				newPiece = pieceHolder.AddComponent<Bishop>();
			}
			else if (pieceType == typeof(Knight))
			{
				newPiece = pieceHolder.AddComponent<Knight>();
			}
			else if (pieceType == typeof(Queen))
			{
				newPiece = pieceHolder.AddComponent<Queen>();
			}
			else if (pieceType == typeof(King))
			{
				newPiece = pieceHolder.AddComponent<King>();
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
