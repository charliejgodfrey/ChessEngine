namespace ChessEngine 
{
    public static class Search 
    {
        public static Move NullMove = new Move(0,0,0,0,0,0);
        public static Move[] EmptyVariation = new Move[100];
        public static int LMRThreshold = 6;
        public static int NodesEvaluated = 0;

        public static (Move, float, Move[]) IterativeDeepeningSearch(Board board, int maxDepth, TranspositionTable TTable)
        {
            Move BestMove = NullMove;
            float Eval = 0;
            Move[] PrincipleVariation = new Move[maxDepth];
            for (int depth = 1; depth <= maxDepth; depth++)
            {
                //if (depth > 7) Console.WriteLine("currently searching to depth: " + depth + "/" + maxDepth);
                LMRThreshold = 4;//(maxDepth * 2 + 4) - depth*2;
                NodesEvaluated = 0;
                (BestMove, Eval, Move[] PV) = AlphaBeta(board, depth, TTable, NullMove);
                if (depth > 0) Console.WriteLine("nodes evaluated for depth " + depth + ": " + NodesEvaluated + " current best move: " + Program.FormatMove(BestMove));
                Evaluation.ShiftKillerMoves();
                Evaluation.InitializeKillerMoves(); //clear killer moves
                PrincipleVariation = PV;
            }
            for (int i = PrincipleVariation.Count() - 1; i >= 0;i--)
            {
                //PrincipleVariation[i].PrintMove();
            }
            if (BestMove.GetData() == NullMove.GetData()) {
                Move[] Moves = MoveGenerator.GenerateMoves(board);
                return (Moves[0], Eval, PrincipleVariation);
            }
            return (BestMove, Eval, PrincipleVariation);
        }

        public static (Move, float, Move[]) AlphaBeta(Board board, int Depth, TranspositionTable TTable, Move PreviousMove, float Alpha = -10000000, float Beta = 10000000)
        {
            //Console.WriteLine("zobrist: " + board.Zobrist);
            TranspositionEntry entry = TTable.Retrieve(board.Zobrist);
            Move HashMove = NullMove;

            if (entry != null) //transposition table stuff
            {
                //Console.WriteLine("transposition");
                if (entry.NodeType == 0) // exact value
                {
                    HashMove = entry.BestMove;
                    if (entry.Depth >= Depth) return (entry.BestMove, entry.Evaluation, EmptyVariation);
                }
                else if (entry.NodeType == 1) // lower bound
                {
                    if (entry.Depth >= Depth) Alpha = ((Alpha > entry.Evaluation) ? Alpha : entry.Evaluation);
                    HashMove = entry.BestMove;
                    if (Alpha >= Beta)
                    {
                        return (entry.BestMove, entry.Evaluation, EmptyVariation);
                    }
                    //HashMove.PrintMove();
                }
                else if (entry.NodeType == 2) // upper bound
                {
                    if (entry.Depth >= Depth) Beta = ((Beta < entry.Evaluation) ? Beta : entry.Evaluation);
                    HashMove = entry.BestMove;
                    if (entry.Evaluation <= Alpha)
                    {
                        return (entry.BestMove, entry.Evaluation, EmptyVariation);
                    }
                }
            }

            if (Depth == 0)
            {
                //return (new Move(), Evaluation.Evaluate(board) * (board.ColourToMove == 0 ? 1 : -1), EmptyVariation);
                return (NullMove, QuiescienceSearch(board, Alpha, Beta), EmptyVariation);
                //return (new Move(), Evaluation.Evaluate(board), new Move[100]);
            }

            float maxEval = -100000000;
            int nodetype = 2;

            bool Broken = false;
            bool FirstMoveSearched = false;
            float previousEval = Alpha;
            Move ReturnMove = NullMove;

            Move[] PrincipleVariation = new Move[100];

            Move[] Moves = MoveGenerator.GenerateMoves(board);
            Evaluation.OrderMoves(board, Moves, HashMove, Depth, PreviousMove); //this increases pruning an insanely huge amount
            
            Move BestMove = NullMove;

            if (Moves[0].GetData() == 0) //no legal moves
            {
                if (MoveGenerator.InCheck(board, board.ColourToMove)) return (NullMove, -100000 * Depth, EmptyVariation); //checkmate
                else return (NullMove, 0, EmptyVariation); //stalemate
            }
            ReturnMove = Moves[0];

            if (Depth > 2 && !MoveGenerator.InCheck(board, board.ColourToMove) && board.GamePhase > 20 && PreviousMove.GetData() != 0) //appropriate for Null Move pruning
            {
                int reduction = 3;// (Depth == 3 ? 3 : 4); //this speeds things up a tonne

                board.MakeEmpty(); //empty move
                (Move TopMove,float Score,Move[] PV) = AlphaBeta(board, Depth - reduction, TTable, Moves[0], -Beta, -Beta + 1); //perform significantly reduced depth search with narrow window
                board.UnmakeEmpty(); //unempty move
                if (-Score >= Beta) 
                {
                    TTable.Store(board.Zobrist, Beta, Depth-reduction, NullMove, 1, true);
                    return (NullMove, Beta, PV);
                }
            }

            for (int i = 0; i < 218; i++)
            {
                if (Moves[i].GetData() == 0) break; //done all moves
                //if (!MoveGenerator.CheckLegal(board, Moves[i])) continue; //illegal move so ignore
                board.MakeMove(Moves[i]);

                Move TopMove;
                float Score;
                Move[] PV;

                if (i > LMRThreshold && Depth > 1) // idea of this is to reduce search of bad variations
                {
                    (TopMove, Score, PV) = AlphaBeta(board, Depth - 2, TTable, Moves[i], -Beta, -Alpha);
                } else { // if it looks like a move worth searching
                    (TopMove, Score, PV) = AlphaBeta(board, Depth - 1, TTable, Moves[i], -Beta, -Alpha);
                }
                // }
                board.UnmakeMove(Moves[i]);
                float Eval = -Score; // good move for opponent is bad for us
                if (Eval > maxEval)
                {
                    maxEval = Eval;
                    ReturnMove = Moves[i];
                    PrincipleVariation = PV;
                }
                if (Eval > Alpha)
                {
                    Alpha = Eval;
                    nodetype = 0;
                }
                if (Eval >= Beta) 
                {
                    Evaluation.UpdateHistoryTable(Moves[i], Depth); //adds it as a good move to look for
                    if (Moves[i].GetCapture() == 0b111){
                    Evaluation.KillerMoves[Depth][0] = Evaluation.KillerMoves[Depth][1];
                    Evaluation.KillerMoves[Depth][1] = Moves[i];
                    }
                    nodetype = 1;
                    break;
                }
            }
            if (nodetype == 1 || (Alpha > previousEval+100) && PreviousMove.GetData() != 0) // the best move in this position was unexpectidly good
            {
                Evaluation.CounterMoves[PreviousMove.GetStart(),PreviousMove.GetTarget()] = ReturnMove; //implement remembering the previous move
            }
            TTable.Store(board.Zobrist, maxEval, Depth, ReturnMove,nodetype,false);
            PrincipleVariation[Depth] = ReturnMove;
            return (ReturnMove, maxEval, PrincipleVariation);
        }

        public static float QuiescienceSearch(Board board, float Alpha = -1000000, float Beta = 1000000)
        {
            NodesEvaluated++;
            if (board.WhiteKing.GetData() == 0) return (board.ColourToMove == 0 ? -1000000 : 100000);
            if (board.BlackKing.GetData() == 0) return (board.ColourToMove == 0 ? -1000000 : 100000);
            float Eval = (board.ColourToMove == 0 ? 1 : -1) * Evaluation.Evaluate(board);
            NodesEvaluated++;
            if (Eval >= Beta) 
            {
                return Beta;
            }

            Alpha = ((Alpha > Eval) ? Alpha : Eval);

            Move[] moves = MoveGenerator.GenerateMoves(board, true); //only generates captures
            Evaluation.OrderMoves(board, moves, NullMove, 0, NullMove);
            for (int i = 0; i < 218; i++)
            {
                if (moves[i].GetData() == 0) break;
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
                //float eval = board.Eval;
                //Evaluation.WeightedMaterial(board);
                //Program.PrintMoves(board);
                return 1;
            }
            int positions = 0;
            Move[] moves = MoveGenerator.GenerateMoves(board);
            for (int i = 0; i < 218; i++) //all move thingys have length 218
            {
                if (moves[i].GetData() == 0) //done all non empty moves
                {
                    break;
                }
                //if (!MoveGenerator.CheckLegal(board, moves[i])) continue;
                board.MakeMove(moves[i]);
                int posy = Perft(depth - 1, board);
                board.UnmakeMove(moves[i]);
                positions += posy;
            }
            return positions;
        }
    }
}