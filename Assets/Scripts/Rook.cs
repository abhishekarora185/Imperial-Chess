using System.Collections.Generic;
using UnityEngine;

public class Rook : AbstractPiece
{

	public bool canCastle;

	public Rook(Position position)
	{
		this.InitializationActions();
		this.SetCurrentPosition(position);
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
		if (this.canCastle)
		{
			this.canCastle = false;
		}
	}

	public override void PerTurnProcessing()
	{
		
	}

	protected override void ComputeMoves()
	{
		// Compute universal moves for rooks
		this.moves = new Dictionary<Position, Bitboard>();

		int column, row;

		for (column = Position.min; column <= Position.max; column++)
		{
			for (row = Position.min; row <= Position.max; row++)
			{
				Position position = new Position(column, row);
				Bitboard positionBitboard = new Bitboard();

				int positionRow, positionColumn;
				for (positionColumn = Position.min; positionColumn <= Position.max; positionColumn++)
				{
					if (positionColumn != column)
					{
						positionBitboard.FlipPosition(new Position(positionColumn, row));
					}
				}
				for (positionRow = Position.min; positionRow <= Position.max; positionRow++)
				{
					if (positionRow != row)
					{
						positionBitboard.FlipPosition(new Position(column, positionRow));
					}
				}

				this.moves[position] = positionBitboard;
			}
		}
	}

	protected override Bitboard AdditionalMoveProcessing(Bitboard movesForCurrentPosition)
	{
		Chessboard chessBoard = GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard;

		if (this.side == Side.Black)
		{
			movesForCurrentPosition = movesForCurrentPosition.ComputeRayIntersections(chessBoard.GetPieceLocations(Side.White), this.GetCurrentPosition(), true);
		}
		else
		{
			movesForCurrentPosition = movesForCurrentPosition.ComputeRayIntersections(chessBoard.GetPieceLocations(Side.Black), this.GetCurrentPosition(), true);
		}
		movesForCurrentPosition = movesForCurrentPosition.ComputeRayIntersections(chessBoard.GetPieceLocations(this.side), this.GetCurrentPosition(), false);

		return movesForCurrentPosition;
	}

	private void InitializationActions()
	{
		this.ComputeMoves();
		this.canCastle = true;
	}

}
