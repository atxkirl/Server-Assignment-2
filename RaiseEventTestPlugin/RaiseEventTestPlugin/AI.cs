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
		StateMachine fsm;

		//CONSTRUCTORS
		public AI() : base()
		{
			fsm = new StateMachine();
		}
		public AI(string _targetReceiverName, string _name, int _health, Vector3 _pos) : base(_targetReceiverName, _name, _health, _pos)
		{
			fsm = new StateMachine();
		}

		//GETTERS
		public AIState GetCurrentState() { return fsm.GetCurrState(); }
		public StateMachine GetStateMachine() { return fsm; }

		//OTHER
		public void AddState(AIState _state)
		{
			_state.SetControllingAI(this);
			fsm.AddState(_state);
		}
	}
}
