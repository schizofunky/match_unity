using UnityEngine;
using System.Collections;

public class Grid {

	private const float SPACING = 0.9f;
	private const float START_X = -1;
	private const float START_Y = 4;

	private int _tileCounter = 0;
	private int _rowCount;
	private int _columnCount;
	private Sprite[] _tileSprites;

	public Grid(int rowCount,int columnCount,Sprite[] tileSprites){
		_rowCount = rowCount; 
		_columnCount = columnCount; 
		_tileSprites = tileSprites;
	}

	public void CreateTiles(){ 
		int totalTiles = _rowCount * _columnCount;
		int i;
		for(i = 0; i < totalTiles; i++){
			CreateTile(_tileCounter++);
		}
	}
	
	private void CreateTile(int index){
		Tile myTile = new Tile(index, _tileSprites[(int)Mathf.Round(Random.value * (_tileSprites.Length-1))]);
		myTile.SetPosition(CalculateTileX(index),CalculateTileY(index));
	}
	
	private float CalculateTileX(int index){
		return START_X + (SPACING * (index % _rowCount));
	}
	
	private float CalculateTileY(int index){		
		return START_Y - (SPACING * Mathf.Floor(index / _columnCount));
	}
}
