using System;
using System.Collections.Generic;
using System.Numerics;

namespace ChessEngine 
{
    public static class Evaluation 
    {
        public static float[] MaterialValues = [100,320,330,500,900,10000,-100,-320,-330,-500,-900,-10000];
        public static PieceTable PawnEvaluationTable = new PieceTable(); //for pawn structure analysis
        public static int[,,] HistoryTable = new int[64, 64, 16]; //stores how good moves are shown to have been in prior searches
        public static Move[][] KillerMoves = new Move[128][];
        public static Move[,] CounterMoves = new Move[64, 64];

        public static float avgMaterial = 0;
        public static float avgPositional = 0;
        public static float avgDiffer = 0;
        public static int count = 0;

        public static float Evaluate(Board board)
        {
            //return WeightedMaterial(board);
            //return board.Eval;
            float WhitePawns = PawnEvaluationTable.Retrieve(board.WhitePawns.GetData());
            float BlackPawns = PawnEvaluationTable.Retrieve(board.BlackPawns.GetData());
            if (WhitePawns == -1000000) //not in the table
            {
                WhitePawns = EvaluatePawnStructure(board.WhitePawns, 0);
                PawnEvaluationTable.Store(board.WhitePawns.GetData(), WhitePawns);
            }
            if (BlackPawns == -1000000) //not in the table
            {
                BlackPawns = EvaluatePawnStructure(board.BlackPawns, 1);
                PawnEvaluationTable.Store(board.BlackPawns.GetData(), BlackPawns);
            }
            float WhiteKingSafety = 0;// EvaluateKingSafety(board, 0) * 10; not sure this actually helps
            float BlackKingSafety = 0;//EvaluateKingSafety(board, 1) * 10;
            float PieceDefense = (DefendedMinorPieces(board, 0) - DefendedMinorPieces(board, 1)) * 5;
            (float WhiteMobility, ulong WhiteAttacks) = EvaluateMobility(board, 0);
            (float BlackMobility, ulong BlackAttacks) = EvaluateMobility(board, 1);
            //Console.WriteLine("white: " + WhiteMobility + " black: " + BlackMobility);
            float Mobility = 10*(WhiteMobility - BlackMobility);
            float PieceCoordination = EvaluateAttackBitboard(board, WhiteAttacks, BlackAttacks) * 10;
            float Pins = (BitOperations.PopCount(DetectPinnedPieces(board, BlackAttacks, 0)) - BitOperations.PopCount(DetectPinnedPieces(board, WhiteAttacks, 1))) * 10;
            float Material = board.Eval;//WeightedMaterial(board);
            // avgMaterial += Math.Abs(Material);
            // avgPositional += Math.Abs(WhitePawns + WhiteKingSafety - BlackPawns - BlackKingSafety + Mobility + PieceCoordination);
            // avgDiffer += Math.Abs((WhitePawns + WhiteKingSafety - BlackPawns - BlackKingSafety + Mobility + PieceCoordination));
            // count++;
            // Console.WriteLine("Mobility Score: " + Mobility + " Piece Coordination: " + PieceCoordination + " Piece Defense: " + PieceDefense + " wpawn: " + WhitePawns + " bpawn: " + BlackPawns);
            // Console.WriteLine("sum of non material: " + (WhitePawns + WhiteKingSafety - BlackPawns - BlackKingSafety + Mobility + PieceCoordination));
            // Console.WriteLine("weighted material: " + board.Eval);
            return Material + WhitePawns + WhiteKingSafety - BlackPawns - BlackKingSafety + Mobility + PieceCoordination;
        }

        public static ulong DetectPinnedPieces(Board board, ulong EnemyAttacks, int Player) //finds the pieces of the players colour that are pinned
        {
            int KingSquare = board.Pieces[Player * 6 + 5].LSB();
            ulong UnderAttack = (Player == 0 ? board.WhitePieces.GetData() : board.BlackPieces.GetData()) & EnemyAttacks;
            ulong KingLines = (MoveGenerator.GenerateBishopAttacks(board, KingSquare).GetData() | MoveGenerator.GenerateRookAttacks(board, KingSquare).GetData());
            ulong Pins = KingLines & UnderAttack;
            return Pins;
        }

