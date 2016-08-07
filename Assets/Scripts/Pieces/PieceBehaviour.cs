using UnityEngine;
using System.Collections.Generic;
using System;

public class PieceBehaviour : MonoBehaviour {

	public bool isInAnimationState;

	private AbstractPiece piece;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (this.isInAnimationState)
		{
			Animations.AnimateMovement(this);
		}
	}

	void OnMouseDown()
	{
		if (GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().hasGameStarted()
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
