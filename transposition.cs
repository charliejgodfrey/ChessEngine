using System.Security.Cryptography;
// namespace ChessEngine
// {
//     public class ZobristHasher
//     {
//         public ulong[][] ZobristTable = new ulong[12][];
//         public ulong SideToMove;
//         public ZobristHasher()
//         {
//             Random rand = new Random();
//             SideToMove = (ulong)(rand.NextDouble() * ulong.MaxValue);
//             for (int p = 0; p < 12; p++)
//             {
//                 ZobristTable[p] = new ulong[64];
//                 for (int i = 0; i < 64; i++)
//                 {
//                     byte[] buffer = new byte[8];
//                     RandomNumberGenerator.Fill(buffer);
//                     ZobristTable[p][i] = BitConverter.ToUInt64(buffer, 0); //generates a random ulong
//                 }
//             }
//         }

//         public ulong Hash(Board board)
//         {
//             ulong hash = 0UL;
//             for (int p = 0; p < 12; p++)
//             {
//                 for (int i = 0; i < 64; i++)
//                 {
//                     if ((board.Pieces[p]&(1UL << i)) != 0) hash ^= ZobristTable[p][i];
//                 }
//             }
//             if (board.ColourToMove == 1) hash ^= SideToMove;
//             return hash;
//         }

//         public ulong PawnHash(Board board)
//         {
//             ulong hash = 0UL;
//             for (int i = 0; i < 64; i++)
//             {
//                 if ((board.Pieces[0]&(1UL << i)) != 0) hash ^= ZobristTable[0][i];
//                 if ((board.Pieces[6]&(1UL << i)) != 0) hash ^= ZobristTable[6][i];
//             }
//             return hash;
//             //we don't worry about the turn when it comes to pawn structure
//         }
//     }
//     public class TranspositionEntry 
//     {
//         public ulong Zobrist;
//         public float Evaluation;
//         public int Depth;
//         public int NodeType;
//         public Move BestMove;
//         public bool NullSearch;
//         public int Age;
//         public TranspositionEntry(ulong zobrist, float evaluation, int depth, Move bestmove, int nodetype, bool NullSearch, int age)
//         {
//             Zobrist = zobrist;
//             Evaluation = evaluation;
//             Depth = depth;
//             BestMove = bestmove;
//             NodeType = nodetype;
//             NullSearch = NullSearch;
//             Age = age;
//         }
//     }

//     public class TranspositionTable
//     {
//         public TranspositionEntry[] table;
//         private const int Exact = 0;
//         private const int LowerBound = 1;
//         private const int UpperBound = 2;
//         private const ulong TranspositionTableSize = (1UL << 12);

//         public TranspositionTable()
//         {
//             table = new TranspositionEntry[TranspositionTableSize];
//         }

//         public void Store(ulong zobrist, float evaluation, int depth, Move bestMove, int nodeType, bool nullSearch, int age)
//         {
//             //return;
//             ulong index =(ulong)(zobrist & (TranspositionTableSize-1)); // Mask for table bounds
//             //if (index >= TranspositionTableSize)Console.WriteLine("we've got a problem");
//             TranspositionEntry existingEntry = table[index];
            
//             // Store entry if it's deeper or if the entry is empty or if the existing entry is from many moves ago
//             if (existingEntry == null || existingEntry.Depth <= depth || (existingEntry.Depth + existingEntry.Age < depth+age))
//             {
//                 table[index] = new TranspositionEntry(zobrist, evaluation, depth, bestMove, nodeType, nullSearch, age);
//             }
//         }

//         public TranspositionEntry Retrieve(ulong zobrist)
//         {
//             //return null;
//             ulong index = (ulong)(zobrist & (TranspositionTableSize - 1));
//             //if (index >= TranspositionTableSize)Console.WriteLine("we've got a problem");
//             TranspositionEntry entry = table[index];

//             return entry != null && entry.Zobrist == zobrist ? entry : null;
//             // this return logic essentially just makes sure that we only return the value if the zobrists are matching, to reduce collisions
//         }

//         public int PercentageFull()
//         {
//             int full = 0;
//             for (ulong i = 0; i < TranspositionTableSize; i++)
//             {
//                 if (table[i] != null) {
//                     full++;
//                 }
//             }
//             return full;
//         }
//     }

//     public class PieceTable //class for specific piece evaluation - mainly pawn structure
//     {
//         public int size = 1<<10;
//         public float[] table;
//         public PieceTable()
//         {
//             table = new float[size];
//         }
//         public void Store(ulong zobrist, float evaluation)
//         {
//             if (evaluation == 0) evaluation = 0.001f; //so we can keep zero for the empty entries
//             table[(int)zobrist & (size-1)] = evaluation;
//         }
//         public float Retrieve(ulong zobrist)
//         {
//             //return 0f;
//             int index = (int)zobrist & (size - 1);
//             float value = table[0];

