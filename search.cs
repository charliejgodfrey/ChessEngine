namespace ChessEngine 
{
    public static class Search 
    {
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
                if (moves[i].GetData() == 0 || MoveGenerator.CheckLegal(RetraceBoard, moves[i]) == false) //done all non empty moves //!MoveGenerator.CheckLegal(board, moves[i])
                {
                    //moves[i].PrintMove();
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