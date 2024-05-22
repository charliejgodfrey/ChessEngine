using System;
using System.Collections.Generic;
using System.Collections;
namespace ChessEngine 
{
    public class Program 
    {
        static void Main()
        {
            //setup stuff
            Board board = new Board();
            board.PrintBoard();
            PreComputeData.InitializeAttackBitboards();
            TranspositionTable TTable = new TranspositionTable();
            board.Eval = Evaluation.WeightedMaterial(board);

            while (true)
            {
                Move move = GetUserMove(board);
                board.MakeMove(move);
                move.PrintMove();
                board.PrintBoard();

                (Move BestMove, float Eval, Move[] PV) = Search.IterativeDeepeningSearch(board, 7, TTable);
                board.MakeMove(BestMove);
                board.PrintBoard();
                BestMove.PrintMove();
                Console.WriteLine("Computer Evaluation Assessment: " + (Eval/100));
            }
        }

        public static void PrintMoves(Board board)
        {
            Move[] moves = MoveGenerator.GenerateMoves(board);
            for (int i = 0; i < 218; i++)
            {
                if (moves[i].GetNullMove() == 1) break;
                moves[i].PrintMove();
            }
        }

        public static Move GetUserMove(Board board)
        {
            while (true)
            {
                int Start = Int32.Parse(Console.ReadLine());
                int Target = Int32.Parse(Console.ReadLine());
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