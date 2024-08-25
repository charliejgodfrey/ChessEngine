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
                    if (board.Pieces[p].IsBitSet(i)) hash ^= ZobristTable[p][i];
                }
            }
            if (board.ColourToMove == 1) hash ^= SideToMove;
            return hash;
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
        public TranspositionEntry(ulong zobrist, float evaluation, int depth, Move bestmove, Move[] legalmoves, int nodetype)
        {
            Zobrist = zobrist;
            Evaluation = evaluation;
            Depth = depth;
            BestMove = bestmove;
            LegalMoves = legalmoves;
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

        public void Store(ulong zobrist, float evaluation, int depth, Move bestmove, Move[] legalmoves, int nodetype)
        {
            if (!table.ContainsKey(zobrist) || table[zobrist].Depth <= depth) //if it is a better indicator of evaluation
            {
                table[zobrist] = new TranspositionEntry(zobrist, evaluation, depth, bestmove, legalmoves, nodetype);
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

    public class PieceTable //class for specific piece evaluation - mainly pawn structure
    {
        public readonly Dictionary<ulong, float> table;
        public PieceTable()
        {
            table = new Dictionary<ulong, float>();
        }
        public void Store(ulong position, float evaluation)
        {
            table[position] = evaluation;
        }
        public float Retrieve(ulong position)
        {
            if (table.TryGetValue(position, out float value))
            {
                return value;
            }
            return -1000000; //clearly not valid
        }
    }
}