        public static float EvaluateAttackBitboard(Board board, ulong WhiteAttacks, ulong BlackAttacks)
        {
            int WhiteSpaceAdvantage = BitOperations.PopCount(WhiteAttacks & 0xFFFFFFFF3C000000); //attacked squares in enemey half and center
            int BlackSpaceAdvantage = BitOperations.PopCount(BlackAttacks & 0x0000003CFFFFFFFF);
            //piece coordination and threats
            ulong WhiteDefendedPieces = WhiteAttacks & board.WhitePieces.GetData();
            ulong BlackDefendedPieces = BlackAttacks & board.BlackPieces.GetData();
            ulong WhiteUndefendedPieces = board.WhitePieces.GetData() & ~WhiteDefendedPieces;
            ulong BlackUndefendedPieces = board.BlackPieces.GetData() & ~BlackDefendedPieces;
            ulong WhiteThreats = WhiteAttacks & BlackUndefendedPieces;
            ulong BlackThreats = BlackAttacks & WhiteUndefendedPieces;
            //king safety bonuses
            int WhiteKingSafety = BitOperations.PopCount(BlackAttacks & PreComputeData.KingAdjacents[board.WhiteKing.LSB()]);
            int BlackKingSafety = BitOperations.PopCount(WhiteAttacks & PreComputeData.KingAdjacents[board.BlackKing.LSB()]);
            WhiteDefendedPieces &= ~board.WhitePawns.GetData(); //defended pawns don't rlly matter
            BlackDefendedPieces &= ~board.BlackPawns.GetData();
            double WhiteBonus = 1.5 * BitOperations.PopCount(WhiteDefendedPieces) + 2 * BitOperations.PopCount(WhiteThreats) + 0.25 * WhiteSpaceAdvantage - 1.25 * BitOperations.PopCount(WhiteUndefendedPieces) - 2.5 * WhiteKingSafety;
            double BlackBonus = 1.5 * BitOperations.PopCount(BlackDefendedPieces) + 2 * BitOperations.PopCount(BlackThreats) + 0.25 * BlackSpaceAdvantage - 1.25 * BitOperations.PopCount(BlackUndefendedPieces) - 2.5 * BlackKingSafety;
            return (float)(WhiteBonus - BlackBonus);
        }

        public static (int, ulong) EvaluateMobility(Board board, int Player)
        {
            ulong AttackedSquares = 0UL;
            int ColourAdd = Player * 6;
            Bitboard FriendlyPieces = (Player == 0 ? board.WhitePieces : board.BlackPieces);
            int KnightMoves = 0;
            Bitboard Knights = new Bitboard(board.Pieces[ColourAdd+1].GetData());
            while (Knights.GetData() > 0) //for each knight
            {
                int square = Knights.LSB();
                ulong Attacks = PreComputeData.KnightAttackBitboards[square].GetData();
                AttackedSquares |= Attacks;
                Attacks &= ~FriendlyPieces.GetData();
                KnightMoves += BitOperations.PopCount(Attacks);
                Knights.ClearBit(square);
            }
            
            int BishopMoves = 0;
            Bitboard Bishops = new Bitboard(board.Pieces[ColourAdd+2].GetData());
            while (Bishops.GetData() > 0) //for each bishop
            {
                int square = Bishops.LSB();
                ulong Attacks = MoveGenerator.GenerateBishopAttacks(board, square).GetData();
                AttackedSquares |= Attacks;
                Attacks &= ~FriendlyPieces.GetData();
                BishopMoves += BitOperations.PopCount(Attacks);
                Bishops.ClearBit(square);
            }

            int RookMoves = 0;
            Bitboard Rooks = new Bitboard(board.Pieces[ColourAdd+3].GetData());
            while (Rooks.GetData() > 0) //for each rook
            {
                int square = Rooks.LSB();
                ulong Attacks = MoveGenerator.GenerateRookAttacks(board, square).GetData();
                AttackedSquares |= Attacks;
                Attacks &= ~FriendlyPieces.GetData();
                RookMoves += BitOperations.PopCount(Attacks);
                Rooks.ClearBit(square);
            }

            int QueenMoves = 0;
            Bitboard Queens = new Bitboard(board.Pieces[ColourAdd+4].GetData());
            while (Queens.GetData() > 0) //for each queen
            {
                int square = Queens.LSB();
                ulong Attacks = (MoveGenerator.GenerateBishopAttacks(board, square).GetData() | MoveGenerator.GenerateRookAttacks(board, square).GetData());
                AttackedSquares |= Attacks;
                Attacks &= ~FriendlyPieces.GetData();
                QueenMoves += BitOperations.PopCount(Attacks);
                Queens.ClearBit(square);
            }

            ulong EastCapture = ((board.Pieces[ColourAdd].GetData() << 9) & 0x7F7F7F7F7F7F7F7F); //this essentially checks there is a piece that can be captured and accounts for overflow stuff
            ulong WestCapture = ((board.Pieces[ColourAdd].GetData() << 7) & 0xFEFEFEFEFEFEFEF); //does the same thing for the other direction
            ulong ForwardMoves = ((board.Pieces[ColourAdd].GetData() << 8) & ~(Player == 0 ? board.BlackPieces.GetData() : board.WhitePieces.GetData()));
            int PawnMoves = BitOperations.PopCount(ForwardMoves);
            AttackedSquares |= (WestCapture | EastCapture);
            int CentreControl = BitOperations.PopCount(AttackedSquares & 0x0000003C3C000000);
            return (KnightMoves + BishopMoves + RookMoves + (board.GamePhase > 32 ? (1/2) : 1) * QueenMoves + PawnMoves + CentreControl * 3, AttackedSquares); 
        }

