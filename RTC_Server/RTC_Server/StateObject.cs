using System;
using System.Text;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace ChatServer
{
    class StateObject
    {
        // Client  socket.
        //public System.Net.Sockets.Socket workSocket = null;
        public StreamSocket workSocket = null;
        public DataWriter writer = null;
        public DataReader reader = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();

        public Guid RoomId { get; set; }
        public string ClientId { get; set; }
        //public Guid Id { get; set; }
        public String Alias { get; set; }

    }
}
