﻿// File: Socket.cs
// Project: ROS_C-Sharp
// 
// ROS#
// Eric McCann <emccann@cs.uml.edu>
// UMass Lowell Robotics Laboratory
// 
// Reimplementation of the ROS (ros.org) ros_cpp client in C#.
// 
// Created: 03/04/2013
// Updated: 07/26/2013

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using n = System.Net;
using ns = System.Net.Sockets;

#endregion

namespace Ros_CSharp.CustomSocket
{
    public class Socket : ns.Socket
    {
        private static SortedList<uint, Socket> _socklist;
        private static uint nextfakefd = 1;
        private static List<uint> _freelist = new List<uint>();
        private uint _fakefd;

        private string attemptedConnectionEndpoint;
        private bool disposed;

        public Socket(ns.Socket sock)
            : this(sock.DuplicateAndClose(Process.GetCurrentProcess().Id))
        {
        }

        public Socket(ns.AddressFamily addressFamily, ns.SocketType socketType, ns.ProtocolType protocolType)
            : base(addressFamily, socketType, protocolType)
        {
            if (_socklist == null)
                _socklist = new SortedList<uint, Socket>();
            _socklist.Add(FD, this);
            //EDB.WriteLine("Making socket w/ FD=" + FD);
        }

        public Socket(ns.SocketInformation socketInformation)
            : base(socketInformation)
        {
            if (_socklist == null)
                _socklist = new SortedList<uint, Socket>();
            _socklist.Add(FD, this);
            //EDB.WriteLine("Making socket w/ FD=" + FD);
        }

        public bool IsDisposed
        {
            get { return disposed; }
        }

        public uint FD
        {
            get
            {
                if (_fakefd == 0)
                {
                    if (_freelist.Count > 0)
                    {
                        _fakefd = _freelist[0];
                        _freelist.RemoveAt(0);
                    }
                    else
                        _fakefd = (nextfakefd++);
                }
                return _fakefd;
            }
        }

        public new void Connect(n.IPAddress[] address, int port)
        {
            attemptedConnectionEndpoint = address[0].ToString();
            base.Connect(address, port);
        }

        public new void Connect(n.IPAddress address, int port)
        {
            attemptedConnectionEndpoint = address.ToString();
            base.Connect(address, port);
        }

        public new void Connect(n.EndPoint ep)
        {
            attemptedConnectionEndpoint = ep.ToString();
            base.Connect(ep);
        }

        public new bool ConnectAsync(ns.SocketAsyncEventArgs e)
        {
            attemptedConnectionEndpoint = e.RemoteEndPoint.ToString();
            return base.ConnectAsync(e);
        }

        public static Socket Get(uint fd)
        {
            if (_socklist == null || !_socklist.ContainsKey(fd))
                return null;
            return _socklist[fd];
        }

        ~Socket()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                EDB.WriteLine("Killing socket w/ FD=" + FD + (attemptedConnectionEndpoint == null ? "" : "\tTO REMOTE HOST\t" + attemptedConnectionEndpoint));
                if (Get(FD) != null)
                {
                    _socklist.Remove(FD);
                }
                disposed = true;
                _freelist.Add(FD);
                base.Dispose(disposing);
            }
        }

        public bool SafePoll(int timeout, ns.SelectMode sm)
        {
            if (disposed) return false;
            bool res = false;
            try
            {
                res = Poll(timeout, sm);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                res = sm == ns.SelectMode.SelectError;
            }
            return res;
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            if (string.IsNullOrEmpty(attemptedConnectionEndpoint))
            {
                if (!Connected)
                    attemptedConnectionEndpoint = "";
                else if (RemoteEndPoint != null)
                {
                    n.IPEndPoint ipep = RemoteEndPoint as n.IPEndPoint;
                    if (ipep != null)
                        attemptedConnectionEndpoint = "" + ipep.Address + ":" + ipep.Port;
                }
            }
            return "" + FD + " -- " + attemptedConnectionEndpoint;
        }
    }
}