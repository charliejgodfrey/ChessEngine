using System;
using System.Collections.Generic;
using System.Collections;
using System.Numerics;
namespace ChessEngine 
{
    public class Program 
    {
        public static int turn = 0;
        public static int MoveNumber = 0;
        static void Main()
        {
            TranspositionTable TTable = new TranspositionTable();
            Board board = new Board();
            //board = new Board("rnbqkbnr/ppp1pppp/8/3P4/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 2");
            PreComputeData.InitializeAttackBitboards();
            Evaluation.InitializeKillerMoves();
            board.Eval = Evaluation.Material(board);
            Console.WriteLine(board.Eval);
            board.PrintBoard();
            Test.LoadFenDataBase();
            Test.EvaluationTest(1000, 0);

            // board = new Board("2r4k/p4b2/4pq2/1p1p1nR1/5P2/P2B4/1P2Q2P/1K4R1 b - - 2 30");
            // board.PrintBoard();
            // board.Zobrist = board.Hasher.Hash(board);
            // board.Eval = Evaluation.Evaluate(board);
            //Console.WriteLine(board.Eval);
            //for (int i = 0; i < 5; i++) Console.WriteLine(Search.Perft(i,board));

            //after program has been loaded
            //PrintMoves(board);
            

            while (true==false)
            {
                (Move BestMove, float Eval, Move[] PV, float[] evals) = Search.IterativeDeepeningSearch(board, 8, new TranspositionTable());
                board.MakeMove(BestMove);
                MoveNumber++;
                Console.WriteLine(MoveNumber + ". " + FormatMove(BestMove) + " -- Evaluation: " + ((Math.Abs(Math.Abs(-1000000/Eval) - Math.Floor(Math.Abs(-1000000/Eval))) < 0.01) ? ("Mate In: " + (Math.Floor(Math.Abs(-1000000/Eval)))) : Eval));
                board.PrintBoard();

                (bool checkmate, bool stalemate) = board.IsCheckMate();
                if (checkmate) 
                {
                    Console.WriteLine("Checkmate!");
                    break;
                }
                if (stalemate) 
                {
                    Console.WriteLine("Stalemate!");
                    break;
                }
                Search.flippy *= -1;

                (BestMove, Eval, PV, evals) = Search.IterativeDeepeningSearch(board, 6, new TranspositionTable());
                board.MakeMove(BestMove);
                MoveNumber++;
                Console.WriteLine(MoveNumber + ". " + FormatMove(BestMove) + " -- Evaluation: " + ((Math.Abs(Math.Abs(-1000000/Eval) - Math.Floor(Math.Abs(-1000000/Eval))) < 0.01) ? ("Mate In: " + (Math.Floor(Math.Abs(-1000000/Eval)))) : Eval));
                board.PrintBoard();

                (checkmate, stalemate) = board.IsCheckMate();
                if (checkmate) 
                {
                    Console.WriteLine("Checkmate!");
                    break;
                }
                if (stalemate) 
                {
                    Console.WriteLine("Stalemate!");
                    break;
                }
                Search.flippy *= -1;

                

                // Move move = GetUserMove(board);
                // board.MakeMove(move);
                // board.PrintBoard();
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
            (bool Check, Move[] moves) = MoveGenerator.GenerateMoves(board);
            for (int i = 0; i < 218; i++)
            {
                if (moves[i].GetData() == 0) continue;
                //if (!MoveGenerator.CheckLegal(board, moves[i])) continue;
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
                (bool Check, Move[] Moves) = MoveGenerator.GenerateMoves(board);
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