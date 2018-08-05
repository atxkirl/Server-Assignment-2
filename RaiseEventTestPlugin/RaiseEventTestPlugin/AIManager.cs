using Photon.Hive.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestPlugin
{
	class AIManager : Singleton<AIManager>
	{
		private int aiID = 0;
		private List<AI> listOfAI;
		private object managerLock = new object();

		private static void AddAI(AI _aiToAdd)
		{
			if (Instance.listOfAI != null)
			{
				//Add new AI to the manager list
				Instance.listOfAI.Add(_aiToAdd);
			}
		}

		public List<AI> GetAIList() { return listOfAI; }

		public static void SpawnAI(string name, int health, Vector3 position, List<AIState> statesToAdd)
		{
			AI enemy = new AI();
			enemy.SetObjectName(name + Instance.aiID);
			enemy.SetHealth(health);
			enemy.SetPos(position);
			enemy.AddState(statesToAdd);
			enemy.GetStateMachine().SetCurrState(statesToAdd[0].GetName());

			//Add AI to the list of managed AIs
			AddAI(enemy);
			//Increase unique ID count
			++Instance.aiID;
			//Sleep Thread to allow Random generator to get new seed
			Thread.Sleep(10);
		}

		public static void ManagerThread()
		{
			lock (Instance.managerLock)
			{
				//Make AIManager's thread wait for RaiseEventTestPlugin to finish setup
				do
				{
					Thread.Sleep(100);
				}
				while (RaiseEventTestPlugin.Instance == null);
				//Make AIManager's thread wait for NumberHelper to finish setup
				do
				{
					Thread.Sleep(100);
				}
				while (NumberHelper.Instance == null);

				//Initalize List
				Instance.listOfAI = new List<AI>();

				//Initialize states
				IdleState Idle = new IdleState("Idle");
				RoamState Roam = new RoamState("Roam");
				ChaseState Chase = new ChaseState("Chase");

				List<AIState> states = new List<AIState>();
				states.Add(Idle);
				states.Add(Roam);
				states.Add(Chase);

				//Adds the AI to the Manager, and tells the server to send SpawnAI packets to all Clients
				SpawnAI("beetle", 10, new Vector3(40, 1.1f, 44), states);
				SpawnAI("beetle", 10, new Vector3(44, 1.1f, 44), states);

				//Update loop
				while (true)
				{
					//Iterate through all the AI and update them
					foreach (AI enemy in Instance.listOfAI)
					{
						enemy.GetStateMachine().Update();
						enemy.SetPacketTarget("SENDTOALL");
						RaiseEventTestPlugin.Instance.PluginHost.BroadcastEvent(target: ReciverGroup.All, senderActor: 0, targetGroup: 0, evCode: (byte)EVENT_CODES.EV_UPDATEAIPOS, data: new Dictionary<byte, object>() { { (byte)245, enemy } }, cacheOp: 0);
						//Wait so that the Random generator will seed differently
						Thread.Sleep(10);
					}
					//Limit update loop to 10hz
					Thread.Sleep(100);
				}
			}
		}
	}
}
