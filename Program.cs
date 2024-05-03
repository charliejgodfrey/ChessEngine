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
            // for (int i = 1; i < 5; i++) {
            //     Console.WriteLine("depth " + i + ": " + Search.perf(i, board.Copy()));
            // }
            Move[] moves = MoveGenerator.GenerateMoves(board);

            for (int i = 0; i < 20; i++) //something fishy going on here
            {
                Move[] movey = MoveGenerator.GenerateMoves(board);
                board.MakeMove(moves[0 * DateTime.Now.Ticks % moves.Length]);
                board.PrintBoard();
                Console.WriteLine("pos num: " + i);
            }
            //int count = 0;
            // for (int i = 0; i < 218; i++)
            // {
            //     if (moves[i].GetData() == 0) {break;}
            //     if (MoveGenerator.CheckLegal(board, moves[i]) == false) continue;
            //     count++;
            //     Console.WriteLine("move number " + count);
            //     moves[i].PrintMove();
            // }
        }
    }
}