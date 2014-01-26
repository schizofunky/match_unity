using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelController : MonoBehaviour {

    public List<Sprite> tileSprites;
	public int rowCount;
	public int columnCount;

	private Grid _levelGrid;
	private Vector3 _lastMousePosition;

	private int _startingTile = -1;
	private GridInfo _gridInfo;

	// Use this for initialization
	void Start () {
		_gridInfo = new GridInfo(rowCount, columnCount, tileSprites);
		_levelGrid = new Grid(_gridInfo);
	}
	
	// Update is called once per frame
	void Update () {
		_levelGrid.Update();
		if(_levelGrid.currentState.Equals(Grid.GridState.IDLE)){
			CheckForInput();
		}	
	}

	private void CheckForInput(){
		if(Input.GetMouseButtonDown(0)){
			_lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			_startingTile = _levelGrid.SelectTileIndexAtXY(_lastMousePosition.x,_lastMousePosition.y);
		}
		else if(_startingTile != -1){
			int tileToSwap = CalculateMouseDirection();
			if(tileToSwap != 0){
				int endingTile = _startingTile+tileToSwap;
				if(CheckTileMovementIsValid(_startingTile,endingTile)){
					_levelGrid.SwapTiles(_startingTile,_startingTile+tileToSwap);
					_startingTile = -1;
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
}
