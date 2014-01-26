using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Animations;

public class Grid : BaseStateMachine{

	private const float SPACING = 0.9f;
	private const int TILE_MOVE_SPEED = 10;

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

	private TileCreator _tileCreator;
	private MatchChecker _matchChecker;

    public Grid(int rowCount, int columnCount, List<Sprite> tileSprites){
		_rowCount = rowCount; 
		_columnCount = columnCount; 
		_tileSprites = tileSprites;
		_tileAnimations = new List<ITileAnimation>();
        _currentMatches = new HashSet<int>();
		currentState = GridState.CREATING_GRID;
	}

	public void Update(){
		UpdateStates();
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

	public void HighlightTile(int tileIndex){
		_tileList[tileIndex].SetSelected(true);
	}

	public void SwapTiles(int tile1Index, int tile2Index){
		_tile1 = tile1Index;
		_tile2 = tile2Index;
		currentState = GridState.CHECK_TILES;
	}

	private void CreateTiles(){
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
		Vector3 possibleMatches = _matchChecker.GetPossibleMatch();
		if(possibleMatches == Vector3.zero){
			currentState = GridState.CREATING_GRID;
		}
		else{
			_prompter.SetMatchPrompt(possibleMatches);
			for (i = 0; i < totalTiles; i++) {
				_tileList[i].CreateSprite();
			}
		}
	}

	private bool UpdateAnimations(){
		bool allAnimationsCompleted = true;
		foreach(ITileAnimation animation in _tileAnimations){
			animation.UpdateAnimation();
			allAnimationsCompleted = allAnimationsCompleted && animation.IsCompleted();
		}
		return allAnimationsCompleted;
	}


	#region idle state

	public void IDLE_EnterState() {
	}
	
	public void IDLE_Update() {
		_prompter.AnimatePrompts();
	}	
	
	public void IDLE_ExitState() {
	}

	#endregion

	#region creating_grid state

	public void CREATING_GRID_EnterState() {
		CreateTiles();
	}
	
	public void CREATING_GRID_Update() {
		if(UpdateAnimations()){			
			_tileAnimations.Clear();
			currentState = GridState.IDLE;
		}
	}	
	
	public void CREATING_GRID_ExitState() {
	}

	#endregion

	#region check_tiles state

	public void CHECK_TILES_EnterState() {
		_tileList[_tile1].SetSelected(false);		
		_tileList[_tile2].SetSelected(false);
		GameObject Tile1 = _tileList[_tile1].gameObject;
		GameObject Tile2 = _tileList[_tile2].gameObject;
		//create animations for both tiles
		_tileAnimations.Add(new TileAnimation(Tile1, Tile2.transform.position,TILE_MOVE_SPEED,TileAnimation.TRANSFORM));
		_tileAnimations.Add(new TileAnimation(Tile2, Tile1.transform.position,TILE_MOVE_SPEED,TileAnimation.TRANSFORM));
		//swap the positions in the array
		Tile tempTile = _tileList[_tile1];
		_tileList[_tile1] = _tileList[_tile2];
		_tileList[_tile2] = tempTile;
	}
	
	public void CHECK_TILES_Update() {
		if(UpdateAnimations()){
			if(_tileList[_tile1].tileContent != _tileList[_tile2].tileContent){
				List<int> tilesToCheck = new List<int>();
				tilesToCheck.Add(_tile1);
				tilesToCheck.Add(_tile2);
				CheckForMatches(tilesToCheck);                    
			}
			if(_currentMatches.Count == 0){
				currentState = GridState.REVERSE_TILES;
			}
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
		Tile tempTile = _tileList[_tile1];
		_tileList[_tile1] = _tileList[_tile2];
		_tileList[_tile2] = tempTile;
	}
	
	public void REVERSE_TILES_Update() {
		if(UpdateAnimations()){
			currentState = GridState.IDLE;
		}
	}	
	
	public void REVERSE_TILES_ExitState() {
		_tileAnimations.Clear();
	}

	#endregion

	#region remove_matches state

	public void REMOVE_MATCHES_EnterState() {
		_prompter.RemovePrompt();
		_tileAnimations.Clear();
		//Remove matched tiles
		foreach (int index in _currentMatches) {
			_tileAnimations.Add(new ScaleAnimation(_tileList[index].gameObject, 0, TILE_MOVE_SPEED, false));
		}
	}
	
	public void REMOVE_MATCHES_Update() {
		if(UpdateAnimations()){
			_tileAnimations.Clear();
			foreach (int index in _currentMatches)
			{
				_tileList[index].Destroy();
				_tileList[index] = null;
			}
			currentState = GridState.REFILL_GRID; 
		}
	}	
	
	public void REMOVE_MATCHES_ExitState() {
	}

	#endregion

	#region refill_grid state
	
	public void REFILL_GRID_EnterState() {		
		_recursiveTileList = _tileCreator.RefillGrid(_tileAnimations);
	}
	
	public void REFILL_GRID_Update() {
		if(UpdateAnimations()){
			currentState = GridState.RECURSIVE_CHECK;
		}
	}	
	
	public void REFILL_GRID_ExitState() {
		_tileAnimations.Clear();
	}
	
	#endregion

	#region recursive_check state
	
	public void RECURSIVE_CHECK_EnterState() {
	}
	
	public void RECURSIVE_CHECK_Update() {
		if(UpdateAnimations()){
			_currentMatches.Clear();
			_tileAnimations.Clear();
			CheckForMatches(_recursiveTileList);   
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
		}
	}	
	
	public void RECURSIVE_CHECK_ExitState() {
	}
	
	#endregion

	#region flushing state
	
	public void FLUSHING_EnterState() {
		Vector3 tilePosition;
		foreach (Tile tile in _tileList) {
			tilePosition = tile.GetPosition();
			_tileAnimations.Add(new TileAnimation(tile.gameObject, new Vector3(tilePosition.x, tilePosition.y - (_rowCount * SPACING), 0), TILE_MOVE_SPEED, TileAnimation.TRANSFORM));
		}
	}
	
	public void FLUSHING_Update() {
		if(UpdateAnimations()){
			int tileIndex;
			for (tileIndex = 0; tileIndex < _tileList.Length; tileIndex++ ) {
				_tileList[tileIndex].Destroy();
				_tileList[tileIndex] = null;
			}
			_tileAnimations.Clear();
			currentState = GridState.CREATING_GRID;
		}
	}	
	
	public void FLUSHING_ExitState() {
	}
	
	#endregion

	private void CheckForMatches(List<int> tilesToCheck)
    {
		_currentMatches = _matchChecker.CheckForMatches(tilesToCheck);
        if (_currentMatches.Count > 0)
		{
            currentState = GridState.REMOVE_MATCHES;
        }
	}
}
