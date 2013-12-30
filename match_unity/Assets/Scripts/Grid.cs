using UnityEngine;
using System.Collections;

public class Grid {

	private const float SPACING = 0.9f;
	private const float START_X = -3;
	private const float START_Y = 4.9f;
	private const int TILE_MOVE_SPEED = 10;

	private const int CHECK_TILES = 0;
	private const int REVERSE_TILES = 1;

	private int _tileCounter = 0;
	private int _rowCount;
	private int _columnCount;
	private Sprite[] _tileSprites;
	private Tile[] _tileList;

	private TileAnimation _tile1Animation;
	private TileAnimation _tile2Animation;

	private int _mode;

	public Grid(int rowCount,int columnCount,Sprite[] tileSprites){
		_rowCount = rowCount; 
		_columnCount = columnCount; 
		_tileSprites = tileSprites;
	}

	public void CreateTiles(){ 
		int totalTiles = _rowCount * _columnCount;		
		_tileList = new Tile[totalTiles];
		int i;
		for(i = 0; i < totalTiles; i++){
			CreateTile(_tileCounter++);
		}
	}

	public int GetTileIndexAtXY(float x, float y){
		int selectedTile = -1;
		//TODO: Should be able to calculate the tileList index using x and y
		int i;
		for(i = 0; i < _tileList.Length; i++){
			if(_tileList[i].HitTest(x,y)){
				selectedTile = i;
				break;
			}
		}
		return selectedTile;
	}

	public void ToggleTileSelection(int tileIndex){
		_tileList[tileIndex].ToggleSelected();
	}

	public void SwapTiles(int tile1Index, int tile2Index){
		GameObject Tile1 = _tileList[tile1Index].gameObject;
		GameObject Tile2 = _tileList[tile2Index].gameObject;
		//create animations for both tiles
		_tile1Animation = new TileAnimation(Tile1, Tile2.transform.position,TILE_MOVE_SPEED);
		_tile2Animation = new TileAnimation(Tile2, Tile1.transform.position,TILE_MOVE_SPEED);
		_mode = CHECK_TILES;
	}

	public bool AnimateTiles(){
		bool animationsCompleted = false;
		_tile1Animation.UpdateAnimation();
		_tile2Animation.UpdateAnimation();
		if(_tile1Animation.IsCompleted()){


			if(_mode == CHECK_TILES){
				if(false){
					//HandleMatchesFound
				}
				else{
					_mode = REVERSE_TILES;
					_tile1Animation.ReverseAnimation();
					_tile2Animation.ReverseAnimation();
				}
			}
			else if(_mode == REVERSE_TILES){
				animationsCompleted = true;
			}
		
		}
		return animationsCompleted;
	}
	
	private void CreateTile(int index){
		int tileIndex = (int)Mathf.Round(Random.value * (_tileSprites.Length-1));
		Tile tile = new Tile(index, _tileSprites[tileIndex],tileIndex);
		tile.SetPosition(CalculateTileX(index),CalculateTileY(index));
		_tileList[index] = tile;
		//Debug.Log(tile.tileIndex);
	}
	
	private float CalculateTileX(int index){
		return START_X + (SPACING * (index % _rowCount));
	}
	
	private float CalculateTileY(int index){		
		return START_Y - (SPACING * Mathf.Floor(index / _columnCount));
	}
}
