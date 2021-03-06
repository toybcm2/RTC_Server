﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace ChatServer
{
    class Program
    {
        // Thread signal.
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        private static Dictionary<string, Chat_Room> rooms = new Dictionary<string, Chat_Room>();
        private static StreamSocketListener listener = new StreamSocketListener();

        public static int Main(string[] args)
        {
            Console.WriteLine("Server Starting up...");
            StartListening();
            return 0;
        }

        public static void StartListening()
        {
            try
            {
                listener.ConnectionReceived += OnConnection;
                IReadOnlyList<HostName> hosts = NetworkInformation.GetHostNames();
                HostName myName = hosts[3];

                Task.Run(async () => { await listener.BindEndpointAsync(myName, "8888"); });
                while (true)
                {
                    allDone.Reset();
                    Console.WriteLine("Waiting for a connection...");         
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void OnConnection(StreamSocketListener listener, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Console.WriteLine("New Client Connected");

            Task.Run(() => {
                //ManualResetEvent wait = new ManualResetEvent(false);
                StateObject client = new StateObject();
                client.workSocket = args.Socket;
                client.reader = new DataReader(args.Socket.InputStream);
                client.writer = new DataWriter(args.Socket.OutputStream);
                client.connected = true;

                while(client.connected)
                    Listen(client);
            });

            allDone.Set();
        }

        public static void Listen(StateObject client)
        {
            try
            {
                byte[] header = new byte[5];
                client.reader.LoadAsync((uint)5).AsTask().Wait();
                client.reader.ReadBytes(header);

                int command = header[0];
                string slength = header[1].ToString() + header[2].ToString() + header[3].ToString() + header[4].ToString();
                uint length = Convert.ToUInt32(slength);

                client.reader.LoadAsync(length).AsTask().Wait();
                string data = client.reader.ReadString(length);

                if (command == 0)
                    SendToAll(client.RoomId, data);
                else
                {
                    CheckArgs(client, data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                client.connected = false;
            }
        }

        public static void SendToAll(string roomId, string data, byte x = 0)
        {
            foreach (var member in rooms[roomId].GetMembers())
            {
                Task.Run(() => { SendAsync(member.Value, data, x); });
            }
        }

        public static async void SendAsync(StateObject client, string data, byte commandResponse = 0)
        {
            try
            {
                string count = data.Length.ToString();
                byte[] header = { commandResponse , 0, 0, 0, 0 };
                switch(count.Length)
                {
                    case 1:
                        header[4] = byte.Parse(count[0].ToString());
                        break;
                    case 2:
                        header[4] = byte.Parse(count[1].ToString());
                        header[3] = byte.Parse(count[0].ToString());
                        break;
                    case 3:
                        header[4] = byte.Parse(count[2].ToString());
                        header[3] = byte.Parse(count[1].ToString());
                        header[2] = byte.Parse(count[0].ToString());
                        break;
                    case 4:
                        header[4] = byte.Parse(count[3].ToString());
                        header[3] = byte.Parse(count[2].ToString());
                        header[2] = byte.Parse(count[1].ToString());
                        header[1] = byte.Parse(count[0].ToString());
                        break;
                }

                client.writer.WriteBytes(header);
                await client.writer.StoreAsync();

                client.writer.WriteString(data);
                await client.writer.StoreAsync();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void CheckArgs(StateObject client, string data)
        {
            string alias, memberId;
            var args = data.Split(':');
            string comand = args[0];

            switch (comand.ToLower())
            {
                case ("connect"):

                    memberId = args[1];
                    alias = args[2];
                    client.admin = args[3] == "True" ? true : false;
                    client.RoomId = args[4];

                    try
                    {
                        rooms[client.RoomId].AddMember(client, memberId, alias);
                        SendToAll(client.RoomId, alias + " Joined the chat.");
                    }
                    catch(KeyNotFoundException e)
                    {
                        Console.WriteLine("client tried to connect to room that doesnt exsist, so now we're making it.");
                        rooms.Add(client.RoomId, new Chat_Room("default", client.RoomId));
                        rooms[client.RoomId].AddMember(client, memberId, alias);
                        SendToAll(client.RoomId, alias + " Joined the chat.");
                    }
                    break;
                case ("queue_request"):

                    memberId = args[1];
                    alias = args[2];
                    
                    Console.WriteLine("user queue request");
                    if(rooms[client.RoomId].HasAdmin())
                    {
                        string request = "queue_request:" + memberId + ":" + alias;
                        SendAsync(rooms[client.RoomId].GetAdmin(), request, 1);
                    }
                    break;

                case ("queue_accept"):
                    memberId = args[1];
                    alias = args[2];
                    uint count = Convert.ToUInt32(args[3]);
                    byte[] image = new byte[count];

                    client.reader.LoadAsync(count).AsTask().Wait();
                    client.reader.ReadBytes(image);

                    string command = "queue_add:" + alias + ":" + count;
                    SendToAll(client.RoomId, comand, 1);
                    foreach (var member in rooms[client.RoomId].GetMembers())
                    {
                        Task.Run(async () =>
                        {
                            member.Value.writer.WriteBytes(image);
                            await member.Value.writer.StoreAsync();
                        }); 

                    }
                    break;

                case ("queue_reject"):
                    break;
            }
        }

        /*public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            try
            {
                String content = String.Empty;

                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket. 
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read 
                    // more data.
                    content = state.sb.ToString();
                    if (content.IndexOf("<EOF>") > -1)
                    {
                        // All the data has been read from the 
                        // client. Display it on the console.
                        content = content.Substring(0, content.IndexOf("<EOF>"));
                        Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
                        //check message for arguments
                        if (content[0].Equals('~'))
                            CheckArgs(content, state);
                        else
                        {
                            //Get Chatroom Id
                            int index = content.LastIndexOf(' ');
                            String g = content.Substring(index);
                            //Get senders Id
                            content = content.Substring(0, index);
                            index = content.LastIndexOf(' ');
                            String m = content.Substring(index);
                            //send message to the chatroom
                            sendToAll(Guid.Parse(g), Guid.Parse(m), content.Substring(0, index));
                        }

                        //Listen for more
                        state.buffer = new byte[StateObject.BufferSize];
                        state.sb.Clear();
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    }
                    else
                    {
                        // Not all data received. Get more.
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    }
                }
            }
            catch(SocketException e)
            {
                Console.WriteLine(">> A client has disconnected unexpectedly");
                Console.WriteLine(e.ToString());
            }
        }

        /*private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data + "<EOF>");

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }*/

    }
}
