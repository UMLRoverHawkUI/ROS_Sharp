﻿// File: RosOutAppender.cs
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

using System.Collections.Generic;
using System.Threading;
using Messages;
using Messages.rosgraph_msgs;
using m = Messages.std_msgs;
using gm = Messages.geometry_msgs;
using nm = Messages.nav_msgs;

#endregion

namespace Ros_CSharp
{
    public class RosOutAppender
    {
        public enum ROSOUT_LEVEL
        {
            DEBUG = 1,
            INFO = 2,
            WARN = 4,
            ERROR = 8,
            FATAL = 16
        }

        public Queue<IRosMessage> log_queue = new Queue<IRosMessage>();
        public Thread publish_thread;
        public object queue_mutex = new object();
        public bool shutting_down;

        public RosOutAppender()
        {
            publish_thread = new Thread(logThread) {IsBackground = true};
            publish_thread.Start();
            AdvertiseOptions<Log> ops = new AdvertiseOptions<Log>(names.resolve("/rosout"), 0) {latch = true};
            SubscriberCallbacks cbs = new SubscriberCallbacks();
            TopicManager.Instance.advertise(ops, cbs);
        }

        public void shutdown()
        {
            lock (queue_mutex)
            {
                shutting_down = true;
                publish_thread.Join();
            }
        }

        public void Append(string m)
        {
            Append(m, ROSOUT_LEVEL.INFO);
        }

        public void Append(string m, ROSOUT_LEVEL lvl)
        {
            Log l = new Log {msg = new m.String(m), level = ((byte) ((int) lvl)), name = new m.String(this_node.Name), file = new m.String("*.cs"), function = new m.String("main"), line = 28};
            string[] advert = this_node.AdvertisedTopics().ToArray();
            l.topics = new m.String[advert.Length];
            for (int i = 0; i < advert.Length; i++)
                l.topics[i] = new m.String(advert[i]);
            lock (queue_mutex)
                log_queue.Enqueue(l);
        }

        public void logThread()
        {
            List<IRosMessage> localqueue = new List<IRosMessage>();
            while (!shutting_down)
            {
                bool nothingtolog = false;
                lock (queue_mutex)
                {
                    if (log_queue.Count > 0)
                    {
                        if (shutting_down) return;
                        localqueue.AddRange(log_queue);
                        log_queue.Clear();
                    }
                    else
                        nothingtolog = true;
                }
                if (nothingtolog)
                {
                    Thread.Sleep(10);
                    continue;
                }
                if (shutting_down) return;
                foreach (IRosMessage msg in localqueue)
                {
                    TopicManager.Instance.publish(names.resolve("/rosout"), msg);
                }
                localqueue.Clear();
                Thread.Sleep(100);
            }
        }
    }
}