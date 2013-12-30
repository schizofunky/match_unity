using UnityEngine;
using System.Collections;

public class LevelController : MonoBehaviour {

	public Sprite[] tileSprites;
	public int rowCount;
	public int columnCount;

	private Grid _levelGrid;

	// Use this for initialization
	void Start () {
		_levelGrid = new Grid(rowCount,columnCount,tileSprites);
		CreateLevel();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void CreateLevel(){
		_levelGrid.CreateTiles();
	}
}
