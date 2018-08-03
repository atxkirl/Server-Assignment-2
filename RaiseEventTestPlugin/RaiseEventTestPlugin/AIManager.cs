using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestPlugin
{
	class AIManager : Singleton<AIManager>
	{
		private List<AI> listOfAI;
		private object managerLock = new object();

		public List<AI> GetAIList() { return listOfAI; }

		public void AddAI(AI _aiToAdd)
		{
			if(listOfAI != null)
			{
				listOfAI.Add(_aiToAdd);
			}
		}

		public static void ManagerThread()
		{
			lock(Instance.managerLock)
			{
				//Initalize List
				Instance.listOfAI = new List<AI>();
				//Initialize AIs
				IdleState Idle = new IdleState("Idle");
				RoamState Roam = new RoamState("Roam");
				ChaseState Chase = new ChaseState("Chase");

				AI beetle = new AI();
				beetle.SetObjectName("beetle");
				beetle.SetHealth(10);
				beetle.SetPos(new Vector3(1, 2, 3));
				beetle.AddState(Idle);
				beetle.AddState(Roam);
				beetle.AddState(Chase);

				Instance.AddAI(beetle);

				//Update loop
				while (true)
				{
					//Iterate through all the AI and update them
					foreach (AI enemy in Instance.listOfAI)
					{
						enemy.GetStateMachine().Update();
					}

					Thread.Sleep(100);
				}
			}
		}
	}
}
