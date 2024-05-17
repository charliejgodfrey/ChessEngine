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

            while (true)
            {
                Move move = GetUserMove(board);
                board.MakeMove(move);
                board.PrintBoard();
                (Move BestMove, float Eval) = Search.AlphaBeta(board, 6);
                board.MakeMove(BestMove);
                board.PrintBoard();

                //BestMove = new Move(0,0,0,0,0,1);
                //move = new Move(0,0,0,0,0,1);
            }
        }

        public static Move GetUserMove(Board board)
        {
            while (true)
            {
                int Start = Int32.Parse(Console.ReadLine());
                Console.WriteLine(Start);
                int Target = Int32.Parse(Console.ReadLine());
                Console.WriteLine(Target);
                Move[] Moves = MoveGenerator.GenerateMoves(board);
                for (int i = 0; i < 218; i++)
                {
                    if (Moves[i].GetStart() == Start && Moves[i].GetTarget() == Target)
                    {
                        return Moves[i];
                    }
                }
            }
        }
    }
}