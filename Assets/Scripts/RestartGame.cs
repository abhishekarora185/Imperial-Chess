using UnityEngine;
using System.Collections;

public class RestartGame : MonoBehaviour {

	void OnMouseDown()
	{
		Application.LoadLevel(Application.loadedLevel);
	}
}
