namespace ChessEngine 
{
    public static class Search 
    {
        public static Move NullMove = new Move(0,0,0,0,0,1);
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
                if (depth > 7) Console.WriteLine("currently searching to depth: " + depth + "/" + maxDepth);
                LMRThreshold = 4;//(maxDepth * 2 + 4) - depth*2;
                NodesEvaluated = 0;
                (BestMove, Eval, Move[] PV) = AlphaBeta(board, depth, TTable);
                if (depth > 7) Console.WriteLine("nodes evaluated: " + NodesEvaluated);
                Evaluation.ShiftKillerMoves();
                //Evaluation.InitializeKillerMoves(); //clear killer moves
                PrincipleVariation = PV;
            }
            for (int i = PrincipleVariation.Count() - 1; i >= 0;i--)
            {
                //PrincipleVariation[i].PrintMove();
            }
            return (BestMove, Eval, PrincipleVariation);
        }

        public static (Move, float, Move[]) AlphaBeta(Board board, int Depth, TranspositionTable TTable, float Alpha = -10000000, float Beta = 10000000)
        {
            //Console.WriteLine("zobrist: " + board.Zobrist);
            TranspositionEntry entry = TTable.Retrieve(board.Zobrist);
            Move HashMove = NullMove;

            if (entry != null) //transposition table stuff
            {
                //Console.WriteLine("transposition");
                if (entry.NodeType == 0 && entry.Depth >= Depth) // exact value
                {
                    return (NullMove, entry.Evaluation, EmptyVariation);
                }
                else if (entry.NodeType == 1) // lower bound
                {
                    if (entry.Depth >= Depth) Alpha = ((Alpha > entry.Evaluation) ? Alpha : entry.Evaluation);
                    HashMove = entry.BestMove;
                    //HashMove.PrintMove();
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
                //return (new Move(), Evaluation.Evaluate(board), new Move[100]);
            }

            // if (Depth == 1 && board.Eval + 100 <= Alpha)
            // {
            //     return (new Move(), Evaluation.Evaluate(board) * (board.ColourToMove == 0 ? 1 : -1), new Move[100]);
            // }

            Move[] Moves = MoveGenerator.GenerateMoves(board);
            Evaluation.OrderMoves(board, Moves, HashMove, Depth); //this increases pruning a insanely huge amount
            
            Move BestMove = NullMove;

            if (Moves[0].GetData() == 0) //no legal moves
            {
                if (MoveGenerator.InCheck(board, board.ColourToMove)) return (NullMove, -1000000 * Depth, EmptyVariation); //checkmate
                else return (NullMove, 0, EmptyVariation); //stalemate
            }
            float maxEval = -100000000;
            Move ReturnMove = Moves[0];
            Move[] PrincipleVariation = new Move[100];
            int nodetype = 0;
            for (int i = 0; i < 218; i++)
            {
                if (Moves[i].GetData() == 0) break; //done all moves
                //if (!MoveGenerator.CheckLegal(board, Moves[i])) continue; //illegal move so ignore

                board.MakeMove(Moves[i]);
                Move TopMove;
                float Score;
                Move[] PV;

                //if (Depth > 6) {

                // if (i==0 || Depth == 1) //the first move
                // {
                //     (TopMove, Score, PV) = AlphaBeta(board, Depth - 1, TTable, -Beta, -Alpha);
                //     // if (Depth == 9) {
                //     //     Console.WriteLine("initial search complete: " + Score);
                //     //     Moves[i].PrintMove();
                //     // }
                // } else {
                //     int Reduction = (i > LMRThreshold && Depth > 1 ? 1 : 0);
                //     (TopMove, Score, PV) = AlphaBeta(board, Depth - 1 - Reduction, TTable, -Alpha - 1, -Alpha); //null window search with potentially reduced depth
                //     if (-Score > Alpha && -Score < Beta) //research required
                //     {
                //         (TopMove, Score, PV) = AlphaBeta(board, Depth - 1, TTable, -Beta, -Alpha); //research with wide window and without reduced depth to increase accuracy
                //         //if (Depth == 9) Console.WriteLine("researched!" + Score);
                //     } //else if (Depth == 9) Console.WriteLine("no research needed");
                // }

                // } else {

                if (i > LMRThreshold && Depth > 1) // idea of this is to reduce search of bad variations
                {
                    (TopMove, Score, PV) = AlphaBeta(board, Depth - 2, TTable, -Beta, -Alpha);
                } else { // if it looks like a move worth searching
                    (TopMove, Score, PV) = AlphaBeta(board, Depth - 1, TTable, -Beta, -Alpha);
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
                    nodetype = 2;
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
            TTable.Store(board.Zobrist, maxEval, Depth, ReturnMove, Moves,nodetype);
            PrincipleVariation[Depth] = ReturnMove;
            return (ReturnMove, maxEval, PrincipleVariation);
        }

        public static float QuiescienceSearch(Board board, float Alpha = -1000000, float Beta = 1000000)
        {
            float Eval = Evaluation.Evaluate(board);
            NodesEvaluated++;
            if (Eval >= Beta) 
            {
                return Beta;
            }

            Alpha = ((Alpha > Eval) ? Alpha : Eval);

            Move[] moves = MoveGenerator.GenerateMoves(board, true); //only generates captures
            Evaluation.OrderMoves(board, moves, NullMove, 0); // the two is a bit arbitrary but seems to be what works the best
            for (int i = 0; i < 218; i++)
            {
                if (moves[i].GetData() == 0) break;
                if (!moves[i].IsCapture())
                {
                    continue;
                }

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