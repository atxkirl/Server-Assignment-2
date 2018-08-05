using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;

namespace TestPlugin
{
	class PacketDecoder : Singleton<PacketDecoder>
	{
		//Text-based Decoding
		public string GetStringFromString(string source, string nameOfData)
		{
			//Name=Test;Password=Test;PrefabPath=Resources/PrefabThing;LastPosition=0.1,0.2,0.3;LastRotation=1.1,2.2,3.3;SenderID=1;
			string[] thing = source.Split(';');

			foreach (string thingInThing in thing)
			{
				if (thingInThing.Contains(nameOfData + "="))
				{
					return thingInThing.Substring(thingInThing.IndexOf("=") + 1);
				}
			}

			return "";
		}

		public string GetVectorFromString(string source, string nameOfData)
		{
			//Name=Test;Password=Test;PrefabPath=Resources/PrefabThing;LastPosition=0.1,0.2,0.3;LastRotation=1.1,2.2,3.3;SenderID=1;
			string vectorSource = GetStringFromString(source, nameOfData);

			if (!vectorSource.Any(char.IsDigit))
				return null;

			string[] vectorThing = vectorSource.Split(',');
			Vector3 returnValue = new Vector3();

			if (vectorThing.Count() != 3)
				return null;

			returnValue.x = float.Parse(vectorThing[0]);
			returnValue.y = float.Parse(vectorThing[1]);
			returnValue.z = float.Parse(vectorThing[2]);

			return (returnValue.x + "," + returnValue.y + "," + returnValue.z);
		}

		public string GetQuaternionFromString(string source, string nameOfData)
		{
			//Name=Test;Password=Test;PrefabPath=Resources/PrefabThing;LastPosition=0.1,0.2,0.3;LastRotation=1.1,2.2,3.3,4.4;SenderID=1;
			string quarternionSource = GetStringFromString(source, nameOfData);

			if (!quarternionSource.Any(char.IsDigit))
				return null;

			string[] quarternionThing = quarternionSource.Split(',');
			Quaternion returnValue = new Quaternion();

			if (quarternionThing.Count() != 4)
				return null;

			returnValue.x = float.Parse(quarternionThing[0]);
			returnValue.y = float.Parse(quarternionThing[1]);
			returnValue.z = float.Parse(quarternionThing[2]);
			returnValue.w = float.Parse(quarternionThing[3]);

			return (returnValue.x + "," + returnValue.y + "," + returnValue.z + "," + returnValue.w);
		}

		//Byte-Array-based Decoding
		//Byte-Array-based Decoding
		/// <summary>
		/// Decoding for CustomObject - Decoding format: TargetReceiverName, ObjectName, ObjectHealth, ObjectPosX, ObjectPosY, ObjectPosZ, ObjectRotX, ObjectRotY, ObjectRotZ, ObjectRotW
		/// </summary>
		public object DecodeCustomObject(byte[] bytes)
		{
			CustomObject customObject = new CustomObject();

			using (var s = new MemoryStream(bytes))
			{
				using (var br = new BinaryReader(s))
				{
					customObject.SetPacketTarget(br.ReadString());
					customObject.SetObjectName(br.ReadString());
					customObject.SetHealth(br.ReadInt32());
					customObject.SetPos(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
					customObject.SetRot(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

					return customObject;
				}
			}
		}

		/// <summary>
		/// Decoding for Player - Decoding format: TargetReceiverName, ObjectName, ObjectHealth, ObjectPosX, ObjectPosY, ObjectPosZ, ObjectRotX, ObjectRotY, ObjectRotZ, ObjectRotW
		/// </summary>
		public object DecodePlayer(byte[] bytes)
		{
			Player customObject = new Player();

			using (var s = new MemoryStream(bytes))
			{
				using (var br = new BinaryReader(s))
				{
					//CustomObject Data
					customObject.SetPacketTarget(br.ReadString());
					customObject.SetObjectName(br.ReadString());
					customObject.SetHealth(br.ReadInt32());
					customObject.SetPos(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
					customObject.SetRot(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

					//Player Data

					return customObject;
				}
			}
		}

		/// <summary>
		/// Decoding for AI - Decoding format: TargetReceiverName, ObjectName, ObjectHealth, ObjectPosX, ObjectPosY, ObjectPosZ, ObjectRotX, ObjectRotY, ObjectRotZ, ObjectRotW, StateName, StateRange, StateTargetPosX, StateTargetPosY, StateTargetPosZ
		/// </summary>
		public object DecodeAI(byte[] bytes)
		{
			AI customObject = new AI();
			AIState currentState = new AIState();

			using (var s = new MemoryStream(bytes))
			{
				using (var br = new BinaryReader(s))
				{
					//CustomObject Data
					customObject.SetPacketTarget(br.ReadString());
					customObject.SetObjectName(br.ReadString());
					customObject.SetHealth(br.ReadInt32());
					customObject.SetPos(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
					customObject.SetRot(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

					//AIState Data
					currentState.SetName(br.ReadString());
					currentState.SetRange(br.ReadSingle());
					currentState.SetTarget(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()), new Quaternion());

					//AI Data
					customObject.GetStateMachine().SetCurrState(currentState.GetName());

					return customObject;
				}
			}
		}

		/// <summary>
		/// Decoding for AIState - Decoding format: StateName, StateRange, StateTargetPosX, StateTargetPosY, StateTargetPosZ
		/// </summary>
		public object DecodeAIState(byte[] bytes)
		{
			AIState customObject = new AIState();

			using (var s = new MemoryStream(bytes))
			{
				using (var br = new BinaryReader(s))
				{
					customObject.SetName(br.ReadString());
					customObject.SetRange(br.ReadSingle());
					customObject.SetTarget(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()), new Quaternion());

					return customObject;
				}
			}
		}
	}
}
