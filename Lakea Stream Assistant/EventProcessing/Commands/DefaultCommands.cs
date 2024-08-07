﻿using Lakea_Stream_Assistant.Enums;
using Lakea_Stream_Assistant.Models.Configuration;
using Lakea_Stream_Assistant.Models.Events;
using Lakea_Stream_Assistant.Models.Events.EventLists;
using Lakea_Stream_Assistant.Models.Tokens;
using Lakea_Stream_Assistant.Processes;
using Lakea_Stream_Assistant.Singletons;
using Lakea_Stream_Assistant.Static;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Helix.Models.Channels.ModifyChannelInformation;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.PubSub.Models.Responses;

namespace Lakea_Stream_Assistant.EventProcessing.Commands
{
    // This class handles default commands for Lakea
    public class DefaultCommands
    {
        private BitsCommand bits;
        private ProcessCommand process;
        private QuoteCommand quotes;
        private readonly Dictionary<string, Func<LakeaCommand, EventItem>> commandFunctions;
        private readonly Dictionary<string, CommandConfiguration> commandConfigs;
        KeepAliveToken keepAliveToken;

        #region Gets

        public BitsCommand BitsCommands { get { return bits; } }

        #endregion

        // Constructor takes object references, sets predefined dictionaries and command active/modonly status
        public DefaultCommands(ConfigSettings settings, ExternalProcesses externalProcesses, KeepAliveToken keepAliveToken)
        {
            this.bits = new BitsCommand(settings.ResourcePath);
            this.process = new ProcessCommand(externalProcesses);
            this.quotes = new QuoteCommand(settings.ResourcePath);
            this.keepAliveToken = keepAliveToken;
            this.commandFunctions = new Dictionary<string, Func<LakeaCommand, EventItem>>
            {
                { "category", categoryCommand },
                { "exit", exitCommand },
                { "process", processCommand },
                { "quote", quoteCommand },
                { "quotecount", quoteCommand },
                { "addquote", quoteCommand },
                { "quoteadd", quoteCommand },
                { "quotefest", quoteCommand },
                { "resetterminal", resetTerminalCommand },
                { "so", shoutOutCommand },
                { "status", statusCommand },
                { "title", titleCommand },
                { "totalbits", totalBitsCommand }
            };
            this.commandConfigs = new Dictionary<string, CommandConfiguration>
            {
                { "category", new CommandConfiguration("Category", settings.Commands.Category.Enabled, settings.Commands.Category.ModOnly) },
                { "exit", new CommandConfiguration("Exit", settings.Commands.Exit.Enabled, settings.Commands.Exit.ModOnly) },
                { "process", new CommandConfiguration("Process", settings.Commands.Process.Enabled, settings.Commands.Process.ModOnly) },
                { "quote", new CommandConfiguration("Quote", settings.Commands.Quotes.Enabled, settings.Commands.Quotes.ModOnly) },
                { "quotecount", new CommandConfiguration("QuoteCount", settings.Commands.Quotes.Enabled, settings.Commands.Quotes.ModOnly) },
                { "addquote", new CommandConfiguration("AddQuote", settings.Commands.Quotes.Enabled, settings.Commands.Quotes.ModOnly) },
                { "quoteadd", new CommandConfiguration("AddQuote", settings.Commands.Quotes.Enabled, settings.Commands.Quotes.ModOnly) },
                { "quotefest", new CommandConfiguration("QuoteFest", settings.Commands.Quotes.Enabled, settings.Commands.Quotes.ModOnly) },
                { "resetterminal", new CommandConfiguration("ResetTerminal", settings.Commands.ResetTerminal.Enabled, settings.Commands.ResetTerminal.ModOnly) },
                { "so", new CommandConfiguration("Shout Out", settings.Commands.ShoutOut.Enabled, settings.Commands.ShoutOut.ModOnly) },
                { "status", new CommandConfiguration("Status", settings.Commands.Status.Enabled, settings.Commands.Status.ModOnly) },
                { "title", new CommandConfiguration("Title", settings.Commands.Title.Enabled, settings.Commands.Status.ModOnly) },
                { "totalbits", new CommandConfiguration("TotalBits", settings.Commands.Status.Enabled, settings.Commands.Status.ModOnly) }
            };
        }

