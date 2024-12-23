namespace ChessEngine
{
    public static class Test 
    {
        public static Position[] TestPositions = new Position[100];
        public static string[] FenDataBase;
        public static float[] FenEvaluations;

        public static double EvaluationTest(int positions, int depth)
        {
            double[] error = new double[depth+1];
            Console.Write("Percentage Complete: 0%");
            for (int i = 0; i < positions; i++)
            {
                Board board = new Board(FenDataBase[i]);
                if (depth == 0) //just the raw evaluation function
                {
                    error[0] += Math.Abs(Math.Clamp(FenEvaluations[i], -1000, 1000) - Math.Floor(Evaluation.Evaluate(board)));
                } else
                {
                    (Move BestMove, float Eval, Move[] PV, float[] Evaluations) = Search.IterativeDeepeningSearch(board, depth, new TranspositionTable());
                    // if (Math.Abs(Math.Clamp(FenEvaluations[i], -1000, 1000) - Math.Clamp(Eval, -1000, 1000)) > 800)
                    // {
                    //     Console.WriteLine(FenDataBase[i]);
                    //     Console.WriteLine("turn: " + (board.ColourToMove == 0 ? "white" : "black"));
                    //     Console.WriteLine("true: " + FenEvaluations[i] + " kronos: " + Eval);
                    // }
                    for (int d = 0; d <= depth; d++)
                    {
                        error[d] += (Math.Abs(Math.Clamp(FenEvaluations[i], -1000, 1000) - Evaluations[d])); //adds on the positive error
                    }
                }
                double PercentageComplete = Math.Floor(100*(float)i/(float)positions);
                for (int p=0;p<24;p++)Console.Write("\b");
                Console.Write("Percentage Complete: " + PercentageComplete + "%");
            }
            for (int p=0;p<24;p++)Console.Write("\b");
            Console.Write("Percentage Complete: 100%");
            Console.WriteLine("\nComplete");
            for (int d = 0; d <= depth; d++)
            {
                Console.WriteLine("Average Error At Depth " + d + ": " + (error[d]/positions));
            }
            Console.WriteLine("Total Cumulative Error: " + error[0]);
            return error[0] / positions; //returns mean error
        }
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
                (Move BestMove, float Eval, Move[] PV, float[] evals) = Search.IterativeDeepeningSearch(board, 5, new TranspositionTable());
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
            string filePath = "ChessPositionEvaluationData.txt";
            string[] lines = File.ReadAllLines(filePath);
            FenDataBase = new string[lines.Length];
            FenEvaluations = new float[lines.Length];
            
            for (int i = 0; i < lines.Length; i+=2)
            {
                FenDataBase[i/2] = lines[i];
                FenEvaluations[i/2] = lines[i+1].Contains('#') ? ((lines[i+1].Contains('+') ? 1000 : -1000)) : float.Parse(lines[i+1]);
                if (lines[i+1].Contains('#')) Console.WriteLine(i);
            }
            Console.WriteLine("Successfully loaded " + (lines.Length/2) + " positions");
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