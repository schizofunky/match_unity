using UnityEngine;
using System.Collections;

public class Tile {
	
	public int tileContent {get; private set;}

	public GameObject gameObject {get; private set;}

	private bool _selected = false;


	public Tile(int id, Sprite sprite, int tileIndex) {
		gameObject = new GameObject();
		gameObject.name = "Tile"+id;
		gameObject.AddComponent("SpriteRenderer");
		gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
		tileContent = tileIndex;
	}

    public void Destroy() {
        UnityEngine.Object.Destroy(gameObject);
    }

    public Vector3 GetPosition() {
        return gameObject.transform.position;
    }

	public void SetPosition(float x, float y){
		gameObject.transform.position = new Vector3(x,y,1);
		gameObject.transform.localScale = new Vector3(2,2,1);
	}

	public bool HitTest(float x, float y){
		bool hit = false;
		Vector3 pos = gameObject.transform.position;
		float size = 0.4f;
		if(x > pos.x - size &&  x < pos.x + size){
			if(y > pos.y - size &&  y < pos.y + size){
				hit = true;
			}
		}
		return hit;
	}

	public void ToggleSelected(){
		_selected = !_selected;
		if(_selected){			
			gameObject.transform.localScale = new Vector3(2.2f,2.2f,1);
		}else{
			gameObject.transform.localScale = new Vector3(2,2,1);
		}
	}

    public void Highlight(float amount) {
        gameObject.renderer.material.color = new Color(1, 1, 1, amount);
    }
}
