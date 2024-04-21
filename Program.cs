using System;
using System.Collections.Generic;
using System.Collections;
namespace ChessEngine 
{
    public class Program 
    {
        static void Main()
        {
            Board board = new Board();
            board.PrintBoard();
            PreComputeData.InitializeAttackBitboards();
            for (int i = 1; i < 5; i++) {
                Console.WriteLine("depth " + i + ": " + Search.perf(i, board.Copy()));
            }
            // Move[] moves = MoveGenerator.GenerateMoves(board);
            // for (int i = 0; i < 218; i++)
            // {
            //     if (moves[i].GetData() == 0) {break;}
            //     moves[i].PrintMove();
            //     Console.WriteLine("move number " + i);
            // }
        }
    }
}