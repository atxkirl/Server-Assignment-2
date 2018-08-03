using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin
{
	/// <summary>
	/// Base class for CustomObjects
	/// </summary>
	public class CustomObject
	{
		protected string targetReceiverName;
		protected string objectName;
		protected int health;
		protected Vector3 pos;

		//CONSTRUCTORS
		public CustomObject(string _targetReceiverName, string _objectName, int _health, Vector3 _pos)
		{
			targetReceiverName = _targetReceiverName;
			objectName = _objectName;
			health = _health;
			pos = _pos;
		}
		public CustomObject(Vector3 _pos) { pos = _pos; }
		public CustomObject()
		{
			targetReceiverName = "DEFAULT";
			objectName = "DEFAULT";
			health = 1;
			pos = new Vector3();
		}

		//GETTERS
		public string GetTargetReceiverName() { return targetReceiverName; }
		public string GetObjectName() { return objectName; }
		public int GetHealth() { return health; }
		public Vector3 GetPos() { return pos; }
		public float GetPosX() { return pos.x; }
		public float GetPosY() { return pos.y; }
		public float GetPosZ() { return pos.z; }

		//SETTERS
		public void SetTargetReceiverName(string _targetReceiverName) { targetReceiverName = _targetReceiverName; }
		public void SetObjectName(string _objectName) { objectName = _objectName; }
		public void SetHealth(int _health) { health = _health; }
		public void SetPos(Vector3 _pos) { pos = _pos; }
		public void SetPos(float _x, float _y, float _z) { pos.x = _x; pos.y = _y; pos.z = _z; }
	}
}
