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
		protected Quaternion rot;

		//CONSTRUCTORS
		public CustomObject(string _targetReceiverName, string _objectName, int _health, Vector3 _pos, Quaternion _rot)
		{
			targetReceiverName = _targetReceiverName;
			objectName = _objectName;
			health = _health;
			pos = _pos;
			rot = _rot;
		}
		public CustomObject(Vector3 _pos, Quaternion _rot)
		{
			pos = _pos;
			rot = _rot;
		}
		public CustomObject()
		{
			targetReceiverName = "DEFAULT";
			objectName = "DEFAULT";
			health = 1;
			pos = new Vector3();
			rot = new Quaternion();
		}

		//GETTERS
		public string GetTargetReceiverName() { return targetReceiverName; }
		public string GetObjectName() { return objectName; }
		public int GetHealth() { return health; }
		public Vector3 GetPos() { return pos; }
		public float GetPosX() { return pos.x; }
		public float GetPosY() { return pos.y; }
		public float GetPosZ() { return pos.z; }
		public Quaternion GetRot() { return rot; }
		public float GetRotX() { return rot.x; }
		public float GetRotY() { return rot.y; }
		public float GetRotZ() { return rot.z; }
		public float GetRotW() { return rot.w; }

		//SETTERS
		public void SetTargetReceiverName(string _targetReceiverName) { targetReceiverName = _targetReceiverName; }
		public void SetObjectName(string _objectName) { objectName = _objectName; }
		public void SetHealth(int _health) { health = _health; }
		public void SetPos(Vector3 _pos) { pos = _pos; }
		public void SetPos(float _x, float _y, float _z) { pos.x = _x; pos.y = _y; pos.z = _z; }
		public void SetRot(Quaternion _rot) { rot = _rot; }
		public void SetRot(float _x, float _y, float _z, float _w) { rot.x = _x; rot.y = _y; rot.z = _z; rot.w = _w; }
	}
}
