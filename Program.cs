using System;
using System.Collections.Generic;
using System.Collections;
using System.Numerics;
namespace ChessEngine 
{
    public class Program 
    {
        public static int turn = 0;
        static void Main()
        {
            //setup stuff
            //Console.WriteLine(Magic.FindMagic(27, PreComputeData.BishopMasks, false));
            Board board = new Board();
            PreComputeData.InitializeAttackBitboards();
            TranspositionTable TTable = new TranspositionTable();
            Evaluation.InitializeKillerMoves();
            board.Eval = Evaluation.WeightedMaterial(board);
            Test.LoadTestPositions();
            board = new Board(Test.FenDataBase[16]);
            board.PrintBoard();
            //board.Zobrist = board.Hasher.Hash(board);
            board.Eval = Evaluation.Evaluate(board);
            //for (int i = 0; i < 7; i++) Console.WriteLine(Search.Perft(i,board));

            //after program has been loaded

            while (1==1)
            {
                (Move BestMove, float Eval, Move[] PV) = Search.IterativeDeepeningSearch(board, 12, TTable);
                board.MakeMove(BestMove);
                Console.WriteLine(FormatMove(BestMove));
                board.PrintBoard();
                Console.WriteLine("tranposition table usage: " + (TTable.PercentageFull()) + " out of " + (1UL << 20));

                // Move move = GetUserMove(board);
                // board.MakeMove(move);
                // board.PrintBoard();



                // Console.WriteLine("tranpositions: " + Search.Transpositions);
               
                // Move[] moves = new Move[3];
                // moves[0] = move;
                // Console.WriteLine(board.Zobrist);


                // move = GetUserMove(board);
                // board.MakeMove(move);
                // board.PrintBoard();
                // moves[1] = move;
                // Console.WriteLine(board.Zobrist);



                // move = GetUserMove(board);
                // board.MakeMove(move);
                // board.PrintBoard();
                // moves[2] = move;
                // Console.WriteLine(board.Zobrist);

                // board.UnmakeMove(moves[2]);
                // board.UnmakeMove(moves[1]);
                // board.UnmakeMove(moves[0]);
                // Console.WriteLine(board.Zobrist);

                // move = GetUserMove(board);
                // board.MakeMove(move);
                // board.PrintBoard();
                // moves[0] = move;
                // Console.WriteLine(board.Zobrist);


                // move = GetUserMove(board);
                // board.MakeMove(move);
                // board.PrintBoard();
                // moves[1] = move;
                // Console.WriteLine(board.Zobrist);



                // move = GetUserMove(board);
                // board.MakeMove(move);
                // board.PrintBoard();
                // moves[2] = move;
                // Console.WriteLine(board.Zobrist);

                // board.UnmakeMove(moves[2]);
                // board.UnmakeMove(moves[1]);
                // board.UnmakeMove(moves[0]);
                // Console.WriteLine(board.Zobrist);
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