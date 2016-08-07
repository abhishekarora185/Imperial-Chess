using UnityEngine;
using System.Collections.Generic;

public class MoveIcon : MonoBehaviour {

	private AbstractPiece movingPiece;

	private Position movePosition;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown()
	{
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
