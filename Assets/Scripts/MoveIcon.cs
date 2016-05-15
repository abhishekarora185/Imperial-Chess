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
		this.TryPlayDeathAnimation();

		if (this.movingPiece.GetType().Name == Constants.PieceClassNames.Pawn)
		{
			this.TryAnimateEnPassantCapture();
		}

		if (this.movingPiece.GetType().Name == Constants.PieceClassNames.King)
		{
			this.TryAnimateRookCastling();
		}

		GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard.MoveTo(this.movingPiece, this.movePosition);
		this.movingPiece.gameObject.GetComponent<PieceBehaviour>().isInAnimationState = true;

		GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().ClearTiles();
		this.movingPiece.PostMoveActions();
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

	private void TryPlayDeathAnimation()
	{
		AbstractPiece pieceAtMovePosition = GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard.GetPieceAtPosition(this.movePosition);

		if (pieceAtMovePosition != null)
		// Switch to action cam for some awesomeness
		{
			GameObject.Find(Constants.ActionCameraObject).GetComponent<ActionCamera>().EnableActionCamera(this.movingPiece, pieceAtMovePosition);
			GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().DestroyPiece(pieceAtMovePosition);
		}
	}

	private void TryAnimateEnPassantCapture()
	{
		Position enPassantPosition;
		if (this.movingPiece.side == Side.Black)
		{
			enPassantPosition = new Position(this.movePosition.GetColumn(), this.movePosition.GetRow() + 1);
		}
		else
		{
			enPassantPosition = new Position(this.movePosition.GetColumn(), this.movePosition.GetRow() - 1);
		}

		AbstractPiece pieceAtEnPassantPosition = GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard.GetPieceAtPosition(enPassantPosition);

		Pawn dyingPawn = null;

		if (pieceAtEnPassantPosition != null && pieceAtEnPassantPosition.GetType().Name == Constants.PieceClassNames.Pawn && pieceAtEnPassantPosition.side != this.movingPiece.side && ((Pawn)pieceAtEnPassantPosition).allowEnPassantCapture)
		{
			dyingPawn = (Pawn) pieceAtEnPassantPosition;
		}

		if (dyingPawn != null)
		{
			GameObject.Find(Constants.ActionCameraObject).GetComponent<ActionCamera>().EnableActionCamera(this.movingPiece, dyingPawn);
			GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().DestroyPiece(dyingPawn);
		}
	}

	private void TryAnimateRookCastling()
	{
		if (this.movePosition.GetRow() == this.movingPiece.GetCurrentPosition().GetRow() && 
			Mathf.Abs(this.movePosition.GetColumn() - this.movingPiece.GetCurrentPosition().GetColumn()) == 2)
		{
			// Castling is happening!
			if (this.movePosition.GetColumn() == 2)
			{
				// King side
				GameObject rookGameObject = null;

				foreach (GameObject piece in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
				{
					if (piece.GetComponent<AbstractPiece>().side == this.movingPiece.side && piece.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Rook && piece.GetComponent<AbstractPiece>().GetCurrentPosition().GetColumn() < this.movePosition.GetColumn())
					{
						rookGameObject = piece;
						break;
					}
				}
				if (rookGameObject != null)
				{
					rookGameObject.GetComponent<PieceBehaviour>().isInAnimationState = true;
				}
			}
			else if (this.movePosition.GetColumn() == 6)
			{
				// Queen side
				GameObject rookGameObject = null;

				foreach (GameObject piece in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
				{
					if (piece.GetComponent<AbstractPiece>().side == this.movingPiece.side && piece.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Rook && piece.GetComponent<AbstractPiece>().GetCurrentPosition().GetColumn() > this.movePosition.GetColumn())
					{
						rookGameObject = piece;
						break;
					}
				}
				if (rookGameObject != null)
				{
					rookGameObject.GetComponent<PieceBehaviour>().isInAnimationState = true;
				}
			}
		}
	}

}
