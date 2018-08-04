using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin
{
	/// <summary>
	/// Player class
	/// </summary>
	public class Player : CustomObject
	{
		//CONSTRUCTORS
		public Player() : base()
		{

		}
		public Player(Vector3 _pos, Quaternion _rot) : base(_pos, _rot)
		{
		}
		public Player(string _targetReceiverName, string _name, int _health, Vector3 _pos, Quaternion _rot) : base(_targetReceiverName, _name, _health, _pos, _rot)
		{
		}
	}
}
