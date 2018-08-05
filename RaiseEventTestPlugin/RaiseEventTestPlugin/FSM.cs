using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin
{
	public class StateMachine
	{
		Dictionary<string, AIState> stateDict;
		AIState currentState, previousState;
		public AI controllingAI;

		public StateMachine()
		{
			currentState = null;
			controllingAI = null;
			stateDict = new Dictionary<string, AIState>();
		}

		public AIState GetCurrState()
		{
			if(currentState != null)
				return currentState;
			return null;
		}

		public AIState GetPrevState()
		{
			if (previousState != null)
				return previousState;
			return null;
		}

		public void AddState(AIState newState)
		{
			//State is null
			if (newState == null)
				return;
			//State already exists within the StateMachine
			if (stateDict.ContainsKey(newState.GetName()))
				return;
			//Check if current state exists
			if (currentState == null)
				currentState = newState;
			//Register state in dictionary
			stateDict.Add(newState.GetName(), newState);
		}

		public bool SetCurrState(string stateName)
		{
			//Check if state exists within the StateMachine
			if(stateDict.ContainsKey(stateName))
			{
				previousState = currentState;

				currentState.Exit();
				currentState = stateDict[stateName];
				currentState.Enter();

				return true;
			}
			return false;
		}

		public void Update()
		{
			if (currentState != null)
			{
				currentState.Update();
			}
		}
	}
}
