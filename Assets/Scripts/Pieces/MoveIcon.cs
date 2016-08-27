/*
 * Author: Abhishek Arora
 * The Behaviour script associated with the visible icons on the board that the player can click on to execute a valid move
 * */

using UnityEngine;
using System.Collections.Generic;

public class MoveIcon : MonoBehaviour {

	// The piece that is making the move
	private AbstractPiece movingPiece;

	// The position to which the movingPiece will move if this script's game object is clicked
	private Position movePosition;

	void OnMouseDown()
	{
		// Execute the move associated with this behaviour script
		GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().moveSelected = true;
		MoveActions.standardMoveActions(new Move(this.movingPiece, this.movePosition));
		GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().ClearTiles();
	}

	public AbstractPiece GetMovingPiece()
	{
		return this.movingPiece;
	}

	public void SetMovingPiece(AbstractPiece piece)
	{
		this.movingPiece = piece;
	}

	public Position GetMovePosition()
	{
		return this.movePosition;
	}

	public void SetMovePosition(Position position)
	{
		this.movePosition = position;
		GameKeeper gameKeeper = GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>();
		this.gameObject.GetComponent<Transform>().position = new Vector3(gameKeeper.GetTransformFromPosition(position).x, 0.05f, gameKeeper.GetTransformFromPosition(position).z);
	}

}
