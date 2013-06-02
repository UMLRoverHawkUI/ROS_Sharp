/******************************** Module Header ********************************\
* Module Name:  SystemIONamedPipeClient.cs
* Project:      CSNamedPipeClient
* Copyright (c) Microsoft Corporation.
* 
* The System.IO.Pipes namespace contains types that provide a means for 
* interprocess communication through anonymous and/or named pipes. 
* http://msdn.microsoft.com/en-us/library/system.io.pipes.aspx
* These classes make the programming of named pipe in .NET much easier and safer
* than P/Invoking native APIs directly. However, the System.IO.Pipes namespace 
* is not available before .NET Framework 3.5. So, if you are programming against 
* .NET Framework 2.0, you have to P/Invoke native APIs to use named pipe.
* 
* The sample code in SystemIONamedPipeClient.Run() uses the 
* Systen.IO.Pipes.NamedPipeClientStream class to connect to the named pipe 
* "\\.\pipe\SamplePipe" to perform message-based duplex communication. The client 
* then writes a message to the pipe and receives the response from the pipe 
* server.
* 
* This source is subject to the Microsoft Public License.
* See http://www.microsoft.com/en-us/openness/licenses.aspx#MPL.
* All other rights reserved.
* 
* THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
* EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
* WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\*******************************************************************************/

using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;


namespace CSNamedPipeClient
{
    class SystemIONamedPipeClient
    {
        private static bool die;

        private static void Kill()
        {
            die = true;
        }

        /// <summary>
        /// Use the types in the System.IO.Pipes namespace to connect to the 
        /// named pipe. This solution is recommended when you program against 
        /// .NET Framework 3.5 or any newer releases of .NET Framework.
        /// </summary>
        public static void Run(string ServerName, string PipeName)
        {
            NamedPipeClientStream pipeClient = null;

            try
            {
                // Try to open the named pipe identified by the pipe name.

                pipeClient = new NamedPipeClientStream(
                    ServerName,         // The server name
                    PipeName,           // The unique pipe name
                    PipeDirection.Out,        // The pipe is duplex
                    PipeOptions.None            // No additional parameters
                    );

                pipeClient.Connect(5000);
                Console.WriteLine("The named pipe ({0}) is connected.",
                    FullPipeName);

                // Set the read mode and the blocking mode of the named pipe. In 
                // this sample, we set data to be read from the pipe as a stream 
                // of messages. This allows a reading process to read varying-
                // length messages precisely as sent by the writing process. In 
                // this mode, you should not use StreamWriter to write the pipe, 
                // or use StreamReader to read the pipe. You can read more about 
                // the difference from http://go.microsoft.com/?linkid=9721786.
                pipeClient.ReadMode = PipeTransmissionMode.Message;

                while (!die)
                {
                    // 
                    // Send a request from client to server
                    // 
                    string message = "zomg something! " + new Random().Next(); ;
                    byte[] bRequest = Encoding.Unicode.GetBytes(message);
                    int cbRequest = bRequest.Length;

                    pipeClient.Write(bRequest, 0, cbRequest);

                    Console.WriteLine("Send {0} bytes to server: \"{1}\"", cbRequest, message.TrimEnd('\0'));

                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The client throws the error: {0}", ex.Message);
            }
            finally
            {
                // Close the pipe.
                if (pipeClient != null)
                {
                    pipeClient.Close();
                    pipeClient = null;
                }
            }
        }
    }
}