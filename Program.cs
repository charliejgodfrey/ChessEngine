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
            board = new Board(Test.TestPositions[1].Fen);
            board.Eval = Evaluation.WeightedMaterial(board);
            Console.WriteLine(MoveGenerator.UnderAttack(board, 58));
            Test.PerftTest(3, Test.TestPositions[1]);
            //Test.PerftTest(4, Test.TestPositions[3]);

            //after program has been loaded
            // Bitboard test = new Bitboard(~0x6UL);
            // test.PrintData();
            // board.PrintBoard();
            //PrintMoves(board);

            while (1==1)
            {
                Console.WriteLine(board.Eval);
                Console.WriteLine(Evaluation.WeightedMaterial(board));
                (Move BestMove, float Eval, Move[] PV) = Search.IterativeDeepeningSearch(board, 7, TTable);
                board.MakeMove(BestMove);
                board.PrintBoard();
                BestMove.PrintMove();
                Console.WriteLine("Computer Evaluation Assessment: " + (Eval/100));
                // Move move = GetUserMove(board);
                // board.MakeMove(move);
                // move.PrintMove();
                // board.PrintBoard();
                //PrintMoves(board);
                //break;
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
            board.WhitePieces.PrintData();
            board.OccupiedSquares.PrintData();
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