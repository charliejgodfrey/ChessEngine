namespace ChessEngine 
{
    public static class Search 
    {
        public static int perf(int depth, Board board)
        {
            //Board board = RootBoard.Copy();
            if (depth == 0)
            {
                return 1;
            }
            int positions = 0;
            Board RetraceBoard = board.Copy();
            Move[] moves = MoveGenerator.GenerateMoves(board);
            for (int i = 0; i < 218; i++) //all move thingys have length 218
            {
                if (moves[i].GetData() == 0) //done all non empty moves
                {
                    break;
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