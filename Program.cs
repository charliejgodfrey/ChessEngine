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
            board = new Board("r1bq1b1r/ppppp1kp/2n5/5p1Q/2PPPp2/4P3/PP4PP/RN2KBNR");
            board.PrintBoard();
            board.Eval = Evaluation.WeightedMaterial(board);
            Console.WriteLine(MoveGenerator.UnderAttack(board, 58));
            //Test.PerftTest(6, Test.TestPositions[0]);
            // Test.PerftTest(4, Test.TestPositions[1]);
            // Test.PerftTest(4, Test.TestPositions[2]);
            // Test.PerftTest(4, Test.TestPositions[3]);
            // Test.PerftTest(4, Test.TestPositions[4]);

            //after program has been loaded
            // Bitboard test = new Bitboard(PreComputeData.KingAdjacents[35]);
            // test.PrintData();
            // board.PrintBoard();
            //PrintMoves(board);

            while (1==1)
            {
                Console.WriteLine("board eval: " + board.Eval);
                Move move = GetUserMove(board);
                board.MakeMove(move);
                move.PrintMove();
                board.PrintBoard();
                board.BlackKing.PrintData();

                (Move BestMove, float Eval, Move[] PV) = Search.IterativeDeepeningSearch(board, 7, TTable);
                board.MakeMove(BestMove);
                board.PrintBoard();
                BestMove.PrintMove();
                Console.WriteLine("Computer Evaluation Assessment: " + (Eval/100));
                //Evaluation.HistoryTable = new int[64, 64, 16];

                // (BestMove, Eval, PV) = Search.IterativeDeepeningSearch(board, 7, TTable);
                // board.MakeMove(BestMove);
                // board.PrintBoard();
                // BestMove.PrintMove();
                // Console.WriteLine("Computer Evaluation Assessment: " + (Eval/100));
                // //break;
                // Evaluation.HistoryTable = new int[64, 64, 16];
                board.RefreshBitboardConfiguration();
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