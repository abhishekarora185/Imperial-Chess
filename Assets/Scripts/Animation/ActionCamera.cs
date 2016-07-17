using UnityEngine;
using System.Collections;

public class ActionCamera : MonoBehaviour {

	// This is needed to prevent the game loop playing during the death audio clip when the player(s) take killing turns too fast in succession
	private Queue deathAudioClipPlayQueue;

	public AudioSource gameLoopAudioSource;
	public AudioSource deathAudioSource;

	// Use this for initialization
	void Start () {
		this.EnableMainCamera();
		this.deathAudioClipPlayQueue = new Queue();
		this.gameLoopAudioSource = GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<AudioSource>();
		this.deathAudioSource = GameObject.Find(Constants.AuxiliaryAudioObject).GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		if (this.deathAudioClipPlayQueue.Count == 0 && !this.gameLoopAudioSource.isPlaying)
		{
			this.gameLoopAudioSource.UnPause();
		}
	}

	public bool isActionCameraEnabled()
	{
		return this.gameObject.GetComponent<Camera>().enabled;
	}

	public void EnableMainCamera()
	{
		GameObject.Find(Constants.MainCameraObject).GetComponent<Camera>().enabled = true;
		this.gameObject.GetComponent<Camera>().enabled = false;
	}

	public void EnableActionCamera(AbstractPiece movingPiece, AbstractPiece dyingPiece)
	{
		// The action camera will always point towards a dying object from the initial position of the object destroying it
		GameKeeper gameKeeper = GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>();

		if (movingPiece.side == Side.White)
		{
			// Positive music
			this.deathAudioSource.clip = (AudioClip)Resources.Load(Constants.AudioClipNames.AudioDirectory + Constants.AudioClipNames.RebelAttack, typeof(AudioClip));
		}
		if (movingPiece.side == Side.Black)
		{
			// Badass music
			this.deathAudioSource.clip = (AudioClip)Resources.Load(Constants.AudioClipNames.AudioDirectory + Constants.AudioClipNames.ImperialMarch, typeof(AudioClip));
		}

		StartCoroutine(this.PlayDeathMusic());

		Vector3 forward = gameKeeper.GetTransformFromPosition(movingPiece.GetCurrentPosition()) - gameKeeper.GetTransformFromPosition(dyingPiece.GetCurrentPosition());
		Vector3 up = new Vector3(0, 1, 0);
		Vector3 cameraPosition = gameKeeper.GetTransformFromPosition(dyingPiece.GetCurrentPosition()) - 2 * gameKeeper.GetSquareSpacing() * forward.normalized;
		cameraPosition.y = 1;

		this.gameObject.GetComponent<Transform>().position = cameraPosition;
		this.gameObject.GetComponent<Transform>().rotation = Quaternion.LookRotation(forward, up);
		
		GameObject.Find(Constants.MainCameraObject).GetComponent<Camera>().enabled = false;
		this.gameObject.GetComponent<Camera>().enabled = true;
	}

	private IEnumerator PlayDeathMusic()
	{
		this.gameLoopAudioSource.Pause();
		this.deathAudioSource.Play();

		// Enqueueing a random object; the number of objects in the queue matter, not the objects themselves
		this.deathAudioClipPlayQueue.Enqueue(new Object());
		yield return new WaitForSeconds(this.deathAudioSource.clip.length);

		this.deathAudioClipPlayQueue.Dequeue();
	}

}
