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
            //board = new Board(Test.FenDataBase[14]);
            board.PrintBoard();
            board.Eval = Evaluation.WeightedMaterial(board);
            //Test.ShowEvaluationScores();
            //Console.WriteLine(Search.Perft(6,board));

            //after program has been loaded

            while (1==1)
            {
                // Move move = GetUserMove(board);
                // board.MakeMove(move);
                // board.PrintBoard();
                
                (Move BestMove, float Eval, Move[] PV) = Search.IterativeDeepeningSearch(board.Copy(), 13, new TranspositionTable());
                board.MakeMove(BestMove);
                board.PrintBoard();
                Console.WriteLine("computer eval: " + Eval);
                Console.WriteLine("board: " + board.Eval + " actual: " + Evaluation.WeightedMaterial(board));
                //BestMove.PrintMove();
                Console.WriteLine(FormatMove(BestMove));
                break;

                // (BestMove, Eval, PV) = Search.IterativeDeepeningSearch(board, 10, TTable);
                // board.MakeMove(BestMove);
                // //board.PrintBoard();
                // //BestMove.PrintMove();
                // Console.WriteLine(FormatMove(BestMove));
                // // Console.WriteLine("Eval: " + (board.Eval/100));
                // // Console.WriteLine("Eval: " + (Evaluation.WeightedMaterial(board)/100));
                // board.RefreshBitboardConfiguration();
                //break;
            }
        }

        public static string FormatMove(Move move)
        {
            string[] Pieces = new string[12] {"", "kn", "b", "r", "q", "k", "", "Kn", "B", "R", "Q", "K"};
            string[] Files = new String[8] {"a", "b", "c", "d", "e", "f", "g", "h"};
            int start = move.GetStart();
            int target = move.GetTarget();
            bool capture = (move.GetCapture() != 7);
            int piece = move.GetPiece();
            string Formated = Pieces[piece] + (capture && piece == 0 ? Files[start % 8] : "");
            string File = Files[target % 8];
            int Rank = target / 8 + 1;
            return (Formated + (capture ? "x" : "") + File + Rank.ToString());
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