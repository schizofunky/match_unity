using UnityEngine;
using System.Collections;

public class Tile {
	private GameObject gameObject;

	public Tile(int id, Sprite sprite) {
		gameObject = new GameObject();
		gameObject.name = "Tile"+id;
		gameObject.AddComponent("SpriteRenderer");
		gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
	}

	public void SetPosition(float x, float y){
		gameObject.transform.position = new Vector3(x,y,1);
		gameObject.transform.localScale = new Vector3(2,2,1);
	}
}
