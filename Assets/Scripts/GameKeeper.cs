using UnityEngine;
using System.Collections.Generic;

public class GameKeeper : MonoBehaviour {

	public GameObject[] prefabs;

	public GameObject whiteMoveIcon;

	public GameObject blackMoveIcon;

	public GameObject whiteTurnIcon;

	public GameObject blackTurnIcon;

	public Chessboard chessBoard;

	private bool gameStarted;

	private float squareSpacing;

	private int piecesToSpawn;

	private int piecesSpawned;

	// Position to coordinate mapping
	private Dictionary<Position, Vector3> squares;

	// Use this for initialization
	void Start () {
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			Application.targetFrameRate = 60;
		}

		this.chessBoard = new Chessboard();
		this.gameStarted = false;

		this.piecesToSpawn = 32;
		this.piecesSpawned = 0;

		Animations.PlayOpeningCrawl();
		this.SetEventHandlers();

		this.ComputeSquares();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void TogglePauseGame()
	{
		if (this.gameStarted)
		{
			this.gameStarted = false;
		}
		else
		{
			this.gameStarted = true;
		}
	}

	public void StartGame()
	{
		this.gameStarted = true;
	}

	public bool hasGameStarted()
	{
		return this.gameStarted;
	}

	public float GetSquareSpacing()
	{
		return this.squareSpacing;
	}

	public Vector3 GetTransformFromPosition(Position position)
	{
		return this.squares[position];
	}

	public GameObject GetMoveIcon(Side side)
	{
		if (side == Side.Black)
		{
			return this.blackMoveIcon;
		}
		else
		{
			return this.whiteMoveIcon;
		}
	}

	public void ClearTiles()
	{
		// Clear any moving tiles that may have been spawned
		foreach (GameObject tileObject in GameObject.FindGameObjectsWithTag(Constants.TileTag))
		{
			Destroy(tileObject);
		}
	}

	public void SpawnPieces()
	{
		Dictionary<Position, GameObject> piecesToSpawn = new Dictionary<Position, GameObject>();

		// Black side is towards the +ve X-axis, White towards the -ve

		int pawnColumn;
		// Black Pawns
		int pawnRow = 7;
		for (pawnColumn = Position.min; pawnColumn <= Position.max; pawnColumn++)
		{
			piecesToSpawn[new Position(pawnColumn, pawnRow)] = prefabs[Constants.PieceCodes.BlackPawn];
		}
		// White Pawns
		pawnRow = 2;
		for (pawnColumn = Position.min; pawnColumn <= Position.max; pawnColumn++)
		{
			piecesToSpawn[new Position(pawnColumn, pawnRow)] = prefabs[Constants.PieceCodes.WhitePawn];
		}

		int[] rookColumns = { 1, 8 };
		int[] bishopColumns = { 3, 6 };
		int[] knightColumns = { 2, 7 };

		// Black Rooks
		int rookRow = 8;
		foreach (int rookColumn in rookColumns)
		{
			piecesToSpawn[new Position(rookColumn, rookRow)] = prefabs[Constants.PieceCodes.BlackRook];
		}

		// White Rooks
		rookRow = 1;
		foreach (int rookColumn in rookColumns)
		{
			piecesToSpawn[new Position(rookColumn, rookRow)] = prefabs[Constants.PieceCodes.WhiteRook];
		}

		// Black Bishops
		int bishopRow = 8;
		foreach (int bishopColumn in bishopColumns)
		{
			piecesToSpawn[new Position(bishopColumn, bishopRow)] = prefabs[Constants.PieceCodes.BlackBishop];
		}

		// White Bishops
		bishopRow = 1;
		foreach (int bishopColumn in bishopColumns)
		{
			piecesToSpawn[new Position(bishopColumn, bishopRow)] = prefabs[Constants.PieceCodes.WhiteBishop];
		}

		// Black Knights
		int knightRow = 8;
		foreach (int knightColumn in knightColumns)
		{
			piecesToSpawn[new Position(knightColumn, knightRow)] = prefabs[Constants.PieceCodes.BlackKnight];
		}

		// White Knights
		knightRow = 1;
		foreach (int knightColumn in knightColumns)
		{
			piecesToSpawn[new Position(knightColumn, knightRow)] = prefabs[Constants.PieceCodes.WhiteKnight];
		}

		// Black and White Queens
		int queenColumn = 5, queenRow = 8;
		piecesToSpawn[new Position(queenColumn, queenRow)] = prefabs[Constants.PieceCodes.BlackQueen];
		
		queenRow = 1;
		piecesToSpawn[new Position(queenColumn, queenRow)] = prefabs[Constants.PieceCodes.WhiteQueen];

		// Black and White Kings
		int kingColumn = 4, kingRow = 8;
		piecesToSpawn[new Position(kingColumn, kingRow)] = prefabs[Constants.PieceCodes.BlackKing];
		
		kingRow = 1;
		piecesToSpawn[new Position(kingColumn, kingRow)] = prefabs[Constants.PieceCodes.WhiteKing];

		foreach (Position spawnPosition in piecesToSpawn.Keys)
		{
			GameObject spawnedPiece = (GameObject)Instantiate(piecesToSpawn[spawnPosition],
				this.squares[spawnPosition],
				piecesToSpawn[spawnPosition].GetComponent<Transform>().rotation);
			spawnedPiece.GetComponent<AbstractPiece>().SetCurrentPosition(spawnPosition);

			this.chessBoard.AddPiece(spawnedPiece.GetComponent<AbstractPiece>());

			this.chessBoard.FlipBitAtPositionOfBitboard(spawnedPiece.GetComponent<AbstractPiece>().side, spawnPosition);
		}

		this.PostSpawnSettings();
	}

