using System;
using System.Collections.Generic;
namespace ChessEngine 
{
    public class Program 
    {
        static void Main()
        {
            Board board = new Board("8/8/8/8/3PP3/1P6/P1P2PPP/8");
            board.PrintBoard();
            PreComputeData.InitializeAttackBitboards();
            List<Move> moves = MoveGenerator.GeneratePawnMoves(board);
            foreach (Move move in moves)
            {
                move.PrintMove();
            }
            Console.WriteLine(moves.Count);
            board.WhitePawns.PrintData();
        }
    }
}