using UnityEngine;
using System.Collections;

public class TileAnimation {

	private bool _completed = false;
	private int _iterations = 0;
	private int _time;
	private Vector3 _startPoint;
	private Vector3 _endPoint;
	private Vector3 _updateValues;
	private GameObject _tile;

	public TileAnimation(GameObject tile, Vector3 endPoint,int time){
		_endPoint = endPoint;
		_startPoint = tile.transform.position;
		_tile = tile;
		_time = time;
		_updateValues = CalculateDistancePerTick();
	}

	public void UpdateAnimation(){
		if(!_completed){
			_iterations++;
			_tile.transform.Translate(_updateValues.x,_updateValues.y, 0);
			if(_iterations >= _time){
				_completed = true;
			}
		}
	}

	public bool IsCompleted(){
		return _completed;
	}

	public void ReverseAnimation(){
		_completed = false;
		_iterations = 0;
		_endPoint = _startPoint;
		_startPoint = _tile.transform.position;
		_updateValues = CalculateDistancePerTick();
	}

	private Vector3 CalculateDistancePerTick(){
		Vector3 difference = _endPoint - _startPoint;
		difference.x = difference.x/_time;
		difference.y = difference.y/_time;
		return difference;
	}

}
