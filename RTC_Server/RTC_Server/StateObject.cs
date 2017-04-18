using System;
using System.Text;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace ChatServer
{
    class StateObject
    {
        public StreamSocket workSocket = null;
        public DataWriter writer = null;
        public DataReader reader = null;
        public bool connected = false;
        public string RoomId { get; set; }
        public string ClientId { get; set; }
        public String Alias { get; set; }

    }
}
