using UnityEngine;
using System.Collections;
using System;

public class BaseStateMachine {

    public Action OnStateUpdate;
    public Action OnStateEnter;
    public Action OnStateExit;

    private Enum _currentState;

    public Enum currentState {
        get
        {
            return _currentState;
        }
        set
        {
            _currentState = value;
            ConfigureCurrentState();
        }
    }

    static void DoNothing() {

    }
	// Update is called once per frame
	public void UpdateStates () {
        if (OnStateUpdate != null) {
            OnStateUpdate();
        }
	}

    private void ConfigureCurrentState() {
        if (OnStateExit != null) {
            OnStateExit();
        }
        OnStateUpdate = ConfigureDelegate<Action>("Update", DoNothing);
        OnStateEnter = ConfigureDelegate<Action>("EnterState", DoNothing);
        OnStateExit = ConfigureDelegate<Action>("ExitState", DoNothing);

        OnStateEnter();
    }

    //Define a generic method that returns a delegate
    //Note the where clause - we need to ensure that the
    //type passed in is a class and not a value type or our
    //cast (As T) will not work
    T ConfigureDelegate<T>(string methodRoot, T Default) where T : class {
        //Find a method called CURRENTSTATE_METHODROOT
        //The method can be either public or private
        var mtd = GetType().GetMethod(_currentState.ToString() + "_" + methodRoot, System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod);
        //If we found a method
        if (mtd != null) {
            //Create a delegate of the type that this
            //generic instance needs and cast it                    
            return Delegate.CreateDelegate(typeof(T), this, mtd) as T;
        } else {
            //If we didn't find a method return the default
            return Default;
        }

    }


}