        // Checks if a command is a default command, if not it is a custom command
        public bool CheckIfCommandIsLakeaCommand(string command)
        {
            if (commandFunctions.ContainsKey(command.ToLower())) return true;
            else return false;
        }

        // Called when a Lakea command is received, checks if the command is enabled and modonly before call relevant function from commandFunctions dictionary
        public EventItem NewLakeaCommand(LakeaCommand eve)
        {
            try
            {
                string command = eve.Args.Command.CommandText.ToLower();
                if (commandConfigs[command].IsEnabled)
                {
                    if (commandConfigs[command].ModOnly)
                    {
                        if (eve.Args.Command.ChatMessage.IsModerator || eve.Args.Command.ChatMessage.IsBroadcaster)
                        {
                            return commandFunctions[command].Invoke(eve);
                        }
                        else
                        {
                            Terminal.Output("Lakea: " + eve.Args.Command.CommandText + " Command -> Access Denied, " + eve.Args.Command.ChatMessage.DisplayName);
                            Logs.Instance.NewLog(LogLevel.Warning, eve.Args.Command.CommandText + " Command -> Access Denied, " + eve.Args.Command.ChatMessage.DisplayName);
                            Dictionary<string, string> args = new Dictionary<string, string>
                            {
                                { "Message", "Sorry @" + eve.Args.Command.ChatMessage.DisplayName + ", only moderators can use that command!" }
                            };
                            return new EventItem(eve.Source, EventType.Lakea_Command, EventTarget.Twitch, EventGoal.Twitch_Send_Chat_Message, eve.Args.Command.CommandText + " Command", "Lakea_Command_Access_Denied", args: args);
                        }
                    }
                    else
                    {
                        return commandFunctions[command].Invoke(eve);
                    }
                }
                else
                {
                    Terminal.Output("Lakea: Default Command " + eve.Args.Command.CommandIdentifier + eve.Args.Command.CommandText + " is Disabled");
                    Logs.Instance.NewLog(LogLevel.Info, "Default Command " + eve.Args.Command.CommandIdentifier + eve.Args.Command.CommandText + " is Disabled");
                    return new EventItem(eve.Source, EventType.Lakea_Command, EventTarget.Null, EventGoal.Null, eve.Args.Command.CommandIdentifier + eve.Args.Command.CommandText);
                }
            }
            catch (Exception ex)
            {
                Terminal.Output("Lakea: Default Command Error -> " + ex.Message);
                Logs.Instance.NewLog(LogLevel.Error, ex);
            }
            return null;
        }

        // Calls on the Twitch API to update the stream category
        private EventItem categoryCommand(LakeaCommand eve)
        {
            Terminal.Output("Lakea: Update Stream Category Command -> Updating Stream Category");
            Logs.Instance.NewLog(LogLevel.Info, "Update Stream Category Command -> Updating Stream Category");
            Dictionary<string, string> args = new Dictionary<string, string>();
            if (eve.Args.Command.ArgumentsAsList.Count > 0)
            {
                GetGamesResponse response = Twitch.GetCategoryInformation(new List<string>() { eve.Args.Command.ArgumentsAsString }).Result;
                if(response.Games.Count() > 0)
                {
                    args.Add("Message", "On it, give me a moment!");
                    ModifyChannelInformationRequest request = new ModifyChannelInformationRequest();
                    request.GameId = response.Games[0].Id;
                    Twitch.UpdateChannelInformation(request);
                    return new EventItem(eve.Source, EventType.Lakea_Command, EventTarget.Twitch, EventGoal.Twitch_Send_Chat_Message, "Update Stream Category Command", "Lakea_Update_Stream_Category", args: args);
                }
                else
                {
                    args.Add("Message", "Twitch doesn't seem to have any categories of the name '" + eve.Args.Command.ArgumentsAsString + "', are you sure that's the right one?");
                }
            }
            else
            {
                GetChannelInformationResponse response = Twitch.GetChannelInformation().Result;
                args.Add("Message", "The current stream title is '" + response.Data[0].GameName + "', don't know why you didn't just look at it though!");
            }
            return new EventItem(eve.Source, EventType.Lakea_Command, EventTarget.Twitch, EventGoal.Twitch_Send_Chat_Message, "Update Stream Category Command", "Lakea_Update_Stream_Category", args: args);
        }

