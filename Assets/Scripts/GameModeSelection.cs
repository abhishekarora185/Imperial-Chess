using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameModeSelection : MonoBehaviour
{

	void Start()
	{
		GameObject.Find(Constants.TextGameObjectNames.ALongTimeAgo).GetComponent<MeshRenderer>().enabled = false;

		GameObject.Find(Constants.TextGameObjectNames.SinglePlayerMode).GetComponent<Button>().onClick.AddListener(() => {
			GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().InitializeGamekeeper(false, true);
			Destroy(this.gameObject);
		});

		GameObject.Find(Constants.TextGameObjectNames.MultiPlayerMode).GetComponent<Button>().onClick.AddListener(() =>
		{
			GameObject.Find(Constants.PieceNames.ChessBoard).GetComponent<GameKeeper>().InitializeGamekeeper(false, false);
			Destroy(this.gameObject);
		});
	}

}