using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Animations;

public class Grid {

	private const float SPACING = 0.9f;
	private const float START_X = -3;
	private const float START_Y = 4.9f;
    private const int TILE_MOVE_SPEED = 10;
    private const int PROMPT_SPEED = 30;

    private const int CREATING_GRID = 0;
	private const int CHECK_TILES = 1;
	private const int REVERSE_TILES = 2;
	private const int REMOVE_MATCHES = 3;
    private const int REFILL_GRID = 4;
    private const int RECURSIVE_CHECK = 5;
    private const int FLUSHING = 6;

	private int _rowCount;
	private int _columnCount;
	private List<Sprite> _tileSprites;
	private Tile[] _tileList;

	private int _tile1;
	private int _tile2;

	private List<ITileAnimation> _tileAnimations;
    private HashSet<int> _currentMatches;
    private List<int> _recursiveTileList;
    private ScaleAnimation[] _matchPrompt = new ScaleAnimation[3];

	private int _mode;

    public Grid(int rowCount, int columnCount, List<Sprite> tileSprites){
		_rowCount = rowCount; 
		_columnCount = columnCount; 
		_tileSprites = tileSprites;
		_tileAnimations = new List<ITileAnimation>();
        _currentMatches = new HashSet<int>();
	}

	public void CreateTiles(){
        int tileCounter = 0;
        int totalTiles = _rowCount * _columnCount;
        List<int> usableSprites;
        _tileList = new Tile[totalTiles];
        int i;
        int leftIndex;
        int topIndex;
        for (i = 0; i < totalTiles; i++){
            leftIndex = i - 2;
            topIndex = i - _columnCount;
            //make sure that the tile is not the same as the tile 2 to the left and 2 above
            usableSprites = Enumerable.Range(0, _tileSprites.Count).ToList<int>();
            if (leftIndex > -1){
                usableSprites.Remove(_tileList[leftIndex].tileContent);
            }
            if (topIndex > -1){
                usableSprites.Remove(_tileList[topIndex].tileContent);
            }
            CreateTile(tileCounter++, _rowCount, usableSprites.ToArray());
        }
        _mode = CREATING_GRID;
        if (!DoMatchesExist()) {
            CreateTiles();
        } else {
            for (i = 0; i < totalTiles; i++) {
                _tileList[i].CreateSprite();
            }
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
		foreach(ITileAnimation animation in _tileAnimations){
			animation.UpdateAnimation();
			allAnimationsCompleted = allAnimationsCompleted && animation.IsCompleted();
		}
		if(allAnimationsCompleted){
            if (_mode == CREATING_GRID) {
                _tileAnimations.Clear();
                animationModeFinished = true;
            }
			else if(_mode == CHECK_TILES){
				if(_tileList[_tile1].tileContent != _tileList[_tile2].tileContent){
                    List<int> tilesToCheck = new List<int>();
                    tilesToCheck.Add(_tile1);
                    tilesToCheck.Add(_tile2);
                    CheckForMatches(tilesToCheck);                    
				}
				if(_currentMatches.Count == 0){
                    ReverseTiles();
				}
			}
			else if(_mode == REVERSE_TILES){
				animationModeFinished = true;
				_tileAnimations.Clear();
			}
			else if(_mode == REMOVE_MATCHES){
				_tileAnimations.Clear();
                foreach (int index in _currentMatches)
                {
                    _tileList[index].Destroy();
					_tileList[index] = null;
                }
				_recursiveTileList = RefillGrid();
				_mode = REFILL_GRID; 
			}
			else if(_mode == REFILL_GRID){				
				_tileAnimations.Clear();
                _mode = RECURSIVE_CHECK;
			}
            else if (_mode == RECURSIVE_CHECK)
            {
                _currentMatches.Clear();
                _tileAnimations.Clear();
                CheckForMatches(_recursiveTileList);   
                if (_currentMatches.Count == 0)
                {
                    if (!DoMatchesExist()) {
                        FlushTiles();
                    } else {
                        animationModeFinished = true;
                    }
                }
            } else if (_mode == FLUSHING) {
                RemoveAllTiles();
                _tileAnimations.Clear();
                CreateTiles();
            }
		
		}
		return animationModeFinished;
	}

    private void ReverseTiles()
    {
        _mode = REVERSE_TILES;
        //reverse the animations
        foreach (TileAnimation animation in _tileAnimations)
        {
            animation.ReverseAnimation();
        }

        //swap them back in the array
        Tile tempTile = _tileList[_tile1];
        _tileList[_tile1] = _tileList[_tile2];
        _tileList[_tile2] = tempTile;
    }
	
	private void CreateTile(int index, int animationRowOffset,int[] availableTiles){
        int tileIndex = (int)Mathf.Round(Random.value * (availableTiles.Length - 1));
        Tile tile = new Tile(index, _tileSprites[availableTiles[tileIndex]], availableTiles[tileIndex]);
		float destinationX = CalculateTileX(index);
		float destinationY = CalculateTileY(index);
		Vector3 animateFrom = new Vector3(destinationX,destinationY,0);
		tile.SetPosition(destinationX,destinationY+(animationRowOffset*SPACING));
		_tileList[index] = tile;
        if (animationRowOffset != 0){
            _tileAnimations.Add(new TileAnimation(_tileList[index].gameObject, animateFrom, TILE_MOVE_SPEED, TileAnimation.TRANSFORM));
        }
	}
	
	private float CalculateTileX(int index){
		return START_X + (SPACING * (index % _rowCount));
	}
	
	private float CalculateTileY(int index){		
		return START_Y - (SPACING * Mathf.Floor(index / _columnCount));
	}

    private void CheckForMatches(List<int> tilesToCheck)
    {
		MatchChecker matchChecker = new MatchChecker();
        _currentMatches = matchChecker.CheckForMatches(_tileList, tilesToCheck, _columnCount);
        if (_currentMatches.Count > 0)
        {
            RemovePrompt();
            _tileAnimations.Clear();
            RemoveMatchedTiles(_currentMatches);
            _mode = REMOVE_MATCHES;
            //matches found
        }
	}

	private void RemoveMatchedTiles(HashSet<int> matchedTiles){
        foreach (int index in matchedTiles) {
            _tileAnimations.Add(new ScaleAnimation(_tileList[index].gameObject, 0, TILE_MOVE_SPEED, false));
		}
	}

    private List<int> RefillGrid()
    {
		int tileListIndex;
        List<int> modifiedTiles = new List<int>();
		for(tileListIndex = _tileList.Length-1; tileListIndex >= 0; tileListIndex--){
			if(_tileList[tileListIndex] == null){
				int tile = FindTileAbove(tileListIndex);
				if(tile != -1){
					_tileList[tileListIndex] = _tileList[tile];
					_tileList[tile] = null;
                    modifiedTiles.Add(tileListIndex);
					_tileAnimations.Add(new TileAnimation(_tileList[tileListIndex].gameObject, new Vector3(CalculateTileX(tileListIndex),CalculateTileY(tileListIndex),0),TILE_MOVE_SPEED,TileAnimation.TRANSFORM));
				}
			}
		}
        GenerateNewTiles(modifiedTiles);
        return modifiedTiles;
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

    private void GenerateNewTiles(List<int> modifiedTiles)
    {
        int newIndex;
		for(int c = 0; c < _columnCount; c++){
			for(int r = _rowCount-1; r >= 0; r--){
				int tileIndex = (r*_columnCount) + c;
				if(_tileList[tileIndex] == null){
					int numberOfTilestoCreate = (int)Mathf.Ceil(((float)tileIndex+1)/_columnCount);
					for(int t = 0; t < numberOfTilestoCreate; t++){
                        newIndex = tileIndex -(t*_columnCount);
                        CreateTile(newIndex, numberOfTilestoCreate, GetUsableSprites(newIndex));
                        _tileList[newIndex].CreateSprite();
                        modifiedTiles.Add(newIndex);
					}
					break;
				}
			}
		}
	}

    private int[] GetUsableSprites(int startIndex) {
        List<int> usableSprites = Enumerable.Range(0, _tileSprites.Count).ToList();
        int leftIndex = startIndex - 1;
        int rightIndex = startIndex + 1;
        int belowIndex = startIndex + _columnCount;
        int totalTiles = _rowCount * _columnCount;
        if (leftIndex > -1) {
            usableSprites.Remove(_tileList[leftIndex].tileContent);
        }
        if (rightIndex < totalTiles && _tileList[rightIndex] != null) {
            usableSprites.Remove(_tileList[rightIndex].tileContent);
        }
        if (belowIndex < totalTiles) {
            usableSprites.Remove(_tileList[belowIndex].tileContent);
        }
        return usableSprites.ToArray();
    }

    public bool DoMatchesExist() {
        int c,row,s;
        for (row = 0; row < _rowCount; row++) {
            for (c = 0; c < _columnCount; c++) {
                s = c + (row * _columnCount);
                //Check to the right, start with 3 along then check the middle 2 
                if (c < _columnCount - 3) {
                    if (_tileList[s].tileContent == _tileList[s + 3].tileContent) {

                        if (_tileList[s].tileContent == _tileList[s + 1].tileContent) {
                            SetMatchPrompt(s, s+1, s+3);
                            return true;
                        }
                        if(_tileList[s].tileContent == _tileList[s + 2].tileContent) {
                            SetMatchPrompt(s, s + 2, s + 3);
                            return true;
                        }
                    }
                }
                //Check below
                if (row < _rowCount - 3) {
                    if (_tileList[s].tileContent == _tileList[s + (3 * _columnCount)].tileContent) {
                        if (_tileList[s].tileContent == _tileList[s + _columnCount].tileContent ){
                            SetMatchPrompt(s, s + _columnCount, s + (3 * _columnCount));
                            return true;
                        }
                        if( _tileList[s].tileContent == _tileList[s + (2 * _columnCount)].tileContent) {
                            SetMatchPrompt(s, s + (2 * _columnCount), s + (3 * _columnCount));
                            return true;
                        }
                    }
                }
            }
        }
        for (row = 0; row < _rowCount - 1; row++) {
            for (c = 0; c < _columnCount - 1; c++) {
            //check the diagonals
            int t, b;
            int l = -1;
            int r = -1;
            int bl = -1;
            int br = -1;
            int tl = -1;
            int tr = -1;
            s = c + (row * _columnCount);
            t = s - _columnCount;
            b = s + _columnCount;            
            
            if (s % _columnCount > 0) {
                l = s - 1;
                bl = b - 1;
                tl = t - 1;
            }
            if ((s+1) % _columnCount > 0) {
                r = s + 1;
                br = b + 1;
                tr = t + 1;
            }
            if (CompareTileContents(tl, tr) && _tileList[tl].tileContent == _tileList[s].tileContent) {
                SetMatchPrompt(s, tl, tr);
                return true;
            }
            if (CompareTileContents(tr, br) && _tileList[tr].tileContent == _tileList[s].tileContent) {
                SetMatchPrompt(s, br, tr);
                return true;
            }
            if (CompareTileContents(br, bl) && _tileList[br].tileContent == _tileList[s].tileContent) {
                SetMatchPrompt(s, br, bl);
                return true;
            }
            if (CompareTileContents(bl, tl) && _tileList[bl].tileContent == _tileList[s].tileContent) {
                SetMatchPrompt(s, bl, tl);
                return true;
            }
            if (CompareTileContents(b, tl) && _tileList[b].tileContent == _tileList[s].tileContent) {
                SetMatchPrompt(s, b, tl);
                return true;
            }
            if (CompareTileContents(b, tr) && _tileList[b].tileContent == _tileList[s].tileContent) {
                SetMatchPrompt(s, b, tr);
                return true;
            }
            if (CompareTileContents(t, bl) && _tileList[t].tileContent == _tileList[s].tileContent) {
                SetMatchPrompt(s, t, bl);
                return true;
            }
            if (CompareTileContents(t, br) && _tileList[t].tileContent == _tileList[s].tileContent) {
                SetMatchPrompt(s, t, br);
                return true;
            }
            if (CompareTileContents(r, tl) && _tileList[r].tileContent == _tileList[s].tileContent) {
                SetMatchPrompt(s, r, tl);
                return true;
            }
            if (CompareTileContents(r, bl) && _tileList[r].tileContent == _tileList[s].tileContent) {
                SetMatchPrompt(s, r, bl);
                return true;
            }
            if (CompareTileContents(l, tr) && _tileList[l].tileContent == _tileList[s].tileContent) {
                SetMatchPrompt(s, l, tr);
                return true;
            }
            if (CompareTileContents(l, br) && _tileList[l].tileContent == _tileList[s].tileContent) {
                SetMatchPrompt(s, l, br);
                return true;
            }
        }
        }
        return false;
    }

    private bool CompareTileContents(int index1,int index2) {
        bool tilesAreTheSame = false;
        if (index1 > -1 && index1 < _columnCount * _rowCount) {
            if (index2 > -1 && index2 < _columnCount * _rowCount) {
                if (_tileList[index1].tileContent == _tileList[index2].tileContent) {
                    tilesAreTheSame = true;
                }
            }
        }
        return tilesAreTheSame;
    }

    private void FlushTiles() {
        _mode = FLUSHING;
        Vector3 tilePosition;
        foreach (Tile tile in _tileList) {
            tilePosition = tile.GetPosition();
            _tileAnimations.Add(new TileAnimation(tile.gameObject, new Vector3(tilePosition.x, tilePosition.y - (_rowCount * SPACING), 0), TILE_MOVE_SPEED, TileAnimation.TRANSFORM));
        }

    }

    private void RemoveAllTiles() {
        int tileIndex;
        for (tileIndex = 0; tileIndex < _tileList.Length; tileIndex++ ) {
            _tileList[tileIndex].Destroy();
            _tileList[tileIndex] = null;
        }
    }

    private void SetMatchPrompt(int index1, int index2, int index3) {
        _matchPrompt[0] = new ScaleAnimation(_tileList[index1].gameObject, 2.5f, PROMPT_SPEED, true);
        _matchPrompt[1] = new ScaleAnimation(_tileList[index2].gameObject, 2.5f, PROMPT_SPEED, true);
        _matchPrompt[2] = new ScaleAnimation(_tileList[index3].gameObject, 2.5f, PROMPT_SPEED, true);
    }

    private void RemovePrompt() {
        if (_matchPrompt[0] != null) {
            _matchPrompt[0].Reset();
            _matchPrompt[1].Reset();
            _matchPrompt[2].Reset();
            _matchPrompt[0] = null;
            _matchPrompt[1] = null;
            _matchPrompt[2] = null;
        }
    }

    public void AnimatePrompts() {
        _matchPrompt[0].UpdateAnimation();
        _matchPrompt[1].UpdateAnimation();
        _matchPrompt[2].UpdateAnimation();
    }
}
