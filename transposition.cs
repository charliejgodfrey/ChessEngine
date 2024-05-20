namespace ChessEngine
{
    public class ZobristHasher
    {
        public ulong[][] ZobristTable = new ulong[12][];
        public ZobristHasher()
        {
            Random rand = new Random();
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
                    if (board.Pieces[p].IsBitSet(i)) hash ^= ZobristTable[p][i];
                }
            }
            return hash;
        }
    }
    public class TranspositionEntry 
    {
        public ulong Zobrist;
        public float Evaluation;
        public int Depth;
        public int NodeType;
        public TranspositionEntry(ulong zobrist, float evaluation, int depth, int nodetype)
        {
            Zobrist = zobrist;
            Evaluation = evaluation;
            Depth = depth;
            NodeType = nodetype;
        }
    }

    public class TranspositionTable
    {
        public readonly Dictionary<ulong, TranspositionEntry> table;
        private const int Exact = 0;
        private const int LowerBound = 1;
        private const int UpperBound = 2;

        public TranspositionTable()
        {
            table = new Dictionary<ulong, TranspositionEntry>();
        }

        public void Store(ulong zobrist, float evaluation, int depth, int nodetype)
        {
            if (!table.ContainsKey(zobrist) || table[zobrist].Depth <= depth) //if it is a better indicator of evaluation
            {
                table[zobrist] = new TranspositionEntry(zobrist, evaluation, depth, nodetype);
            }
        }

        public TranspositionEntry Retrieve(ulong zobrist)
        {
            if (table.TryGetValue(zobrist, out TranspositionEntry entry))
            {
                return entry;
            }
            return null;
        }
    }
}