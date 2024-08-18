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
            PreComputeData.InitializeAttackBitboards();
            TranspositionTable TTable = new TranspositionTable();
            Evaluation.InitializeKillerMoves();
            board.Eval = Evaluation.WeightedMaterial(board);
            Test.LoadTestPositions();
            board = new Board(Test.TestPositions[3].Fen);
            Console.WriteLine(MoveGenerator.UnderAttack(board, 16));

            Test.PerftTest(4, Test.TestPositions[3]);

            //after program has been loaded
            board.PrintBoard();

            while (1==2)
            {
                (Move BestMove, float Eval, Move[] PV) = Search.IterativeDeepeningSearch(board, 7, TTable);
                board.MakeMove(BestMove);
                //board.PrintBoard();
                BestMove.PrintMove();
                Console.WriteLine("Computer Evaluation Assessment: " + (Eval/100));

                // Move move = GetUserMove(board);
                // board.MakeMove(move);
                // move.PrintMove();
                // board.PrintBoard();
                //break;
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