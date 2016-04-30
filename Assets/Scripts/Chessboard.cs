using UnityEngine;
using System.Collections.Generic;

public class Chessboard {

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

}
