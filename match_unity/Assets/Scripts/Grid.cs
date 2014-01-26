using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Animations;

public class Grid : BaseStateMachine{
    public enum GridState{
        CREATING_GRID = 0,
        CHECK_TILES = 1,    
        REVERSE_TILES = 2,    
        REMOVE_MATCHES = 3,    
        REFILL_GRID = 4,    
        RECURSIVE_CHECK = 5,  
        FLUSHING = 6,
		IDLE = 7
    } 

	private GridInfo _gridInfo;
	private List<ITileAnimation> _tileAnimations;
    private HashSet<int> _currentMatches;
	private List<int> _activeTileList;
	private MatchPrompter _prompter;
	private TileCreator _tileCreator;
	private MatchChecker _matchChecker;

    public Grid(GridInfo gridInfo){
		_gridInfo = gridInfo;
		_tileAnimations = new List<ITileAnimation>();
        _currentMatches = new HashSet<int>();
		_activeTileList = new List<int>();
		currentState = GridState.CREATING_GRID;
	}

	public void Update(){
		UpdateStates();
	}

	/*
	 * Returns a tile at the chosen location, used to determine if the player clicked on a tile 
	 */ 
	public int SelectTileIndexAtXY(float x, float y){
		int index = _tileCreator.getTileAtXY(x, y);
		if(index > -1){
			_gridInfo.tileList[index].SetSelected(true);
		}
		return index;
	}

	/*
	 * When the player gestures to swap two tiles this is called which kicks the state machine into action
	 */ 
	public void SwapTiles(int tile1Index, int tile2Index){
		_activeTileList.Clear();
		_activeTileList.Add (tile1Index);
		_activeTileList.Add (tile2Index);
		currentState = GridState.CHECK_TILES;
	}

	/*
	 * Gets called across all states to play any animations involving any tile transformation
	 */
	private bool UpdateAnimations(){
		bool allAnimationsCompleted = true;
		foreach(ITileAnimation animation in _tileAnimations){
			animation.UpdateAnimation();
			allAnimationsCompleted = allAnimationsCompleted && animation.IsCompleted();
		}
		return allAnimationsCompleted;
	}

	#region idle state

	public void IDLE_Update() {
		_prompter.AnimatePrompts();
	}	

	#endregion

	#region creating_grid state

	public void CREATING_GRID_EnterState() {
		int tileCounter = 0;
		int totalTiles = _gridInfo.totalTiles;
		List<int> usableSprites;
		
		int i;
		int leftIndex;
		int topIndex;
		_tileCreator =  new TileCreator(_gridInfo);
		_matchChecker = new MatchChecker(_gridInfo);
		for (i = 0; i < totalTiles; i++){
			leftIndex = i - 2;
			topIndex = i - _gridInfo.columnCount;
			//make sure that the tile is not the same as the tile 2 to the left and 2 above
			usableSprites = Enumerable.Range(0, _gridInfo.tileSprites.Count).ToList<int>();
			if (leftIndex > -1){
				usableSprites.Remove(_gridInfo.tileList[leftIndex].tileContent);
			}
			if (topIndex > -1){
				usableSprites.Remove(_gridInfo.tileList[topIndex].tileContent);
			}
			TileAnimation animation = _tileCreator.CreateTile(tileCounter++, _gridInfo.rowCount, usableSprites.ToArray());
			if(animation != null){
				_tileAnimations.Add(animation);
			}
		}
		_prompter = new MatchPrompter(_gridInfo.tileList);
		Vector3 possibleMatches = _matchChecker.GetPossibleMatch();
		if(possibleMatches == Vector3.zero){
			currentState = GridState.CREATING_GRID;
		}
		else{
			_prompter.SetMatchPrompt(possibleMatches);
			for (i = 0; i < totalTiles; i++) {
				_gridInfo.tileList[i].CreateSprite();
			}
		}
	}
	
	public void CREATING_GRID_Update() {
		if(UpdateAnimations()){			
			_tileAnimations.Clear();
			currentState = GridState.IDLE;
		}
	}	

	#endregion

	#region check_tiles state