//             return value; //returns 0 on an invalid evaluation
//         }
//     }
// }

using System;
using System.Collections.Generic;

namespace ChessEngine
{
    public class ZobristHasher
    {
        public ulong[][] ZobristTable = new ulong[12][];
        public ulong SideToMove;

        public ZobristHasher()
        {
            Random rand = new Random();
            SideToMove = (ulong)(rand.NextDouble() * ulong.MaxValue);
            for (int p = 0; p < 12; p++)
            {
                ZobristTable[p] = new ulong[64];
                for (int i = 0; i < 64; i++)
                {
                    byte[] buffer = new byte[8];
                    RandomNumberGenerator.Fill(buffer);
                    ZobristTable[p][i] = BitConverter.ToUInt64(buffer, 0); // generates a random ulong
                }
            }
        }

        public ulong Hash(Board board)
        {
            ulong hash = 0UL;
            for (int p = 0; p < 12; p++)
            {
                for (int i = 0; i < 64; i++)
                {
                    if ((board.Pieces[p] & (1UL << i)) != 0) hash ^= ZobristTable[p][i];
                }
            }
            if (board.ColourToMove == 1) hash ^= SideToMove;
            return hash;
        }

        public ulong PawnHash(Board board)
        {
            ulong hash = 0UL;
            for (int i = 0; i < 64; i++)
            {
                if ((board.Pieces[0] & (1UL << i)) != 0) hash ^= ZobristTable[0][i];
                if ((board.Pieces[6] & (1UL << i)) != 0) hash ^= ZobristTable[6][i];
            }
            return hash;
            // we don't worry about the turn when it comes to pawn structure
        }
    }

    public class TranspositionEntry
    {
        public ulong Zobrist;
        public float Evaluation;
        public int Depth;
        public int NodeType;
        public Move BestMove;
        public bool NullSearch;
        public int Age;

        public TranspositionEntry(ulong zobrist, float evaluation, int depth, Move bestmove, int nodetype, bool nullSearch, int age)
        {
            Zobrist = zobrist;
            Evaluation = evaluation;
            Depth = depth;
            BestMove = bestmove;
            NodeType = nodetype;
            NullSearch = nullSearch;
            Age = age;
        }
    }

    public class TranspositionTable
    {
        // Now using a dictionary to store the transposition entries
        private Dictionary<ulong, TranspositionEntry> table;

        private const int Exact = 0;
        private const int LowerBound = 1;
        private const int UpperBound = 2;

        public TranspositionTable()
        {
            table = new Dictionary<ulong, TranspositionEntry>();
        }

        public void Store(ulong zobrist, float evaluation, int depth, Move bestMove, int nodeType, bool nullSearch, int age)
        {
            TranspositionEntry existingEntry;
            
            // Check if the entry already exists in the table
            if (table.TryGetValue(zobrist, out existingEntry))
            {
                // Store entry if it's deeper or if the existing entry is from many moves ago
                if (existingEntry.Depth <= depth || (existingEntry.Depth + existingEntry.Age < depth + age))
                {
                    table[zobrist] = new TranspositionEntry(zobrist, evaluation, depth, bestMove, nodeType, nullSearch, age);
                }
            }
            else
            {
                // If the entry doesn't exist, add a new one
                table[zobrist] = new TranspositionEntry(zobrist, evaluation, depth, bestMove, nodeType, nullSearch, age);
            }
        }

        public TranspositionEntry Retrieve(ulong zobrist)
        {
            TranspositionEntry entry;
            // Try to get the entry using the zobrist hash as the key
            if (table.TryGetValue(zobrist, out entry))
            {
                return entry;
            }
            else
            {
                return null; // Return null if the entry doesn't exist
            }
        }

        public int PercentageFull()
        {
            // You can optionally implement a method to check how full the table is
            return table.Count; // Returns the number of entries in the dictionary
        }
    }

    public class PieceTable // Class for specific piece evaluation - mainly pawn structure
    {
        public int size = 1 << 10;
        public float[] table;

        public PieceTable()
        {
            table = new float[size];
        }

        public void Store(ulong zobrist, float evaluation)
        {
            if (evaluation == 0) evaluation = 0.001f; // So we can keep zero for the empty entries
            table[(int)zobrist & (size - 1)] = evaluation;
        }

        public float Retrieve(ulong zobrist)
        {
            int index = (int)zobrist & (size - 1);
            return table[index];
        }
    }
}
