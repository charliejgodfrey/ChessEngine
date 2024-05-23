using System;
using System.Collections.Generic;
using System.Numerics;

namespace ChessEngine 
{
    public static class Evaluation 
    {
        public static float[] MaterialValues = [100,320,330,500,900,10000,-100,-320,-330,-500,-900,-10000];
        public static PieceTable PawnEvaluationTable = new PieceTable();
        public static int[,,] HistoryTable = new int[64, 64, 16];

        public static float Evaluate(Board board)
        {
            float WhitePawns = PawnEvaluationTable.Retrieve(board.WhitePawns.GetData());
            float BlackPawns = PawnEvaluationTable.Retrieve(board.BlackPawns.GetData());
            if (WhitePawns == -1000000) //not in the table
            {
                WhitePawns = EvaluatePawnStructure(board.WhitePawns, 0);
                PawnEvaluationTable.Store(board.WhitePawns.GetData(), WhitePawns);
            }
            if (BlackPawns == -1000000) //not in the table
            {
                BlackPawns = EvaluatePawnStructure(board.BlackPawns, 1);
                PawnEvaluationTable.Store(board.BlackPawns.GetData(), BlackPawns);
            }
            return board.Eval + WhitePawns - BlackPawns;
        }

        public static float EvaluatePawnStructure(Bitboard Pawns, int Player)
        {
            return 0;
        }

        public static int PawnChain(Bitboard Pawns, int Player)
        {
            return 0;
        }

        public static int PawnMajorities(Bitboard WhitePawns, Bitboard BlackPawns, int Player)
        {
            Bitboard LeftWhite = new Bitboard(WhitePawns.GetData() & 0x0F0F0F0F0F0F0F0F);
            Bitboard LeftBlack = new Bitboard(BlackPawns.GetData() & 0x0F0F0F0F0F0F0F0F);
            Bitboard RightWhite = new Bitboard(WhitePawns.GetData() & 0xF0F0F0F0F0F0F0F0);
            Bitboard RightBlack = new Bitboard(BlackPawns.GetData() & 0xF0F0F0F0F0F0F0F0);
            int LeftMajority = LeftWhite.ActiveBits() - LeftBlack.ActiveBits();
            int RightMajority = RightWhite.ActiveBits() - RightBlack.ActiveBits();
            return (LeftMajority * LeftMajority * (LeftMajority > 0 ? 1 : -1)) + (RightMajority * RightMajority * (LeftMajority > 0 ? 1 : -1));
        }

        public static float Material(Board board)
        {
            float score = 0;
            for (int i = 0; i < 12; i++) // for each piece excluding king
            {
                score += board.Pieces[i].ActiveBits() * MaterialValues[i];
            }
            return score;
        }

        public static float WeightedMaterial(Board board)
        {
            float score = 0;
            for (int p = 0; p < 12; p++)
            {
                for (int i = 0; i < 64; i++)
                {
                    if (board.Pieces[p].IsBitSet(i)) 
                    {
                        score += MaterialValues[p];
                        if (p < 6) //white piece
                        {
                            score += PieceSquareTable[p][((7  - (i / 8)) * 8 + i % 8)];
                        } else{ //black piece
                            score -= PieceSquareTable[p-6][i];
                        }
                    }
                }
            }
            return score;
        }

        public static void OrderMoves(Board board, Move[] Moves, Move HashMove)
        {
            List<(Move, float)> nonEmptyMoves = new List<(Move, float)>();
            foreach (var move in Moves)
            {
                if (move.GetData() != 0)
                {
                    nonEmptyMoves.Add((move, EvaluateMove(board, move, HashMove)));
                }
            }
            nonEmptyMoves.Sort((move1, move2) => move2.Item2.CompareTo(move1.Item2)); //this is cheaper after null moves are filtered out
            for (int i = 0; i < nonEmptyMoves.Count; i++)
            {
                Moves[i] = nonEmptyMoves[i].Item1;
            }

            //Array.Sort(Moves, (move1, move2) => EvaluateMove(board, move2, HashMove).CompareTo(EvaluateMove(board, move1, HashMove)));
        }

        public static float EvaluateMove(Board board, Move move, Move HashMove)
        {
            if (move.GetData() == 0) return -100000000; // empty move
            if (move.GetData() == HashMove.GetData()) return 1000000;
            float score = 0;
            score += HistoryTable[move.GetStart(), move.GetTarget(), move.GetFlag()];
            int capture = move.GetCapture();
            if (capture != 7)
            {
                score += 10 * Math.Abs(MaterialValues[capture]);
                score -= Math.Abs(MaterialValues[move.GetPiece()]);
            }

            if ((move.GetFlag() & 0b1000) > 0) //move is a promotion
            {
                score += move.GetPiece() * 10;
            }
            int target = move.GetTarget();
            if (((target % 8 == 3) || (target % 8 == 4)) && ((target / 8 == 3) || (target / 8 == 4)))
            {
                score += (1000 - MaterialValues[move.GetPiece()]) / 4;
            }
            return score;
        }

        public static void UpdateHistoryTable(Move move, int Depth)
        {
            HistoryTable[move.GetStart(), move.GetTarget(), move.GetFlag()] += Depth*Depth;
        }

        // piece square tables used for evaluation

        public static int[] PawnTable = { 
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5, 10, 25, 25, 10,  5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5, -5,-10,  0,  0,-10, -5,  5,
            5, 10, 10,-20,-20, 10, 10,  5,
            0,  0,  0,  0,  0,  0,  0,  0};

        public static int[] KnightTable = {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50};
        
        public static int[] BishopTable = {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20};

        public static int[] RookTable = {
             0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            0,  0,  0,  5,  5,  0,  0,  0};

        public static int[] QueenTable = {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -5,  0,  5,  5,  5,  5,  0, -5,
            0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20};

        public static int[] KingTable = {
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
            20, 20,  0,  0,  0,  0, 20, 20,
            20, 30, 10,  0,  0, 10, 30, 20};

        public static int[][] PieceSquareTable = {PawnTable, KnightTable, BishopTable, RookTable, QueenTable, KingTable};
    }
}