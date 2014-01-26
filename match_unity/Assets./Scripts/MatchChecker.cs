using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchChecker{

	private int _originalTileContent;
	private Tile[] _tileList;
	private int _columnCount;	
	private int _rowCount;
	private int _lastColumnId;
	private int _mapSize;
	private HashSet<int> _tilesMatched;

	public MatchChecker(Tile[] tileList,int columnCount, int rowCount){
		_tileList = tileList;
		_columnCount = columnCount;
		_rowCount = rowCount;
		_lastColumnId = columnCount-1;
		_mapSize = tileList.Length-1;
	}

	public HashSet<int> CheckForMatches(List<int> changedTiles){
		_tilesMatched = new HashSet<int>();
		foreach(int tileIndex in changedTiles){
			CheckTileForMatches(tileIndex);
		}
		return _tilesMatched;
	}

	public Vector3 GetPossibleMatch() {
		Vector3 possibleMatches;
		int c,row,s;
		for (row = 0; row < _rowCount; row++) {
			for (c = 0; c < _columnCount; c++) {
				s = c + (row * _columnCount);
				//Check to the right, start with 3 along then check the middle 2 
				if (c < _columnCount - 3) {
					if (_tileList[s].tileContent == _tileList[s + 3].tileContent) {
						
						if (_tileList[s].tileContent == _tileList[s + 1].tileContent) {
							possibleMatches = new Vector3(s, s+1, s+3);
							return possibleMatches;
						}
						if(_tileList[s].tileContent == _tileList[s + 2].tileContent) {
							possibleMatches = new Vector3(s, s + 2, s + 3);
							return possibleMatches;
						}
					}
				}
				//Check below
				if (row < _rowCount - 3) {
					if (_tileList[s].tileContent == _tileList[s + (3 * _columnCount)].tileContent) {
						if (_tileList[s].tileContent == _tileList[s + _columnCount].tileContent ){
							possibleMatches = new Vector3(s, s + _columnCount, s + (3 * _columnCount));
							return possibleMatches;
						}
						if( _tileList[s].tileContent == _tileList[s + (2 * _columnCount)].tileContent) {
							possibleMatches = new Vector3(s, s + (2 * _columnCount), s + (3 * _columnCount));
							return possibleMatches;
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
					possibleMatches = new Vector3(s, tl, tr);
					return possibleMatches;
				}
				if (CompareTileContents(tr, br) && _tileList[tr].tileContent == _tileList[s].tileContent) {
					possibleMatches = new Vector3(s, br, tr);
					return possibleMatches;
				}
				if (CompareTileContents(br, bl) && _tileList[br].tileContent == _tileList[s].tileContent) {
					possibleMatches = new Vector3(s, br, bl);
					return possibleMatches;
				}
				if (CompareTileContents(bl, tl) && _tileList[bl].tileContent == _tileList[s].tileContent) {
					possibleMatches = new Vector3(s, bl, tl);
					return possibleMatches;
				}
				if (CompareTileContents(b, tl) && _tileList[b].tileContent == _tileList[s].tileContent) {
					possibleMatches = new Vector3(s, b, tl);
					return possibleMatches;
				}
				if (CompareTileContents(b, tr) && _tileList[b].tileContent == _tileList[s].tileContent) {
					possibleMatches = new Vector3(s, b, tr);
					return possibleMatches;
				}
				if (CompareTileContents(t, bl) && _tileList[t].tileContent == _tileList[s].tileContent) {
					possibleMatches = new Vector3(s, t, bl);
					return possibleMatches;
				}
				if (CompareTileContents(t, br) && _tileList[t].tileContent == _tileList[s].tileContent) {
					possibleMatches = new Vector3(s, t, br);
					return possibleMatches;
				}
				if (CompareTileContents(r, tl) && _tileList[r].tileContent == _tileList[s].tileContent) {
					possibleMatches = new Vector3(s, r, tl);
					return possibleMatches;
				}
				if (CompareTileContents(r, bl) && _tileList[r].tileContent == _tileList[s].tileContent) {
					possibleMatches = new Vector3(s, r, bl);
					return possibleMatches;
				}
				if (CompareTileContents(l, tr) && _tileList[l].tileContent == _tileList[s].tileContent) {
					possibleMatches = new Vector3(s, l, tr);
					return possibleMatches;
				}
				if (CompareTileContents(l, br) && _tileList[l].tileContent == _tileList[s].tileContent) {
					possibleMatches = new Vector3(s, l, br);
					return possibleMatches;
					//return true;
				}
			}
		}
		return Vector3.zero;
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
            else if (columnId == _lastColumnId){//current index is the right edge of the grid                
				outOfBounds = (((index - increment) % _columnCount) == 0);
			}
		}
		//The check is for up-down
		if(index < 0 || index > _mapSize){
			outOfBounds = true;
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

}
