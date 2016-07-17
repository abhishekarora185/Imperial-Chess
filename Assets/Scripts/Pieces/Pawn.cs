using System.Collections.Generic;
using UnityEngine;

public class Pawn : AbstractPiece {

	public bool allowEnPassantCapture;

	private Position initialPosition;

	public Pawn(Position position)
	{
		this.SetCurrentPosition(position);
		this.InitializationActions();
	}
	
	// Use this for initialization
	void Start()
	{
		this.InitializationActions();
	}

	// Update is called once per frame
	void Update()
	{

	}

	public override void PostMoveActions()
	{
		// If the pawn has moved two steps this turn, it is allowed to be captured via the en passant rule
		if (this.side == Side.Black)
		{
			if (this.GetCurrentPosition().Equals(new Position(this.initialPosition.GetColumn(), this.initialPosition.GetRow() - 2)))
			{
				this.allowEnPassantCapture = true;
			}
			// The attacking pawn can check behind it to see if it passed an en passant-flagged pawn
			AbstractPiece passedPiece = this.chessBoard.GetPieceAtPosition(new Position(this.GetCurrentPosition().GetColumn(), this.GetCurrentPosition().GetRow() + 1));
			if (passedPiece != null && passedPiece.GetType().Name.CompareTo(this.GetType().Name) == 0 && ((Pawn)passedPiece).allowEnPassantCapture)
			{
				this.chessBoard.KillPieceAtPosition(passedPiece.GetCurrentPosition(), this);
			}
		}
		else
		{
			if (this.GetCurrentPosition().Equals(new Position(this.initialPosition.GetColumn(), this.initialPosition.GetRow() + 2)))
			{
				this.allowEnPassantCapture = true;
			}
			AbstractPiece passedPiece = this.chessBoard.GetPieceAtPosition(new Position(this.GetCurrentPosition().GetColumn(), this.GetCurrentPosition().GetRow() - 1));
			if (passedPiece != null && passedPiece.GetType().Name.CompareTo(this.GetType().Name) == 0 && ((Pawn)passedPiece).allowEnPassantCapture)
			{
				this.chessBoard.KillPieceAtPosition(passedPiece.GetCurrentPosition(), this);
			}
		}

		// Pawn Promotion
		if (this.side == Side.Black && this.GetCurrentPosition().GetRow() == Position.min ||
			this.side == Side.White && this.GetCurrentPosition().GetRow() == Position.max)
		{
			// TODO: Figure out how do this the GameObject-unaware way by directly adding a Queen object to this object's chessboard
			this.TryPromoteGameObjectPawn();
		}
	}

	public void TryPromoteGameObjectPawn()
	{
		// Inconsistent code where the piece is aware of GameObjects
		if (this.gameObject != null)
		{
			if (this.side == Side.Black && this.GetCurrentPosition().GetRow() == Position.min)
			{
				// Pawn promotion
				GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().SpawnPromotedPiece(Constants.PieceCodes.BlackQueen, this.GetCurrentPosition());
				Destroy(this.gameObject);
			}
			else if (this.side == Side.White && this.GetCurrentPosition().GetRow() == Position.max)
			{
				// Pawn promotion
				GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().SpawnPromotedPiece(Constants.PieceCodes.WhiteQueen, this.GetCurrentPosition());
				Destroy(this.gameObject);
			}
		}
	}

	public override void PerTurnProcessing()
	{

		if (this.chessBoard.CurrentMovingSide() == this.side && this.allowEnPassantCapture)
		{
			// Disable en passant vulnerability if the pawn is still alive on the player's turn
			this.allowEnPassantCapture = false;
		}
	}

	protected override void ComputeMoves()
	{
		// Compute moves based on side for pawns
		this.moves = new Dictionary<Position,Bitboard>();
		int row, column;

		if (this.side == Side.Black)
		{
			// Starting position
			row = 7;
			for (column = Position.min; column <= Position.max; column++)
			{
				Position position = new Position(column, row);
				Bitboard positionBitboard = new Bitboard();
				positionBitboard.FlipPosition(new Position(column, row - 1));
				positionBitboard.FlipPosition(new Position(column, row - 2));
				this.moves[position] = positionBitboard;
			}

			// All other forward positions (till pawn promotion)
			for (row = 6; row >= 2; row--)
			{
				for (column = Position.min; column <= Position.max; column++)
				{
					Position position = new Position(column, row);
					Bitboard positionBitboard = new Bitboard();
					positionBitboard.FlipPosition(new Position(column, row - 1));
					this.moves[position] = positionBitboard;
				}
			}
		}
		else
		{
			// Starting position
			row = 2;
			for (column = Position.min; column <= Position.max; column++)
			{
				Position position = new Position(column, row);
				Bitboard positionBitboard = new Bitboard();
				positionBitboard.FlipPosition(new Position(column, row + 1));
				positionBitboard.FlipPosition(new Position(column, row + 2));
				this.moves[position] = positionBitboard;
			}

			// All other forward positions (till pawn promotion)
			for (row = 3; row <= 7; row++)
			{
				for (column = Position.min; column <= Position.max; column++)
				{
					Position position = new Position(column, row);
					Bitboard positionBitboard = new Bitboard();
					positionBitboard.FlipPosition(new Position(column, row + 1));
					this.moves[position] = positionBitboard;
				}
			}
		}
	}

