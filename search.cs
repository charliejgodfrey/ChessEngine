namespace ChessEngine 
{
    public static class Search 
    {
        public static Move NullMove = new Move(0,0,0,0,0,1);
        public static (Move, float) AlphaBeta(Board board, int Depth, float Alpha = -1000000, float Beta = 1000000)
        {
            if (Depth == 0)
            {
                return (new Move(), QuiescienceSearch(board) * (board.ColourToMove == 0 ? 1 : -1));
            }

            Move[] Moves = MoveGenerator.GenerateMoves(board);
            Evaluation.OrderMoves(board, Moves);
            
            Board RetraceBoard = board.Copy();
            Move BestMove = NullMove;

            if (Moves[0].GetData() == 0) //no legal moves
            {
                if (!MoveGenerator.CheckLegal(board, NullMove)) return (NullMove, -1000000 * Depth); //checkmate
                else return (NullMove, 0); //stalemate
            }

            for (int i = 0; i < 218; i++)
            {
                Board TemporaryBoard = board.Copy();
                if (Moves[i].GetData() == 0) break; //done all moves
                if (!MoveGenerator.CheckLegal(board, Moves[i])) continue; //illegal move so ignore

                TemporaryBoard.MakeMove(Moves[i]);
                (Move TopMove, float Score) = AlphaBeta(TemporaryBoard, Depth - 1, -Beta, -Alpha);
                float Eval = -Score; // good move for opponent is bad for us
                if (Eval >= Beta) 
                {
                    return (NullMove, Beta); 
                }
                if (Eval > Alpha)
                {
                    Alpha = Eval;
                    BestMove = Moves[i];
                }
            }
            return (BestMove, Alpha);
        }

        public static float QuiescienceSearch(Board board, float Alpha = -1000000, float Beta = 1000000)
        {
            //float Eval = Evaluation.WeightedMaterial(board);
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

                Board TemporaryBoard = board.Copy();
                TemporaryBoard.MakeMove(moves[i]);
                Eval = -QuiescienceSearch(TemporaryBoard, -Beta, -Alpha);

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
                Board TemporaryBoard = board.Copy();
                if (moves[i].GetData() == 0) //done all non empty moves
                {
                    break;
                }
                if (!MoveGenerator.CheckLegal(board, moves[i])) continue;
                TemporaryBoard.MakeMove(moves[i]);
                int posy = Perft(depth - 1, TemporaryBoard);
                positions += posy;
            }
            return positions;
        }
    }
}