	public void SpawnPromotedPiece(int pieceCode, Position spawnPosition)
	{
		// Used specifically for Pawn Promotion

		GameObject spawnedPiece = (GameObject)Instantiate(this.prefabs[pieceCode],
				this.squares[spawnPosition],
				this.prefabs[pieceCode].GetComponent<Transform>().rotation);
		spawnedPiece.GetComponent<AbstractPiece>().SetCurrentPosition(spawnPosition);
		this.chessBoard.AddPiece(spawnedPiece.GetComponent<AbstractPiece>());

		this.PostPromotionSpawnSettings(spawnedPiece);
	}

	public void DestroyPiece(AbstractPiece piece)
	{
		if (Animations.isSpaceship(piece))
		{
			piece.gameObject.GetComponent<ParticleSystem>().Play();
			piece.gameObject.GetComponent<AudioSource>().Play();
			piece.gameObject.GetComponent<MeshRenderer>().enabled = false;
			Destroy(piece.gameObject, piece.gameObject.GetComponent<ParticleSystem>().duration);
		}
		else
		{
			Destroy(piece.gameObject);
		}
	}

	// Compute Position-coordinate mappings
	private void ComputeSquares()
	{
		this.squares = new Dictionary<Position, Vector3>();

		int row, column;
		float posX, posZ;

		posX = this.gameObject.GetComponent<MeshRenderer>().bounds.extents.x * -7 / 8;
		posZ = this.gameObject.GetComponent<MeshRenderer>().bounds.extents.z * -7 / 8;
		this.squareSpacing = this.gameObject.GetComponent<MeshRenderer>().bounds.extents.x / 4;

		for (column = Position.min; column <= Position.max; column++)
		{
			for (row = Position.min; row <= Position.max; row++)
			{
				Position position = new Position(column, row);
				this.squares[position] = new Vector3(posX + (row - 1) * this.squareSpacing, 0, posZ + (column - 1) * this.squareSpacing);
			}
		}
	}

