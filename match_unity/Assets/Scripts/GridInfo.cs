using UnityEngine;
using System;
using System.Collections.Generic;

public class GridInfo
{
	public int rowCount {get; private set;}
	public int columnCount {get; private set;}
	public List<Sprite> tileSprites {get; private set;}
	public Tile[] tileList {get; set;}
	public int totalTiles {
		get{
			return rowCount * columnCount;
		}
	}

	public GridInfo (int rowCount,int columnCount,List<Sprite> tileSprites)
	{
		this.rowCount = rowCount;
		this.columnCount = columnCount;
		this.tileSprites = tileSprites;
		tileList = new Tile[totalTiles];
	}
}

