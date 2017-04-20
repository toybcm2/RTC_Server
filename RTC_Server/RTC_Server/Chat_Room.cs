using System;
using System.Collections.Generic;

namespace ChatServer
{
    class Chat_Room
    {
        private Dictionary<string, StateObject> chatMembers = new Dictionary<string, StateObject>();
        private string name;
        private string Id;
        private StateObject Admin = null;

        public Chat_Room(String name, string roomId)
        {
            SetName(name);
            Id = roomId;
        }

        public void SetName(String s)
        {
            this.name = s;
        }

        public String GetName()
        {
            return this.name;
        }

        public string GetId()
        {
            return this.Id;
        }

        public void AddMember(StateObject newMember, string memberId, string alias)
        {
            newMember.ClientId = memberId;
            newMember.Alias = alias;
            chatMembers.Add(memberId, newMember);
        }

        public void RemoveMember()
        {

        }

        public Dictionary<string, StateObject> GetMembers()
        {
            return chatMembers;
        }

        public StateObject GetAdmin()
        {
            StateObject admin = new StateObject();
            foreach (var mem in chatMembers)
            {
                if(mem.Value.admin)
                {
                    admin = mem.Value;
                    break;
                }
            }
            return admin;
        }

        public bool HasAdmin()
        {
            foreach (var mem in chatMembers)
            {
                if (mem.Value.admin)
                    return true;
            }
            return false;
        }
    }
}