        // Checks if it was the broadcaster that called the command, then kills the keep alive token so the main thread can end the application
        private EventItem exitCommand(LakeaCommand eve)
        {
            Terminal.Output("Lakea: Exit Command -> " + eve.Args.Command.CommandText);
            Logs.Instance.NewLog(LogLevel.Info, "Exit Command -> " + eve.Args.Command.CommandText);
            if (eve.Args.Command.ChatMessage.IsBroadcaster)
            {
                keepAliveToken.Kill();
                return null;
            }
            else
            {
                Terminal.Output("Lakea: Exit Command -> Access Denied, " + eve.Args.Command.ChatMessage.DisplayName);
                Logs.Instance.NewLog(LogLevel.Warning, "Exit Command -> Access Denied, " + eve.Args.Command.ChatMessage.DisplayName);
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    { "Message", "Sorry @" + eve.Args.Command.ChatMessage.DisplayName + ", only @" + eve.Args.Command.ChatMessage.Channel + " can use that command!" }
                };
                return new EventItem(eve.Source, EventType.Lakea_Command, EventTarget.Twitch, EventGoal.Twitch_Send_Chat_Message, "Exit Command", "Lakea_Exit_Command", args: args);
            }
        }

        // Calls the Processes object to process the command before returning the output in a new EventItem object
        private EventItem processCommand(LakeaCommand eve)
        {
            Terminal.Output("Lakea: Process Command -> " + eve.Args.Command.ArgumentsAsString);
            Logs.Instance.NewLog(LogLevel.Info, "Process Command -> " + eve.Args.Command.ArgumentsAsString);
            Dictionary<string, string> args = process.NewProcessCommand(eve);
            return new EventItem(eve.Source, EventType.Lakea_Command, EventTarget.Twitch, EventGoal.Twitch_Send_Chat_Message, "Process Command", "Lakea_Process_Command", args: args);
        }

        // Calls on the Quotes object to process the quote command before returning the output in a new EventItem object
        private EventItem quoteCommand(LakeaCommand eve)
        {
            Terminal.Output("Lakea: Quote Command -> " + eve.Args.Command.CommandText);
            Logs.Instance.NewLog(LogLevel.Info, "Quote Command -> " + eve.Args.Command.CommandText);
            Dictionary<string, string> args = quotes.NewQuoteCommand(eve);
            if ("quotefest".Equals(eve.Args.Command.CommandText))
            {
                return new EventItem(eve.Source, EventType.Lakea_Command, EventTarget.Twitch, EventGoal.Twitch_Send_Chat_Message_List, "Quote Command", "Lakea_Quote_Command", args: args);
            }
            else
            {
                return new EventItem(eve.Source, EventType.Lakea_Command, EventTarget.Twitch, EventGoal.Twitch_Send_Chat_Message, "Quote Command", "Lakea_Quote_Command", args: args);
            }
        }

        // Resets the terminal with a full refresh on a new thread
        private EventItem resetTerminalCommand(LakeaCommand eve)
        {
            Terminal.Output("Lakea: Reset Terminal Command -> Resetting Terminal");
            Logs.Instance.NewLog(LogLevel.Info, "Reset Terminal Command -> Resetting Terminal");
            Dictionary<string, string> args = new Dictionary<string, string>
            {
                { "Message", "On it, give me a moment!" }
            };
            Terminal.ResetTerminal();
            return new EventItem(eve.Source, EventType.Lakea_Command, EventTarget.Twitch, EventGoal.Twitch_Send_Chat_Message, "Reset Terminal Command", "Lakea_Reset_Terminal_Command", args: args);
        }