        public static int DefendedMinorPieces(Board board, int Player)
        {
            int ColourAdd = (Player == 0 ? 8 : -8);
            Bitboard Pawns = (Player == 0 ? board.WhitePawns : board.BlackPawns);
            Bitboard PawnAttacks = new Bitboard((Pawns.GetData() << (1 + ColourAdd)) | (Pawns.GetData() << (ColourAdd - 1)));
            int DefendedMinorPieces = BitOperations.PopCount(PawnAttacks.GetData() & (board.Pieces[Player * 6 + 1].GetData() | board.Pieces[Player * 6 + 2].GetData()));
            return DefendedMinorPieces;
        }

        public static float OutpostEvaluation(Bitboard Pawns, int Player)
        {
            int ColourAdd = (Player == 0 ? 8 : -8);
            Bitboard PawnAttacks = new Bitboard((Pawns.GetData() << (1 + ColourAdd)) | (Pawns.GetData() << (ColourAdd - 1)));
            PawnAttacks.AND(Player == 0 ? 0xFFFFFFFF00000000 : 0x00000000FFFFFFFF); //only looking at potential outposts which would be in the other players half of the board
            int Outposts = 0;
            while (PawnAttacks.GetData() > 0)
            {
                int square = PawnAttacks.LSB();
                if ((Pawns.GetData() & (0x1010101010101010UL << square % 8))==0) //is on a half open file
                {
                    Outposts++;
                }
                PawnAttacks.ClearBit(square);
            }
            return Outposts;
        }

        public static float EvaluateKingSafety(Board board, int Player)
        {
            int PawnDefenseScore = BitOperations.PopCount(board.Pieces[Player*6].GetData() & (0x7UL << (board.Pieces[Player * 6 + 5].LSB() - 1 + (Player == 0 ? 8 : -8))));
            return PawnDefenseScore;
        }

        public static float EvaluatePawnStructure(Bitboard Pawns, int Player)
        {
            float score = 0;
            score -= PawnDoubles(Pawns, Player) * 15;
            score -= PawnIsolated(Pawns, Player) * 15;
            //score += PawnChains(Pawns) * 3;
            score += OutpostEvaluation(Pawns, Player) * 10;
            //score -= WhiteBackwardsPawns
            return score;
        }

        public static float PawnChains(Bitboard Pawns) //counts how many pawns in the position are defended by other pawns, meaning they're in a chain
        {
            int ConnectedPawns = BitOperations.PopCount(Pawns.GetData() & ((Pawns.GetData() << 9) | (Pawns.GetData() << 7)));
            return ConnectedPawns;
        }

