namespace ChessEngine 
{
    public static class Search 
    {
        public static Move NullMove = new Move(0,0,0,0,0,1);
        public static (Move, float) AlphaBeta(Board board, int Depth, float Alpha = -1000000, float Beta = 1000000)
        {
            if (Depth == 0)
            {
                return (new Move(), Evaluation.Material(board));
            }

            Move[] Moves = MoveGenerator.GenerateMoves(board);
            Board RetraceBoard = board.Copy();
            Move BestMove = NullMove;

            if (Moves[0].GetData() == 0) //no legal moves
            {
                if (!MoveGenerator.CheckLegal(board, NullMove)) return (NullMove, -1000000 * Depth); //checkmate
                else return (NullMove, 0); //stalemate
            }

            for (int i = 0; i < 218; i++)
            {
                if (Moves[i].GetData() == 0) break; //done all moves
                if (!MoveGenerator.CheckLegal(board, Moves[i])) continue; //illegal move so ignore

                board.MakeMove(Moves[i]);
                (Move TopMove, float Score) = AlphaBeta(board, Depth - 1, -Beta, -Alpha);
                board = RetraceBoard.Copy();
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
        public static int perf(int depth, Board board)
        {
            // if (board.WhiteKing.GetData() == 0 || board.BlackKing.GetData() == 0) //check if the current game state is terminal
            // {
            //     //Console.WriteLine("terminal state found 9");
            //     return 0;
            // }
            if (depth == 0)
            {
                return 1;
            }
            int positions = 0;
            Board RetraceBoard = board.Copy();
            Move[] moves = MoveGenerator.GenerateMoves(board);
            for (int i = 0; i < 218; i++) //all move thingys have length 218
            {
                if (moves[i].GetData() == 0 || MoveGenerator.CheckLegal(board, moves[i]) == false) //done all non empty moves //!MoveGenerator.CheckLegal(board, moves[i])
                {
                    continue;
                }
                board.MakeMove(moves[i]);
                int posy = perf(depth - 1, board);
                board = RetraceBoard.Copy();
                positions += posy;
            }
            return positions;
        }
    }
}