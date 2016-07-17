using UnityEngine;
using System.Collections;

public class TurnIcon : MonoBehaviour {

	private float aliveTime;

	private Vector3 initialAngle;

	// Use this for initialization
	void Start () {
		this.aliveTime = 2.0f;
		if (!GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().isGameOver())
		{
			// Allow the turn icon to persist
			StartCoroutine(this.KillTurnIcon());
		}
		this.initialAngle = this.gameObject.GetComponent<Transform>().eulerAngles;

		GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().TogglePauseGame();
	}
	
	// Update is called once per frame
	void Update () {
		// Play a neat rotation animation
		Vector3 currentAngle = this.gameObject.GetComponent<Transform>().eulerAngles;

		currentAngle = new Vector3(currentAngle.x,
				Mathf.Lerp(currentAngle.y, initialAngle.y + 90, 0.1f),
				currentAngle.z
			);

		this.gameObject.GetComponent<Transform>().eulerAngles = currentAngle;
	}

	private IEnumerator KillTurnIcon()
	{
		yield return new WaitForSeconds(this.aliveTime);
		GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().TogglePauseGame();
		Destroy(this.gameObject);
	}
}
