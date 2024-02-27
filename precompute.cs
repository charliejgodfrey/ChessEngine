//this is where the code for precomputing certain values is
using System;
using System.Collections.Generic;

namespace ChessEngine 
{
    static class PreComputeData
    {
        public static Bitboard[] KnightAttackBitboards = new Bitboard[64];
        public static Bitboard[] KingAttackBitboards = new Bitboard[64];
        public static Bitboard[] PawnAttackBitboards = new Bitboard[64];

        public static void InitializeAttackBitboards()
        {
            PreComputeKnightAttacks();
            PreComputePawnAttacks();
            PreComputeKingAttacks();
        }

        

        private static void PreComputeKingAttacks()
        {
            int[][] directions = [[0,1],[1,1],[1,0],[1,-1],[0,-1],[-1,-1],[-1,0],[-1,1]];
            for (int i = 0; i < 64; i++)
            {
                KingAttackBitboards[i] = new Bitboard();
                int file = i % 8;
                int rank = i / 8; //file and rank of square index
                for (int q = 0; q < 8; q++)
                {
                    int NewFile = file + directions[q][0]; //new locations
                    int NewRank = rank + directions[q][1];
                    if (NewRank >= 0 && NewRank <= 7 && NewFile >= 0 && NewFile <= 7) //make sure it's on the board
                    {
                        KingAttackBitboards[i].SetBit(NewRank*8 + NewFile); //set the bitboard to contain the square as attacked
                    }
                }
            }
        }

        private static void PreComputePawnAttacks()
        {
            for (int i = 0; i < 64; i++)
            {
                PawnAttackBitboards[i] = new Bitboard();
                int file = i % 8;
                int rank = i / 8; 
                if (file != 0 && rank != 7) PawnAttackBitboards[i].SetBit(i + 7);//makes sure there aren't any attacks that would be off the board
                if (file != 7 && rank != 7) PawnAttackBitboards[i].SetBit(i + 9); //----^
            }
        }

        private static void PreComputeKnightAttacks() //procedure to initialise the knight attacks array
        {
            int[][] directions = [[1,2],[2,1],[2,-1],[1,-2],[-1,-2],[-2,-1],[-2,1],[-1,2]];
            for (int i = 0; i < 64; i++)
            {
                KnightAttackBitboards[i] = new Bitboard();
                for (int q = 0; q < 8; q++)
                {
                    int[] direction = directions[q];
                    int[] square = [i % 8, i / 8];
                    square[0] += direction[0];
                    square[1] += direction[1];
                    int index = square[0] + square[1]*8;
                    if (square[0] >= 0 && square[1] >= 0 && square[0] <= 7 && square[1] <= 7)
                    {
                        KnightAttackBitboards[i].SetBit(index);
                    }
                }
            }
        }
    }
}