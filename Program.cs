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
            board = new Board();
            board.PrintBoard();
            board.Eval = Evaluation.WeightedMaterial(board);

            //after program has been loaded

            while (1==1)
            {

                // Move move = GetUserMove(board);
                // board.MakeMove(move);
                // board.PrintBoard();

                //break;
                //Evaluation.HistoryTable = new int[64, 64, 16];

                (Move BestMove, float Eval, Move[] PV) = Search.IterativeDeepeningSearch(board, 9, TTable);
                board.MakeMove(BestMove);
                board.PrintBoard();
                BestMove.PrintMove();
                Console.WriteLine("Computer Evaluation Assessment: " + (Eval/100));
                break;
              //  break;
                // Console.WriteLine("eval percentage from positional: " + (Evaluation.avgPositional / (Evaluation.avgPositional + Evaluation.avgMaterial)));
                // Console.WriteLine("avergae difference: " + (Evaluation.avgDiffer/Evaluation.count / 100));
                // Evaluation.avgPositional = 0;
                // Evaluation.avgMaterial = 0;
                // //break;
                // Evaluation.HistoryTable = new int[64, 64, 16];
                //board.RefreshBitboardConfiguration();
            }
        }

        public static void PrintMoves(Board board)
        {
            Move[] moves = MoveGenerator.GenerateMoves(board);
            for (int i = 0; i < 218; i++)
            {
                if (moves[i].GetData() == 0) continue;
                if (!MoveGenerator.CheckLegal(board, moves[i])) continue;
                Console.WriteLine(i + ".");
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
                    if (Moves[i].GetFlag() == 0b0010 && Start == -1) return Moves[i];
                    if (Moves[i].GetFlag() == 0b0011 && Start == -2) return Moves[i];
                }
            }
        }
    }
}