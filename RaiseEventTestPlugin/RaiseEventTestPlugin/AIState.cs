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
		protected float waitTime;
		protected AI controllingAI = null;
		protected Player target = null;
		protected Stopwatch timer = null;
		protected Vector3 returnPosition = null;

		//CONSTRUCTORS
		public AIState()
		{
			name = "DEFAULT";
			range = 1.0f;
			waitTime = 0.0f;
			timer = new Stopwatch();
			timer.Start();
			returnPosition = new Vector3();
		}
		public AIState(string _name)
		{
			name = _name;
			range = 1.0f;
			waitTime = 0.0f;
			timer = new Stopwatch();
			timer.Start();
			returnPosition = new Vector3();
		}
		public AIState(string _name, AI _controllingAI)
		{
			name = _name;
			range = 1.0f;
			waitTime = 0.0f;
			timer = new Stopwatch();
			timer.Start();
			controllingAI = _controllingAI;
			returnPosition = new Vector3();
		}

		//GETTERS
		public string GetName() { return name; }
		public float GetRange() { return range; }
		public AI GetControllingAI() { return controllingAI; }
		public Player GetTarget() { return target; }
		public float GetTargetPosX() { if (target != null) return target.GetPosX(); else return -100.0f; }
		public float GetTargetPosY() { if (target != null) return target.GetPosY(); else return -100.0f; }
		public float GetTargetPosZ() { if (target != null) return target.GetPosZ(); else return -100.0f; }
		public float GetReturnPosX() { if (returnPosition != null) return returnPosition.x; else return -100.0f; }
		public float GetReturnPosY() { if (returnPosition != null) return returnPosition.y; else return -100.0f; }
		public float GetReturnPosZ() { if (returnPosition != null) return returnPosition.z; else return -100.0f; }

		//SETTERS
		public void SetName(string _name) { name = _name; }
		public void SetRange(float _range) { range = _range; }
		public void SetTarget(Player _target) { target = _target; }
		public void SetTarget(Vector3 _targetPos, Quaternion _targetRot) { target = new Player(_targetPos, _targetRot); }
		public void SetControllingAI(AI _controllingAI) { controllingAI = _controllingAI; }
		public void SetReturnPosition(Vector3 _returnPosition) { returnPosition = _returnPosition; }

		//FUNCTIONS
		public virtual void Update()
		{

		}
		public virtual void Enter()
		{
			//Set a waitTime for the state to wait before changing state
			//NOTE: Only do this if the state is supposed to change state overtime
			waitTime = (float)NumberHelper.Instance.RandomSecondsBetweenRange(0, 1);
			//Restart elapsed time
			timer.Restart();
			//Reset target
			target = null;
		}
		public virtual void Exit()
		{
		}
		public AIState ShallowCopy()
		{
			return (AIState)this.MemberwiseClone();
		}
		public AIState DeepCopy()
		{
			AIState copy = (AIState)this.MemberwiseClone();
			copy.timer = new Stopwatch();
			copy.controllingAI = new AI();
			copy.target = new Player();

			return copy;
		}
	}

	public class IdleState : AIState
	{
		//CONSTRUCTORS
		public IdleState(string stateName) : base()
		{
			base.SetName(stateName);
			base.SetRange(5);
		}
		public IdleState(string stateName, AI _controllingAI) : base()
		{
			base.name = stateName;
			base.controllingAI = _controllingAI;
			base.SetRange(5);
		}

		//FUNCTIONS
		public override void Enter()
		{
			base.Enter();

			//Set the Idle waitTime to be between 2 and 5 seconds.
			waitTime = (float)NumberHelper.Instance.RandomSecondsBetweenRange(1, 3);
		}

		public override void Update()
		{
			float shortestDist = float.MaxValue;
			Player closestPlayer = null;

			foreach (Player player in RaiseEventTestPlugin.Instance.GetConnectedPlayers())
			{
				float dist = (player.GetPos() - controllingAI.GetPos()).Length();
				if (dist < shortestDist)
				{
					shortestDist = dist;
					closestPlayer = player;
				}
			}

			//Check if there is a player within chase range
			if (shortestDist < range && closestPlayer != null)
			{
				target = closestPlayer;
				controllingAI.GetStateMachine().SetCurrState("Chase");
			}
			else
			{
				if (timer.ElapsedMilliseconds > waitTime)
				{
					//Transition to Roam state
					controllingAI.GetStateMachine().SetCurrState("Roam");
				}
			}
		}
	}

	public class RoamState : AIState
	{
		Vector3 roamTarget = null;

		//CONSTRUCTORS
		public RoamState(string stateName)
		{
			base.SetName(stateName);
			base.SetRange(10);
		}
		public RoamState(string stateName, AI _controllingAI)
		{
			base.name = stateName;
			base.controllingAI = _controllingAI;
			base.SetRange(10);
		}

		//FUNCTIONS
		public override void Enter()
		{
			base.Enter();

			//Set the Roam waitTime to be between 5 and 10 seconds.
			waitTime = (float)NumberHelper.Instance.RandomSecondsBetweenRange(5, 10);
			//Set a position for the AI to roam towards
			roamTarget = new Vector3();
			roamTarget.y = controllingAI.GetPosY();
			roamTarget.x = controllingAI.GetPosX() + (float)NumberHelper.Instance.RandomNumberBetweenRange(0, 1);
			if (NumberHelper.Instance.RandomBool())
				roamTarget.x = -roamTarget.x;
			roamTarget.z = controllingAI.GetPosZ() + (float)NumberHelper.Instance.RandomNumberBetweenRange(0, 1);
			if (NumberHelper.Instance.RandomBool())
				roamTarget.z = -roamTarget.z;
		}

		public override void Update()
		{
			float shortestDist = float.MaxValue;
			Player closestPlayer = null;

			foreach (Player player in RaiseEventTestPlugin.Instance.GetConnectedPlayers())
			{
				float dist = (player.GetPos() - controllingAI.GetPos()).Length();
				if (dist < shortestDist)
				{
					shortestDist = dist;
					closestPlayer = player;
				}
			}

			//Check if there is a player within chase range
			if (shortestDist < range && closestPlayer != null)
			{
				target = closestPlayer;
				controllingAI.GetStateMachine().SetCurrState("Chase");
			}
			else
			{
				if (timer.ElapsedMilliseconds > waitTime)
				{
					//Transition to Roam state
					controllingAI.GetStateMachine().SetCurrState("Idle");
				}
				else
				{
					if(controllingAI.GetPos() != roamTarget)
					{
						Vector3 move = new Vector3();
						move.x = roamTarget.x - controllingAI.GetPosX();
						move.z = roamTarget.z - controllingAI.GetPosZ();
						move.y = controllingAI.GetPosY();
						move.Normalize();

						controllingAI.SetPos(controllingAI.GetPosX() + (move.x * 0.05f), controllingAI.GetPosY(), controllingAI.GetPosZ() + (move.z * 0.05f));
					}
				}
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
			base.SetRange(10);
			attackingRange = 2f;
		}
		public ChaseState(string stateName, AI _controllingAI)
		{
			base.name = stateName;
			base.controllingAI = _controllingAI;
			base.SetRange(10);
			attackingRange = 2f;
		}

		//GETTERS
		public float GetAttackRange() { return attackingRange; }

		//SETTERS
		public void SetAttackRange(float _attackingRange) { attackingRange = _attackingRange; }

		//FUNCTIONS
		public override void Enter()
		{
			base.Enter();

			//Set the target
			target = controllingAI.GetStateMachine().GetPrevState().GetTarget();

			if (target == null)
				controllingAI.GetStateMachine().SetCurrState("Idle");

			//Set the enter position
			returnPosition = controllingAI.GetPos();
		}

		public override void Update()
		{
			foreach (Player player in RaiseEventTestPlugin.Instance.GetConnectedPlayers())
			{
				if (player.GetObjectName() == target.GetObjectName())
				{
					target = player;

					Vector3 move = new Vector3();
					move.x = target.GetPosX() - controllingAI.GetPosX();
					move.z = target.GetPosZ() - controllingAI.GetPosZ();
					move.y = controllingAI.GetPosY();
					move.Normalize();

					controllingAI.SetPos(controllingAI.GetPosX() + (move.x * 0.15f), controllingAI.GetPosY(), controllingAI.GetPosZ() + (move.z * 0.15f));
					
					float dist = (target.GetPos() - controllingAI.GetPos()).Length();

					if (dist > range)
					{
						controllingAI.GetStateMachine().SetCurrState("Idle");
					}
					else if (dist < attackingRange)
					{
						controllingAI.GetStateMachine().SetCurrState("Attack");
					}
				}
			}
		}
	}

	public class AttackState : AIState
	{
		int attackDamage;

		//CONSTRUCTORS
		public AttackState(string stateName)
		{
			base.SetName(stateName);
			attackDamage = 10;
		}
		public AttackState(string stateName, AI _controllingAI)
		{
			base.name = stateName;
			base.controllingAI = _controllingAI;
			attackDamage = 10;
		}

		//GETTERS
		public int GetAttackDamage() { return attackDamage; }

		//SETTERS
		public void SetAttackDamage(int _attackDamage) { attackDamage = _attackDamage; }

		//FUNCTIONS
		public override void Enter()
		{
			base.Enter();

			//Set the target
			target = controllingAI.GetStateMachine().GetPrevState().GetTarget();
		}

		public override void Update()
		{
			if(target != null && target.GetHealth() > 0)
			{
				//Deal damage
				target.SetHealth(target.GetHealth() - attackDamage);
				if (target.GetHealth() < 0)
					target.SetHealth(0);

				target.SetPacketTarget(target.GetObjectName());
				RaiseEventTestPlugin.Instance.PluginHost.BroadcastEvent(target: 0, senderActor: 0, targetGroup: 0, evCode: (byte)EVENT_CODES.EV_UPDATEPLAYERHEALTH, data: new Dictionary<byte, object>() { { (byte)245, target } }, cacheOp: 0);
			}
			controllingAI.GetStateMachine().SetCurrState("Chase");
		}
	}
}
