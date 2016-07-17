using System.Collections.Generic;
using UnityEngine;

public class Knight : AbstractPiece
{

	public Knight(Position position)
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
	}

	public override void PerTurnProcessing()
	{

	}

	protected override void ComputeMoves()
	{
		// Compute universal moves for knights
		this.moves = new Dictionary<Position, Bitboard>();

		int column, row;

		for (column = Position.min; column <= Position.max; column++)
		{
			for (row = Position.min; row <= Position.max; row++)
			{
				Position position = new Position(column, row);
				Bitboard positionBitboard = new Bitboard();

				List<Position> knightPositions = new List<Position>();
				knightPositions.Add(new Position(column + 2, row + 1));
				knightPositions.Add(new Position(column + 2, row - 1));
				knightPositions.Add(new Position(column - 2, row + 1));
				knightPositions.Add(new Position(column - 2, row - 1));
				knightPositions.Add(new Position(column + 1, row + 2));
				knightPositions.Add(new Position(column + 1, row - 2));
				knightPositions.Add(new Position(column - 1, row + 2));
				knightPositions.Add(new Position(column - 1, row - 2));

				foreach (Position knightPosition in knightPositions)
				{
					if (knightPosition.GetColumn() >= Position.min && knightPosition.GetColumn() <= Position.max
						&& knightPosition.GetRow() >= Position.min && knightPosition.GetRow() <= Position.max)
					{
						positionBitboard.FlipPosition(knightPosition);
					}
				}

				this.moves[position] = positionBitboard;
			}
		}
	}

	protected override Bitboard AdditionalMoveProcessing(Bitboard movesForCurrentPosition)
	{
		if (this.side == Side.Black)
		{
			movesForCurrentPosition = movesForCurrentPosition.ComputeRayIntersections(this.chessBoard.GetPieceLocations(Side.White), this.GetCurrentPosition(), true);
		}
		else
		{
			movesForCurrentPosition = movesForCurrentPosition.ComputeRayIntersections(this.chessBoard.GetPieceLocations(Side.Black), this.GetCurrentPosition(), true);
		}
		movesForCurrentPosition = movesForCurrentPosition.ComputeRayIntersections(this.chessBoard.GetPieceLocations(this.side), this.GetCurrentPosition(), false);

		return movesForCurrentPosition;
	}

	private void InitializationActions()
	{
		this.ComputeMoves();
	}

}
