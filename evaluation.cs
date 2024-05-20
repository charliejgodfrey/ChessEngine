using System;
using System.Collections.Generic;
using System.Numerics;

namespace ChessEngine 
{
    public static class Evaluation 
    {
        public static float[] MaterialValues = [100,320,330,500,900,10000,-100,-320,-330,-500,-900,-10000];
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
                            score += PieceSquareTable[p][63 - i];
                        } else{ //black piece
                            score -= PieceSquareTable[p-6][i];
                        }
                    }
                }
            }
            return score;
        }

        public static void OrderMoves(Board board, Move[] Moves)
        {
            Array.Sort(Moves, (move1, move2) => EvaluateMove(board, move2).CompareTo(EvaluateMove(board, move1)));
        }

        public static float EvaluateMove(Board board, Move move)
        {
            if (move.GetData() == 0) return -100000000; // empty move
            float score = 0;
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
            return score;
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