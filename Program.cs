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
            // Console.WriteLine("perft 3: " + Search.Perft(3, board));
            // Console.WriteLine("perft 4: " + Search.Perft(4, board));
            // Console.WriteLine("perft 5: " + Search.Perft(3, board));
            while (true)
            {
                (Move BestMove, float Eval, Move[] PV) = Search.IterativeDeepeningSearch(board, 6, TTable);
                board.MakeMove(BestMove);
                board.PrintBoard();
                BestMove.PrintMove();

                Move move = GetUserMove(board);
                //PrintMoves(board);
                board.MakeMove(move);
                move.PrintMove();
                board.PrintBoard();

            }
        }

        public static void PrintMoves(Board board)
        {
            Move[] moves = MoveGenerator.GenerateMoves(board);
            for (int i = 0; i < 218; i++)
            {
                if (moves[i].GetData() == 0) break;
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