#region license
/* This file is part of Ucore.

    Ucore is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Foobar is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Ucore.  If not, see<http://www.gnu.org/licenses/>.
*/

/* developer gnomexfire
   about
   Lammas this is a simple chat server intranet , communicates with TCP protocols .
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using R1_lammas.view;

namespace R1_lammas.Class
{
    #region Ucore
    /// <summary>
    /// structs and functions server configuration
    /// </summary>
    public class Ucore
    {
        /// <summary>
        /// struct represente configuration server
        /// </summary>
        public struct Myserver
        {
            public string Ip; // server ip
            public int Port; // port listen
            public string Name; // server name 
            public int Maxuser; // max user online
        }
        public Myserver Userver;

        /// <summary>
        /// simple function return ip
        /// </summary>
        /// <returns></returns>
        public string Getmyip()
        {
            var uip = IPAddress.None;
            foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                uip = ip;
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    break;
            }
            return uip.ToString();
        }
    }
    #endregion

    /// <summary>
    /// is the server configuration
    /// </summary>

    public delegate void StatusChangeHandler(object sender, StatusChangedArgs e);

    #region Lammas
    /// <summary>
    /// this class represent event send receive message
    /// </summary>
    public class Lammas
    {
        
        /// <summary>
        /// total de usuario
        /// </summary>
        public static Hashtable Userfull { get; set; } /* = new Hashtable(MyW.Core.Userver.Maxuser);*/
        /// <summary>
        /// total de conexao
        /// </summary>
        public static Hashtable Connectionfull { get; set; }

        // ip server
        public IPAddress IpAddress { get; set; }
        public TcpListener TlsClient { get; set; }

        public static event StatusChangeHandler Statuschanged;
        public static StatusChangedArgs Sevent;

        /// <summary>
        /// event delegate
        /// </summary>
        /// <param name="e"></param>
        private static void OnStatus(StatusChangedArgs e)
        {
            var statusChangeHandler = Statuschanged;
            statusChangeHandler?.Invoke(null, e);
        }

        public static Thread Threadlisten;
        public static TcpClient Tcpclient ;
        public static bool ServerStatus { get; set; }

        /// <summary>
        /// constructor class Lammas
        /// </summary>
        /// <param name="ip">ip server</param>
        public Lammas(IPAddress ip)
        {
            IpAddress = ip;

            // start hashtable
            if(MyW.Core.Userver.Maxuser == 0) { return;}
            Userfull = new Hashtable(MyW.Core.Userver.Maxuser);
            Connectionfull = new Hashtable(MyW.Core.Userver.Maxuser);

        }

        /// <summary>
        /// this function start server
        /// </summary>
        public void Start()
        {
            try
            {
                // start tcplistener ip and port
                TlsClient = new TcpListener(IpAddress, MyW.Core.Userver.Port);
                TlsClient.Start();
                // create thread
                Threadlisten = new Thread(new ThreadStart(ContinueInQueue));
                Threadlisten.Start();

                // update status server
                ServerStatus = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"public void Start()" + Environment.NewLine +
                                  ex.HResult + Environment.NewLine +
                                  ex.ToString());
                ServerStatus = false;
            }
        }

        /// <summary>
        /// thread create
        /// </summary>
        private void ContinueInQueue()
        {
            while (ServerStatus)
            {
                Tcpclient = TlsClient.AcceptTcpClient();
                IndexConnection n = new IndexConnection(Tcpclient);
            }
        }
        
        #region function's

        /// <summary>
        /// send message all clients
        /// </summary>
        /// <param name="origem">client</param>
        /// <param name="s">content chat</param>
        public static void SendMessage(string origem,
            string s)
        {
            Sevent = new StatusChangedArgs(origem + " disse: " + s);
            OnStatus(Sevent);
            MyW.Logchat(origem + " disse: " + s);

            TcpClient[] clients = new TcpClient[Userfull.Count];
            Userfull.Values.CopyTo(clients,0);

            for (int i = 0; i < clients.Length; i++)
            {
                try
                {
                    if(s.Trim() == string.Empty || clients[i] == null) { continue;}
                    var swWriter = new StreamWriter(clients[i].GetStream());
                    swWriter.WriteLine(origem + " disse: " + s);
                    swWriter.Flush();
                    swWriter = null;
                }
                catch (Exception ex)
                {
                    RemoveUser(clients[i]);
                    Console.WriteLine(ex.HResult + Environment.NewLine +
                                      ex.ToString());    
                }
            }

        }

        /// <summary>
        /// server send information message to all clients
        /// </summary>
        /// <param name="s"></param>
        public static void SendAdminMessage(string s)
        {
            Sevent = new StatusChangedArgs("Sistema: "+ s);
            OnStatus(Sevent);
            MyW.Logchat("Sistema: " + s);


            TcpClient[] clients = new TcpClient[Lammas.Userfull.Count];

            Lammas.Userfull.Values.CopyTo(clients,0);

            foreach (TcpClient t in clients)
            {
                try
                {
                    if (s.Trim() == string.Empty ||
                        t == null)
                    {
                        continue;
                    }
                    var streamWriter = new StreamWriter(t.GetStream());
                    streamWriter.WriteLine("Sistema: " + s);
                    streamWriter.Flush();
                    streamWriter = null;
                }
                catch (Exception ex)
                {
                    RemoveUser(t);    
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// simple function add ip and name client in Hashtable
        /// </summary>
        /// <param name="client">ip client</param>
        /// <param name="user">name client</param>
        public static void Adduser(TcpClient client,
            string user)
        {
            Lammas.Userfull.Add(user,client);
            Lammas.Connectionfull.Add(client, user);
            
            SendAdminMessage(Connectionfull[client] + " entrou !");
            MyW.Logchat(Connectionfull[client] + " entrou !");
        }

        /// <summary>
        /// simple function remove client's hashtable
        /// </summary>
        /// <param name="client"></param>
        public static void RemoveUser(TcpClient client)
        {
            if (Lammas.Connectionfull[client] != null)
            {
                var saiu = Connectionfull[client];
                MyW.Logchat(Connectionfull[client] + " saiu !");
                Lammas.Userfull.Remove(Lammas.Connectionfull[client]);
                Lammas.Connectionfull.Remove(client);

                SendAdminMessage(saiu + " saiu !");

            }
        }

        #endregion
    }
    #endregion
    
    #region IndexConnection and StatusChangedArgs
    /// <summary>
    /// class represent thread client array
    /// </summary>
    public class IndexConnection
    {
        private TcpClient _tcpClient;

        // thread send message o client
        private Thread _thSender;
        private StreamReader _srRecptor;
        private StreamWriter _srSend;
        private string user;
        private string strReply;

        /// <summary>
        /// constructor class
        /// </summary>
        /// <param name="tcp"></param>
        public IndexConnection(TcpClient tcp)
        {
            _tcpClient = tcp;
            // thread accept client listen message
            _thSender = new Thread(new ThreadStart(AcceptNewClient));
            // thread call method AcceptNewClient()
            _thSender.Start();

        }

        /// <summary>
        /// accept new client
        /// </summary>
        private void AcceptNewClient()
        {
            _srRecptor = new StreamReader(_tcpClient.GetStream());
            _srSend = new StreamWriter(_tcpClient.GetStream());

            // check number clients
            if (Lammas.Userfull.Count == MyW.Core.Userver.Maxuser ||
                Lammas.Userfull.Count > MyW.Core.Userver.Maxuser)
            {
                _srSend.WriteLine("0|Chat busy");
                _srSend.Flush();
                CloseConnetion();
                return;
            }

            // read client information accont
            user = _srRecptor.ReadLine();
            
            if (user != string.Empty)
            {
                // list name not be used
                if (user != null && BlockName.ListName.Contains(user))
                {
                    // 0 nick invalid
                    _srSend.WriteLine("0|Nick user invalid");
                    _srSend.Flush();
                    CloseConnetion();
                    return;
                }
                else if(user != null && Lammas.Userfull.Contains(user))
                {
                    _srSend.WriteLine("0|Nick in use");
                    _srSend.Flush();
                    CloseConnetion();
                    return;
                }
                // accept user
                else
                {
                    _srSend.WriteLine("1");
                    _srSend.Flush();
                    
                    // add user in hashtable
                    Lammas.Adduser(_tcpClient,user);
                }
            }
            else
            {
                CloseConnetion();
                return;
            }

            try
            {
                while ((strReply = _srRecptor.ReadLine()) != String.Empty)
                {
                    if (strReply == null)
                    {
                        Lammas.RemoveUser(_tcpClient);
                    }
                    else
                    {
                        // filter message or command

                        // send message all user (chat)
                        Lammas.SendMessage(user,strReply);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.HResult + Environment.NewLine +
                    ex.ToString());                
            }
        }

        /// <summary>
        /// close all object's open
        /// </summary>
        public void CloseConnetion()
        {
            _thSender.Abort();
            _tcpClient.Close();
            _srRecptor.Close();
            _srSend.Close();
        }
    }


    public class StatusChangedArgs : EventArgs
    {
        // event
        public string Eventmsg;

        // return message event
        public string Eventmessage
        {
            get { return Eventmsg; }
            set { Eventmsg = value; }
        }

        // constructor message event
        public StatusChangedArgs(string s)
        {
            Eventmsg = s;
        }
    }
    #endregion

}