        public static float WhiteBackwardsPawns(Bitboard wPawns, Bitboard bPawns) //this needs testing
        {
            Bitboard stops = new Bitboard(wPawns.GetData() << 8); //squares ahead of the pawns
            Bitboard wSpans = new Bitboard(((wPawns.GetData() << 9) & (0x7F7F7F7F7F7F7F7F)) | ((wPawns.GetData() << 7) & (0xFEFEFEFEFEFEFEFE))); //squares attacked by white pawns
            Bitboard bSpans = new Bitboard(((bPawns.GetData() >> 9) & (0xFEFEFEFEFEFEFEFE)) | (bPawns.GetData() >> 7) & (0x7F7F7F7F7F7F7F7F)); //squares attacked by black pawns
            return BitOperations.PopCount(stops.GetData() & bSpans.GetData() & ~wSpans.GetData());
        }

        public static float PawnIsolated(Bitboard Pawns, int Player)
        {
            float score = 0;
            Bitboard Checks = new Bitboard(Pawns.GetData());
            for (int file = 1; file < 7; file++) // to not include the edge files
            {
                if ((Pawns.GetData() & ((0x0101010101010101UL << file-1) | (0x0101010101010101UL << file+1))) == 0 && (Pawns.GetData() & (0x0101010101010101UL << file)) != 0)
                {
                    score += 1;
                }
            }
            if (((Pawns.GetData() & (0x0101010101010101UL << 1)) == 0) & ((Pawns.GetData() & 0x0101010101010101UL) != 0)) //for the edge tiles, this needs different logic to stop overflow wrapping around the board
            {
                score += 1; //for the A-file
            }
            if (((Pawns.GetData() & (0x0101010101010101UL << 6)) == 0) & ((Pawns.GetData() & (0x0101010101010101UL<<7)) != 0)) // for the H file
            {
                score += 1;
            }
            return score;
        }

        public static float PawnDoubles(Bitboard Pawns, int Player) 
        {
            int doubles = 0;
            for (int file = 0; file < 8; file++) 
            {
                if (BitOperations.PopCount(Pawns.GetData() & (0x0101010101010101UL << file)) > 1) //checking for the number of pawns in the file using the essentially the active bits method without having to create a new bitboard object
                {
                    doubles++;
                }
            }
            return doubles;
        }

        public static int PawnMajorities(Bitboard WhitePawns, Bitboard BlackPawns, int Player) //not currently used but might implement later
        {
            Bitboard LeftWhite = new Bitboard(WhitePawns.GetData() & 0x0F0F0F0F0F0F0F0F);
            Bitboard LeftBlack = new Bitboard(BlackPawns.GetData() & 0x0F0F0F0F0F0F0F0F);
            Bitboard RightWhite = new Bitboard(WhitePawns.GetData() & 0xF0F0F0F0F0F0F0F0);
            Bitboard RightBlack = new Bitboard(BlackPawns.GetData() & 0xF0F0F0F0F0F0F0F0);
            int LeftMajority = LeftWhite.ActiveBits() - LeftBlack.ActiveBits();
            int RightMajority = RightWhite.ActiveBits() - RightBlack.ActiveBits();
            return (LeftMajority * LeftMajority * (LeftMajority > 0 ? 1 : -1)) + (RightMajority * RightMajority * (LeftMajority > 0 ? 1 : -1));
        }

        public static float Material(Board board)
        {
            float score = 0;
            for (int i = 0; i < 12; i++) // for each piece excluding king
            {
                score += board.Pieces[i].ActiveBits() * MaterialValues[i];
            }
            return score;
        }

        public static float WeightedMaterial(Board board)
        {
            float score = 0;
            float NewGamePhase = 0;
            for (int p = 0; p < 12; p++)
            {
                for (int i = 0; i < 64; i++)
                {
                    if (board.Pieces[p].IsBitSet(i)) 
                    {
                        score += MaterialValues[p];
                        NewGamePhase += Math.Abs(MaterialValues[p]);
                        if (p < 6) //white piece
                        {
                            score += PieceSquareTable[p][Evaluation.Flip[i]];
                        } else{ //black piece
                            score -= PieceSquareTable[p-6][i];
                        }
                    }
                }
            }
            board.GamePhase = NewGamePhase;
            return score;
        }

