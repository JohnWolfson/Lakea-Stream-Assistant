﻿using Lakea_Stream_Assistant.Exceptions;
using System.Diagnostics;

namespace Lakea_Stream_Assistant.Enums
{
    //This class converts string to their respective enum types
    public class EnumConverter
    {

        #region Event Enums

        //Returns 'EventSource' type from string
        public EventSource ConvertEventSourceString(string source)
        {
            source = prepareString(source);
            switch (source)
            {
                case "basecamp": return EventSource.Base_Camp;
                case "twitch": return EventSource.Twitch;
                case "lakea": return EventSource.Lakea;
                default: throw new EnumConversionException("Can not convert '" + source + "' to type 'EventSource'");
            }
        }

        //Returns 'EventType' type from string
        public EventType ConvertEventTypeString(string source)
        {
            source = prepareString(source);
            switch (source)
            {
                case "twitchbits": return EventType.Twitch_Bits;
                case "twitchcommand": return EventType.Twitch_Command;
                case "twitchfollow": return EventType.Twitch_Follow;
                case "twitchraid": return EventType.Twitch_Raid;
                case "twitchredeem": return EventType.Twitch_Redeem;
                case "twitchsubscription": return EventType.Twitch_Subscription;
                case "lakeacallback": return EventType.Lakea_Callback;
                case "lakeatimer": return EventType.Lakea_Timer_Start;
                default: throw new EnumConversionException("Can not convert '" + source + "' to type 'EventType'");
            }
        }

        //Returns 'EventTarget' type from string
        public EventTarget ConvertEventTargetString(string source)
        {
            source = prepareString(source);
            switch (source)
            {
                case "null": return EventTarget.Null;
                case "basecamp": return EventTarget.Base_Camp;
                case "twitch": return EventTarget.Twitch;
                case "obs": return EventTarget.OBS;
                default: throw new EnumConversionException("Can not convert '" + source + "' to type 'EventTarget'");
            }
        }

        //Returns 'EventGoal' type from string
        public EventGoal ConvertEventGoalString(string source)
        {
            source = prepareString(source);
            switch (source)
            {
                case "null": return EventGoal.Null;
                case "obsenablesource": return EventGoal.OBS_Enable_Source;
                case "obsdisablesource": return EventGoal.OBS_Disable_Source;
                case "obsenablerandomsource": return EventGoal.OBS_Enable_Random_Source;
                case "obsdisablerandomsource": return EventGoal.OBS_Disable_Random_Source;
                case "obsloopsources": return EventGoal.OBS_Loop_Sources;
                case "obschangescene": return EventGoal.OBS_Change_Scene;
                case "twitchsendchatmessage": return EventGoal.Twitch_Send_Chat_Message;
                case "twitchsendwhispermessage": return EventGoal.Twitch_Send_Whisper_Message;
                default: throw new EnumConversionException("Can not convert '" + source + "' to type 'EventGoal'");
            }
        }

        #endregion

        #region Lakea Enums

        //Returns 'LogLevel' type from string
        public LogLevel ConvertLogLevelString(string source)
        {
            source = prepareString(source);
            switch (source)
            {
                case "info": return LogLevel.Info;
                case "warning": return LogLevel.Warning;
                case "error": return LogLevel.Error;
                case "fatal": return LogLevel.Fatal;
                default: throw new EnumConversionException("Can not convert '" + source + "' to type 'LogLevel'");
            }
        }

        #endregion

        #region System Enums

        public ProcessWindowStyle ConvertWindowStyleString(string source)
        {
            source = prepareString(source);
            switch (source)
            {
                case "normal": return ProcessWindowStyle.Normal;
                case "hidden": return ProcessWindowStyle.Hidden;
                case "minimised": return ProcessWindowStyle.Minimized;
                case "maximised": return ProcessWindowStyle.Maximized;
                default: throw new EnumConversionException("Can not convert '" + source + "' to type 'LogLevel'");
            }
        }

        #endregion

        //Cuts source string down to minimise chance of user error
        private string prepareString(string source)
        {
            source = source.ToLower();
            source = source.Trim();
            source = source.Replace(" ", "");
            source = source.Replace("_", "");
            return source;
        }
    }
}