	public void CHECK_TILES_EnterState() {
		_gridInfo.tileList[_activeTileList[0]].SetSelected(false);		
		_gridInfo.tileList[_activeTileList[1]].SetSelected(false);
		GameObject Tile1 = _gridInfo.tileList[_activeTileList[0]].gameObject;
		GameObject Tile2 = _gridInfo.tileList[_activeTileList[1]].gameObject;
		//create animations for both tiles
		_tileAnimations.Add(new TileAnimation(Tile1, Tile2.transform.position,TileCreator.TILE_MOVE_SPEED,TileAnimation.TRANSFORM));
		_tileAnimations.Add(new TileAnimation(Tile2, Tile1.transform.position,TileCreator.TILE_MOVE_SPEED,TileAnimation.TRANSFORM));
		//swap the positions in the array
		Tile tempTile = _gridInfo.tileList[_activeTileList[0]];
		_gridInfo.tileList[_activeTileList[0]] = _gridInfo.tileList[_activeTileList[1]];
		_gridInfo.tileList[_activeTileList[1]] = tempTile;
	}
	
	public void CHECK_TILES_Update() {
		if(UpdateAnimations()){
			if(_gridInfo.tileList[_activeTileList[0]].tileContent != _gridInfo.tileList[_activeTileList[1]].tileContent){
				_currentMatches = _matchChecker.CheckForMatches(_activeTileList);                   
			}
			currentState = (_currentMatches.Count == 0) ? GridState.REVERSE_TILES : GridState.REMOVE_MATCHES;
		}
	}

	#endregion

	#region reverse_tiles state
	public void REVERSE_TILES_EnterState() {
		//reverse the animations
		foreach (TileAnimation animation in _tileAnimations)
		{
			animation.ReverseAnimation();
		}
		
		//swap them back in the array
		Tile tempTile = _gridInfo.tileList[_activeTileList[0]];
		_gridInfo.tileList[_activeTileList[0]] = _gridInfo.tileList[_activeTileList[1]];
		_gridInfo.tileList[_activeTileList[1]] = tempTile;
	}
	
	public void REVERSE_TILES_Update() {
		if(UpdateAnimations()){
			_tileAnimations.Clear();
			currentState = GridState.IDLE;
		}
	}	

	#endregion

	#region remove_matches state

	public void REMOVE_MATCHES_EnterState() {
		_prompter.RemovePrompt();
		_tileAnimations.Clear();
		//Remove matched tiles
		foreach (int index in _currentMatches) {
			_tileAnimations.Add(new ScaleAnimation(_gridInfo.tileList[index].gameObject, 0, TileCreator.TILE_MOVE_SPEED, false));
		}
	}
	
	public void REMOVE_MATCHES_Update() {
		if(UpdateAnimations()){
			_tileAnimations.Clear();
			foreach (int index in _currentMatches)
			{
				_gridInfo.tileList[index].Destroy();
				_gridInfo.tileList[index] = null;
			}
			currentState = GridState.REFILL_GRID; 
		}
	}	

	#endregion

	#region refill_grid state
	
	public void REFILL_GRID_EnterState() {		
		_activeTileList = _tileCreator.RefillGrid(_tileAnimations);
	}
	
	public void REFILL_GRID_Update() {
		if(UpdateAnimations()){
			_tileAnimations.Clear();
			currentState = GridState.RECURSIVE_CHECK;
		}
	}	
	
	#endregion

	#region recursive_check state
		
	public void RECURSIVE_CHECK_Update() {
		if(UpdateAnimations()){
			_currentMatches.Clear();
			_tileAnimations.Clear();  
			_currentMatches = _matchChecker.CheckForMatches(_activeTileList);
			if (_currentMatches.Count == 0)
			{
				Vector3 possibleMatches = _matchChecker.GetPossibleMatch();
				if(possibleMatches == Vector3.zero){
					currentState = GridState.FLUSHING;
				}
				else{
					_prompter.SetMatchPrompt(possibleMatches);
					currentState = GridState.IDLE;
				}
			}
			else{
				currentState = GridState.REMOVE_MATCHES;
			}
		}
	}	

	#endregion

	#region flushing state
	
	public void FLUSHING_EnterState() {
		_tileCreator.CreateFlushAnimations(_tileAnimations);
	}
	
	public void FLUSHING_Update() {
		if(UpdateAnimations()){
			int tileIndex;
			for (tileIndex = 0; tileIndex < _gridInfo.tileList.Length; tileIndex++ ) {
				_gridInfo.tileList[tileIndex].Destroy();
				_gridInfo.tileList[tileIndex] = null;
			}
			_tileAnimations.Clear();
			currentState = GridState.CREATING_GRID;
		}
	}	
	#endregion
}
