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
	private const int TILE_MOVE_SPEED = 10;

	private List<Sprite> _tileSprites;
	private Tile[] _tileList;
	private int _rowCount;
	private int _columnCount;

	public TileCreator (List<Sprite> tileSprites,Tile[] tileList, int rowCount, int columnCount)
	{
		_tileSprites = tileSprites;
		_tileList = tileList; 
		_rowCount = rowCount;
		_columnCount = columnCount;
	}

	public TileAnimation CreateTile(int index, int animationRowOffset,int[] availableTiles){
		TileAnimation animation = null;
		int tileIndex = (int)Mathf.Round(Random.value * (availableTiles.Length - 1));
		Tile tile = new Tile(index, _tileSprites[availableTiles[tileIndex]], availableTiles[tileIndex]);
		float destinationX = CalculateTileX(index);
		float destinationY = CalculateTileY(index);
		Vector3 animateFrom = new Vector3(destinationX,destinationY,0);
		tile.SetPosition(destinationX,destinationY+(animationRowOffset*SPACING));
		_tileList[index] = tile;
		if (animationRowOffset != 0){
			animation = new TileAnimation(_tileList[index].gameObject, animateFrom, TILE_MOVE_SPEED, TileAnimation.TRANSFORM);
		}
		return animation;
	}

	public List<int> RefillGrid(List<ITileAnimation> tileAnimations)
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
					tileAnimations.Add(new TileAnimation(_tileList[tileListIndex].gameObject, new Vector3(CalculateTileX(tileListIndex),CalculateTileY(tileListIndex),0),TILE_MOVE_SPEED,TileAnimation.TRANSFORM));
				}
			}
		}
		GenerateNewTiles(modifiedTiles,tileAnimations);
		return modifiedTiles;
	}
	
	private float CalculateTileX(int index){
		return START_X + (SPACING * (index % _rowCount));
	}
	
	private float CalculateTileY(int index){		
		return START_Y - (SPACING * Mathf.Floor(index / _columnCount));
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
	
	private void GenerateNewTiles(List<int> modifiedTiles,List<ITileAnimation> tileAnimations)
	{
		int newIndex;
		for(int c = 0; c < _columnCount; c++){
			for(int r = _rowCount-1; r >= 0; r--){
				int tileIndex = (r*_columnCount) + c;
				if(_tileList[tileIndex] == null){
					int numberOfTilestoCreate = (int)Mathf.Ceil(((float)tileIndex+1)/_columnCount);
					for(int t = 0; t < numberOfTilestoCreate; t++){
						newIndex = tileIndex -(t*_columnCount);
						TileAnimation animation = CreateTile(newIndex, numberOfTilestoCreate, GetUsableSprites(newIndex));
						if(animation != null){
							tileAnimations.Add(animation);
						}
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
}


