using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class TileAnimation : ITileAnimation{

	public const int TRANSFORM = 0;
	public const int SCALE = 1;
	private bool _completed = false;
	private int _iterations = 0;
	private int _time;
	private Vector3 _startPoint;
	private Vector3 _endPoint;
	private Vector3 _updateValues;
	private GameObject _tile;
	private int _type;

	public TileAnimation(GameObject tile, Vector3 endPoint,int time,int type){
		_endPoint = endPoint;
		_type = type;
		_tile = tile;
		_time = time;
		_startPoint = GetStartValue();
		_updateValues = CalculateDistancePerTick();
	}

	public void UpdateAnimation(){
		if(!_completed){
			_iterations++;
			switch(_type){
			case TRANSFORM:
				_tile.transform.Translate(_updateValues.x,_updateValues.y, 0);
				break;
			case SCALE:
				float uniformScale = _tile.transform.localScale.x+_updateValues.x;
				_tile.transform.localScale = new Vector3(uniformScale,uniformScale,1);
				break;
			}
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
		_startPoint = GetStartValue();
		_updateValues = CalculateDistancePerTick();
	}

	private Vector3 CalculateDistancePerTick(){
		Vector3 difference = _endPoint - _startPoint;
		difference.x = difference.x/_time;
		difference.y = difference.y/_time;
		return difference;
	}

	private Vector3 GetStartValue(){
		Vector3 value;
		switch(_type){
		case TRANSFORM:
			value = _tile.transform.position;
			break;			
		case SCALE:
			value = _tile.transform.localScale;
			break;
		default:
			value = _tile.transform.position;
			break;
		}
		return value;
	}

}
