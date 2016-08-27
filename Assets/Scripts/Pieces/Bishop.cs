﻿/*
 * Author: Abhishek Arora
 * The Chess Engine class that controls Bishops
 * */

using System.Collections.Generic;
using UnityEngine;

public class Bishop : AbstractPiece
{

	public Bishop()
	{
		this.InitializationActions();
	}

	public Bishop(Position position)
	{
		this.InitializationActions();
		this.SetCurrentPosition(position);
	}

	// Use this for initialization
	void Start()
	{
		this.InitializationActions();
	}

	public override void PostMoveActions()
	{
		// No post-move actions for Bishops
	}

	public override void PerTurnProcessing()
	{
		// No per-turn processing for Bishops
	}

	protected override void ComputeMoves()
	{
		// Compute universal moves for bishops
		this.moves = new Dictionary<Position, Bitboard>();

		int column, row;

		for (column = Position.min; column <= Position.max; column++)
		{
			for (row = Position.min; row <= Position.max; row++)
			{
				Position position = new Position(column, row);
				Bitboard positionBitboard = new Bitboard();

				int positionRow, positionColumn;

				for (positionColumn = column + 1;
					positionColumn <= Position.max;
					positionColumn++)
				{
					positionRow = row + (positionColumn - column);
					if (positionRow <= Position.max)
					{
						positionBitboard.FlipPosition(new Position(positionColumn, positionRow));
					}

					positionRow = row - (positionColumn - column);
					if (positionRow >= Position.min)
					{
						positionBitboard.FlipPosition(new Position(positionColumn, positionRow));
					}
				}

				for (positionColumn = column - 1;
					positionColumn >= Position.min;
					positionColumn--)
				{
					positionRow = row + (column - positionColumn);
					if (positionRow <= Position.max)
					{
						positionBitboard.FlipPosition(new Position(positionColumn, positionRow));
					}

					positionRow = row - (column - positionColumn);
					if (positionRow >= Position.min)
					{
						positionBitboard.FlipPosition(new Position(positionColumn, positionRow));
					}
				}

				this.moves[position] = positionBitboard;
			}
		}
	}

	// For Bishops, only update static calculations with the positions of other pieces
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

	protected override void InitializationActions()
	{
		this.ComputeMoves();
	}
}
