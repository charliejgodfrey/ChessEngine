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
                    ZobristTable[p][i] = (ulong)(rand.NextDouble() * ulong.MaxValue); //generates a random ulong
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
                    if ((board.Pieces[p]&(1UL << i)) != 0) hash ^= ZobristTable[p][i];
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
                if ((board.Pieces[0]&(1UL << i)) != 0) hash ^= ZobristTable[0][i];
                if ((board.Pieces[6]&(1UL << i)) != 0) hash ^= ZobristTable[6][i];
            }
            return hash;
            //we don't worry about the turn when it comes to pawn structure
        }
    }
    public class TranspositionEntry 
    {
        public ulong Zobrist;
        public float Evaluation;
        public int Depth;
        public int NodeType;
        public Move[] LegalMoves;
        public Move BestMove;
        public bool NullSearch;
        public TranspositionEntry(ulong zobrist, float evaluation, int depth, Move bestmove, int nodetype, bool NullSearch)
        {
            Zobrist = zobrist;
            Evaluation = evaluation;
            Depth = depth;
            BestMove = bestmove;
            NodeType = nodetype;
            NullSearch = NullSearch;
        }
    }

    public class TranspositionTable
    {
        public TranspositionEntry[] table;
        private const int Exact = 0;
        private const int LowerBound = 1;
        private const int UpperBound = 2;
        private const int TranspositionTableSize = 1 << 28;

        public TranspositionTable()
        {
            table = new TranspositionEntry[TranspositionTableSize];
        }

        public void Store(ulong zobrist, float evaluation, int depth, Move bestMove, int nodeType, bool nullSearch)
    {
        int index = (int)(zobrist & (TranspositionTableSize - 1)); // Mask for table bounds
        TranspositionEntry existingEntry = table[index];
        
        // Store entry if it's deeper or if the entry is empty
        if (existingEntry == null || existingEntry.Depth <= depth)
        {
            table[index] = new TranspositionEntry(zobrist, evaluation, depth, bestMove, nodeType, nullSearch);
        }
    }

        public TranspositionEntry Retrieve(ulong zobrist)
        {
            int index = (int)(zobrist & (TranspositionTableSize - 1));
            TranspositionEntry entry = table[index];

            return entry != null && entry.Zobrist == zobrist ? entry : null;
            // this return logic essentially just makes sure that we only return the value if the zobrists are matching, to reduce collisions
        }
    }

    public class PieceTable //class for specific piece evaluation - mainly pawn structure
    {
        public int size = 1<<20;
        public float[] table;
        public PieceTable()
        {
            table = new float[size];
        }
        public void Store(ulong zobrist, float evaluation)
        {
            if (evaluation == 0) evaluation = 0.001f; //so we can keep zero for the empty entries
            table[(int)zobrist & (size-1)] = evaluation;
        }
        public float Retrieve(ulong zobrist)
        {
            int index = (int)zobrist & (size - 1);
            float value = table[0];

            return value; //returns 0 on an invalid evaluation
        }
    }
}