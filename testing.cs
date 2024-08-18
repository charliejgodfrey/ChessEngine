namespace ChessEngine
{
    public static class Test 
    {
        public static Position[] TestPositions = new Position[100];

        public static void PerftTest(int depth, Position position)
        {
            Board TestBoard = new Board(position.Fen);
            for (int i = 1; i <= depth; i++)
            {
                int result = Search.Perft(i, TestBoard);
                if (result == position.Perft[i])
                {
                    Console.WriteLine("perft depth: " + i + " result: passed");
                } else {
                    Console.WriteLine("perft depth: " + i + " result: " + Search.Perft(i, TestBoard) + " expected result: " + position.Perft[i]);
                }
            }
        }

        public static void LoadTestPositions()
        {
            TestPositions[0] = new Position("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w", new int[] {0, 20, 400, 8902, 197281, 4865609}, 0);
            TestPositions[1] = new Position("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w", new int[] {0, 48, 2039, 97862, 4085603, 193690690}, 0);
            TestPositions[2] = new Position("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w", new int[] {0, 14, 191, 2812, 43238, 674624}, 0);
            TestPositions[3] = new Position("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w", new int[] {0, 6, 264, 9467, 422333, 15833292}, 0);
            TestPositions[4] = new Position("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w", new int[] {0, 44, 1486, 62379, 2103487, 89941194}, 0);
        }
    }

    public class Position
    {
        public string Fen;
        public int[] Perft;
        public int BackwardsPawns;
        public Position(string Fen, int[] Perft, int BackwardsPawns)
        {
            this.Fen = Fen;
            this.Perft = Perft;
            this.BackwardsPawns = BackwardsPawns;
        }
    }
}