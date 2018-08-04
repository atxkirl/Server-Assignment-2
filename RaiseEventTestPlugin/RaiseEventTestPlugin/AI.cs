using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin
{
	/// <summary>
	/// Artificial Intelligence class
	/// </summary>
	public class AI : CustomObject
	{
		StateMachine fsm = null;

		//CONSTRUCTORS
		public AI() : base()
		{
			fsm = new StateMachine();
			fsm.controllingAI = this;
		}
		public AI(string _targetReceiverName, string _name, int _health, Vector3 _pos, Quaternion _rot) : base(_targetReceiverName, _name, _health, _pos, _rot)
		{
			fsm = new StateMachine();
			fsm.controllingAI = this;
		}
		public AI(string _targetReceiverName, string _name, int _health, Vector3 _pos, Quaternion _rot, List<AIState> _states) : base(_targetReceiverName, _name, _health, _pos, _rot)
		{
			fsm = new StateMachine();
			fsm.controllingAI = this;

			//Add all the states to the statemachine
			foreach (AIState state in _states)
			{
				fsm.AddState(state);
			}
		}

		//GETTERS
		public AIState GetCurrentState() { return fsm.GetCurrState(); }
		public StateMachine GetStateMachine() { return fsm; }

		//OTHER
		public void AddState(AIState _state)
		{
			AIState copiedState = _state.ShallowCopy();

			if(fsm == null)
			{
				fsm = new StateMachine();
				fsm.controllingAI = this;
			}
			if(fsm != null)
			{
				copiedState.SetControllingAI(this);
				fsm.AddState(copiedState);
			}
		}
		public void AddState(List<AIState> _states)
		{
			if (fsm == null)
			{
				fsm = new StateMachine();
			}
			if (fsm != null)
			{
				foreach(AIState _state in _states)
				{
					AIState copiedState = _state.ShallowCopy();

					copiedState.SetControllingAI(this);
					fsm.AddState(copiedState);
				}
			}
		}
	}
}
