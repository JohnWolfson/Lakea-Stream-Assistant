﻿using Lakea_Stream_Assistant.Models.Events.EventAbstracts;
using Lakea_Stream_Assistant.Enums;
using Lakea_Stream_Assistant.Models.Events.EventLists;

namespace Lakea_Stream_Assistant.Models.Events
{
    public class LakeaCallback : Event
    {
        private IDictionary<string, string> args;
        private string callbackID;

        public LakeaCallback(EventSource source, EventType type, string callbackID, IDictionary<string, string> args) 
        {
            this.source = source;
            this.type = type;
            this.callbackID = callbackID;
            this.args = args;
        }
        public override EventSource Source { get { return source; } }
        public override EventType Type { get { return type; } }
        public string CallbackID { get { return callbackID; } }
        public IDictionary<string, string> Args { get { return args; } }

        public string[] GetCallbackArguments(EventItem item)
        {
            string[] newArgs = new string[0];
            switch (item.EventGoal)
            {
                case EventGoal.Twitch_Send_Chat_Message:
                    newArgs = callbackArgsForTwitchChatMessage(item);
                    break;
            }
            return newArgs;
        }

        private string[] callbackArgsForTwitchChatMessage(EventItem item)
        {
            int index = Int32.Parse(args["sourceNumber"]);
            return new string[] { item.Args[index] };
        }
    }
}