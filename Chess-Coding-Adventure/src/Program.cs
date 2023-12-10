namespace CodingAdventureBot;
using Chess.Core;
using System;
using System.Collections.Generic;

public static class Program
{
    public static void Main(string[] args)
    {
        EngineUCI engine = new();

        GenOpenings();


        string command = String.Empty;
        while (command != "quit")
        {
            command = Console.ReadLine();
            engine.ReceiveCommand(command);
        }

    }

    static void GenOpenings()
    {
        var book = new OpeningBook(Chess_Coding_Adventure.Properties.Resources.Book);
        HashSet<string> used = new();
        List<Opening> openings = new();
        Board board = new Board();

        const int numPly = 2 * 8;

        while(openings.Count < 512)
        {
            TryAddOpening();
        }
        Console.WriteLine(openings.Count);
        string data = System.Text.Json.JsonSerializer.Serialize(openings.ToArray());
        Console.WriteLine(data);

        void TryAddOpening()
        {
            board.LoadStartPosition();
            List<string> moveList = new();

            for (int i = 0; i < numPly; i++)
            {
                if (book.TryGetBookMove(board, out string moveString))
                {
                    var move = MoveUtility.GetMoveFromUCIName(moveString, board);
                    moveList.Add(moveString);
                    board.MakeMove(move);
                }
                else
                {
                    return;
                }

            }

            string fen = FenUtility.CurrentFen(board).Split(" ")[0];
            if (!used.Contains(fen))
            {
                used.Add(fen);
                openings.Add(new Opening() { moves = moveList.ToArray() });
            }


        }

    }

    public struct Opening
    {
        public string[] moves { get; set; }
    }



}