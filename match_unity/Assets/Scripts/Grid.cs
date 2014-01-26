using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Animations;

public class Grid {

	private const float SPACING = 0.9f;
    private const int TILE_MOVE_SPEED = 10;

    public enum GridState{
        CREATING_GRID = 0,
        CHECK_TILES = 1,    
        REVERSE_TILES = 2,    
        REMOVE_MATCHES = 3,    
        REFILL_GRID = 4,    
        RECURSIVE_CHECK = 5,  
        FLUSHING = 6  
    } 

	private int _rowCount;
	private int _columnCount;
	private List<Sprite> _tileSprites;
	private Tile[] _tileList;

	private int _tile1;
	private int _tile2;

	private List<ITileAnimation> _tileAnimations;
    private HashSet<int> _currentMatches;
    private List<int> _recursiveTileList;
	private MatchPrompter _prompter;

	private GridState _mode;
	private TileCreator _tileCreator;
	private MatchChecker _matchChecker;

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
		_tileCreator =  new TileCreator(_tileSprites,_tileList,_rowCount,_columnCount);
		_matchChecker = new MatchChecker(_tileList,_columnCount,_rowCount);
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
			TileAnimation animation = _tileCreator.CreateTile(tileCounter++, _rowCount, usableSprites.ToArray());
			if(animation != null){
				_tileAnimations.Add(animation);
			}
        }
		_prompter = new MatchPrompter(_tileList);
        _mode = GridState.CREATING_GRID;
		Vector3 possibleMatches = _matchChecker.GetPossibleMatch();
		if(possibleMatches == Vector3.zero){
			CreateTiles();
		}
		else{
			_prompter.SetMatchPrompt(possibleMatches);
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
		_mode = GridState.CHECK_TILES;
	}
	
	public void AnimatePrompts() {
		_prompter.AnimatePrompts();
	}

	public bool AnimateTiles(){
		bool animationModeFinished = false;
		bool allAnimationsCompleted = true;
		foreach(ITileAnimation animation in _tileAnimations){
			animation.UpdateAnimation();
			allAnimationsCompleted = allAnimationsCompleted && animation.IsCompleted();
		}
		if(allAnimationsCompleted){
            if (_mode == GridState.CREATING_GRID) {
                _tileAnimations.Clear();
                animationModeFinished = true;
            }
			else if(_mode == GridState.CHECK_TILES){
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
			else if(_mode == GridState.REVERSE_TILES){
				animationModeFinished = true;
				_tileAnimations.Clear();
			}
			else if(_mode == GridState.REMOVE_MATCHES){
				_tileAnimations.Clear();
                foreach (int index in _currentMatches)
                {
                    _tileList[index].Destroy();
					_tileList[index] = null;
                }
				_recursiveTileList = _tileCreator.RefillGrid(_tileAnimations);
				_mode = GridState.REFILL_GRID; 
			}
			else if(_mode == GridState.REFILL_GRID){				
				_tileAnimations.Clear();
                _mode = GridState.RECURSIVE_CHECK;
			}
            else if (_mode == GridState.RECURSIVE_CHECK)
            {
                _currentMatches.Clear();
                _tileAnimations.Clear();
                CheckForMatches(_recursiveTileList);   
                if (_currentMatches.Count == 0)
                {
					Vector3 possibleMatches = _matchChecker.GetPossibleMatch();
					if(possibleMatches == Vector3.zero){
						FlushTiles();
					}
					else{
						_prompter.SetMatchPrompt(possibleMatches);
						animationModeFinished = true;
					}
                }
            } else if (_mode == GridState.FLUSHING) {
                RemoveAllTiles();
                _tileAnimations.Clear();
                CreateTiles();
            }
		
		}
		return animationModeFinished;
	}









    private void ReverseTiles()
    {
        _mode = GridState.REVERSE_TILES;
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
    private void CheckForMatches(List<int> tilesToCheck)
    {
		_currentMatches = _matchChecker.CheckForMatches(tilesToCheck);
        if (_currentMatches.Count > 0)
        {
			_prompter.RemovePrompt();
            _tileAnimations.Clear();
            RemoveMatchedTiles(_currentMatches);
            _mode = GridState.REMOVE_MATCHES;
            //matches found
        }
	}

	private void RemoveMatchedTiles(HashSet<int> matchedTiles){
        foreach (int index in matchedTiles) {
            _tileAnimations.Add(new ScaleAnimation(_tileList[index].gameObject, 0, TILE_MOVE_SPEED, false));
		}
	}

    private void FlushTiles() {
        _mode = GridState.FLUSHING;
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
}
