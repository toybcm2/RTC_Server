using System;
using System.Text;

namespace ChatServer
{
    class StateObject
    {
        // Client  socket.
        public System.Net.Sockets.Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();

        public Guid Id { get; set; }
        public String Alias { get; set; }

    }
}
