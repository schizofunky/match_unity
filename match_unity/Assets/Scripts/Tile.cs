using UnityEngine;
using System.Collections;

public class Tile {
	
	public int tileIndex {get; private set;}

	private GameObject _gameObject;


	public Tile(int id, Sprite sprite, int tileIndex) {
		_gameObject = new GameObject();
		_gameObject.name = "Tile"+id;
		_gameObject.AddComponent("SpriteRenderer");
		_gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
		this.tileIndex = tileIndex;
	}

	public void SetPosition(float x, float y){
		_gameObject.transform.position = new Vector3(x,y,1);
		_gameObject.transform.localScale = new Vector3(2,2,1);
	}
}
