/*
 * Author: Abhishek Arora
 * The Behaviour script that controls the game flow, right from the opening crawl to the end of the game
 * */

using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameKeeper : MonoBehaviour {

	// Debug mode does away with needless animations to get things done fast
	public static bool isDebug = false;

	// The piece game object blueprints, stored through the Unity editor
	public GameObject[] prefabs;

	// The move icon blueprints
	public GameObject whiteMoveIcon;

	public GameObject blackMoveIcon;

	// The turn icon blueprints
	public GameObject whiteTurnIcon;

	public GameObject blackTurnIcon;

	// The Master board state, the one the user sees
	public Chessboard chessBoard;

	// Has a move already been chosen this turn? (doesn't let user input after a move has been chosen)
	public bool moveSelected;

	// Has a King been checkmated?
	private bool gameOver;

	// Is user input allowed/is the opening animation done?
	private bool gameStarted;

	// The width of a square of the Master chessboard object in world space
	private float squareSpacing;

	// The pieces that need to be spawned
	private int piecesToSpawn;

	// The pieces that have been spawned
	private int piecesSpawned;

	// Master chessboard Position to world space coordinate mapping
	private Dictionary<Position, Vector3> squares;

	// Use this for initialization
	void Start () {
		this.InitializeGamekeeper();
	}

	public void InitializeGamekeeper()
	{
		GameObject.Find(Constants.TextGameObjectNames.OpeningCrawl).AddComponent<OpeningCrawl>();

		this.chessBoard = new Chessboard();
		this.gameOver = false;
		this.gameStarted = false;
		this.moveSelected = false;
		this.piecesSpawned = 0;

		Animations.PlayOpeningCrawl();
		this.SetEventHandlers();

		this.ComputeSquares();
	}

	// Enables or disables user input
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
			spawnedPiece.GetComponent<PieceBehaviour>().setPiece(Constants.PieceCodes.pieceCodeToPieceTypeMapping[spawnArrangement[spawnPosition]]);
			spawnedPiece.GetComponent<PieceBehaviour>().getPiece().SetCurrentPosition(spawnPosition);

			if (spawnedPiece.name.ToLower().Contains(Side.Black.ToString().ToLower()))
			{
				spawnedPiece.GetComponent<PieceBehaviour>().getPiece().side = Side.Black;
			}
			else
			{
				spawnedPiece.GetComponent<PieceBehaviour>().getPiece().side = Side.White;
			}

			// Pawns' initial positions need to be set, and their moves need to be computed based on their side
			if (spawnedPiece.GetComponent<PieceBehaviour>().getPiece().GetType() == typeof(Pawn))
			{
				((Pawn)spawnedPiece.GetComponent<PieceBehaviour>().getPiece()).initializePawn((spawnedPiece.GetComponent<PieceBehaviour>().getPiece().GetCurrentPosition()));
			}

			this.chessBoard.AddPiece(spawnedPiece.GetComponent<PieceBehaviour>().getPiece());

			this.chessBoard.FlipBitAtPositionOfBitboard(spawnedPiece.GetComponent<PieceBehaviour>().getPiece().side, spawnPosition);
		}

		this.PostSpawnSettings();
	}

	public void SpawnPromotedPiece(int pieceCode, Position spawnPosition)
	{
		// Used specifically for Pawn Promotion

		GameObject spawnedPiece = (GameObject)Instantiate(this.prefabs[pieceCode],
				this.squares[spawnPosition],
				this.prefabs[pieceCode].GetComponent<Transform>().rotation);

		// The chessBoard would have created a Queen piece during the pawn's post move actions, so just set the new game object's piece to the last one in the piece queue of the chessBoard
		spawnedPiece.GetComponent<PieceBehaviour>().setPiece(this.chessBoard.getActivePieces().ToArray()[this.chessBoard.getActivePieces().Count - 1]);

		this.PostPromotionSpawnSettings(spawnedPiece);
	}

	// Returns the GameObject associated with a given Piece (the Piece must belong to the master Chessboard that is a member of this class)
	public GameObject GetGameObjectFromPiece(AbstractPiece piece)
	{
		GameObject gameObjectOfPiece = null;

		foreach (GameObject pieceGameObject in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
		{
			if (pieceGameObject.GetComponent<PieceBehaviour>().getPiece().Equals(piece))
			{
				gameObjectOfPiece = pieceGameObject;
			}
		}

		return gameObjectOfPiece;
	}

	// Destroys a piece game object in the most rudimentary way possible
	public void DestroyPiece(PieceBehaviour piece)
	{
		if (Animations.isSpaceship(piece.getPiece()))
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

	// A standard chess board arrangement; used for the game proper
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

	// A custom arrangement for Debug mode
	private Dictionary<Position, int> GetDebugSpawnArrangement()
	{
		// Just so you can start with the board in a given state instead of having to play your way to that state
		Dictionary<Position, int> arrangement = new Dictionary<Position, int>();
		int column;

		for (column = Position.min; column <= Position.max; column++)
		{
			// Black Pawns
			//arrangement[new Position(column, 7)] = Constants.PieceCodes.BlackPawn;

			// White Pawns
			//arrangement[new Position(column, 7)] = Constants.PieceCodes.WhitePawn;
		}

		// Black Rooks
		arrangement[new Position(1, 8)] = Constants.PieceCodes.BlackRook;
		arrangement[new Position(8, 8)] = Constants.PieceCodes.BlackRook;

		//White Rooks
		arrangement[new Position(1, 1)] = Constants.PieceCodes.WhiteRook;
		arrangement[new Position(8, 1)] = Constants.PieceCodes.WhiteRook;

		// Black Knights
		//arrangement[new Position(2, 8)] = Constants.PieceCodes.BlackKnight;
		//arrangement[new Position(7, 8)] = Constants.PieceCodes.BlackKnight;

		//White Knights
		//arrangement[new Position(2, 1)] = Constants.PieceCodes.WhiteKnight;
		//arrangement[new Position(7, 1)] = Constants.PieceCodes.WhiteKnight;

		// Black Bishops
		//arrangement[new Position(3, 8)] = Constants.PieceCodes.BlackBishop;
		//arrangement[new Position(6, 8)] = Constants.PieceCodes.BlackBishop;

		//White Bishops
		//arrangement[new Position(3, 1)] = Constants.PieceCodes.WhiteBishop;
		//arrangement[new Position(6, 1)] = Constants.PieceCodes.WhiteBishop;

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
					if (typeof(Bishop).IsInstanceOfType(piece.GetComponent<PieceBehaviour>().getPiece()))
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
					if (typeof(Knight).IsInstanceOfType(piece.GetComponent<PieceBehaviour>().getPiece()))
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
					if (typeof(Rook).IsInstanceOfType(piece.GetComponent<PieceBehaviour>().getPiece()))
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
					if (typeof(Queen).IsInstanceOfType(piece.GetComponent<PieceBehaviour>().getPiece()) ||
						typeof(King).IsInstanceOfType(piece.GetComponent<PieceBehaviour>().getPiece()))
					{
						piece.GetComponent<PieceBehaviour>().isInAnimationState = true;
					}
				}

				EventManager.StopListening(Constants.EventNames.RooksLanded);
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
				this.moveSelected = false;
			}
			else
			{
				// Game Over!
				this.gameOver = true;
				this.moveSelected = true;
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
				GameObject.Find(Constants.VictoryText).GetComponent<Text>().enabled = true;
				GameObject.Find(Constants.RestartText).GetComponent<Text>().enabled = true;
				GameObject.Find(Constants.RestartText).GetComponent<Button>().onClick.AddListener(() => {
					Application.LoadLevel(Application.loadedLevel);
				});

				GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<AudioSource>().Stop();
				GameObject.Find(Constants.ActionCameraObject).GetComponent<ActionCamera>().deathAudioSource.Play();
			}
		});

	}

	// Done so that pieces can be called with their actual names and without the Unity " (Clone)" suffix
	private void TrimName(PieceBehaviour piece)
	{
		if (piece.gameObject.name.IndexOf(Constants.PieceNames.Clone) > -1)
		{
			piece.gameObject.name = piece.gameObject.name.Substring(0, piece.gameObject.name.IndexOf(Constants.PieceNames.Clone));
		}
	}

	// After pieces are spawned
	private void PostSpawnSettings()
	{
		foreach (GameObject spawnedObject in GameObject.FindGameObjectsWithTag(Constants.PieceTag))
		{
			TrimName(spawnedObject.GetComponent<PieceBehaviour>());
			spawnedObject.GetComponent<PieceBehaviour>().getPiece().SetChessboard(this.chessBoard);

			if (!isDebug && !hasGameStarted())
			{
				spawnedObject.transform.position = Animations.InitializeStartAnimationSettings(spawnedObject.GetComponent<PieceBehaviour>());
			}

			if (typeof(Queen).IsInstanceOfType(spawnedObject.GetComponent<PieceBehaviour>().getPiece()) || typeof(King).IsInstanceOfType(spawnedObject.GetComponent<PieceBehaviour>().getPiece()))
			{
				Animations.CorrectVerticalOffsets(spawnedObject.GetComponent<PieceBehaviour>());
			}

			if (isDebug || (typeof(Pawn).IsInstanceOfType(spawnedObject.GetComponent<PieceBehaviour>().getPiece()) && !this.hasGameStarted()))
			{
				// Pawns should start moving immediately
				spawnedObject.GetComponent<PieceBehaviour>().isInAnimationState = true;
			}

			if (isDebug)
			{
				float finalXPosition = GetTransformFromPosition(spawnedObject.GetComponent<PieceBehaviour>().getPiece().GetCurrentPosition()).x + Animations.HardcodedOffset(spawnedObject.GetComponent<PieceBehaviour>()).x;
				float finalYPosition = float.Parse(spawnedObject.GetComponent<MeshRenderer>().bounds.extents.y.ToString("0.00")) + Animations.HardcodedOffset(spawnedObject.GetComponent<PieceBehaviour>()).y;
				float finalZPosition = GetTransformFromPosition(spawnedObject.GetComponent<PieceBehaviour>().getPiece().GetCurrentPosition()).z + Animations.HardcodedOffset(spawnedObject.GetComponent<PieceBehaviour>()).z;
				spawnedObject.GetComponent<Transform>().position = new Vector3(finalXPosition, finalYPosition, finalZPosition);
			}
		}
	}

	// After a pawn is promoted
	private void PostPromotionSpawnSettings(GameObject spawnedObject)
	{
		// Assuming the object is a Queen

		TrimName(spawnedObject.GetComponent<PieceBehaviour>());

		spawnedObject.transform.position = Animations.InitializeStartAnimationSettings(spawnedObject.GetComponent<PieceBehaviour>());

		Animations.CorrectVerticalOffsets(spawnedObject.GetComponent<PieceBehaviour>());

		spawnedObject.GetComponent<PieceBehaviour>().isInAnimationState = true;
	}

}
