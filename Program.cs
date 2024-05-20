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
            board.Eval = Evaluation.WeightedMaterial(board);
            Console.WriteLine(board.Zobrist);
            // Console.WriteLine("perft 3: " + Search.Perft(3, board));
            // Console.WriteLine("perft 4: " + Search.Perft(4, board));
            // Console.WriteLine("perft 5: " + Search.Perft(3, board));
            while (true)
            {
                Move move = GetUserMove(board);
                //PrintMoves(board);
                board.MakeMove(move);
                move.PrintMove();
                board.PrintBoard();
                Console.WriteLine("Updating Eval: " + board.Eval);
                Console.WriteLine("Weighted Material: " + Evaluation.WeightedMaterial(board));
                (Move BestMove, float Eval) = Search.AlphaBeta(board, 5);
                board.MakeMove(BestMove);
                board.PrintBoard();
                BestMove.PrintMove();
                Console.WriteLine("Evaluation: " + Eval);
                Console.WriteLine("Weighted Material: " + Evaluation.WeightedMaterial(board));
                Console.WriteLine("Updating Eval: " + board.Eval);
                Console.WriteLine("Updating: " + board.Zobrist);
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