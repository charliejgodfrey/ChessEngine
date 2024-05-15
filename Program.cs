using System;
using System.Collections.Generic;
using System.Collections;
namespace ChessEngine 
{
    public class Program 
    {
        static void Main()
        {
            Board board = new Board("r2k3r/8/8/8/8/8/8/R3K2R");
            board.PrintBoard();
            PreComputeData.InitializeAttackBitboards();
            for (int i = 1; i < 4; i++) {
                Console.WriteLine("depth " + i + ": " + Search.perf(i, board.Copy()));
            }
            Move[] moves = MoveGenerator.GenerateMoves(board);
            board.MakeMove(moves[23]);
            board.PrintBoard();

            int count = 0;
            for (int i = 0; i < 218; i++)
            {
                if (moves[i].GetData() == 0) {break;}
                if (MoveGenerator.CheckLegal(board, moves[i]) == false) continue;
                count++;
                Console.WriteLine("move number " + count + ": ");
                moves[i].PrintMove();
            }
        }
    }
}