using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Photon.Hive;
using Photon.Hive.Plugin;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace TestPlugin
{
	public enum EVENT_CODES
	{
		//Testing Events
		TEST_PING = 1,
		TEST_OBJECTPACKET,
		TEST_STRINGPACKET,

		//AI Events
		EV_INITAI,
		EV_SPAWNAI,
		EV_UPDATEAIPOS,

		//Player Events
		EV_UPDATEPLAYERPOS,

		//User Events
		EV_USERJOINING,
		EV_USERLEAVING,
		EV_LISTALLUSERS,

		//SQLDatabase Events
		EV_SEARCHDB,
		EV_INSERTDB,
		EV_UPDATEDB,

		//Login Events
		EV_LOGIN_PASS,
		EV_LOGIN_FAIL,

		//Friend Events
		EV_ADD_FRIEND,
		EV_REMOVE_FRIEND,
		EV_ACCEPT_FRIEND,
		EV_DECLINE_FRIEND,
		EV_LIST_FRIEND,
		EV_LIST_FRIEND_DATA,

		//CustomObject Events
		CO_CUSTOMOBJECT,
		CO_PLAYER,
		CO_AI,
		CO_AISTATE
	}

	public class RaiseEventTestPlugin : PluginBase
	{
		private MySqlConnection conn;
		private string SenderName;
		private string ReturnMessage;
		private List<Player> ConnectedPlayers;

		public List<Player> GetConnectedPlayers() { return ConnectedPlayers; }

		public static RaiseEventTestPlugin Instance = null;

		public string ServerString
		{
			get;
			private set;
		}

		public int CallsCount
		{
			get;
			private set;
		}

		public RaiseEventTestPlugin()
		{
			this.UseStrictMode = true;
			this.ServerString = "ServerMessage";
			this.CallsCount = 0;
			Instance = this;

			// --- Connect to MySQL.
			ConnectToMySQL();
		}

		public override bool SetupInstance(IPluginHost host, Dictionary<string, string> config, out string errorMsg)
		{
			//Create static instance
			if(Instance != null)
				Instance = this;

			//Register CustomObject
			host.TryRegisterType(typeof(CustomObject), (byte)EVENT_CODES.CO_CUSTOMOBJECT,
			PacketEncoder.Instance.EncodeCustomObject,
			PacketDecoder.Instance.DecodeCustomObject);

			//Register Player
			host.TryRegisterType(typeof(Player), (byte)EVENT_CODES.CO_PLAYER,
			PacketEncoder.Instance.EncodePlayer,
			PacketDecoder.Instance.DecodePlayer);

			//Register AI
			host.TryRegisterType(typeof(AI), (byte)EVENT_CODES.CO_AI,
			PacketEncoder.Instance.EncodeAI,
			PacketDecoder.Instance.DecodeAI);

			//Register AIState
			host.TryRegisterType(typeof(AIState), (byte)EVENT_CODES.CO_AISTATE,
			PacketEncoder.Instance.EncodeAIState,
			PacketDecoder.Instance.DecodeAIState);

			return base.SetupInstance(host, config, out errorMsg);
		}

		public void Init()
		{
			ConnectedPlayers = new List<Player>();

			ThreadStart threadRef = new ThreadStart(AIManager.ManagerThread);
			Thread managerThread = new Thread(threadRef);
			managerThread.Start();
		}

		public override string Name
		{
			get
			{
				return this.GetType().Name;
			}
		}

		public override void OnRaiseEvent(IRaiseEventCallInfo info)
		{
			try
			{
				base.OnRaiseEvent(info);
			}
			catch (Exception ex)
			{
				this.PluginHost.BroadcastErrorInfoEvent(ex.ToString(), info);
				return;
			}

			string stringData = "";
			try
			{
				stringData = Encoding.Default.GetString((byte[])info.Request.Data);
				SenderName = PacketDecoder.Instance.GetStringFromString(stringData, "Name");
			}
			catch(Exception ex)
			{
				//this.PluginHost.BroadcastErrorInfoEvent(ex.ToString(), info);
				Console.WriteLine(ex.ToString());
			}

			CustomObject packetData = null;
			try
			{
				packetData = (CustomObject)PacketDecoder.Instance.DecodeCustomObject((byte[])info.Request.Data);
			}
			catch (Exception ex)
			{
				//this.PluginHost.BroadcastErrorInfoEvent(ex.ToString(), info);
				Console.WriteLine(ex.ToString());
			}

			///ASSIGNMENT 2 STUFF
			///ASSIGNMENT 2 STUFF
			///ASSIGNMENT 2 STUFF
			//TEST PACKET
			if (info.Request.EvCode == (byte)EVENT_CODES.TEST_PING)
			{
				SendListOfPlayers(info);
			}

			//Client asking for all AIs
			if (info.Request.EvCode == (byte)EVENT_CODES.EV_INITAI)
			{
				if(packetData != null)
				{
					//Make sure AIManager thread is fully initialized and running
					do
					{
						Thread.Sleep(100);
					}
					while (AIManager.Instance == null);

					//Iterate and send AI to the requesting client
					foreach (AI enemy in AIManager.Instance.GetAIList())
					{
						//Set packetTarget to be the requesting client name so ONLY the requesting client will open
						//This is to prevent duplicate AI from spawning on other clients when a new client joins the server
						enemy.SetPacketTarget(packetData.GetPacketTarget());
						this.PluginHost.BroadcastEvent(target: ReciverGroup.All, senderActor: 0, targetGroup: 0, evCode: (byte)EVENT_CODES.EV_SPAWNAI, data: new Dictionary<byte, object>() { { (byte)245, enemy } }, cacheOp: 0);
					}
				}
			}

			//Client updating position within server
			else if (info.Request.EvCode == (byte)EVENT_CODES.EV_UPDATEPLAYERPOS)
			{
				foreach (Player connectedPlayer in ConnectedPlayers)
				{
					if (connectedPlayer.GetObjectName() == packetData.GetObjectName())
					{
						connectedPlayer.SetPos(packetData.GetPos());
						return;
					}
				}
			}

			//Client asking for List of all connected clients
			else if (info.Request.EvCode == (byte)EVENT_CODES.EV_LISTALLUSERS)
				SendListOfPlayers(info);

			//User joining server
			else if (info.Request.EvCode == (byte)EVENT_CODES.EV_USERJOINING)
				UserJoined(info);
			//User leaving server
			else if (info.Request.EvCode == (byte)EVENT_CODES.EV_USERLEAVING)
				UserLeft(info);

			///ASSIGNMENT 1 STUFF
			///ASSIGNMENT 1 STUFF
			///ASSIGNMENT 1 STUFF

			//Search Users Data from Database from Server
			else if (info.Request.EvCode == (byte)EVENT_CODES.EV_SEARCHDB)
				SearchUsersDatabase(info);
			//Insert Users Data into Database from Server
			else if (info.Request.EvCode == (byte)EVENT_CODES.EV_INSERTDB)
				InsertUsersDatabase(info);
			//Update Users Data in Database from Server
			else if (info.Request.EvCode == (byte)EVENT_CODES.EV_UPDATEDB)
				UpdateUsersDatabase(info);

			//Add Friend in Database from Server
			else if (info.Request.EvCode == (byte)EVENT_CODES.EV_ADD_FRIEND)
				InsertFriendsDatabase(info);
			//Remove Friend in Database from Server
			else if (info.Request.EvCode == (byte)EVENT_CODES.EV_REMOVE_FRIEND)
				DeleteFriendsDatabase(info);
			//Accept Friend in Database from Server
			else if (info.Request.EvCode == (byte)EVENT_CODES.EV_ACCEPT_FRIEND)
				UpdateFriendsDatabase(info);
			//Decline Friend in Database from Server
			else if (info.Request.EvCode == (byte)EVENT_CODES.EV_DECLINE_FRIEND)
				UpdateFriendsDatabase(info);
			//List Friend in Database from Server
			else if (info.Request.EvCode == (byte)EVENT_CODES.EV_LIST_FRIEND)
				SearchFriendsDatabase(info);
		}

		//Sends out all the Players that are connected
		public void SendListOfPlayers(IRaiseEventCallInfo info)
		{
			if(ConnectedPlayers.Count > 0)
			{
				Player requestingClient = null;
				try
				{
					requestingClient = (Player)PacketDecoder.Instance.DecodePlayer((byte[])info.Request.Data);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}

				if(requestingClient != null)
				{
					foreach (Player player in ConnectedPlayers)
					{
						player.SetPacketTarget(requestingClient.GetPacketTarget());
						this.PluginHost.BroadcastEvent(target: ReciverGroup.All, senderActor: 0, targetGroup: 0, evCode: (byte)EVENT_CODES.EV_LISTALLUSERS, data: new Dictionary<byte, object>() { { (byte)245, player } }, cacheOp: 0);
					}
				}
			}
		}

		//Register connecting players to the server
		public void UserJoined(IRaiseEventCallInfo info)
		{
			Player playerToAdd = (Player)PacketDecoder.Instance.DecodePlayer((byte[])info.Request.Data);
			//Only check for duplicate players if theres at least 1 player in ConnectedPlayers
			if (ConnectedPlayers.Count > 0)
			{
				foreach (Player player in ConnectedPlayers)
				{
					//Look for player with the same name
					if (player.GetObjectName() == playerToAdd.GetObjectName())
						return;
				}
			}
			//No duplicate player found
			ConnectedPlayers.Add(playerToAdd);
		}

		//Unregister leaving players from the server
		public void UserLeft(IRaiseEventCallInfo info)
		{
			Player playerToRemove = (Player)PacketDecoder.Instance.DecodePlayer((byte[])info.Request.Data);
			if(ConnectedPlayers.Count > 0)
			{
				foreach (Player player in ConnectedPlayers)
				{
					//Look for player with the same name
					if (player.GetObjectName() == playerToRemove.GetObjectName())
					{
						ConnectedPlayers.Remove(player);
					}
				}
			}
		}

		///////////////////
		//  USERS TABLE  //
		///////////////////

		public void SearchUsersDatabase(IRaiseEventCallInfo info)
        {
            string packetData = Encoding.Default.GetString((byte[])info.Request.Data);

            string sql = "SELECT password FROM users WHERE name = '" + PacketDecoder.Instance.GetStringFromString(packetData, "Name") + "'";
            string queryPassword = ConnectToMySQL(sql, false);
            DisconnectFromMySQL();

            sql = "SELECT name FROM users WHERE name = '" + PacketDecoder.Instance.GetStringFromString(packetData, "Name") + "'";
            string queryName = ConnectToMySQL(sql, false);
            DisconnectFromMySQL();

			if (queryPassword == PacketDecoder.Instance.GetStringFromString(packetData, "Password") && queryName == PacketDecoder.Instance.GetStringFromString(packetData, "Name"))
            {   //Input password is the same as the one in DB, then login
				sql = "SELECT last_position FROM users WHERE name = '" + PacketDecoder.Instance.GetStringFromString(packetData, "Name") + "'";
				string queryPosition = ConnectToMySQL(sql, false);
				DisconnectFromMySQL();

				sql = "SELECT last_rotation FROM users WHERE name = '" + PacketDecoder.Instance.GetStringFromString(packetData, "Name") + "'";
				string queryRotation = ConnectToMySQL(sql, false);
				DisconnectFromMySQL();

				ReturnMessage = "";
				PacketEncoder.Instance.AppendData(ref ReturnMessage, "Name", SenderName);
				PacketEncoder.Instance.AppendData(ref ReturnMessage, "Update", "LOGIN SUCCESS");
				PacketEncoder.Instance.AppendData(ref ReturnMessage, "LastPosition", queryPosition);
				PacketEncoder.Instance.AppendData(ref ReturnMessage, "LastRotation", queryRotation);

				this.PluginHost.BroadcastEvent(target: ReciverGroup.All, senderActor: 0, targetGroup: 0, evCode: (byte)EVENT_CODES.EV_LOGIN_PASS, data: new Dictionary<byte, object>() { { (byte)245, ReturnMessage } }, cacheOp: 0);
			}
            else if (queryPassword != PacketDecoder.Instance.GetStringFromString(packetData, "Password") && queryName == PacketDecoder.Instance.GetStringFromString(packetData, "Name"))
            {   //Input password does not match the one in DB, then update password
				ReturnMessage = "";
				PacketEncoder.Instance.AppendData(ref ReturnMessage, "Name", SenderName);
				PacketEncoder.Instance.AppendData(ref ReturnMessage, "Update", "INVALID PASSWORD");

				this.PluginHost.BroadcastEvent(target: ReciverGroup.All, senderActor: 0, targetGroup: 0, evCode: (byte)EVENT_CODES.EV_LOGIN_FAIL, data: new Dictionary<byte, object>() { { (byte)245, ReturnMessage } }, cacheOp: 0);

				//Update password
				//UpdateDatabase(info, customQuery);
            }
            else
            {   //Password does not exist in DB meaning is new user, then insert user to DB
				ReturnMessage = "";
				PacketEncoder.Instance.AppendData(ref ReturnMessage, "Name", SenderName);
				PacketEncoder.Instance.AppendData(ref ReturnMessage, "Update", "ADDING NEW USER");

				this.PluginHost.BroadcastEvent(target: ReciverGroup.All, senderActor: 0, targetGroup: 0, evCode: info.Request.EvCode, data: new Dictionary<byte, object>() { { (byte)245, ReturnMessage } }, cacheOp: 0);

				//Run insert function
				InsertUsersDatabase(info);
            }
        }

        public void InsertUsersDatabase(IRaiseEventCallInfo info)
		{
			string packetData = Encoding.Default.GetString((byte[])info.Request.Data);

			string sql = "INSERT INTO users (name, password, date_created) VALUES ('" + PacketDecoder.Instance.GetStringFromString(packetData, "Name") + "', '" + PacketDecoder.Instance.GetStringFromString(packetData, "Password") + "', now())";
            ConnectToMySQL(sql, true);
            DisconnectFromMySQL();
        }

		public void UpdateUsersDatabase(IRaiseEventCallInfo info)
        {
            string packetData = Encoding.Default.GetString((byte[])info.Request.Data);

            string sql = "UPDATE users SET password = '" + PacketDecoder.Instance.GetStringFromString(packetData, "Password") + "', prefab_path = '" + PacketDecoder.Instance.GetStringFromString(packetData, "PrefabPath") + "', last_position = '" + PacketDecoder.Instance.GetVectorFromString(packetData, "LastPosition") + "', last_rotation = '" + PacketDecoder.Instance.GetQuaternionFromString(packetData, "LastRotation") + "' WHERE name = '" + PacketDecoder.Instance.GetStringFromString(packetData, "Name") + "'";
			ConnectToMySQL(sql, true);
            DisconnectFromMySQL();
        }

		///////////////////
		// FRIENDS TABLE //
		///////////////////

		public void SearchFriendsDatabase(IRaiseEventCallInfo info)
		{
			string packetData = Encoding.Default.GetString((byte[])info.Request.Data);
			List<string> result = new List<string>();

			string sql = "SELECT name_friendee FROM friends WHERE name_friender = '" + PacketDecoder.Instance.GetStringFromString(packetData, "NameFriender") + "' AND relationship_status = 1";

			ConnectToMySQL();
			using (MySqlCommand cmd = new MySqlCommand(sql, conn))
			{
				cmd.CommandType = System.Data.CommandType.Text;
				using (MySqlDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader.GetString(0));
					}
					reader.Close();
				}
				cmd.Cancel();
			}

			PacketEncoder.Instance.AppendData(ref ReturnMessage, "Name", SenderName);

			ReturnMessage += "Friends=";
			foreach (string thing in result)
			{
				ReturnMessage += thing + ",";
			}
			ReturnMessage = ReturnMessage.Remove(ReturnMessage.Length - 1);
			ReturnMessage += ";";

			this.PluginHost.BroadcastEvent(target: ReciverGroup.All, senderActor: 0, targetGroup: 0, evCode: (byte)EVENT_CODES.EV_LIST_FRIEND_DATA, data: new Dictionary<byte, object>() { { (byte)245, ReturnMessage } }, cacheOp: 0);
		}

		public void InsertFriendsDatabase(IRaiseEventCallInfo info)
		{	//Use this to insert data into friends table
			string packetData = Encoding.Default.GetString((byte[])info.Request.Data);

			if (PacketDecoder.Instance.GetStringFromString(packetData, "NameFriender") == "" || PacketDecoder.Instance.GetStringFromString(packetData, "NameFriendee") == "")
				return; //Catch any unwanted inserts to database with empty inputs

			string sql = "INSERT INTO friends (name_friender, name_friendee, relationship_status) VALUES ('" + PacketDecoder.Instance.GetStringFromString(packetData, "NameFriender") + "', '" + PacketDecoder.Instance.GetStringFromString(packetData, "NameFriendee") + "', 0)";
			ConnectToMySQL(sql, true);
			DisconnectFromMySQL();
		}

		public void InsertFriendsDatabase(string friender, string friendee)
		{   //Use this to insert data into friends table
			if (friender == "" || friendee == "")
				return;	//Catch any unwanted inserts to database with empty inputs

			string sql = "INSERT INTO friends (name_friender, name_friendee, relationship_status) VALUES ('" + friender + "', '" + friendee + "', 1)";
			ConnectToMySQL(sql, true);
			DisconnectFromMySQL();
		}

		public void UpdateFriendsDatabase(IRaiseEventCallInfo info)
		{	//Use this to update data in friends table
			string packetData = Encoding.Default.GetString((byte[])info.Request.Data);
			int relationshipStatus = 0;

			if (info.Request.EvCode == (byte)EVENT_CODES.EV_ACCEPT_FRIEND)
				relationshipStatus = 1;
			if (info.Request.EvCode == (byte)EVENT_CODES.EV_DECLINE_FRIEND)
				relationshipStatus = 2;

			string sql = "SELECT name_friender FROM friends WHERE name_friendee = '" + PacketDecoder.Instance.GetStringFromString(packetData, "NameFriendee") + "' AND relationship_status = 0";
			string friender = ConnectToMySQL(sql, false);
			DisconnectFromMySQL();

			sql = "UPDATE friends SET relationship_status = " + relationshipStatus + " WHERE name_friender = '" + friender + "' AND name_friendee = '" + PacketDecoder.Instance.GetStringFromString(packetData, "NameFriendee") + "'";
			ConnectToMySQL(sql, true);
			DisconnectFromMySQL();

			//Now time to update the other party
			if(relationshipStatus == 1)
			{
				InsertFriendsDatabase(PacketDecoder.Instance.GetStringFromString(packetData, "NameFriendee"), friender);
			}
			else if (relationshipStatus == 2)
			{
				DeleteFriendsDatabase(friender, PacketDecoder.Instance.GetStringFromString(packetData, "NameFriendee"));
			}
		}

		public void DeleteFriendsDatabase(IRaiseEventCallInfo info)
		{   //Use this to delete data from friends table
			string packetData = Encoding.Default.GetString((byte[])info.Request.Data);

			string sql = "DELETE FROM friends WHERE name_friender = '" + PacketDecoder.Instance.GetStringFromString(packetData, "NameFriender") + "' AND name_friendee = '" + PacketDecoder.Instance.GetStringFromString(packetData, "NameFriendee") + "'";
			ConnectToMySQL(sql, true);
			DisconnectFromMySQL();

			sql = "DELETE FROM friends WHERE name_friendee = '" + PacketDecoder.Instance.GetStringFromString(packetData, "NameFriender") + "' AND name_friender = '" + PacketDecoder.Instance.GetStringFromString(packetData, "NameFriendee") + "'";
			ConnectToMySQL(sql, true);
			DisconnectFromMySQL();
		}

		public void DeleteFriendsDatabase(string friender, string friendee)
		{   //Use this to delete data from friends table
			string sql = "DELETE FROM friends WHERE name_friender = '" + friender + "' AND name_friendee = '" + friendee + "'";
			ConnectToMySQL(sql, true);
			DisconnectFromMySQL();
		}

		public void ConnectToMySQL()
		{
			// Connect to MySQL
			string connStr = "server=localhost;user=root;database=photon;port=3306;password=1234;SslMode=none";
            conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
		}

        public string ConnectToMySQL(string query, bool nonQuery)
        {
            // Connect to MySQL
            string connStr = "server=localhost;user=root;database=photon;port=3306;password=1234;SslMode=none";
            conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                
                if(nonQuery)
                {
                    cmd.ExecuteNonQuery();
                    return cmd.ToString();
                }
                else
                {
                    return (string)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                return "Failed:" + ex.ToString();
            }
        }

        public void DisconnectFromMySQL()
		{
			conn.Close();
		}
	}
}