using UnityEngine;
using System.Collections;

public class PieceInputHandler : MonoBehaviour {

	public bool isInAnimationState;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (this.isInAnimationState)
		{
			Animations.AnimateMovement(this.gameObject.GetComponent<AbstractPiece>());
		}
	}

	void OnMouseDown()
	{
		// This assumes (everywhere) that each game object that has this script attached also has an AbstractPiece script attached

		if (GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().hasGameStarted() && GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard.CurrentMovingSide() == this.gameObject.GetComponent<AbstractPiece>().side)
		{
			Bitboard moveBitboard = this.gameObject.GetComponent<AbstractPiece>().GetMovesForCurrentPosition();

			GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().ClearTiles();

			foreach (Position movePosition in moveBitboard.GetPositions())
			{
				GameObject moveTile = Instantiate(GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().GetMoveIcon(this.gameObject.GetComponent<AbstractPiece>().side));
				moveTile.GetComponent<MoveIcon>().SetMovingPiece(this.gameObject.GetComponent<AbstractPiece>());
				moveTile.GetComponent<MoveIcon>().SetMovePosition(movePosition);
			}
		}
	}
}
