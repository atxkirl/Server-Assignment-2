﻿using System;
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
		AIState currentState;
		private Object FSMLock = new Object();

		public StateMachine()
		{
			currentState = null;
			stateDict = new Dictionary<string, AIState>();
		}

		public AIState GetCurrState()
		{
			if(currentState != null)
				return currentState;
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

		public void SetCurrState(string stateName)
		{
			//Check if state exists within the StateMachine
			if(stateDict.ContainsKey(stateName))
			{
				currentState.Exit();
				currentState = stateDict[stateName];
				currentState.Enter();

				RaiseEventTestPlugin.Instance.PluginHost.BroadcastEvent(target: 0, senderActor: 0, targetGroup: 0, evCode: (byte)EVENT_CODES.EV_LOGIN_PASS, data: new Dictionary<byte, object>() { { (byte)245, "Current State is: " + currentState.GetName() } }, cacheOp: 0);
			}
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
