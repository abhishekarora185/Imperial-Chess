using UnityEngine;
using System.Collections;

public class StarWarsLogo : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (this.gameObject.GetComponent<Transform>().position.x > 70)
		{
			Animations.StartOpeningCrawl();
		}
	}
}
