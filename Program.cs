using System;
using System.Collections.Generic;
namespace ChessEngine 
{
    public class Program 
    {
        static void Main()
        {
            Board board = new Board("rn1qk2r/ppP2pbp/2p1pnp1/3p4/1PPP1B2/2N1PQ1P/P4PP1/R3KB1R");
            board.PrintBoard();
            PreComputeData.InitializeAttackBitboards();
            Move[] moves = MoveGenerator.GenerateKingMoves(board);
            int NumberOfMoves = 0;
            foreach (Move move in moves)
            {
                if (move.GetData() != 0)
                {
                    move.PrintMove();
                    NumberOfMoves++;
                }
            }
            Console.WriteLine(NumberOfMoves);
        }
    }
}