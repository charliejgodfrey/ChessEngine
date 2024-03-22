using System;
using System.Collections.Generic;
using System.Collections;

namespace ChessEngine
{
    public static class Magic
    {
        public static (ulong, Bitboard[]) FindMagic(int square, ulong[] SlidingMasks)
        {
            ulong mask = SlidingMasks[square];
            ulong magic;
            bool result;
            Bitboard[] table;
            do
            {
                magic = RandomUlong() & RandomUlong() & RandomUlong(); //magics normally have fewer active bits
                (result, table) = TestMagic(magic, square);
            }
            while (result == false);
            return (magic, table);
        }

        public static (bool, Bitboard[]) TestMagic(ulong magic, int square)
        {
            Bitboard[] table = new Bitboard[8196];
            Bitboard[] BlockerConfigurations = GenerateBlockerConfigurations(square, PreComputeData.RookMasks);
            foreach (Bitboard blocker in BlockerConfigurations)
            {
                ulong index = blocker.GetData() * magic;
                index = index >> 52;

                Bitboard moves = Magic.GenerateRookMoves(square, blocker);
                if (table[index] == null)
                {
                    table[index] = moves;
                } else if (table[index].GetData() != moves.GetData()) {
                    return (false, table);
                }
            }
            return (true, table);
        }

        public static Bitboard[] GenerateBlockerConfigurations(int square, ulong[] SlidingMasks)
        {
            Bitboard mask = new Bitboard(SlidingMasks[square]);
            Bitboard[] blockers = new Bitboard[4096];
            for (ulong rank = 0; rank < 64; rank++)
            {
                for (ulong file = 0; file < 64; file++)
                {
                    Bitboard FileBlockers = new Bitboard(file << 1);
                    blockers[rank | (file << 6)] = new Bitboard((FileBlockers.Rotate90() << (square % 8)) | (rank << (1+(square/8)*8)));
                    blockers[rank | (file << 6)].ClearBit(square);
                }
            }
            return blockers;
        }

        public static Bitboard GenerateBishopMoves(int square, Bitboard blockers)
        {
            Bitboard Attacks = new Bitboard();
            int pos = square;
            int file = square % 8;
            int rank = square / 8;
            int[] directions = {7,-7,9,-9};
            int[] distances = {7-file, 7-rank, file, rank}; //how many squares to the edge of the board
        }

        public static Bitboard GenerateRookMoves(int square, Bitboard blockers) //slow way of generating rook moves for the lookup table
        {
            Bitboard Attacks = new Bitboard();
            int pos = square;
            int file = square % 8;
            int rank = square / 8;
            int[] directions = {1,8,-1,-8};
            int[] distances = {7-file, 7-rank, file, rank}; //how many squares to the edge of the board
            //blockers.PrintData();
            //Console.WriteLine("============");
            for (int d = 0; d < 4; d++)
            {
                int direction = directions[d];
                pos = square + direction;
                for (int i = 0; i < distances[d]; i++)
                {
                    Attacks.SetBit(pos);
                    if (blockers.IsBitSet(pos)) //is the attack being blocked
                    {
                        break;
                    }
                    pos += direction;
                }
            }
            return Attacks;
        }

        public static ulong RandomUlong()
        {
            Random random = new Random();
            return (ulong)random.Next(1, int.MaxValue) * (ulong)random.Next(1, int.MaxValue); //multiplying two ints to get a ulong size number
        }
    }
}