	// Set Event Handlers for the game start animations
	private void SetEventHandlers()
	{
		EventManager.StartListening(Constants.EventNames.PawnsLanded, () =>
		{
			foreach (GameObject piece in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
			{
				if (piece.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Bishop)
				{
					piece.GetComponent<PieceInputHandler>().isInAnimationState = true;
				}
			}

			EventManager.StopListening(Constants.EventNames.PawnsLanded);
		});

		EventManager.StartListening(Constants.EventNames.BishopsLanded, () =>
		{
			foreach (GameObject piece in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
			{
				if (piece.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Knight)
				{
					piece.GetComponent<PieceInputHandler>().isInAnimationState = true;
				}
			}

			EventManager.StopListening(Constants.EventNames.BishopsLanded);
		});

		EventManager.StartListening(Constants.EventNames.KnightsLanded, () =>
		{
			foreach (GameObject piece in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
			{
				if (piece.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Rook)
				{
					piece.GetComponent<PieceInputHandler>().isInAnimationState = true;
				}
			}

			EventManager.StopListening(Constants.EventNames.KnightsLanded);
		});

		EventManager.StartListening(Constants.EventNames.RooksLanded, () =>
		{
			foreach (GameObject piece in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
			{
				if (piece.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Queen ||
					piece.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.King)
				{
					piece.GetComponent<PieceInputHandler>().isInAnimationState = true; 
				}
			}

			EventManager.StopListening(Constants.EventNames.BishopsLanded);
		});

		EventManager.StartListening(Constants.EventNames.PieceSpawned, () =>
		{
			this.piecesSpawned++;

			if (this.piecesSpawned == this.piecesToSpawn)
			{
				this.StartGame();
				EventManager.TriggerEvent(Constants.EventNames.NewPlayerTurn);
				EventManager.StopListening(Constants.EventNames.PieceSpawned);
			}
		});

		EventManager.StartListening(Constants.EventNames.NewPlayerTurn, () =>
		{
			this.chessBoard.ChangeMovingSide();

			if (this.chessBoard.CurrentMovingSide() == Side.Black)
			{
				Instantiate(this.blackTurnIcon);
			}
			else
			{
				Instantiate(this.whiteTurnIcon);
			}
		});

	}

	private void TrimName(AbstractPiece piece)
	{
		if (piece.gameObject.name.IndexOf(Constants.PieceNames.Clone) > -1)
		{
			piece.gameObject.name = piece.gameObject.name.Substring(0, piece.gameObject.name.IndexOf(Constants.PieceNames.Clone));
		}
	}

	private void PostSpawnSettings()
	{
		foreach (GameObject spawnedObject in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
		{
			TrimName(spawnedObject.GetComponent<AbstractPiece>());
			spawnedObject.GetComponent<AbstractPiece>().SetChessboard(this.chessBoard);

			if (!hasGameStarted())
			{
				spawnedObject.transform.position = Animations.InitializeStartAnimationSettings(spawnedObject.GetComponent<AbstractPiece>());
			}

			if (spawnedObject.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Queen || spawnedObject.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.King)
			{
				Animations.CorrectVerticalOffsets(spawnedObject.GetComponent<AbstractPiece>());
			}

			if (spawnedObject.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Pawn && !this.hasGameStarted())
			{
				// Pawns should start moving immediately
				spawnedObject.GetComponent<PieceInputHandler>().isInAnimationState = true;
			}
		}
	}

	private void PostPromotionSpawnSettings(GameObject spawnedObject)
	{
		// Assuming the object is a Queen

		TrimName(spawnedObject.GetComponent<AbstractPiece>());
		spawnedObject.GetComponent<AbstractPiece>().SetChessboard(this.chessBoard);

		spawnedObject.transform.position = Animations.InitializeStartAnimationSettings(spawnedObject.GetComponent<AbstractPiece>());

		Animations.CorrectVerticalOffsets(spawnedObject.GetComponent<AbstractPiece>());

		spawnedObject.GetComponent<PieceInputHandler>().isInAnimationState = true;
	}

}
