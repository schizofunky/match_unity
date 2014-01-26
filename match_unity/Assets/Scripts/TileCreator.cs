using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Animations;

public class TileCreator
{
	private const float SPACING = 0.9f;
	private const float START_X = -3;
	private const float START_Y = 4.9f;
	public const int TILE_MOVE_SPEED = 10;

	private GridInfo _gridInfo; 

	public TileCreator (GridInfo gridInfo){
		_gridInfo = gridInfo;
	}

	public int getTileAtXY(float x, float y){
		int tileIndex = -1;
		float size = 0.4f;
		float xPos = Mathf.Ceil((size+(x - START_X))/SPACING)-1;
		float yPos = Mathf.Ceil((size+(START_Y - y))/SPACING)-1;
		if(xPos > -1 && xPos < 10){
			if(yPos > -1 && yPos < 10){
				tileIndex = (int)(xPos + (yPos*_gridInfo.rowCount));	
			}
		}
		return tileIndex;
	}

	public TileAnimation CreateTile(int index, int animationRowOffset,int[] availableTiles){
		TileAnimation animation = null;
		int tileIndex = (int)Mathf.Round(Random.value * (availableTiles.Length - 1));
		Tile tile = new Tile(index, _gridInfo.tileSprites[availableTiles[tileIndex]], availableTiles[tileIndex]);
		float destinationX = CalculateTileX(index);
		float destinationY = CalculateTileY(index);
		Vector3 animateFrom = new Vector3(destinationX,destinationY,0);
		tile.SetPosition(destinationX,destinationY+(animationRowOffset*SPACING));
		_gridInfo.tileList[index] = tile;
		if (animationRowOffset != 0){
			animation = new TileAnimation(_gridInfo.tileList[index].gameObject, animateFrom, TILE_MOVE_SPEED, TileAnimation.TRANSFORM);
		}
		return animation;
	}

	public List<int> RefillGrid(List<ITileAnimation> tileAnimations)
	{
		int tileListIndex;
		List<int> modifiedTiles = new List<int>();
		for(tileListIndex = _gridInfo.tileList.Length-1; tileListIndex >= 0; tileListIndex--){
			if(_gridInfo.tileList[tileListIndex] == null){
				int tile = FindTileAbove(tileListIndex);
				if(tile != -1){
					_gridInfo.tileList[tileListIndex] = _gridInfo.tileList[tile];
					_gridInfo.tileList[tile] = null;
					modifiedTiles.Add(tileListIndex);
					tileAnimations.Add(new TileAnimation(_gridInfo.tileList[tileListIndex].gameObject, new Vector3(CalculateTileX(tileListIndex),CalculateTileY(tileListIndex),0),TILE_MOVE_SPEED,TileAnimation.TRANSFORM));
				}
			}
		}
		GenerateNewTiles(modifiedTiles,tileAnimations);
		return modifiedTiles;
	}

	public void CreateFlushAnimations(List<ITileAnimation> animationList){
		Vector3 tilePosition;
		foreach (Tile tile in _gridInfo.tileList) {
			tilePosition = tile.GetPosition();
			animationList.Add(new TileAnimation(tile.gameObject, new Vector3(tilePosition.x, tilePosition.y - (_gridInfo.rowCount * SPACING), 0), TILE_MOVE_SPEED, TileAnimation.TRANSFORM));
		}
	}

	private float CalculateTileX(int index){
		return START_X + (SPACING * (index % _gridInfo.rowCount));
	}
	
	private float CalculateTileY(int index){		
		return START_Y - (SPACING * Mathf.Floor(index / _gridInfo.columnCount));
	}


	
	private int FindTileAbove(int startIndex){
		int line = -1;
		for(int index = startIndex - _gridInfo.columnCount; index >= 0; index-=_gridInfo.columnCount){	
			if(_gridInfo.tileList[index] != null){
				line = index;
				break;
			}
		}
		return line;
	}
	
	private void GenerateNewTiles(List<int> modifiedTiles,List<ITileAnimation> tileAnimations)
	{
		int newIndex;
		for(int c = 0; c < _gridInfo.columnCount; c++){
			for(int r = _gridInfo.rowCount-1; r >= 0; r--){
				int tileIndex = (r*_gridInfo.columnCount) + c;
				if(_gridInfo.tileList[tileIndex] == null){
					int numberOfTilestoCreate = (int)Mathf.Ceil(((float)tileIndex+1)/_gridInfo.columnCount);
					for(int t = 0; t < numberOfTilestoCreate; t++){
						newIndex = tileIndex -(t*_gridInfo.columnCount);
						TileAnimation animation = CreateTile(newIndex, numberOfTilestoCreate, GetUsableSprites(newIndex));
						if(animation != null){
							tileAnimations.Add(animation);
						}
						_gridInfo.tileList[newIndex].CreateSprite();
						modifiedTiles.Add(newIndex);
					}
					break;
				}
			}
		}
	}
	
	private int[] GetUsableSprites(int startIndex) {
		List<int> usableSprites = Enumerable.Range(0, _gridInfo.tileSprites.Count).ToList();
		int leftIndex = startIndex - 1;
		int rightIndex = startIndex + 1;
		int belowIndex = startIndex + _gridInfo.columnCount;
		int totalTiles = _gridInfo.totalTiles;
		if (leftIndex > -1) {
			usableSprites.Remove(_gridInfo.tileList[leftIndex].tileContent);
		}
		if (rightIndex < totalTiles && _gridInfo.tileList[rightIndex] != null) {
			usableSprites.Remove(_gridInfo.tileList[rightIndex].tileContent);
		}
		if (belowIndex < totalTiles) {
			usableSprites.Remove(_gridInfo.tileList[belowIndex].tileContent);
		}
		return usableSprites.ToArray();
	}
}


