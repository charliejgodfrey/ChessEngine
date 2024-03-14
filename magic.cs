using System;
using System.Collections.Generic;
using System.Collections;

namespace ChessEngine
{
    public static class Magic
    {
        public static ulong FindMagic(int square, ulong[] SlidingMasks)
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
            return magic;
        }

        // public static (bool, Hashtable) CheckMagic(ulong magic, int square)
        // {
        //     Hashtable indices = new Hashtable();
        //     foreach (Bitboard blocker in GenerateBlockerConfigurations(square, PreComputeData.RookMasks))
        //     {
        //         ulong value = blocker.GetData() * magic;
        //         ulong index = value >> 52; //try with 52

        //         Bitboard moves = Magic.GenerateRookMoves(square, blocker);
        //         if (!indices.ContainsKey(index))
        //         {
        //             indices.Add(index, moves);
        //             break;
        //         }
        //         else if (indices[index].GetData() == moves.GetData())
        //         {
        //             indices.Add(index, moves);
        //         }
        //         else {
        //             return (false, indices);
        //         }
        //     }
        //     return (true, indices);
        // }

        public static (bool, Bitboard[]) TestMagic(ulong magic, int square)
        {
            Bitboard[] table = new Bitboard[4096];
            Bitboard[] BlockerConfigurations = GenerateBlockerConfigurations(square, PreComputeData.RookMasks);
            foreach (Bitboard blocker in BlockerConfigurations)
            {
                // Console.WriteLine("-----------------");
                // blocker.PrintData();
                Bitboard index = new Bitboard(blocker.GetData() * magic);
                int shift = (64 - index.ActiveBits());
                index.SetData(index.GetData() << 52);

                if (index.GetData() >= 4096)
                {
                    Console.WriteLine("to big");
                    return (false, table);
                }

                Bitboard moves = Magic.GenerateRookMoves(square, blocker);
                Bitboard TableEntry = table[index.GetData()];
                if (TableEntry == null)
                {
                    table[index.GetData()] = moves;
                } else if (TableEntry.GetData() != moves.GetData()) {
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
                }
            }
            return blockers;
        }

        public static Bitboard GenerateRookMoves(int square, Bitboard blockers) //pretty confident this works
        {
            Bitboard Attacks = new Bitboard();
            int pos = square;
            int file = square % 8;
            int rank = square / 8;
            int[] directions = {1,8,-1,8};
            int[] distances = {7-file, 7-rank, file, rank}; //how many squares to the edge of the board
            for (int d = 0; d < 4; d++)
            {
                int direction = directions[d];
                pos = square + direction;
                for (int i = 0; i < distances[d]; i++)
                {
                    Attacks.SetBit(pos);
                    pos += direction;
                    if (blockers.IsBitSet(pos)) //is the attack being blocked
                    {
                        break;
                    }
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