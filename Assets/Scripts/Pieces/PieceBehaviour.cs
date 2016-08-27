/*
 * Author: Abhishek Arora
 * The Behaviour script attached to Piece game objects which handles their Unity events and hosts a Chess Engine piece for engine computations
 * */

using UnityEngine;
using System.Collections.Generic;
using System;

public class PieceBehaviour : MonoBehaviour {

	// If the piece is currently in motion/fade action
	public bool isInAnimationState;

	// The link between the game object and the chess engine
	private AbstractPiece piece;
	
	// Update is called once per frame
	void Update () {
		if (this.isInAnimationState)
		{
			Animations.AnimateMovement(this);
		}
	}

	void OnMouseDown()
	{
		// Display all valid moves of this piece from its current position as move icons
		if (GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().hasGameStarted()
			&& !GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().moveSelected
			&& GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard.CurrentMovingSide() == this.getPiece().side)
		{
			Bitboard moveBitboard = this.getPiece().GetSafeMovesForCurrentPosition();

			GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().ClearTiles();

			foreach (Position movePosition in moveBitboard.GetPositions())
			{
				GameObject moveTile = Instantiate(GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().GetMoveIcon(this.getPiece().side));
				moveTile.GetComponent<MoveIcon>().SetMovingPiece(this.getPiece());
				moveTile.GetComponent<MoveIcon>().SetMovePosition(movePosition);
			}
		}
	}

	public void setPiece(Type type)
	{
		this.piece = (AbstractPiece) Activator.CreateInstance(type);
	}

	public void setPiece(AbstractPiece piece)
	{
		this.piece = piece;
	}

	public AbstractPiece getPiece()
	{
		return this.piece;
	}
}
