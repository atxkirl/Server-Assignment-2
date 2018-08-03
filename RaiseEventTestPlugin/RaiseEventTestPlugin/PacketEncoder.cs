using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin
{
	public class PacketEncoder : Singleton<PacketEncoder>
	{
		//Text-based Encoding

		public void AppendData(ref string source, string nameOfData, int stuffToAppend) { source += nameOfData + "=" + stuffToAppend + ";"; }
		public void AppendData(ref string source, string nameOfData, string stuffToAppend) { source += nameOfData + "=" + stuffToAppend + ";"; }
		public void AppendData(ref string source, string nameOfData, Vector3 stuffToAppend) { source += nameOfData + "=" + stuffToAppend.x + "," + stuffToAppend.y + "," + stuffToAppend.z + ";"; }
		public void AppendData(ref string source, string nameOfData, Quaternion stuffToAppend) { source += nameOfData + "=" + stuffToAppend.x + "," + stuffToAppend.y + "," + stuffToAppend.z + "," + stuffToAppend.w + ";"; }

		//Byte-array-based Encoding
		/// <summary>
		/// Encoding for CustomObject - Encoding format: TargetReceiverName, ObjectName, ObjectHealth, ObjectPosX, ObjectPosY, ObjectPosZ
		/// </summary>
		public byte[] EncodeCustomObject(object obj)
		{
			CustomObject customObject = obj as CustomObject;
			if (customObject == null)
			{
				Console.WriteLine("CustomObject null");
				return null;
			}

			byte[] returnObject;

			//Write to byte array
			using (var ms = new MemoryStream())
			{
				using (var bw = new BinaryWriter(ms))
				{
					bw.Write(customObject.GetTargetReceiverName());
					bw.Write(customObject.GetObjectName());
					bw.Write(customObject.GetHealth());
					bw.Write(customObject.GetPosX());
					bw.Write(customObject.GetPosY());
					bw.Write(customObject.GetPosZ());

					returnObject = ms.ToArray();
				}
			}

			return returnObject;
		}

		/// <summary>
		/// Encoding for Player - Encoding format: TargetReceiverName, ObjectName, ObjectHealth, ObjectPosX, ObjectPosY, ObjectPosZ
		/// </summary>
		public byte[] EncodePlayer(object obj)
		{
			Player customObject = obj as Player;
			if (customObject == null)
			{
				Console.WriteLine("Player null");
				return null;
			}

			byte[] returnObject;
			byte[] customObjectData;

			//Write to byte array
			using (var ms = new MemoryStream())
			{
				using (var bw = new BinaryWriter(ms))
				{
					//Write CustomObject base data to byte array
					customObjectData = EncodeCustomObject(obj);

					//Write player data

					returnObject = ConcatArrays(customObjectData, ms.ToArray());
				}
			}

			//Concat the two byte arrays
			return returnObject;
		}

		/// <summary>
		/// Encoding for AI - Encoding format: TargetReceiverName, ObjectName, ObjectHealth, ObjectPosX, ObjectPosY, ObjectPosZ, StateName, StateRange, StateTargetPosX, StateTargetPosY, StateTargetPosZ
		/// </summary>
		public byte[] EncodeAI(object obj)
		{
			AI customObject = obj as AI;
			if (customObject == null)
			{
				Console.WriteLine("AI null");
				return null;
			}

			byte[] returnObject;
			byte[] stateData;
			byte[] customObjectData;

			//Write to byte array
			using (var ms = new MemoryStream())
			{
				using (var bw = new BinaryWriter(ms))
				{
					//Write CustomObject base data to byte array
					customObjectData = EncodeCustomObject(obj);

					//Write currentState data
					stateData = EncodeAIState(customObject.GetCurrentState());

					returnObject = ConcatArrays(customObjectData, stateData);
				}
			}

			//Concat the two byte arrays
			return returnObject;
		}

		/// <summary>
		/// Encoding for AIState - Encoding format: StateName, StateRange, TargetPosX, TargetPosY, TargetPosZ
		/// </summary>
		public byte[] EncodeAIState(object obj)
		{
			AIState customObject = obj as AIState;
			if (customObject == null)
			{
				Console.WriteLine("AIState null");
				return null;
			}

			byte[] returnObject;

			//Write to byte array
			using (var ms = new MemoryStream())
			{
				using (var bw = new BinaryWriter(ms))
				{
					//Write data
					bw.Write(customObject.GetName());
					bw.Write(customObject.GetRange());
					bw.Write(customObject.GetTargetPosX());
					bw.Write(customObject.GetTargetPosY());
					bw.Write(customObject.GetTargetPosZ());

					returnObject = ms.ToArray();
				}
			}

			return returnObject;
		}

		/// <summary>
		/// Concatonates two byte arrays together, adding the second byte array to the end of the first array
		/// </summary>
		private byte[] ConcatArrays(byte[] first, byte[] second)
		{
			if (first == null)
			{
				Console.WriteLine("First Byte Array null");
				return null;
			}
			if (second == null)
			{
				Console.WriteLine("Second Byte Array null");
				return null;
			}

			int length = first.Length + second.Length;
			byte[] sum = new byte[length];
			first.CopyTo(sum, 0);
			second.CopyTo(sum, first.Length);

			return sum;
		}
	}
}
