using System;
using System.Collections.Generic;
using System.Collections;

namespace ChessEngine
{
    public static class Magic
    {
        public static (ulong, Bitboard[]) FindMagic(int square, ulong[] SlidingMasks, bool IsRook)
        {
            ulong mask = SlidingMasks[square];
            ulong magic;
            bool result;
            Bitboard[] table;
            Bitboard[] BishopBlockers = GenerateBishopBlockerConfigurations(square, PreComputeData.BishopMasks);
            Bitboard[] RookBlockers = GenerateRookBlockerConfigurations(square, PreComputeData.RookMasks);
            do
            {
                magic = RandomUlong() & RandomUlong() & RandomUlong(); //magics normally have fewer active bits
                (result, table) = TestMagic(magic, square, IsRook, (IsRook ? RookBlockers : BishopBlockers));
            }
            while (result == false);
            Console.WriteLine("magic found: " + magic);
            return (magic, table);
        }

        public static (bool, Bitboard[]) TestMagic(ulong magic, int square, bool IsRook, Bitboard[] BlockerConfigurations)
        {
            int BitsNeeded = (IsRook ? RookBits[square] : BishopBits[square]);
            Bitboard[] table = new Bitboard[(int)Math.Pow(2, BitsNeeded)];
            foreach (Bitboard blocker in BlockerConfigurations)
            {
                ulong index = blocker.GetData() * magic;
                index = index >> (64 - (IsRook ? RookBits[square] : BishopBits[square]));

                Bitboard moves = IsRook ? Magic.GenerateRookMoves(square, blocker) : Magic.GenerateBishopMoves(square, blocker);
                if (table[index] == null)
                {
                    table[index] = moves;
                } else if (table[index].GetData() != moves.GetData()) {
                    return (false, table);
                }
            }
            return (true, table);
        }

        public static Bitboard[] GenerateRookBlockerConfigurations(int square, ulong[] SlidingMasks)
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

        public static Bitboard[] GenerateBishopBlockerConfigurations(int square, ulong[] SlidingMasks)
        {
            int file = square % 8;
            int rank = square / 8;
            int[] directions = {7,-7,9,-9};
            int[] distances = {Math.Min(file, 7-rank), Math.Min(7-file, rank), Math.Min(7-file, 7-rank), Math.Min(file, rank)};
            int ReleventBitLength = distances[0] + distances[1] + distances[2] + distances[3];
            Bitboard[] blockers = new Bitboard[(int)Math.Pow(2, ReleventBitLength)];
            for (int i = 0; i < Math.Pow(2, ReleventBitLength); i++) //iterates though every combo of blockers
            {
                Bitboard blocker = new Bitboard();
                int pos = square;
                int count = 0; //remembers the total number of squares looked at
                for (int d = 0; d < 4; d++) //for each direction
                {
                    int direction = directions[d];
                    pos = square + direction;
                    for (int step = 0; step < distances[d]; step++)
                    {
                        if ((i & (1 << count)) != 0) {blocker.SetBit(pos);} //sets the blocker
                        pos += direction;
                        count++;
                    }
                }
                blockers[i] = blocker;
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
            int[] distances = {Math.Min(file, 7-rank), Math.Min(7-file, rank), Math.Min(7-file, 7-rank), Math.Min(file, rank)}; //how many squares to the edge of the board
            for (int d = 0; d < 4; d++) //for each direction
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

        public static Bitboard GenerateRookMoves(int square, Bitboard blockers) //slow way of generating rook moves for the lookup table
        {
            Bitboard Attacks = new Bitboard();
            int pos = square;
            int file = square % 8;
            int rank = square / 8;
            int[] directions = {1,8,-1,-8};
            int[] distances = {7-file, 7-rank, file, rank}; //how many squares to the edge of the board
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

        public static int[] RookBits = {
        12, 11, 11, 11, 11, 11, 11, 12,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        12, 11, 11, 11, 11, 11, 11, 12
        };

        public static int[] BishopBits = {
        6, 5, 5, 5, 5, 5, 5, 6,
        5, 5, 5, 5, 5, 5, 5, 5,
        5, 5, 7, 7, 7, 7, 5, 5,
        5, 5, 7, 10, 10, 7, 5, 5,
        5, 5, 7, 10, 10, 7, 5, 5,
        5, 5, 7, 7, 7, 7, 5, 5,
        5, 5, 5, 5, 5, 5, 5, 5,
        6, 5, 5, 5, 5, 5, 5, 6
        };
    }
}