	protected override Bitboard AdditionalMoveProcessing(Bitboard movesForCurrentPosition)
	{
		// For pawns, extra moves would constitute:
		// 1. Normal attack
		// 2. En passant

		// Normal attack
		// Check the row ahead for opponent locations
		int rowToCheck;
		Side sideToCheck;
		if (this.side == Side.Black)
		{
			rowToCheck = this.GetCurrentPosition().GetRow() - 1;
			sideToCheck = Side.White;
		}
		else
		{
			rowToCheck = this.GetCurrentPosition().GetRow() + 1;
			sideToCheck = Side.Black;
		}

		Bitboard friendlyLocations = GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard.GetPieceLocations(this.side);
		Bitboard opponentLocations = GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard.GetPieceLocations(sideToCheck);

		// First, remove any moves that go through friendly or opponent
		movesForCurrentPosition = movesForCurrentPosition.ComputeRayIntersections(friendlyLocations, this.GetCurrentPosition(), false);
		movesForCurrentPosition = movesForCurrentPosition.ComputeRayIntersections(opponentLocations, this.GetCurrentPosition(), false);

		if (this.GetCurrentPosition().GetColumn() > Position.min)
		{
			if (opponentLocations.ValueAtPosition(new Position(this.GetCurrentPosition().GetColumn() - 1, rowToCheck)) > 0)
			{
				movesForCurrentPosition.FlipPosition(new Position(this.GetCurrentPosition().GetColumn() - 1, rowToCheck));
			}
		}
		if (this.GetCurrentPosition().GetColumn() < Position.max)
		{
			if (opponentLocations.ValueAtPosition(new Position(this.GetCurrentPosition().GetColumn() + 1, rowToCheck)) > 0)
			{
				movesForCurrentPosition.FlipPosition(new Position(this.GetCurrentPosition().GetColumn() + 1, rowToCheck));
			}
		}

		// En passant
		rowToCheck = this.GetCurrentPosition().GetRow();

		if (this.GetCurrentPosition().GetColumn() > Position.min)
		{
			if (opponentLocations.ValueAtPosition(new Position(this.GetCurrentPosition().GetColumn() - 1, rowToCheck)) > 0)
			{
				// Also check if the piece at this location is a pawn which has just performed a double jump
				AbstractPiece passedPiece = GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard.GetPieceAtPosition(new Position(this.GetCurrentPosition().GetColumn() - 1, rowToCheck));
				if (passedPiece.GetType().Name.CompareTo(this.GetType().Name) == 0 && ((Pawn)passedPiece).allowEnPassantCapture)
				{
					int attackPoint = rowToCheck;
					if (this.side == Side.Black)
					{
						attackPoint--;
					}
					else
					{
						attackPoint++;
					}
					movesForCurrentPosition.FlipPosition(new Position(this.GetCurrentPosition().GetColumn() - 1, attackPoint));
				}
			}
		}
		if (this.GetCurrentPosition().GetColumn() < Position.max)
		{
			if (opponentLocations.ValueAtPosition(new Position(this.GetCurrentPosition().GetColumn() + 1, rowToCheck)) > 0)
			{
				AbstractPiece passedPiece = GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard.GetPieceAtPosition(new Position(this.GetCurrentPosition().GetColumn() + 1, rowToCheck));
				if (passedPiece != null && passedPiece.GetType().Name.CompareTo(this.GetType().Name) == 0 && ((Pawn)passedPiece).allowEnPassantCapture)
				{
					int attackPoint = rowToCheck;
					if (this.side == Side.Black)
					{
						attackPoint--;
					}
					else
					{
						attackPoint++;
					}
					movesForCurrentPosition.FlipPosition(new Position(this.GetCurrentPosition().GetColumn() + 1, attackPoint));
				}
			}
		}

		return movesForCurrentPosition;
	}

	public override AbstractPiece CopyPiece(AbstractPiece pieceToCopy)
	{
		pieceToCopy = base.CopyPiece(pieceToCopy);

		Pawn pawnToCopy = (Pawn)pieceToCopy;

		pawnToCopy.allowEnPassantCapture = this.allowEnPassantCapture;
		pawnToCopy.initialPosition = this.initialPosition;

		return pawnToCopy;
	}

	private void InitializationActions()
	{
		this.ComputeMoves();
		this.allowEnPassantCapture = false;
		this.initialPosition = this.GetCurrentPosition();
	}

}
