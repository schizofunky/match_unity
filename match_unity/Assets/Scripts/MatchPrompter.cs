using UnityEngine;
using System;
using Assets.Scripts.Animations;

	public class MatchPrompter
	{
		private const int PROMPT_SPEED = 30;
		private ScaleAnimation[] _matchPrompt = new ScaleAnimation[3];
		private Tile[] _tileList;

		public MatchPrompter (Tile[] tileList)
		{
			_tileList = tileList;
		}

		public void SetMatchPrompt(Vector3 matchIndices) {
		_matchPrompt[0] = new ScaleAnimation(_tileList[(int)matchIndices.x].gameObject, 2.5f, PROMPT_SPEED, true);
		_matchPrompt[1] = new ScaleAnimation(_tileList[(int)matchIndices.y].gameObject, 2.5f, PROMPT_SPEED, true);
		_matchPrompt[2] = new ScaleAnimation(_tileList[(int)matchIndices.z].gameObject, 2.5f, PROMPT_SPEED, true);
		}
		
		public void RemovePrompt() {
			if (_matchPrompt[0] != null) {
				_matchPrompt[0].Reset();
				_matchPrompt[1].Reset();
				_matchPrompt[2].Reset();
				_matchPrompt[0] = null;
				_matchPrompt[1] = null;
				_matchPrompt[2] = null;
			}
		}
		
		public void AnimatePrompts() {
			_matchPrompt[0].UpdateAnimation();
			_matchPrompt[1].UpdateAnimation();
			_matchPrompt[2].UpdateAnimation();
		}
	}


