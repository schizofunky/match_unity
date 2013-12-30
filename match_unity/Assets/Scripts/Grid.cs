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

	private int _tile1;
	private int _tile2;

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
		_tile1 = tile1Index;
		_tile2 = tile2Index;
		GameObject Tile1 = _tileList[tile1Index].gameObject;
		GameObject Tile2 = _tileList[tile2Index].gameObject;
		//create animations for both tiles
		_tile1Animation = new TileAnimation(Tile1, Tile2.transform.position,TILE_MOVE_SPEED);
		_tile2Animation = new TileAnimation(Tile2, Tile1.transform.position,TILE_MOVE_SPEED);

		//swap the positions in the array
		Tile tempTile = _tileList[tile1Index];
		_tileList[tile1Index] = _tileList[tile2Index];
		_tileList[tile2Index] = tempTile;
		_mode = CHECK_TILES;
	}

	public bool AnimateTiles(){
		bool animationsCompleted = false;
		_tile1Animation.UpdateAnimation();
		_tile2Animation.UpdateAnimation();
		if(_tile1Animation.IsCompleted()){


			if(_mode == CHECK_TILES){
				if(CheckForMatches(_tile1,_tile2)){
					//HandleMatchesFound
					animationsCompleted = true;
				}
				else{
					_mode = REVERSE_TILES;
					//reverse the animations
					_tile1Animation.ReverseAnimation();
					_tile2Animation.ReverseAnimation();

					//swap them back in the array
					Tile tempTile = _tileList[_tile1];
					_tileList[_tile1] = _tileList[_tile2];
					_tileList[_tile2] = tempTile;
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

	private bool CheckForMatches(int tile1Index, int tile2Index){
		return SubCheck(tile1Index) ||	SubCheck(tile2Index);
	}

	private bool SubCheck(int tileIndex){
		int startTileContent = _tileList[tileIndex].tileContent;
		int currentIndex = tileIndex;
		int leftMatches = 0;
		int rightMatches = 0;
		int aboveMatches = 0;
		int belowMatches = 0;
		bool matchFound = false;
		bool subMatchFound;
		do{
			if(currentIndex % _columnCount == 0){
				break;
			}
			currentIndex = currentIndex - 1;
			subMatchFound = (_tileList[currentIndex].tileContent == startTileContent); 
			if(subMatchFound){
				leftMatches++;
			}
		}
		while(subMatchFound);	
		currentIndex = tileIndex;		
		do{
			currentIndex = currentIndex + 1;
			if(currentIndex % _columnCount == 0){
				break;
			}
			subMatchFound = (_tileList[currentIndex].tileContent == startTileContent); 
			if(subMatchFound){
				rightMatches++;
			}
		}
		while(subMatchFound);
		currentIndex = tileIndex;		
		do{
			currentIndex = currentIndex + _columnCount;
			if(currentIndex >= _tileList.Length){
				break;
			}
			subMatchFound = (_tileList[currentIndex].tileContent == startTileContent); 
			if(subMatchFound){
				belowMatches++;
			}
		}
		while(subMatchFound);
		currentIndex = tileIndex;		
		do{
			currentIndex = currentIndex - _columnCount;
			if(currentIndex < 0){
				break;
			}
			subMatchFound = (_tileList[currentIndex].tileContent == startTileContent); 
			if(subMatchFound){
				aboveMatches++;
			}
		}
		while(subMatchFound);
		if(leftMatches + rightMatches > 1 || aboveMatches + belowMatches > 1){
			//3 or more have been matched
			matchFound = true;
		}
		return matchFound;
		
	}
}
