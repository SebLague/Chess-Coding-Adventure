namespace CodingAdventureBot;
using Chess.Core;
using System;
using System.Collections.Generic;

public static class Program
{
    public static void Main(string[] args)
    {
        EngineUCI engine = new();

        string command = String.Empty;
        while (command != "quit")
        {
            command = Console.ReadLine();
            engine.ReceiveCommand(command);
        }

    }

}