using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Animations {

	public static Material opaqueBlackMaterial;
	public static Material opaqueWhiteMaterial;
	public static Material fadeBlackMaterial;
	public static Material fadeWhiteMaterial;

	public static void PlayOpeningCrawl()
	{
		GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().StartCoroutine(ALongTimeAgo());
		GameObject.Find(Constants.VictoryText).GetComponent<Text>().enabled = false;
		GameObject.Find(Constants.RestartText).GetComponent<Text>().enabled = false;
		GameObject.Find(Constants.QuitText).GetComponent<Text>().enabled = false;
		GameObject.Find(Constants.SureText).GetComponent<Text>().enabled = false;
		GameObject.Find(Constants.YesText).GetComponent<Text>().enabled = false;
		GameObject.Find(Constants.NoText).GetComponent<Text>().enabled = false;
		GameObject.Find(Constants.CreditsText).GetComponent<Text>().enabled = false;
	}

	public static IEnumerator ALongTimeAgo()
	{
		GameObject ALongTimeAgo = GameObject.Find(Constants.TextGameObjectNames.ALongTimeAgo);
		ALongTimeAgo.GetComponent<MeshRenderer>().enabled = true;
		yield return new WaitForSeconds(5);
		ALongTimeAgo.GetComponent<MeshRenderer>().enabled = false;
		if (GameObject.Find(Constants.TextGameObjectNames.StarWarsLogo) != null)
		{
			LaunchStarWarsLogo();
		}
	}

	public static void LaunchStarWarsLogo()
	{
		GameObject StarWarsLogo = GameObject.Find(Constants.TextGameObjectNames.StarWarsLogo);
		StarWarsLogo.GetComponent<Rigidbody>().velocity = new Vector3(10.0f, 0, 0);

		GameObject AudioSource = GameObject.Find(Constants.TextGameObjectNames.OpeningCrawl);
		AudioSource.GetComponent<AudioSource>().Play();
	}

	public static void StartOpeningCrawl()
	{
		GameObject OpeningCrawl = GameObject.Find(Constants.TextGameObjectNames.OpeningCrawl);
		OpeningCrawl.GetComponent<Rigidbody>().velocity = new Vector3(1f, 1f, 0);
	}

	public static void RotateCameraToChessboard(Vector3 targetAngle, bool playedRevealBoardClip)
	{
		// After the opening crawl disappears, gradually lower the camera onto the chessboard
		GameObject OpeningCrawl = GameObject.Find(Constants.TextGameObjectNames.OpeningCrawl);

		Vector3 currentAngle = Camera.main.gameObject.GetComponent<Transform>().eulerAngles;

		currentAngle = new Vector3(Mathf.Lerp(currentAngle.x, targetAngle.x, Time.deltaTime),
				currentAngle.y,
				currentAngle.z
			);

		if (currentAngle.x == targetAngle.x)
		{
			OpeningCrawl.GetComponent<OpeningCrawl>().DestroyOpeningCrawlObjects();
		}

		Camera.main.gameObject.GetComponent<Transform>().eulerAngles = currentAngle;

		// If the reveal board clip needs to be played for the first time, play it!
		if (!playedRevealBoardClip)
		{
			GameObject chessBoard = GameObject.Find(Constants.PieceNames.ChessBoard);
			GameObject revealBoardAudioSource = GameObject.Find(Constants.AuxiliaryAudioObject);

			chessBoard.GetComponent<GameKeeper>().SpawnPieces();

			if (!GameKeeper.isDebug)
			{
				revealBoardAudioSource.GetComponent<AudioSource>().Play();
				chessBoard.GetComponent<GameKeeper>().StartCoroutine(BeginGameLoop(revealBoardAudioSource.GetComponent<AudioSource>().clip.length));
			}
			else
			{
				chessBoard.GetComponent<GameKeeper>().StartCoroutine(BeginGameLoop(0));
			}
		}
	}

	public static IEnumerator BeginGameLoop(float time)
	{
		yield return new WaitForSeconds(time);

		// Load and play the game loop clip
		AudioSource gameLoopAudioSource = GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<AudioSource>();
		gameLoopAudioSource.loop = true;
		gameLoopAudioSource.Play();
	}

	public static void AnimateMovement(PieceBehaviour movingPiece)
	{
		GameKeeper gameKeeper = GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>();

		float finalXPosition = gameKeeper.GetTransformFromPosition(movingPiece.getPiece().GetCurrentPosition()).x + HardcodedOffset(movingPiece).x;
		float finalYPosition = float.Parse(movingPiece.gameObject.GetComponent<MeshRenderer>().bounds.extents.y.ToString("0.00")) + HardcodedOffset(movingPiece).y;
		float finalZPosition = gameKeeper.GetTransformFromPosition(movingPiece.getPiece().GetCurrentPosition()).z + HardcodedOffset(movingPiece).z;

		float lerpSpeed;
		if (gameKeeper.hasGameStarted())
		{
			if (GameObject.Find(Constants.ActionCameraObject).GetComponent<ActionCamera>().isActionCameraEnabled())
			{
				// Nice and slow
				lerpSpeed = 0.02f;
			}
			else
			{
				// No one's gonna wait that long
				lerpSpeed = 0.04f;
			}
		}
		else
		{
			lerpSpeed = 0.025f;
		}

		Vector3 currentLocation = movingPiece.gameObject.GetComponent<Transform>().position;
		Vector3 newLocation;
		bool stillInMotion = false;

		if (isSpaceship(movingPiece.getPiece()))
		{
			// Fly in, and then land
			if (Mathf.Abs(currentLocation.x - finalXPosition) >= 0.01f || Mathf.Abs(currentLocation.z - finalZPosition) >= 0.01f)
			{
				newLocation = new Vector3(Mathf.Lerp(currentLocation.x, finalXPosition, lerpSpeed),
					Mathf.Lerp(currentLocation.y, finalYPosition + 2, lerpSpeed),
					Mathf.Lerp(currentLocation.z, finalZPosition, lerpSpeed));
				movingPiece.gameObject.GetComponent<Transform>().position = newLocation;
				stillInMotion = true;
			}
			else if (Mathf.Abs(currentLocation.y - finalYPosition) >= 0.01f)
			{
				newLocation = new Vector3(currentLocation.x,
					Mathf.Lerp(currentLocation.y, finalYPosition, lerpSpeed),
					currentLocation.z);
				movingPiece.gameObject.GetComponent<Transform>().position = newLocation;
				stillInMotion = true;
			}
		}
		else
		{
			// Fade in like an adept Force user (or rather, Z warrior)
			Material meshMaterial = movingPiece.gameObject.GetComponent<MeshRenderer>().material;
			float currentAlpha = meshMaterial.GetColor("_Color").a, targetAlpha = 1.0f;
			meshMaterial.SetColor("_Color", new Color(meshMaterial.GetColor("_Color").r, meshMaterial.GetColor("_Color").g, meshMaterial.GetColor("_Color").b, Mathf.Lerp(currentAlpha, targetAlpha, lerpSpeed)));

			// Slide super gracefully
			if (Mathf.Abs(currentLocation.x - finalXPosition) >= 0.01f || Mathf.Abs(currentLocation.z - finalZPosition) >= 0.01f)
			{
				newLocation = new Vector3(Mathf.Lerp(currentLocation.x, finalXPosition, lerpSpeed),
					currentLocation.y,
					Mathf.Lerp(currentLocation.z, finalZPosition, lerpSpeed));
				movingPiece.gameObject.GetComponent<Transform>().position = newLocation;
				stillInMotion = true;
			}
			else if (Mathf.Abs(currentAlpha - targetAlpha) >= 0.01f)
			{
				stillInMotion = true;
			}
			else if (Mathf.Abs(currentAlpha - targetAlpha) < 0.01f)
			{
				// This case is required to make the pieces opaque
				if (movingPiece.getPiece().side == Side.Black)
				{
					movingPiece.gameObject.GetComponent<MeshRenderer>().material = opaqueBlackMaterial;
				}
				else
				{
					movingPiece.gameObject.GetComponent<MeshRenderer>().material = opaqueWhiteMaterial;
				}
			}
		}

		if (!stillInMotion)
		{
			// Ready to roll
			movingPiece.gameObject.GetComponent<PieceBehaviour>().isInAnimationState = false;
			TriggerNextStartAnimation(movingPiece.getPiece());
			if (gameKeeper.hasGameStarted())
			{
				ActionCamera actionCamera = GameObject.Find(Constants.ActionCameraObject).GetComponent<ActionCamera>();
				if (actionCamera.isActionCameraEnabled())
				{
					actionCamera.EnableMainCamera();
				}

				if (GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().chessBoard.CurrentMovingSide() == movingPiece.getPiece().side)
				{
					EventManager.TriggerEvent(Constants.EventNames.NewPlayerTurn);
				}
			}
			else
			{
				EventManager.TriggerEvent(Constants.EventNames.PieceSpawned);
			}
		}
	}

	public static void TriggerNextStartAnimation(AbstractPiece piece)
	{
		if (piece.GetType().Name == Constants.PieceClassNames.Pawn)
		{
			EventManager.TriggerEvent(Constants.EventNames.PawnsLanded);
		}
		else if (piece.GetType().Name == Constants.PieceClassNames.Bishop)
		{
			EventManager.TriggerEvent(Constants.EventNames.BishopsLanded);
		}
		else if (piece.GetType().Name == Constants.PieceClassNames.Knight)
		{
			EventManager.TriggerEvent(Constants.EventNames.KnightsLanded);
		}
		else if (piece.GetType().Name == Constants.PieceClassNames.Rook)
		{
			EventManager.TriggerEvent(Constants.EventNames.RooksLanded);
		}
	}

	public static void CorrectVerticalOffsets(PieceBehaviour piece)
	{
		if (!isSpaceship(piece.getPiece()))
		{
			// The Kings and Queens don't have a fancy start animation to get their positions right, so we'll have to do so here
			Vector3 currentPosition = piece.gameObject.GetComponent<Transform>().position;
			piece.gameObject.GetComponent<Transform>().position = new Vector3(currentPosition.x + HardcodedOffset(piece).x,
																				currentPosition.y + HardcodedOffset(piece).y,
																				currentPosition.z + HardcodedOffset(piece).z);
		}
	}

	public static Vector3 HardcodedOffset(PieceBehaviour piece)
	{
		// Some models are slightly displaced (yet to figure out how to fix this)
		// For now, compute offsets for the Position to coordinate mapping so as to align the model with the square it's on

		if (piece.getPiece().GetType().Name == Constants.PieceClassNames.Pawn && piece.getPiece().side == Side.White)
		{
			// X-wing is displaced on the x-axis by about a 1/8 square
			return new Vector3(GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().GetSquareSpacing() / 8, 0, 0);
		}
		else if (piece.getPiece().GetType().Name == Constants.PieceClassNames.Knight && piece.getPiece().side == Side.Black)
		{
			// Slave-1 is displaced on the x-axis by about a 1/8 square
			return new Vector3(GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().GetSquareSpacing() / 8, 0, 0);
		}
		else if (piece.getPiece().GetType().Name == Constants.PieceClassNames.Bishop && piece.getPiece().side == Side.White)
		{
			// Y-wing is displaced on the z-axis by about a quarter square
			return new Vector3(0, 0, -GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().GetSquareSpacing() / 4);
		}
		else if (piece.getPiece().GetType().Name == Constants.PieceClassNames.Rook && piece.getPiece().side == Side.White)
		{
			// CR-90 Corvette is displaced on the x-axis by about a 1/16 square
			return new Vector3(GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().GetSquareSpacing() / 16, 0, 0);
		}
		else if (piece.getPiece().GetType().Name == Constants.PieceClassNames.Queen && piece.getPiece().side == Side.Black)
		{
			// Darth Vader is displaced on the y-axis by a certain amount
			return new Vector3(0, 0.8f - piece.gameObject.GetComponent<Transform>().position.y, 0);
		}
		else if (piece.getPiece().GetType().Name == Constants.PieceClassNames.Queen && piece.getPiece().side == Side.White)
		{
			// Princess Leia is displaced on the y-axis by a certain amount
			return new Vector3(0, 1.0f - piece.gameObject.GetComponent<Transform>().position.y, 0);
		}
		else if (piece.getPiece().GetType().Name == Constants.PieceClassNames.King && piece.getPiece().side == Side.Black)
		{
			// Darth Sidious is displaced on the y-axis by a certain amount
			return new Vector3(0, 1.35f - piece.gameObject.GetComponent<Transform>().position.y, 0);
		}
		else if (piece.getPiece().GetType().Name == Constants.PieceClassNames.King && piece.getPiece().side == Side.White)
		{
			// Luke Skywalker is displaced on the y-axis by a certain amount
			return new Vector3(0, 1.3f - piece.gameObject.GetComponent<Transform>().position.y, 0);
		}
		else
		{
			return new Vector3(0, 0, 0);
		}

	}

	public static bool isSpaceship(AbstractPiece piece)
	{
		// The pieces that are represented as spaceships will have different animations, sound effects, etc.
		List<string> spaceshipPieceClassNames = new List<string>
		{
			Constants.PieceClassNames.Pawn,
			Constants.PieceClassNames.Rook,
			Constants.PieceClassNames.Bishop,
			Constants.PieceClassNames.Knight
		};

		if (spaceshipPieceClassNames.Contains(piece.GetType().Name))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public static Vector3 InitializeStartAnimationSettings(PieceBehaviour piece)
	{
		// Also take the liberty of initializing our shaders here, as their utility is somewhat related to the initial animation...

		// Initially, aircraft will be off the board and at a higher elevation than that of the board
		if (!GameKeeper.isDebug)
		{
			if (isSpaceship(piece.getPiece()))
			{
				Vector3 initialTransform = piece.gameObject.GetComponent<Transform>().position;
				int multiplier;
				if (piece.getPiece().side == Side.Black)
				{
					multiplier = 1;

					// The spaceship models are given an opaque shader
					opaqueBlackMaterial = piece.gameObject.GetComponent<MeshRenderer>().material;
				}
				else
				{
					multiplier = -1;
					opaqueWhiteMaterial = piece.gameObject.GetComponent<MeshRenderer>().material;
				}
				Vector3 finalTransform = new Vector3(initialTransform.x + multiplier * 2, initialTransform.y, initialTransform.z);
				return finalTransform;
			}
			else
			{
				// Assuming each King/Queen mesh has only one shader since they don't need glass
				Material meshMaterial = piece.gameObject.GetComponent<MeshRenderer>().material;
				if (piece.getPiece().side == Side.Black)
				{
					// The core piece models are given a fade shader
					fadeBlackMaterial = meshMaterial;
				}
				else
				{
					fadeWhiteMaterial = meshMaterial;
				}
				meshMaterial.SetColor("_Color", new Color(meshMaterial.GetColor("_Color").r, meshMaterial.GetColor("_Color").g, meshMaterial.GetColor("_Color").b, 0.0f));
				return piece.gameObject.GetComponent<Transform>().position;
			}
		}
		else
		{
			return piece.gameObject.GetComponent<Transform>().position;
		}
	}

	public static void DisplayQuitMessage()
	{
		GameObject.Find(Constants.SureText).GetComponent<Text>().enabled = true;
		GameObject.Find(Constants.YesText).GetComponent<Text>().enabled = true;
		GameObject.Find(Constants.NoText).GetComponent<Text>().enabled = true;

		GameObject.Find(Constants.YesText).GetComponent<Button>().onClick.AddListener(() => {
			GameObject.Find(Constants.SureText).GetComponent<Text>().enabled = false;
			GameObject.Find(Constants.YesText).GetComponent<Text>().enabled = false;
			GameObject.Find(Constants.NoText).GetComponent<Text>().enabled = false;
			GameObject.Find(Constants.QuitText).GetComponent<Text>().enabled = false;

			// Leave this commented if needless dramatization is a nobler cause than respecting the player's time
			Application.Quit();

			// Leave this commented if the player's time is to be valued
			//GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().StartCoroutine(ShowCredits());
		});
		GameObject.Find(Constants.NoText).GetComponent<Button>().onClick.AddListener(() => {
			GameObject.Find(Constants.SureText).GetComponent<Text>().enabled = false;
			GameObject.Find(Constants.YesText).GetComponent<Text>().enabled = false;
			GameObject.Find(Constants.NoText).GetComponent<Text>().enabled = false;
		});
	}

	private static IEnumerator ShowCredits()
	{
		// Makes the game creator's name fade in slowly to the epic Force Theme

		GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<AudioSource>().Stop();
		GameObject.Find(Constants.AuxiliaryAudioObject).GetComponent<AudioSource>().Stop();
		AudioSource creditsAudioSource = GameObject.Find(Constants.QuitAudioObject).GetComponent<AudioSource>();
		GameObject.Find(Constants.CreditsText).GetComponent<Text>().enabled = true;
		GameObject.Find(Constants.CreditsText).GetComponent<Text>().canvasRenderer.SetAlpha(0.01f);
		GameObject.Find(Constants.CreditsText).GetComponent<Text>().CrossFadeAlpha(1.0f, creditsAudioSource.clip.length / 2, false);
		creditsAudioSource.Play();
		yield return new WaitForSeconds(creditsAudioSource.clip.length);
		Application.Quit();
	}
}