        public static void OrderMoves(Board board, Move[] Moves, Move HashMove, int Depth, Move PreviousMove)
        {
            List<(Move, float)> nonEmptyMoves = new List<(Move, float)>();
            foreach (var move in Moves)
            {
                if (move.GetData() != 0)
                {
                    nonEmptyMoves.Add((move, EvaluateMove(board, move, HashMove, Depth, PreviousMove)));
                }
            }
            nonEmptyMoves.Sort((move1, move2) => move2.Item2.CompareTo(move1.Item2)); //this is cheaper after null moves are filtered out
            for (int i = 0; i < nonEmptyMoves.Count; i++)
            {
                Moves[i] = nonEmptyMoves[i].Item1;
            }

            //Array.Sort(Moves, (move1, move2) => EvaluateMove(board, move2, HashMove, Depth).CompareTo(EvaluateMove(board, move1, HashMove, Depth)));
        }

        public static float EvaluateMove(Board board, Move move, Move HashMove, int Depth, Move PreviousMove)
        {
            float score = 0;
            if (move.GetData() == 0) return -10000000; // empty move
            if (move.GetData() == HashMove.GetData()) return 1000000;
            if (move.GetData() == KillerMoves[Depth][0].GetData()) score += 2500;
            if (move.GetData() == KillerMoves[Depth][1].GetData()) score += 2000;
            if (move.GetData() == CounterMoves[PreviousMove.GetStart(),PreviousMove.GetTarget()].GetData()) score += 1500;
            int HistoryScore = HistoryTable[move.GetStart(), move.GetTarget(), move.GetFlag()];
            if (HistoryScore > 1000) {HistoryScore =1000;HistoryTable[move.GetStart(), move.GetTarget(), move.GetFlag()] = 1000;}
            score += HistoryScore;
            int capture = move.GetCapture();
            if (capture != 7)
            {
                score += 10 * Math.Abs(MaterialValues[capture]);
                score -= Math.Abs(MaterialValues[move.GetPiece()]);
            }

            if ((move.GetFlag() & 0b1000) > 0) //move is a promotion
            {
                score += move.GetPiece() * 10;
            }
            int target = move.GetTarget();
            if (((target % 8 == 3) || (target % 8 == 4)) && ((target / 8 == 3) || (target / 8 == 4)))
            {
                score += (1000 - MaterialValues[move.GetPiece()]) / 4;
            }
            return score;
        }

        public static void UpdateHistoryTable(Move move, int Depth)
        {
            if (move.IsCapture()) return;
            HistoryTable[move.GetStart(), move.GetTarget(), move.GetFlag()] += Depth*Depth;
        }

        public static void ShiftKillerMoves()
        {
            for (int i = 2; i < 128; i++) 
            {
                KillerMoves[i] = new Move[2] {KillerMoves[i-2][0], KillerMoves[i-2][1]};
            }
        }

        public static void InitializeCounterMoves()
        {
            Move[,] CounterMoves = new Move[64,64];
        }

        public static void InitializeKillerMoves()
        {
            for (int i = 0; i < 128; i++)
            {
                KillerMoves[i] = new Move[2];
            }
        }

        // piece square tables used for evaluation

        public static int[] PawnTable = { 
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5, 10, 25, 25, 10,  5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5, -5,-10,  0,  0,-10, -5,  5,
            5, 10, 10,-20,-20, 10, 10,  5,
            0,  0,  0,  0,  0,  0,  0,  0};

        public static int[] KnightTable = {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50};
        
        public static int[] BishopTable = {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20};

        public static int[] RookTable = {
             0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            0,  0,  0,  5,  5,  0,  0,  0};

        public static int[] QueenTable = {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -5,  0,  5,  5,  5,  5,  0, -5,
            0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20};

        public static int[] KingTable = {
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
            20, 20,  0,  0,  0,  0, 20, 20,
            20, 30, 10,  0,  0, 10, 30, 20};
        
        public static int[] Flip = {
            56,  57,  58,  59,  60,  61,  62,  63,
            48,  49,  50,  51,  52,  53,  54,  55,
            40,  41,  42,  43,  44,  45,  46,  47,
            32,  33,  34,  35,  36,  37,  38,  39,
            24,  25,  26,  27,  28,  29,  30,  31,
            16,  17,  18,  19,  20,  21,  22,  23,
            8,   9,  10,  11,  12,  13,  14,  15,
            0,   1,   2,   3,   4,   5,   6,   7};

        public static int[][] PieceSquareTable = {PawnTable, KnightTable, BishopTable, RookTable, QueenTable, KingTable};
    }
}