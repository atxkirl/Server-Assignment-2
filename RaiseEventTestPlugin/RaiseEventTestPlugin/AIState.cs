using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin
{
	/// <summary>
	/// Base class for AI states
	/// </summary>
	public class AIState
	{
		protected string name;
		protected float range;
		protected AI controllingAI = null;
		protected Player target = null;
		protected Random random = null;
		protected Stopwatch timer = null;

		//CONSTRUCTORS
		public AIState()
		{
			name = "DEFAULT";
			range = 1;
			target = null;
			controllingAI = null;
			timer = new Stopwatch();
			random = new Random();
			timer.Start();
		}
		public AIState(string _name)
		{
			name = _name;
			range = 1;
			target = null;
			controllingAI = null;
			timer = new Stopwatch();
			timer.Start();
			random = new Random();
		}
		public AIState(string _name, AI _controllingAI)
		{
			name = _name;
			controllingAI = _controllingAI;
			timer = new Stopwatch();
			timer.Start();
			random = new Random();
		}

		//GETTERS
		public string GetName() { return name; }
		public float GetRange() { return range; }
		public AI GetControllingAI() { return controllingAI; }
		public Player GetTarget() { return target; }
		public float GetTargetPosX() { if (target != null) return target.GetPosX(); else return -100.0f; }
		public float GetTargetPosY() { if (target != null) return target.GetPosY(); else return -100.0f; }
		public float GetTargetPosZ() { if (target != null) return target.GetPosZ(); else return -100.0f; }

		//SETTERS
		public void SetName(string _name) { name = _name; }
		public void SetRange(float _range) { range = _range; }
		public void SetTarget(Player _target) { target = _target; }
		public void SetTarget(Vector3 _target) { target = new Player(_target); }
		public void SetControllingAI(AI _controllingAI) { controllingAI = _controllingAI; }

		//FUNCTIONS
		public virtual void Update()
		{

		}
	}

	public class IdleState : AIState
	{
		float idleTime;

		//CONSTRUCTORS
		public IdleState(string stateName) : base()
		{
			base.SetName(stateName);
		}
		public IdleState(string stateName, AI _controllingAI) : base()
		{
			base.name = stateName;
			base.controllingAI = _controllingAI;
		}

		//FUNCTIONS
		public override void Update()
		{
			float shortestDist = float.MaxValue;
			Player closestPlayer = null;
			
			if(RaiseEventTestPlugin.Instance.GetConnectedPlayers().Count > 0)
			{
				foreach (Player player in RaiseEventTestPlugin.Instance.GetConnectedPlayers())
				{
					float dist = (controllingAI.GetPos() - player.GetPos()).Length();
					if (dist < shortestDist)
					{
						shortestDist = dist;
						closestPlayer = player;
					}
				}
			}

			RaiseEventTestPlugin.Instance.PluginHost.BroadcastEvent(target: 0, senderActor: 0, targetGroup: 0, evCode: (byte)EVENT_CODES.EV_LOGIN_PASS, data: new Dictionary<byte, object>() { { (byte)245, "State Update. Pt 1" } }, cacheOp: 0);

			//Check if there is a player within chase range
			if (shortestDist < range && closestPlayer != null)
			{
				target = closestPlayer;
				controllingAI.GetStateMachine().SetCurrState("Chase");

				RaiseEventTestPlugin.Instance.PluginHost.BroadcastEvent(target: 0, senderActor: 0, targetGroup: 0, evCode: (byte)EVENT_CODES.EV_LOGIN_PASS, data: new Dictionary<byte, object>() { { (byte)245, "Changing State to Chase." } }, cacheOp: 0);
			}
			else
			{
				//Idle the AI for a random amount of time
				//between 0f - 2f
				idleTime = (float)(random.NextDouble() * 2.0);

				if (timer.ElapsedMilliseconds > idleTime)
				{
					//Transition to Roam state
					controllingAI.GetStateMachine().SetCurrState("Roam");

					RaiseEventTestPlugin.Instance.PluginHost.BroadcastEvent(target: 0, senderActor: 0, targetGroup: 0, evCode: (byte)EVENT_CODES.EV_LOGIN_PASS, data: new Dictionary<byte, object>() { { (byte)245, "Changing State to Roam." } }, cacheOp: 0);
				}

				RaiseEventTestPlugin.Instance.PluginHost.BroadcastEvent(target: 0, senderActor: 0, targetGroup: 0, evCode: (byte)EVENT_CODES.EV_LOGIN_PASS, data: new Dictionary<byte, object>() { { (byte)245, "State Else." } }, cacheOp: 0);
			}

			RaiseEventTestPlugin.Instance.PluginHost.BroadcastEvent(target: 0, senderActor: 0, targetGroup: 0, evCode: (byte)EVENT_CODES.EV_LOGIN_PASS, data: new Dictionary<byte, object>() { { (byte)245, "State Update. Pt 2" } }, cacheOp: 0);
		}
	}

	public class RoamState : AIState
	{
		//CONSTRUCTORS
		public RoamState(string stateName)
		{
			base.SetName(stateName);
		}
		public RoamState(string stateName, AI _controllingAI)
		{
			base.name = stateName;
			base.controllingAI = _controllingAI;
		}

		//FUNCTIONS
		public override void Update()
		{
			float shortestDist = float.MaxValue;
			Player closestPlayer = null;
			foreach (Player player in RaiseEventTestPlugin.Instance.GetConnectedPlayers())
			{
				float dist = (controllingAI.GetPos() - player.GetPos()).Length();
				if (dist < shortestDist)
				{
					shortestDist = dist;
					closestPlayer = player;
				}
			}

			//Check if there is a player within chase range
			if (shortestDist < range)
			{
				if (closestPlayer != null)
				{
					target = closestPlayer;
					controllingAI.GetStateMachine().SetCurrState("Chase");
				}
			}
			else
			{
				//Move the AI for a random amount of time

				//Transition to Idle state
				controllingAI.GetStateMachine().SetCurrState("Idle");
			}
		}
	}

	public class ChaseState : AIState
	{
		float attackingRange;

		//CONSTRUCTORS
		public ChaseState(string stateName)
		{
			base.SetName(stateName);
		}
		public ChaseState(string stateName, AI _controllingAI)
		{
			base.name = stateName;
			base.controllingAI = _controllingAI;
		}

		//GETTERS
		public float GetAttackRange() { return attackingRange; }

		//SETTERS
		public void SetAttackRange(float _attackingRange) { attackingRange = _attackingRange; }

		//FUNCTIONS
		public override void Update()
		{
			if (target == null)
				controllingAI.GetStateMachine().SetCurrState("Idle");
			else if ((controllingAI.GetPos() - target.GetPos()).Length() < range)
				controllingAI.GetStateMachine().SetCurrState("Idle");
			else if ((controllingAI.GetPos() - target.GetPos()).Length() < attackingRange)
				controllingAI.GetStateMachine().SetCurrState("Attack");
		}
	}
}