        // Returns a new EvenItem object that sends a shoutout message for the entered username to the Twitch chat
        private EventItem shoutOutCommand(LakeaCommand eve)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            Terminal.Output("Lakea: Shout Out Command -> " + eve.Args.Command.ArgumentsAsString);
            if(eve.Args.Command.ArgumentsAsList.Count > 0)
            {
                args.Add("Message", "Hey guys, go give @" + eve.Args.Command.ArgumentsAsList[0] + " some love and support! You can find them at https://www.twitch.tv/" + eve.Args.Command.ArgumentsAsList[0]);
            }
            else
            {
                Terminal.Output("Lakea: Shout Out Command -> No User Name given to Shout Out");
                Logs.Instance.NewLog(LogLevel.Warning, "Shout Out Command -> No User Given");
                args.Add("Message", "You didn't tell me who to shout out @{DisplayName}! Who am I shouting out?");
            }
            return new EventItem(eve.Source, EventType.Lakea_Command, EventTarget.Twitch, EventGoal.Twitch_Send_Chat_Message, "Shout Out Command", "Lakea_Shout_Out_Command", args: args);
        }

        // Returns a new EventItem that sends a message to the Twitchchat to confirm Lakea is still active
        private EventItem statusCommand(LakeaCommand eve)
        {
            Terminal.Output("Lakea: Status Command -> Active");
            Logs.Instance.NewLog(LogLevel.Info, "Status Command -> Active");
            Dictionary<string, string> args = new Dictionary<string, string>
            {
                { "Message", "I'm still active, all is well Cuppa" }
            };
            return new EventItem(eve.Source, EventType.Lakea_Command, EventTarget.Twitch, EventGoal.Twitch_Send_Chat_Message, "Status Command", "Lakea_Status_Command", args: args);
        }

        // Calls on the Twitch API to update the stream title
        private EventItem titleCommand(LakeaCommand eve)
        {
            Terminal.Output("Lakea: Update Stream Title Command -> Updating Stream Title");
            Logs.Instance.NewLog(LogLevel.Info, "Update Stream Title Command -> Updating Stream Title");
            Dictionary<string, string> args = new Dictionary<string, string>();
            if(eve.Args.Command.ArgumentsAsList.Count > 0)
            {  
                args.Add("Message", "On it, give me a moment!");
                ModifyChannelInformationRequest request = new ModifyChannelInformationRequest();
                request.Title = eve.Args.Command.ArgumentsAsString;
                Twitch.UpdateChannelInformation(request);
                return new EventItem(eve.Source, EventType.Lakea_Command, EventTarget.Twitch, EventGoal.Twitch_Send_Chat_Message, "Update Stream Title Command", "Lakea_Update_Stream_Title", args: args);
            }
            else
            {
                GetChannelInformationResponse response = Twitch.GetChannelInformation().Result;
                args.Add("Message", "The current stream title is '" + response.Data[0].Title + "', don't know why you didn't just look at it though!");
            }
            return new EventItem(eve.Source, EventType.Lakea_Command, EventTarget.Twitch, EventGoal.Twitch_Send_Chat_Message, "Update Stream Title Command", "Lakea_Update_Stream_Title", args: args);
        }

        // Returns a new EventItem that sends a message to Twitch chat with the users total bits cheered
        private EventItem totalBitsCommand(LakeaCommand eve)
        {
            Terminal.Output("Lakea: Total Bits Command -> " + eve.Args.Command.ChatMessage.DisplayName);
            Logs.Instance.NewLog(LogLevel.Info, "Total Bits Command -> " + eve.Args.Command.ChatMessage.DisplayName);
            Dictionary<string, string> args = bits.NewTotalBitsCommand(eve);
            return new EventItem(eve.Source, EventType.Lakea_Command, EventTarget.Twitch, EventGoal.Twitch_Send_Chat_Message, "Total Bits Command", "Lakea_Total_Bits_Command", args: args);
        }
    }
}
