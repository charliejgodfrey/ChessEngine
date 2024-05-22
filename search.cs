namespace ChessEngine 
{
    public static class Search 
    {
        public static Move NullMove = new Move(0,0,0,0,0,1);
        public static Move[] EmptyVariation = new Move[100];

        public static (Move, float, Move[]) IterativeDeepeningSearch(Board board, int maxDepth, TranspositionTable TTable)
        {
            Move BestMove = NullMove;
            float Eval = 0;
            Move[] PrincipleVariation = new Move[maxDepth];
            for (int depth = 1; depth <= maxDepth; depth++)
            {
                Console.WriteLine("Depth Currently Searching..." + depth);
                (BestMove, Eval, Move[] PV) = AlphaBeta(board, depth, TTable);
                PrincipleVariation = PV;
            }
            return (BestMove, Eval, PrincipleVariation);
        }

        public static (Move, float, Move[]) AlphaBeta(Board board, int Depth, TranspositionTable TTable, float Alpha = -1000000, float Beta = 1000000)
        {
            // Console.WriteLine("zobrist: " + board.Zobrist);
            TranspositionEntry entry = TTable.Retrieve(board.Zobrist);
            Move HashMove = NullMove;

            if (entry != null)
            {
                if (entry.NodeType == 0 && entry.Depth >= Depth) // exact value
                {
                    return (NullMove, entry.Evaluation, EmptyVariation);
                }
                else if (entry.NodeType == 1) // lower bound
                {
                    if (entry.Depth >= Depth) Alpha = ((Alpha > entry.Evaluation) ? Alpha : entry.Evaluation);
                    HashMove = entry.BestMove;
                    HashMove.PrintMove();
                }
                else if (entry.NodeType == 2)
                {
                    if (entry.Depth >= Depth) Beta = ((Beta < entry.Evaluation) ? Beta : entry.Evaluation);
                    HashMove = entry.BestMove;
                }
                if (Alpha >= Beta)
                {
                    return (NullMove, entry.Evaluation, EmptyVariation);
                }
            }

            if (Depth == 0)
            {
                return (new Move(), QuiescienceSearch(board) * (board.ColourToMove == 0 ? 1 : -1), new Move[100]);
            }

            Move[] Moves = MoveGenerator.GenerateMoves(board);
            Evaluation.OrderMoves(board, Moves, HashMove);
            
            Board RetraceBoard = board.Copy();
            Move BestMove = NullMove;

            if (Moves[0].GetData() == 0) //no legal moves
            {
                if (!MoveGenerator.CheckLegal(board, NullMove)) return (NullMove, -1000000 * Depth, EmptyVariation); //checkmate
                else return (NullMove, 0, EmptyVariation); //stalemate
            }
            float maxEval = -100000000;
            Move ReturnMove = NullMove;
            Move[] PrincipleVariation = new Move[100];
            for (int i = 0; i < 218; i++)
            {
                //Board TemporaryBoard = board.Copy();
                if (Moves[i].GetData() == 0) break; //done all moves
                if (!MoveGenerator.CheckLegal(board, Moves[i])) continue; //illegal move so ignore

                board.MakeMove(Moves[i]);
                (Move TopMove, float Score, Move[] PV) = AlphaBeta(board, Depth - 1, TTable, -Beta, -Alpha);
                board.UnmakeMove(Moves[i]);
                float Eval = -Score; // good move for opponent is bad for us
                if (Eval > maxEval)
                {
                    maxEval = Eval;
                    ReturnMove = Moves[i];
                    PrincipleVariation = PV;
                }
                Alpha = Math.Max(Alpha, Eval);
                if (Eval >= Beta) break;
            }
            int nodetype = (maxEval <= Alpha) ? 2 : (maxEval >= Beta) ? 1 : 0;

            TTable.Store(board.Zobrist, maxEval, Depth, ReturnMove, nodetype);
            PrincipleVariation[Depth] = ReturnMove;
            return (ReturnMove, maxEval, PrincipleVariation);
        }

        public static float QuiescienceSearch(Board board, float Alpha = -1000000, float Beta = 1000000)
        {
            float Eval = board.Eval;
            if (Eval >= Beta) 
            {
                return Beta;
            }

            Alpha = ((Alpha > Eval) ? Alpha : Eval);

            Move[] moves = MoveGenerator.GenerateMoves(board); //can incorperate specialist function for only captures at some point

            for (int i = 0; i < 218; i++)
            {
                if (!moves[i].IsCapture())
                {
                    continue;
                }

                //Board TemporaryBoard = board.Copy();
                board.MakeMove(moves[i]);
                Eval = -QuiescienceSearch(board, -Beta, -Alpha);
                board.UnmakeMove(moves[i]);
                if (Eval >= Beta)
                {
                    return Beta;
                }
                Alpha = ((Alpha > Eval) ? Alpha : Eval);
            }
            return Alpha;
        }

        public static int Perft(int depth, Board board)
        {
            if (depth == 0)
            {
                return 1;
            }
            int positions = 0;
            Move[] moves = MoveGenerator.GenerateMoves(board);
            for (int i = 0; i < 218; i++) //all move thingys have length 218
            {
                //Board TemporaryBoard = board.Copy();
                if (moves[i].GetData() == 0) //done all non empty moves
                {
                    break;
                }
                if (!MoveGenerator.CheckLegal(board, moves[i])) continue;
                board.MakeMove(moves[i]);
                int posy = Perft(depth - 1, board);
                board.UnmakeMove(moves[i]);
                positions += posy;
            }
            return positions;
        }
    }
}