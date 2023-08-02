﻿using Lakea_Stream_Assistant.Models.Configuration;
using Lakea_Stream_Assistant.Models.Events;
using Lakea_Stream_Assistant.Singletons;

namespace Lakea_Stream_Assistant
{
    class Program
    {
        //Point of Entry
        static void Main(string[] args)
        {
            Console.WriteLine("Lakea is waking up...");
            Config config = new LoadConfig().LoadConfigFromFile();
            HandleEvents eventHandler = new HandleEvents(config.Events);
            OBS.Init(config);
            while (!OBS.Initiliased)
            {
                Thread.Sleep(100);
            }
            Twitch.Init(config, eventHandler);
            while (Twitch.ServicesConnected.Item1 < Twitch.ServicesConnected.Item2)
            {
                Thread.Sleep(100);
            }
            Console.WriteLine("Lakea: All set and ready to go!");

            //Pauses main thread to prevent application terminating
            Console.ReadLine();
        }
    }
}