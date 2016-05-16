using UnityEngine;
using System.Collections.Generic;

public class GameKeeper : MonoBehaviour {

	public static bool isDebug = false;

	public GameObject[] prefabs;

	public GameObject whiteMoveIcon;

	public GameObject blackMoveIcon;

	public GameObject whiteTurnIcon;

	public GameObject blackTurnIcon;

	public Chessboard chessBoard;

	private bool gameOver;

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
		this.gameOver = false;
		this.gameStarted = false;
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

	public bool isGameOver()
	{
		return this.gameOver;
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
		// For debugging purposes, change the value of the following dictionary
		Dictionary<Position, int> spawnArrangement;
		
		if (!isDebug)
		{
			spawnArrangement = this.GetDefaultSpawnArrangement();
		}
		else
		{
			spawnArrangement = this.GetDebugSpawnArrangement();
		}

		this.piecesToSpawn = spawnArrangement.Count;

		foreach (Position spawnPosition in spawnArrangement.Keys)
		{
			GameObject prefab = prefabs[spawnArrangement[spawnPosition]];

			GameObject spawnedPiece = (GameObject)Instantiate(prefab,
				this.squares[spawnPosition],
				prefab.GetComponent<Transform>().rotation);
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
			piece.gameObject.GetComponent<ParticleSystem>().Play();
			piece.gameObject.GetComponent<AudioSource>().Play();
			piece.gameObject.GetComponent<MeshRenderer>().enabled = false;
			Destroy(piece.gameObject, piece.gameObject.GetComponent<ParticleSystem>().duration);
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

	private Dictionary<Position, int> GetDefaultSpawnArrangement()
	{
		Dictionary<Position, int> arrangement = new Dictionary<Position, int>();
		int column;

		for (column = Position.min; column <= Position.max; column++)
		{
			// Black Pawns
			arrangement[new Position(column, 7)] = Constants.PieceCodes.BlackPawn;

			// White Pawns
			arrangement[new Position(column, 2)] = Constants.PieceCodes.WhitePawn;
		}

		// Black Rooks
		arrangement[new Position(1, 8)] = Constants.PieceCodes.BlackRook;
		arrangement[new Position(8, 8)] = Constants.PieceCodes.BlackRook;

		//White Rooks
		arrangement[new Position(1, 1)] = Constants.PieceCodes.WhiteRook;
		arrangement[new Position(8, 1)] = Constants.PieceCodes.WhiteRook;

		// Black Knights
		arrangement[new Position(2, 8)] = Constants.PieceCodes.BlackKnight;
		arrangement[new Position(7, 8)] = Constants.PieceCodes.BlackKnight;

		//White Knights
		arrangement[new Position(2, 1)] = Constants.PieceCodes.WhiteKnight;
		arrangement[new Position(7, 1)] = Constants.PieceCodes.WhiteKnight;

		// Black Bishops
		arrangement[new Position(3, 8)] = Constants.PieceCodes.BlackBishop;
		arrangement[new Position(6, 8)] = Constants.PieceCodes.BlackBishop;

		//White Bishops
		arrangement[new Position(3, 1)] = Constants.PieceCodes.WhiteBishop;
		arrangement[new Position(6, 1)] = Constants.PieceCodes.WhiteBishop;

		// Kings & Queens
		arrangement[new Position(4, 8)] = Constants.PieceCodes.BlackKing;
		arrangement[new Position(5, 8)] = Constants.PieceCodes.BlackQueen;
		arrangement[new Position(4, 1)] = Constants.PieceCodes.WhiteKing;
		arrangement[new Position(5, 1)] = Constants.PieceCodes.WhiteQueen;

		return arrangement;
	}

	private Dictionary<Position, int> GetDebugSpawnArrangement()
	{
		// Just so you can start with the board in a given state instead of having to play your way to that state
		Dictionary<Position, int> arrangement = new Dictionary<Position, int>();

		int column;

		for (column = Position.min; column <= Position.min - 1; column++)
		{
			// Black Pawns
			//arrangement[new Position(column + 1, 4)] = Constants.PieceCodes.BlackPawn;

			// White Pawns
			//arrangement[new Position(column, 2)] = Constants.PieceCodes.WhitePawn;
		}

		// Black Rooks
		arrangement[new Position(1, 8)] = Constants.PieceCodes.BlackRook;
		arrangement[new Position(8, 8)] = Constants.PieceCodes.BlackRook;

		//White Rooks
		arrangement[new Position(1, 1)] = Constants.PieceCodes.WhiteRook;
		arrangement[new Position(8, 1)] = Constants.PieceCodes.WhiteRook;
        /*
		// Black Knights
		arrangement[new Position(2, 8)] = Constants.PieceCodes.BlackKnight;
		arrangement[new Position(7, 8)] = Constants.PieceCodes.BlackKnight;

		//White Knights
		arrangement[new Position(2, 1)] = Constants.PieceCodes.WhiteKnight;
		arrangement[new Position(7, 1)] = Constants.PieceCodes.WhiteKnight;

		// Black Bishops
		arrangement[new Position(3, 8)] = Constants.PieceCodes.BlackBishop;
		arrangement[new Position(6, 8)] = Constants.PieceCodes.BlackBishop;

		//White Bishops
		arrangement[new Position(3, 1)] = Constants.PieceCodes.WhiteBishop;
		arrangement[new Position(6, 1)] = Constants.PieceCodes.WhiteBishop;
        */
		// Kings & Queens
		arrangement[new Position(4, 8)] = Constants.PieceCodes.BlackKing;
		arrangement[new Position(5, 8)] = Constants.PieceCodes.BlackQueen;
		arrangement[new Position(4, 1)] = Constants.PieceCodes.WhiteKing;
		arrangement[new Position(5, 1)] = Constants.PieceCodes.WhiteQueen;

		return arrangement;
	}

	// Set Event Handlers for the game start animations
	private void SetEventHandlers()
	{
		EventManager.StartListening(Constants.EventNames.PawnsLanded, () =>
		{
			if (!isDebug)
			{
				foreach (GameObject piece in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
				{
					if (piece.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Bishop)
					{
						piece.GetComponent<PieceBehaviour>().isInAnimationState = true;
					}
				}

				EventManager.StopListening(Constants.EventNames.PawnsLanded);
			}
		});

		EventManager.StartListening(Constants.EventNames.BishopsLanded, () =>
		{
			if (!isDebug)
			{
				foreach (GameObject piece in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
				{
					if (piece.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Knight)
					{
						piece.GetComponent<PieceBehaviour>().isInAnimationState = true;
					}
				}

				EventManager.StopListening(Constants.EventNames.BishopsLanded);
			}
		});

		EventManager.StartListening(Constants.EventNames.KnightsLanded, () =>
		{
			if (!isDebug)
			{
				foreach (GameObject piece in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
				{
					if (piece.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Rook)
					{
						piece.GetComponent<PieceBehaviour>().isInAnimationState = true;
					}
				}

				EventManager.StopListening(Constants.EventNames.KnightsLanded);
			}
		});

		EventManager.StartListening(Constants.EventNames.RooksLanded, () =>
		{
			if (!isDebug)
			{
				foreach (GameObject piece in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
				{
					if (piece.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Queen ||
						piece.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.King)
					{
						piece.GetComponent<PieceBehaviour>().isInAnimationState = true;
					}
				}

				EventManager.StopListening(Constants.EventNames.BishopsLanded);
			}
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
			if (!this.chessBoard.IsKingInCheckmate(this.chessBoard.CurrentMovingSide()))
			{
				if (this.chessBoard.CurrentMovingSide() == Side.Black)
				{
					Instantiate(this.blackTurnIcon);
				}
				else
				{
					Instantiate(this.whiteTurnIcon);
				}
			}
			else
			{
				// Game Over!
				this.gameOver = true;
				EventManager.StopListening(Constants.EventNames.NewPlayerTurn);

				if (this.chessBoard.CurrentMovingSide() == Side.Black)
				{
					Instantiate(this.whiteTurnIcon);
					GameObject.Find(Constants.ActionCameraObject).GetComponent<ActionCamera>().deathAudioSource.clip = (AudioClip)Resources.Load(Constants.AudioClipNames.AudioDirectory + Constants.AudioClipNames.RebelAttack, typeof(AudioClip));
				}
				else
				{
					Instantiate(this.blackTurnIcon);
					GameObject.Find(Constants.ActionCameraObject).GetComponent<ActionCamera>().deathAudioSource.clip = (AudioClip)Resources.Load(Constants.AudioClipNames.AudioDirectory + Constants.AudioClipNames.ImperialMarch, typeof(AudioClip));
				}
				GameObject.Find(Constants.VictoryText).GetComponent<TextMesh>().text = Constants.VictoryText;
				GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<AudioSource>().Stop();
				GameObject.Find(Constants.ActionCameraObject).GetComponent<ActionCamera>().deathAudioSource.Play();
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

			if (!isDebug && !hasGameStarted())
			{
				spawnedObject.transform.position = Animations.InitializeStartAnimationSettings(spawnedObject.GetComponent<AbstractPiece>());
			}

			if (spawnedObject.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Queen || spawnedObject.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.King)
			{
				Animations.CorrectVerticalOffsets(spawnedObject.GetComponent<AbstractPiece>());
			}

			if (isDebug || (spawnedObject.GetComponent<AbstractPiece>().GetType().Name == Constants.PieceClassNames.Pawn && !this.hasGameStarted()))
			{
				// Pawns should start moving immediately
				spawnedObject.GetComponent<PieceBehaviour>().isInAnimationState = true;
			}

			if (isDebug)
			{
				float finalXPosition = GetTransformFromPosition(spawnedObject.GetComponent<AbstractPiece>().GetCurrentPosition()).x + Animations.HardcodedOffset(spawnedObject.GetComponent<AbstractPiece>()).x;
				float finalYPosition = float.Parse(spawnedObject.GetComponent<MeshRenderer>().bounds.extents.y.ToString("0.00")) + Animations.HardcodedOffset(spawnedObject.GetComponent<AbstractPiece>()).y;
				float finalZPosition = GetTransformFromPosition(spawnedObject.GetComponent<AbstractPiece>().GetCurrentPosition()).z + Animations.HardcodedOffset(spawnedObject.GetComponent<AbstractPiece>()).z;
				spawnedObject.GetComponent<AbstractPiece>().gameObject.GetComponent<Transform>().position = new Vector3(finalXPosition, finalYPosition, finalZPosition);
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

		spawnedObject.GetComponent<PieceBehaviour>().isInAnimationState = true;
	}

}
