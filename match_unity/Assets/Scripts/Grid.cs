using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid {

	private const float SPACING = 0.9f;
	private const float START_X = -3;
	private const float START_Y = 4.9f;
	private const int TILE_MOVE_SPEED = 10;

	private const int CHECK_TILES = 0;
	private const int REVERSE_TILES = 1;
	private const int REMOVE_MATCHES = 2;
	private const int REFILL_GRID = 3;

	private int _tileCounter = 0;
	private int _rowCount;
	private int _columnCount;
	private Sprite[] _tileSprites;
	private Tile[] _tileList;

	private int _tile1;
	private int _tile2;

	private List<TileAnimation> _tileAnimations;
	private List<int> _currentMatches;

	private int _mode;

	public Grid(int rowCount,int columnCount,Sprite[] tileSprites){
		_rowCount = rowCount; 
		_columnCount = columnCount; 
		_tileSprites = tileSprites;
		_tileAnimations = new List<TileAnimation>();
	}

	public void CreateTiles(){ 
		int totalTiles = _rowCount * _columnCount;		
		_tileList = new Tile[totalTiles];
		int i;
		for(i = 0; i < totalTiles; i++){
			CreateTile(_tileCounter++,0);
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
		_tileAnimations.Add(new TileAnimation(Tile1, Tile2.transform.position,TILE_MOVE_SPEED,TileAnimation.TRANSFORM));
		_tileAnimations.Add(new TileAnimation(Tile2, Tile1.transform.position,TILE_MOVE_SPEED,TileAnimation.TRANSFORM));
		//swap the positions in the array
		Tile tempTile = _tileList[tile1Index];
		_tileList[tile1Index] = _tileList[tile2Index];
		_tileList[tile2Index] = tempTile;
		_mode = CHECK_TILES;
	}

	public bool AnimateTiles(){
		bool animationModeFinished = false;
		bool allAnimationsCompleted = true;
		foreach(TileAnimation animation in _tileAnimations){
			animation.UpdateAnimation();
			allAnimationsCompleted = allAnimationsCompleted && animation.IsCompleted();
		}
		if(allAnimationsCompleted){

			if(_mode == CHECK_TILES){
				if(_tileList[_tile1].tileContent != _tileList[_tile2].tileContent){
					_currentMatches = CheckForMatches(_tile1,_tile2);
					if(_currentMatches.Capacity > 0){
						_tileAnimations.Clear();
						RemoveMatchedTiles(_currentMatches);
						_mode = REMOVE_MATCHES;
						//matches found
					}
				}
				else{
					_mode = REVERSE_TILES;
					//reverse the animations
					foreach(TileAnimation animation in _tileAnimations){
						animation.ReverseAnimation();
					}

					//swap them back in the array
					Tile tempTile = _tileList[_tile1];
					_tileList[_tile1] = _tileList[_tile2];
					_tileList[_tile2] = tempTile;
				}
			}
			else if(_mode == REVERSE_TILES){
				animationModeFinished = true;
				_tileAnimations.Clear();
			}
			else if(_mode == REMOVE_MATCHES){
				_tileAnimations.Clear();
				foreach(int index in _currentMatches){
					_tileList[index].gameObject.SetActive(false);
					_tileList[index] = null;
				}
				RefillGrid();
				_mode = REFILL_GRID; 
			}
			else if(_mode == REFILL_GRID){				
				_tileAnimations.Clear();				
				animationModeFinished = true;
			}
		
		}
		return animationModeFinished;
	}
	
	private void CreateTile(int index, int animationRowOffset){
		int tileIndex = (int)Mathf.Round(Random.value * (_tileSprites.Length-1));
		Tile tile = new Tile(index, _tileSprites[tileIndex],tileIndex);
		float destinationX = CalculateTileX(index);
		float destinationY = CalculateTileY(index);
		Vector3 animateFrom = new Vector3(destinationX,destinationY,0);
		tile.SetPosition(destinationX,destinationY+(animationRowOffset*SPACING));
		_tileList[index] = tile;
		_tileAnimations.Add(new TileAnimation(_tileList[index].gameObject, animateFrom,TILE_MOVE_SPEED,TileAnimation.TRANSFORM));
	}
	
	private float CalculateTileX(int index){
		return START_X + (SPACING * (index % _rowCount));
	}
	
	private float CalculateTileY(int index){		
		return START_Y - (SPACING * Mathf.Floor(index / _columnCount));
	}

	private List<int> CheckForMatches(int tile1Index, int tile2Index){
		MatchChecker matchChecker = new MatchChecker();
		List<int> tilesToCheck = new List<int>();
		tilesToCheck.Add(tile1Index);
		tilesToCheck.Add(tile2Index);
		List<int> matchedTiles = matchChecker.CheckForMatches(_tileList,tilesToCheck,_columnCount);
		return matchedTiles;
	}

	private void RemoveMatchedTiles(List<int> matchedTiles){
		foreach(int index in matchedTiles){
			_tileAnimations.Add(new TileAnimation(_tileList[index].gameObject, new Vector3(0,0,0),TILE_MOVE_SPEED,TileAnimation.SCALE));
		}
	}

	private void RefillGrid(){
		int tileListIndex;
		for(tileListIndex = _tileList.Length-1; tileListIndex >= 0; tileListIndex--){
			if(_tileList[tileListIndex] == null){
				int tile = FindTileAbove(tileListIndex);
				if(tile != -1){
					_tileList[tileListIndex] = _tileList[tile];
					_tileList[tile] = null;
					_tileAnimations.Add(new TileAnimation(_tileList[tileListIndex].gameObject, new Vector3(CalculateTileX(tileListIndex),CalculateTileY(tileListIndex),0),TILE_MOVE_SPEED,TileAnimation.TRANSFORM));
				}
			}
		}
		GenerateNewTiles();
	}

	private int FindTileAbove(int startIndex){
		int line = -1;
		for(int index = startIndex - _columnCount; index >= 0; index-=_columnCount){	
			if(_tileList[index] != null){
				line = index;
				break;
			}
		}
		return line;
	}

	private void GenerateNewTiles(){
		for(int c = 0; c < _columnCount; c++){
			for(int r = _rowCount-1; r >= 0; r--){
				int tileIndex = (r*_columnCount) + c;
				if(_tileList[tileIndex] == null){
					int numberOfTilestoCreate = (int)Mathf.Ceil(((float)tileIndex+1)/_columnCount);
					Debug.Log(r+" "+c+" "+numberOfTilestoCreate+" "+tileIndex);
					for(int t = 0; t < numberOfTilestoCreate; t++){
						CreateTile(tileIndex -(t*_columnCount),numberOfTilestoCreate);
					}
					break;
				}
			}
		}
	}
}
