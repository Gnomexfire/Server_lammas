#region license
/* This file is part of R1_lammas.

    R1_lammas is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Foobar is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with R1_lammas.  If not, see<http://www.gnu.org/licenses/>.
*/

/* developer gnomexfire
   about
   Lammas this is a simple chat server intranet , communicates with TCP protocols .
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using R1_lammas.Class;

namespace R1_lammas.view
{
    /// <summary>
    /// Interaction logic for MyW.xaml
    /// </summary>
    public partial class MyW : Window
    {
        public static Ucore Core = new Ucore();
        public static Lammas MyLammas;
        public DispatcherTimer TimerTick = new DispatcherTimer();

        public MyW()
        {
            InitializeComponent();
            TimerTick.Interval = new TimeSpan(0,0,2);
            TimerTick.Tick += (sender, args) =>
            {
                //LblTotalUsuarioOn.Content = Lammas.Userfull.Count;
                Statusbarupdate(Lammas.ServerStatus,Lammas.Userfull.Count);
            };
            TimerTick.Start();
        }

        /// <summary>
        /// simple update status
        /// </summary>
        /// <param name="serverstatus">online or offline</param>
        /// <param name="fulluser">full user online</param>
        internal void Statusbarupdate(bool serverstatus=false ,int fulluser=0)
        {
            // server status online or offline
            LblServicoStatus.Content = serverstatus ? @"Ligado" : @"Desligado";
            // full user online
            LblTotalUsuarioOn.Content = fulluser;
        }

        ///// <summary>
        ///// simple function kill all thread create by application
        ///// </summary>
        //internal void CloseAllThreadOpen()
        //{
        //    Dispatcher.CurrentDispatcher.Thread.Abort();
        //    Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        //    Application.Current.Shutdown();
        //}

        /// <summary>
        /// load 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyW_OnLoaded(object sender, RoutedEventArgs e)
        {
            Lblmeuip.Content = Core.Getmyip();

            // adjust the settings
            Core.Userver.Maxuser = 30; // max user
            Core.Userver.Ip = Core.Getmyip(); // ip server
            Core.Userver.Port = 2502; // set port
            Core.Userver.Name = "Lammas"; // server name
            Logchat("ajustando configuracao . . .");

            // instance class lammas send ip server as parameter
            MyLammas = new Lammas(IPAddress.Parse(Core.Userver.Ip));

            Step(); // use start server
            Logchat(Lammas.ServerStatus ? "Status: Ligado" : "Status: Desligado");

            Logchat("server ip: " + Core.Userver.Ip);
            Logchat("port: " + Core.Userver.Port);
            Logchat("name: " + Core.Userver.Name);
            Logchat("max user: " + Core.Userver.Maxuser);

        }

        /// <summary>
        /// user start server
        /// </summary>
        internal void Step(bool disable=false)
        {
            // start server
            MyLammas.Start();
            if (!Lammas.ServerStatus)
            {
                MessageBox.Show(@"Fail");
            }

            // disable = true stop server
        }
       
        public static void Logchat(string s)
        {
            //new Thread(() =>
            //{
            //    ((MyW)Application.Current.MainWindow).TxtLog.AppendText(DateTime.Now + " - " + s + Environment.NewLine);
            //}).Start();

            //Dispatcher.CurrentDispatcher.Invoke((Action)(() =>
            //{
            //    ((MyW)Application.Current.MainWindow).TxtLog.AppendText(DateTime.Now + " - " + s + Environment.NewLine);
            //}));

            //((MyW) Application.Current.MainWindow).TxtLog.Dispatcher.Invoke(
            //     (Action) (() => ((MyW) Application.Current.MainWindow).TxtLog.AppendText(s + Environment.NewLine)));

            //if (((MyW) Application.Current.MainWindow).TxtLog.CheckAccess())
            //{
            //    ((MyW)Application.Current.MainWindow).TxtLog.AppendText(DateTime.Now + " - " + s + Environment.NewLine);
            //}
            //else
            //{
            //    Dispatcher.CurrentDispatcher.Invoke((Action)(() =>
            //    {
            //        ((MyW)Application.Current.MainWindow).TxtLog.AppendText(DateTime.Now + " - " + s + Environment.NewLine);
            //    }));
            //}

            //Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
            //{
            //    ((MyW)Application.Current.MainWindow).TxtLog.AppendText(DateTime.Now + " - " + s + Environment.NewLine);
            //} ));

            Application.Current.Dispatcher.Invoke(
            DispatcherPriority.Normal, (Action)delegate
            {
                // Update UI component here
                ((MyW)Application.Current.MainWindow).TxtLog.AppendText(DateTime.Now + " - " + s + Environment.NewLine);
                ((MyW)Application.Current.MainWindow).TxtLog.ScrollToEnd();
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
}
