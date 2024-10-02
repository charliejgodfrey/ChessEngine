namespace ChessEngine
{
    public static class Test 
    {
        public static Position[] TestPositions = new Position[100];
        public static string[] FenDataBase = new string[100];

        public static void PerftTest(int depth, Position position)
        {
            Board TestBoard = new Board(position.Fen);
            for (int i = 1; i <= depth; i++)
            {
                int result = Search.Perft(i, TestBoard);
                if (result == position.Perft[i])
                {
                    Console.WriteLine("perft depth: " + i + " nodes: " + result + " result: passed");
                } else {
                    Console.WriteLine("perft depth: " + i + " result: " + result + " expected result: " + position.Perft[i]);
                }
            }
        }

        public static void ShowEvaluationScores() 
        {
            for (int i = 0; i < FenDataBase.Length; i++)
            {
                if (FenDataBase[i]==null) break;
                Board board = new Board(FenDataBase[i]);
                Console.WriteLine("Position " + i + ":");
                board.PrintBoard();
                Console.WriteLine("static eval: " + Evaluation.Evaluate(board));
                Console.WriteLine("weighted material: " + Evaluation.WeightedMaterial(board));
                (Move BestMove, float Eval, Move[] PV) = Search.IterativeDeepeningSearch(board, 5, new TranspositionTable());
                Console.WriteLine("depth eval: " + Eval);
            }
        }

        public static void LoadTestPositions()
        {
            TestPositions[0] = new Position("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w", new int[] {0, 20, 400, 8902, 197281, 4865609, 119060324}, 0);
            TestPositions[1] = new Position("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w", new int[] {0, 48, 2039, 97862, 4085603, 193690690}, 0);
            TestPositions[2] = new Position("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w", new int[] {0, 14, 191, 2812, 43238, 674624}, 0);
            TestPositions[3] = new Position("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w", new int[] {0, 6, 264, 9467, 422333, 15833292}, 0);
            TestPositions[4] = new Position("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w", new int[] {0, 44, 1486, 62379, 2103487, 89941194}, 0);
            TestPositions[5] = new Position("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w", new int[] {0, 46, 2079, 89890, 3894594, 164075551}, 0);
            LoadFenDataBase();
        }

        public static void LoadFenDataBase()
        {
            FenDataBase[0] = "rnbqkb1r/ppppp1pp/5n2/5p2/2PP4/8/PP2PPPP/RNBQKBNR w"; //first 10 are opening positions
            FenDataBase[1] = "rnbqkbnr/ppp2ppp/8/3pp3/2P5/6P1/PP1PPP1P/RNBQKBNR w";
            FenDataBase[2] = "r1bqkbnr/pppp1ppp/2n5/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R w";
            FenDataBase[3] = "r1bqkbnr/pp1ppppp/2n5/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R w";
            FenDataBase[4] = "rnbqkbnr/pp2pppp/8/2pp4/8/5NP1/PPPPPP1P/RNBQKB1R w";
            FenDataBase[5] = "rnbqkbnr/pp2pppp/8/2pp4/3P1B2/8/PPP1PPPP/RN1QKBNR w";
            FenDataBase[6] = "rnbqkb1r/ppp1pppp/5n2/3p4/3P1B2/5P2/PPP1P1PP/RN1QKBNR w";
            FenDataBase[7] = "rnbqk1nr/ppp2ppp/4p3/3p4/1bPP4/2N5/PP2PPPP/R1BQKBNR w";
            FenDataBase[8] = "rnbqkb1r/ppp1pp1p/3p1np1/8/3PP3/2N5/PPP2PPP/R1BQKBNR w";
            FenDataBase[9] = "r1bqkbnr/1ppp1ppp/p1n5/1B2p3/4P3/5N2/PPPP1PPP/RNBQK2R w";

            FenDataBase[10] = "r1bq1rk1/ppp1bppp/2n2n2/4p3/3pP3/P2P1N2/1PPBBPPP/RN1Q1RK1 w"; //middle game positions
            FenDataBase[11] = "r1bq1rk1/2p2pbp/ppnppnp1/8/P2PP3/2N1BN1P/1PP1BPP1/R2Q1RK1 w";
            FenDataBase[12] = "2r1k2r/qp1bbp2/p2ppp2/7p/3NP2P/6P1/PPP2PB1/R2Q1RK1 w";
            FenDataBase[13] = "r1b1rnk1/pp2qppp/2p5/3p3n/3P4/2NBPP2/PPQ1N1PP/R4RK1 w";
            FenDataBase[14] = "r1bqk1nr/pp2ppbp/3p2p1/2p5/2BnP3/2N2N2/PPPP1PPP/R1BQ1RK1 w";
            FenDataBase[15] = "r1br2k1/pp2qpp1/5n1p/2b5/1n2P3/2N2NP1/PP2PPBP/RQB2RK1 w";
            FenDataBase[16] = "r1bq1rk1/ppp1n1bp/3p1n2/2PPp1p1/4Pp2/2NN1P2/PP1BB1PP/R2Q1RK1 w";
            FenDataBase[17] = "1r1qr1k1/ppp2pbp/3pb1p1/4n3/2P5/1PN3P1/PB1QPPBP/3R1RK1 w";
            FenDataBase[18] = "2kr1b1r/ppqn1p2/2p1p1np/3pPbp1/3P4/1N1NB3/PPP1BPPP/R2Q1RK1 w";
            FenDataBase[19] = "r3k2r/pp3pp1/2nqpn1p/3p1b2/2pP4/2P1PN2/PP1NBPPP/R2Q1RK1 w";

            FenDataBase[20] = "2k5/5p2/3Pn3/1B2Pp1p/7P/4K1P1/8/8 w"; //endgame positions
            FenDataBase[21] = "1r6/6p1/b4pk1/P7/1B3K2/5P2/6P1/7R w";
            FenDataBase[22] = "4r3/8/p7/1p1pk1p1/3N2b1/PBP3P1/1P3K2/8 w";
            FenDataBase[23] = "8/4pp2/4k1p1/7p/P4R1P/r4PP1/4PK2/8 w";
            FenDataBase[24] = "6k1/1p3pp1/p6p/8/4r3/7P/PP3PP1/2R3K1 w";
            FenDataBase[25] = "8/R4ppk/1p2b2p/8/P3N1P1/5PK1/r5P1/8 w";
            FenDataBase[26] = "4r3/2k5/2nRp1p1/p1p2p1p/K1P2P1P/1PB3P1/1P6/8 w";
            FenDataBase[27] = "4r3/r4k2/pR4p1/4P1Bp/2p1KP1P/P1P5/6P1/8 w";
            FenDataBase[28] = "3n1k2/4b1p1/5p1p/p7/P3P3/2pN1PP1/1R2K2P/8 w";
            FenDataBase[29] = "8/3n3p/1pKBk1p1/2p1p1P1/2P4P/1P6/5P2/8 w";
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