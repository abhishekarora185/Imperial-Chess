/*
 * Author: Abhishek Arora
 * The Behaviour script that controls the Opening Crawl and triggers the rotation of the camera towards the board
 * */

using UnityEngine;
using System.Collections;

public class OpeningCrawl : MonoBehaviour {

	// The angle to which the camera has to rotate after the opening crawl has flown away
	private Vector3 targetRotationAngle;
	private bool skipOpeningCrawl;
	private bool revealBoardClipStarted;

	// Use this for initialization
	void Start () {
		this.targetRotationAngle = Camera.main.GetComponent<Transform>().eulerAngles;
		targetRotationAngle.x = targetRotationAngle.x + 90;
		this.skipOpeningCrawl = false;

		if (GameKeeper.isDebug)
		{
			this.skipOpeningCrawl = true;
		}

		this.revealBoardClipStarted = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			// Skip the opening crawl if the user clicks anything
			this.HideOpeningCrawlObjects();
			this.skipOpeningCrawl = true;
		}

		if (this.gameObject.GetComponent<Transform>().position.y > 5)
		{
			// Hide the Star Wars logo
			if (GameObject.Find(Constants.TextGameObjectNames.StarWarsLogo) != null)
			{
				GameObject.Find(Constants.TextGameObjectNames.StarWarsLogo).SetActive(false);
			}
		}

		if (this.gameObject.GetComponent<Transform>().position.y > 78 || this.skipOpeningCrawl)
		{
			// Turn all attention towards the board
			if (this.gameObject.GetComponent<AudioSource>().isPlaying)
			{
				this.gameObject.GetComponent<AudioSource>().Stop();
			}

			// Allow user to quit at this point
			GameObject.Find(Constants.QuitText).GetComponent<UnityEngine.UI.Text>().enabled = true;
			GameObject.Find(Constants.QuitText).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
				Animations.DisplayQuitMessage();
			});

			Animations.RotateCameraToChessboard(this.GetTargetRotationAngle(), this.revealBoardClipStarted);
			this.revealBoardClipStarted = true;
		}
	}

	public void DestroyOpeningCrawlObjects()
	{
		// Clean up objects that won't be used later
		if (GameObject.Find(Constants.TextGameObjectNames.ALongTimeAgo) != null)
		{
			GameObject.Find(Constants.TextGameObjectNames.ALongTimeAgo).SetActive(false);
		}
		if (GameObject.Find(Constants.TextGameObjectNames.StarWarsLogo) != null)
		{
			GameObject.Find(Constants.TextGameObjectNames.StarWarsLogo).SetActive(false);
		}
		this.gameObject.SetActive(false);
	}

	public Vector3 GetTargetRotationAngle()
	{
		return this.targetRotationAngle;
	}

	// Hide opening crawl objects till they are cleaned up
	private void HideOpeningCrawlObjects()
	{
		if (GameObject.Find(Constants.TextGameObjectNames.ALongTimeAgo) != null)
		{
			GameObject.Find(Constants.TextGameObjectNames.ALongTimeAgo).GetComponent<MeshRenderer>().enabled = false;
		}
		if (GameObject.Find(Constants.TextGameObjectNames.StarWarsLogo) != null)
		{
			GameObject.Find(Constants.TextGameObjectNames.StarWarsLogo).GetComponent<SpriteRenderer>().enabled = false;
		}
		foreach (MeshRenderer textMesh in this.gameObject.GetComponentsInChildren<MeshRenderer>())
		{
			textMesh.enabled = false;
		}
		foreach (SpriteRenderer spriteRenderer in this.gameObject.GetComponentsInChildren<SpriteRenderer>())
		{
			spriteRenderer.enabled = false;
		}
	}
}
