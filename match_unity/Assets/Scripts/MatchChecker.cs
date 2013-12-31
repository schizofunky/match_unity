using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchChecker{

	private int _originalTileContent;
	private Tile[] _tileList;
	private int _columnCount;	
	private int _lastColumnId;
	private int _mapSize;
	private List<int> _tilesMatched;

	public List<int> CheckForMatches(Tile[] tileList,List<int> changedTiles,int columnCount){
		_tileList = tileList;
		_columnCount = columnCount;
		_lastColumnId = columnCount-1;
		_tilesMatched = new List<int>();
		_mapSize = tileList.Length-1;
		foreach(int tileIndex in changedTiles){
			CheckTileForMatches(tileIndex);
		}
		return _tilesMatched;
	}

	private void CheckTileForMatches(int index){
		_originalTileContent = _tileList[index].tileContent;
		//Checks all 4 directions around the current tile for matches
		AddMatchesToBeRemovedToList(index,CheckDirection(index,-1),CheckDirection(index,1),CheckDirection(index,-_columnCount),CheckDirection(index,_columnCount));		
	}

	/*
	 * Recursively moves along a column/row searching for matches
	 */
	private int CheckDirection(int currentIndex, int incrementValue){
		int matches = 0;
		bool matchFound = false;
		do{
			currentIndex = currentIndex + incrementValue;
			if(IndexIsOutOfBounds(currentIndex,incrementValue)){
				break;
			}
			matchFound = (_tileList[currentIndex].tileContent == _originalTileContent); 
			if(matchFound){
				matches++;
			}
		}
		while(matchFound);
		return matches;
	}

	/*
	 * Checks to see if the new tile is actually next to the old tile
	 */ 
	private bool IndexIsOutOfBounds(int index, int increment){
		bool outOfBounds = false;
		if(Mathf.Abs(increment) == 1){
			//the check is for left-right
			int columnId = index % _columnCount;
			if(columnId == 0){//current index is the left edge of the grid
				outOfBounds = (((index - increment) % _columnCount) == _lastColumnId);
			}
			else if(columnId == _lastColumnId){//current index is the right edge of the grid
				outOfBounds = (((index - increment) % _columnCount) == 0);
			}
		}
		else{
			//The check is for up-down
			if(index < 0 || index > _mapSize){
				outOfBounds = true;
			}
		}
		return outOfBounds;
	}
	
	/*
	 * Checks the number of identical tiles across/down and if 3 exist adds them to the matched list 
	 */ 
	private void AddMatchesToBeRemovedToList(int startingtile,int left,int right,int above, int below){
		int matchIndex;
		if(left + right > 1){
			for(matchIndex = startingtile-left; matchIndex <= startingtile+right; matchIndex++){
				_tilesMatched.Add(matchIndex);
			}
		}
		if(above + below > 1){
			for(matchIndex = startingtile-(above*_columnCount); matchIndex <= startingtile+(below*_columnCount); matchIndex+=_columnCount){
				_tilesMatched.Add(matchIndex);
			}
		}
	}

}
