/*
 * Author: Abhishek Arora
 * The Behaviour script that triggers the opening crawl
 * */

using UnityEngine;
using System.Collections;

public class StarWarsLogo : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		// After the logo is sufficiently lost in the background, start the opening crawl
		if (this.gameObject.GetComponent<Transform>().position.x > 70)
		{
			Animations.StartOpeningCrawl();
		}
	}
}
