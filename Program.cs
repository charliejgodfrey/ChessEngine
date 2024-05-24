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
            Board board = new Board("r1b1k2r/pppp1ppp/2n1pn2/8/3P1q2/P4N2/1PPBQPPP/R3KB1R");
            board.PrintBoard();
            PreComputeData.InitializeAttackBitboards();
            TranspositionTable TTable = new TranspositionTable();
            Evaluation.InitializeKillerMoves();
            board.Eval = Evaluation.WeightedMaterial(board);
            for (int i = 1; i < 6; i++)
            {
                //Console.WriteLine("perft: " + i + " result: " + Search.Perft(i, board));
            }

            while (true)
            {
                (Move BestMove, float Eval, Move[] PV) = Search.IterativeDeepeningSearch(board, 7, TTable);
                Console.WriteLine("eval: " + board.Eval);
                Console.WriteLine("correct eval: " + Evaluation.WeightedMaterial(board));
                board.MakeMove(BestMove);
                board.PrintBoard();
                BestMove.PrintMove();
                Console.WriteLine("eval: " + board.Eval);
                Console.WriteLine("correct eval: " + Evaluation.WeightedMaterial(board));
                Console.WriteLine("Computer Evaluation Assessment: " + (Eval/100));

                Move move = GetUserMove(board);
                board.MakeMove(move);
                move.PrintMove();
                board.PrintBoard();
                Console.WriteLine("eval: " + board.Eval);
                Console.WriteLine("correct eval: " + Evaluation.WeightedMaterial(board));
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