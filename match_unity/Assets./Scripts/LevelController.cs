using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelController : MonoBehaviour {

    public List<Sprite> tileSprites;
	public int rowCount;
	public int columnCount;

	private const int INPUT = 0;
	private const int ANIMATING = 1;
    private const int PROMPT_DELAY = 300;

	private Grid _levelGrid;
	private Vector3 _lastMousePosition;

    private int _mode;

	private int _startingTile = -1;
    private int _waitCounter = 0;

	// Use this for initialization
	void Start () {
        _levelGrid = new Grid(rowCount, columnCount, tileSprites);
		CreateLevel();
	}
	
	// Update is called once per frame
	void Update () {
		switch(_mode){
			case INPUT:
				CheckForInput();
                if (_waitCounter++ >= PROMPT_DELAY) {
                    _levelGrid.AnimatePrompts();
                }
				break;
			case ANIMATING:
				bool animationsCompleted = _levelGrid.AnimateTiles();
				if(animationsCompleted){
                    if (_startingTile > -1) { 
					    _levelGrid.ToggleTileSelection(_startingTile);
					    _startingTile = -1;
                    }
                    _waitCounter = 0;
					_mode = INPUT;
				}
				break;
		}

	}

	private void CheckForInput(){
		if(Input.GetMouseButtonDown(0)){
			_lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			int selectedTile =  _levelGrid.GetTileIndexAtXY(_lastMousePosition.x,_lastMousePosition.y);
			if(selectedTile == -1){
				_startingTile = -1;
			}
			else{
				_startingTile = selectedTile;
				_levelGrid.ToggleTileSelection(selectedTile);
			}
		}
		else if(_startingTile != -1){
			int tileToSwap = CalculateMouseDirection();
			if(tileToSwap != 0){
				int endingTile = _startingTile+tileToSwap;
				if(CheckTileMovementIsValid(_startingTile,endingTile)){
					_levelGrid.SwapTiles(_startingTile,_startingTile+tileToSwap);
					_mode = ANIMATING;
				}
			}
		}
	
	}

	private bool CheckTileMovementIsValid(int start, int end){
		bool valid = true;
		if(end < 0 || end >= rowCount*columnCount){
			valid = false;
		}
		else if(start % columnCount == 0 && end == start-1){
			valid = false;
		}
		else if(end % columnCount == 0 && start == end-1){
			valid = false;
		}
		return valid;
	}

	/*
	 * Checks which direction the mouse/finger moves so that the tile can be switched to that direction
	 */

	private int CalculateMouseDirection(){
		//TODO: Need to ensure that a tile exists e.g. no < 0 or > length or > rowcount > columncount 
		Vector3 startPosition = _lastMousePosition;
		Vector3 endPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector3 difference =  endPosition - startPosition;
		float minChange = 0.3f;
		int direction = 0;
		if(difference.x > minChange){
			if(IsInRange(difference.y,minChange)){
				direction = 1;
				//EAST
			}
		}
		else if (difference.x < -minChange){
			if(IsInRange(difference.y,minChange)){
				direction = -1;
				//WEST
			}
		}
		else if(difference.y > minChange){
			if(IsInRange(difference.x,minChange)){
				direction = -columnCount;
				//SOUTH
			}
		}
		else if (difference.y < -minChange){
			if(IsInRange(difference.x,minChange)){
				direction = columnCount;
				//NORTH
			}	
		}
		return direction;
	}

	private bool IsInRange(float value,float range){
		return value > -range && value < range;
	}

	private void CreateLevel(){
		_levelGrid.CreateTiles();
        _mode = ANIMATING;
	}
}
