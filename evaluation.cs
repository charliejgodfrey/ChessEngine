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
        public static int[] PasserValues = [28, 33, 41, 72, 177, 260];

        public static ulong NotA = ~0x0101010101010101UL;
        public static ulong NotH = ~0x8080808080808080UL;

        public static float avgMaterial = 0;
        public static float avgPositional = 0;
        public static float avgDiffer = 0;
        public static int count = 0;

        public static float Evaluate(Board board)
        {
            (ulong WhitePawnAttacks, ulong BlackPawnAttacks) = PawnAttacks(board);
            (float WhiteMiddleMobility, float WhiteEndMobility, ulong WhiteAttacks, float WhiteGamePhase, float WhiteMiddleGameMaterial, float WhiteEndGameMaterial) = EvaluateMobility(board, 0, BlackPawnAttacks);
            (float BlackMiddleMobility, float BlackEndMobility, ulong BlackAttacks, float BlackGamePhase, float BlackMiddleGameMaterial, float BlackEndGameMaterial) = EvaluateMobility(board, 1, WhitePawnAttacks);
            (float PawnPhase, float PawnMiddleScore, float PawnEndScore) = PawnMaterial(board);
            float PawnStructure = EvaluatePawnStructure(board);
            (float MiddleKingScore, float EndKingScore) = KingMaterial(board);
            (float MiddleKingSafety, float EndKingSafety) = EvaluateKingSafety(board, WhiteAttacks, BlackAttacks);

            float MutualDefenseBonus = BitOperations.PopCount(board.WhitePieces & WhiteAttacks) - BitOperations.PopCount(board.BlackPieces & BlackAttacks);

            board.GamePhase = WhiteGamePhase + BlackGamePhase + PawnPhase; //20000 for the kings;

            float Score = (((WhiteMiddleMobility-BlackMiddleMobility+WhiteMiddleGameMaterial-BlackMiddleGameMaterial + PawnMiddleScore + MiddleKingScore + MiddleKingSafety) * (board.GamePhase)) + (WhiteEndMobility-BlackEndMobility+WhiteEndGameMaterial-BlackEndGameMaterial+PawnEndScore + EndKingSafety + EndKingScore) * (8000-board.GamePhase))/8000; //this is disgusting for efficiency

            return (float)( 
            Score + 
            PawnStructure + 
            3 * MutualDefenseBonus
            );
            //return board.Eval;
        }

        public static (float, float) EvaluateKingSafety(Board board, ulong WhiteAttacks, ulong BlackAttacks) //responsible for evaluating king safety
        {
            int WhiteKing = BitOperations.TrailingZeroCount(board.Pieces[5]);
            int BlackKing = BitOperations.TrailingZeroCount(board.Pieces[11]);
            //escape squares
            int WhiteEscapeSquares = BitOperations.PopCount(PreComputeData.KingAttackBitboards[WhiteKing].GetData() & ~BlackAttacks);
            int BlackEscapeSquares = BitOperations.PopCount(PreComputeData.KingAttackBitboards[BlackKing].GetData() & ~WhiteAttacks);

            //pawn shield
            int WhiteShield = BitOperations.PopCount(PreComputeData.KingShieldMasks[0,WhiteKing] & board.Pieces[0]);
            int BlackShield = BitOperations.PopCount(PreComputeData.KingShieldMasks[1,BlackKing] & board.Pieces[6]);

            //near open files
            int WhiteKingFile = WhiteKing / 8;
            int BlackKingFile = BlackKing / 8;
            int WhiteOpens = 0;
            int BlackOpens = 0;
            
            int PawnCount = BitOperations.PopCount((0x0101010101010101UL << (WhiteKingFile)) & (board.Pieces[0] | board.Pieces[6]));
            WhiteOpens += PawnCount == 0 ? 30 : PawnCount == 1 ? 16 : 0;
            if (WhiteKingFile != 7) //preventing wrap around
            {
                PawnCount = BitOperations.PopCount((0x0101010101010101UL << (WhiteKingFile + 1)) & (board.Pieces[0] | board.Pieces[6]));
                WhiteOpens += PawnCount == 0 ? 15 : PawnCount == 1 ? 5 : 0;
            }
            if (WhiteKingFile != 0) //preventing wrap around
            {
                PawnCount = BitOperations.PopCount((0x0101010101010101UL << (WhiteKingFile - 1)) & (board.Pieces[0] | board.Pieces[6]));
                WhiteOpens += PawnCount == 0 ? 15 : PawnCount == 1 ? 5 : 0;
            }

            PawnCount = BitOperations.PopCount((0x0101010101010101UL << (BlackKingFile)) & (board.Pieces[0] | board.Pieces[6]));
            BlackOpens += PawnCount == 0 ? 30 : PawnCount == 1 ? 16 : 0;
            if (BlackKingFile != 7) //preventing wrap around
            {
                PawnCount = BitOperations.PopCount((0x0101010101010101UL << (BlackKingFile + 1)) & (board.Pieces[0] | board.Pieces[6]));
                BlackOpens += PawnCount == 0 ? 15 : PawnCount == 1 ? 5 : 0;
            }
            if (BlackKingFile != 0) //preventing wrap around
            {
                PawnCount = BitOperations.PopCount(((0x0101010101010101UL << (BlackKingFile - 1)) & (board.Pieces[0] | board.Pieces[6])));
                BlackOpens += PawnCount == 0 ? 15 : PawnCount == 1 ? 5 : 0;
            }

            //the stuff to do with piece proximity is calculated in the mobility function
            float WhiteMiddleScore = 8*WhiteEscapeSquares + WhiteShield*10 - WhiteOpens;
            float BlackMiddleScore = 8*BlackEscapeSquares + BlackShield*10 - BlackOpens;

            float WhiteEndScore = 8*WhiteEscapeSquares; //king activity becomes much more important
            float BlackEndScore = 8*BlackEscapeSquares; //but we don't want to get mated either


            return (WhiteMiddleScore - BlackMiddleScore, WhiteEndScore-BlackEndScore);
        }

        public static (ulong, ulong) PawnAttacks(Board board)
        {
            ulong WhitePawnAttacks = (((board.Pieces[0] & NotA) << 7) | ((board.Pieces[0] & NotH) << 9));
            ulong BlackPawnAttacks = (((board.Pieces[6] & NotA) >> 9) | ((board.Pieces[6] & NotH) >> 7));
            return (WhitePawnAttacks, BlackPawnAttacks);
        }

        public static (float, float) KingMaterial(Board board)
        {
            int WhiteKing = BitOperations.TrailingZeroCount(board.Pieces[5]);
            int BlackKing = BitOperations.TrailingZeroCount(board.Pieces[11]);

            return (mg_tables[5][Flip[WhiteKing]] - mg_tables[5][BlackKing], eg_tables[5][Flip[WhiteKing]] - eg_tables[5][BlackKing]);
        }

        public static float EvaluatePawnStructure(Board board)
        {
            float PawnEvaluation = PawnEvaluationTable.Retrieve(board.PawnZobrist);
            
            if (PawnEvaluation != 0) //in the table
            {
                return PawnEvaluation;
            }
            //should implement this but need to create a specialist zobrist just for a positions pawn structure


            int IsoPawns = IsolatedPawns(board.Pieces[0]) - IsolatedPawns(board.Pieces[6]);
            (int middle, int end) = PassedPawns(board);
            int BackPawns = BackwardsPawns(board);
            int ProtectedPawns = PawnChains(board);

            float MiddleGameScore = IsoPawns*-5 + middle + BackPawns*-9;
            float EndGameScore = IsoPawns*-15 + end + BackPawns*-24;
            float Evaluation = ((MiddleGameScore * (board.GamePhase-20000) + EndGameScore * (28000 - board.GamePhase))/8000);
            PawnEvaluationTable.Store(board.PawnZobrist, Evaluation);

            return Evaluation;
        }

        public static int PawnChains(Board board)
        {
            ulong WhitePawnAttacks = (((board.Pieces[0] & NotA) << 7) | ((board.Pieces[0] & NotH) << 9));
            ulong BlackPawnAttacks = (((board.Pieces[6] & NotA) << 7) | ((board.Pieces[6] & NotH) << 9));

            int ConnectedPawns = BitOperations.TrailingZeroCount(WhitePawnAttacks & board.Pieces[0]) - BitOperations.TrailingZeroCount(BlackPawnAttacks & board.Pieces[6]);
            return ConnectedPawns;
        }

        public static int BackwardsPawns(Board board)
        {
            ulong WhitePawnAttacks = ((board.Pieces[0] & NotA) << 7) | ((board.Pieces[0] & NotH) << 9);
            ulong BlackPawnAttacks = ((board.Pieces[0] & NotA) >> 9) | ((board.Pieces[0] & NotH) >> 7);

            ulong WhiteBackwards = ((board.Pieces[0]<<8) & BlackPawnAttacks & ~WhitePawnAttacks) >> 8;
            ulong BlackBackwards = ((board.Pieces[6]>>8) & WhitePawnAttacks & ~BlackPawnAttacks) << 8;
            return (BitOperations.PopCount(WhiteBackwards) - BitOperations.PopCount(BlackBackwards));
        }

        public static int IsolatedPawns(ulong Pawns)
        {
            ulong PawnLocations = Pawns;
            int Count = 0;
            while (Pawns > 0)
            {
                int square = BitOperations.TrailingZeroCount(Pawns);
                if ((PreComputeData.IsolationMasks[square] & PawnLocations) == 0) Count++;
                Pawns ^= (1UL << square);
            }
            return Count;
        }

        public static (int, int) PassedPawns(Board board)
        {
            ulong WhitePawns = board.Pieces[0];
            ulong BlackPawns = board.Pieces[6];
            int Count = 0;
            int EndGameScore = 0;
            int MiddleGameScore = 0;
            while (WhitePawns > 0)
            {
                int square = BitOperations.TrailingZeroCount(WhitePawns);
                if ((PreComputeData.PasserMasks[0,square] & BlackPawns) == 0)
                {
                    Count++;
                    EndGameScore += PasserValues[square / 8 - 1]; //closer to promoting is better
                    MiddleGameScore += 10;
                }
                WhitePawns ^= (1UL << square);
            }
            WhitePawns = board.Pieces[0]; //so the pawns aren't deleted
            while (BlackPawns > 0)
            {
                int square = BitOperations.TrailingZeroCount(BlackPawns);
                if ((PreComputeData.PasserMasks[1,square] & WhitePawns) == 0)
                {
                    Count--;
                    EndGameScore -= PasserValues[6 - square / 8]; //closer to promoting is better
                    MiddleGameScore -= 10;
                }
                BlackPawns ^= (1UL << square);
            }

            return (MiddleGameScore, EndGameScore);
        }

        public static (float, float, float) PawnMaterial(Board board) //returns the game phase from pawns and the weighted material of just pawns
        {
            float MiddleScore = 0; //stuff for the weighted material
            float EndScore = 0;
            float NewGamePhase = 0;

            ulong Pawns = board.Pieces[0];
            while (Pawns > 0) //for each white pawn
            {
                int i = BitOperations.TrailingZeroCount(Pawns);
                Pawns &= ~(1UL << i);
                NewGamePhase += MaterialValues[0]; //stuff for the weighted material
                MiddleScore += MaterialValues[0];
                EndScore += MaterialValues[0];
                MiddleScore += mg_tables[0][Evaluation.Flip[i]];
                EndScore += eg_tables[0][Evaluation.Flip[i]];
            }
            Pawns = board.Pieces[6];
            while (Pawns > 0) //for each white pawn
            {
                int i = BitOperations.TrailingZeroCount(Pawns);
                Pawns &= ~(1UL << i);
                NewGamePhase += MaterialValues[0]; //stuff for the weighted material
                MiddleScore -= MaterialValues[0];
                EndScore -= MaterialValues[0];
                MiddleScore -= mg_tables[0][i];
                EndScore -= eg_tables[0][i];
            }

            return (NewGamePhase, MiddleScore, EndScore);
        }

        public static (float, float, ulong, float, float, float) EvaluateMobility(Board board, int Player, ulong PawnAttacks)
        {
            float NewGamePhase = 0;
            float MiddleScore = 0; //stuff for the weighted material
            float EndScore = 0;

            ulong Undefended = ~PawnAttacks; //squares not attacked by opponents pawns
            ulong AttackedSquares = 0UL;
            int ColourAdd = Player * 6;
            ulong FriendlyPieces = (Player == 0 ? board.WhitePieces : board.BlackPieces);

            float OpenFileScore = 0;
            float KingProximityDefense = 0;
            float KingProximityAttack = 0;
            int FriendlyKing = BitOperations.TrailingZeroCount(board.Pieces[5+ColourAdd]);
            int EnemyKing = BitOperations.TrailingZeroCount(board.Pieces[ColourAdd == 6 ? 5 : 11]);

            int KnightMoves = 0;
            ulong Knights = board.Pieces[ColourAdd+1];
            while (Knights > 0) //for each knight
            {
                int square = BitOperations.TrailingZeroCount(Knights);
                ulong Attacks = PreComputeData.KnightAttackBitboards[square].GetData();
                AttackedSquares |= Attacks;
                Attacks &= ~FriendlyPieces;
                KnightMoves += 2*BitOperations.PopCount(Attacks & Undefended);
                Knights &= ~(1UL << square);

                KingProximityDefense += distance(square, FriendlyKing) < 3 ? 10 : 0;
                KingProximityAttack += distance(square, EnemyKing) < 4 ? 8 : 0;

                NewGamePhase += MaterialValues[1]; //stuff for the weighted material
                MiddleScore += MaterialValues[1];
                EndScore += MaterialValues[1];
                if (Player == 0)
                {
                    MiddleScore += mg_tables[1][Evaluation.Flip[square]];
                    EndScore += eg_tables[1][Evaluation.Flip[square]];
                } else 
                {
                    MiddleScore += mg_tables[1][square];
                    EndScore += eg_tables[1][square];
                }
            }
            
            int BishopMoves = 0;
            ulong Bishops = board.Pieces[ColourAdd+2];
            while (Bishops > 0) //for each bishop
            {
                int square = BitOperations.TrailingZeroCount(Bishops);
                ulong Attacks = MoveGenerator.GenerateBishopAttacks(board, square);
                AttackedSquares |= Attacks;
                Attacks &= ~FriendlyPieces;
                BishopMoves += BitOperations.PopCount(Attacks) + BitOperations.PopCount(Attacks&Undefended); //reduced bonus for when the seen squares are defended by a pawn 
                Bishops &= ~(1UL << square);

                KingProximityDefense += distance(square, FriendlyKing) < 3 ? 10 : 0;
                KingProximityAttack += distance(square, EnemyKing) < 4 ? 15 : 0;

                NewGamePhase += MaterialValues[2]; //stuff for the weighted material
                MiddleScore += MaterialValues[2];
                EndScore += MaterialValues[2];
                if (Player == 0)
                {
                    MiddleScore += mg_tables[2][Evaluation.Flip[square]];
                    EndScore += eg_tables[2][Evaluation.Flip[square]];
                } else 
                {
                    MiddleScore += mg_tables[2][square];
                    EndScore += eg_tables[2][square];
                }
            }

            int RookMoves = 0;
            ulong Rooks = board.Pieces[ColourAdd+3];
            while (Rooks > 0) //for each rook
            {
                int square = BitOperations.TrailingZeroCount(Rooks);
                ulong Attacks = MoveGenerator.GenerateRookAttacks(board, square);
                AttackedSquares |= Attacks;
                Attacks &= ~FriendlyPieces;
                RookMoves += BitOperations.PopCount(Attacks) + BitOperations.PopCount(Attacks&Undefended);
                Rooks &= ~(1UL << square);

                KingProximityDefense += distance(square, FriendlyKing) < 4 ? 8 : 0;
                KingProximityAttack += distance(square, EnemyKing) < 6 ? 20 : 0;

                int PawnsOnFile = BitOperations.PopCount((board.Pieces[0] | board.Pieces[6]) & (0x0101010101010101UL << (square & 8)));
                OpenFileScore += PawnsOnFile == 0 ? 40 : PawnsOnFile == 1 ? 18 : 0;

                NewGamePhase += MaterialValues[3]; //stuff for the weighted material
                MiddleScore += MaterialValues[3];
                EndScore += MaterialValues[3];
                if (Player == 0)
                {
                    MiddleScore += mg_tables[3][Evaluation.Flip[square]];
                    EndScore += eg_tables[3][Evaluation.Flip[square]];
                } else 
                {
                    MiddleScore += mg_tables[3][square];
                    EndScore += eg_tables[3][square];
                }
            }

            int QueenMoves = 0;
            ulong Queens = board.Pieces[ColourAdd+4];
            while (Queens > 0) //for each queen
            {
                int square = BitOperations.TrailingZeroCount(Queens);
                ulong Attacks = (MoveGenerator.GenerateBishopAttacks(board, square) | MoveGenerator.GenerateRookAttacks(board, square));
                AttackedSquares |= Attacks;
                Attacks &= ~FriendlyPieces;
                QueenMoves += BitOperations.PopCount(Attacks) + BitOperations.PopCount(Attacks&Undefended);
                Queens &= ~(1UL << square);

                KingProximityDefense += distance(square, FriendlyKing) < 3 ? 10 : 0;
                KingProximityAttack += distance(square, EnemyKing) < 4 ? 45 : 0;

                NewGamePhase += MaterialValues[4]; //stuff for the weighted material
                MiddleScore += MaterialValues[4];
                EndScore += MaterialValues[4];
                if (Player == 0)
                {
                    MiddleScore += mg_tables[4][Evaluation.Flip[square]];
                    EndScore += eg_tables[4][Evaluation.Flip[square]];
                } else 
                {
                    MiddleScore += mg_tables[4][square];
                    EndScore += eg_tables[4][square];
                }
            }

            AttackedSquares |= PreComputeData.KingAttackBitboards[FriendlyKing].GetData(); //adds the king attacked squares to the attacked squares
            ulong EastCapture;
            ulong WestCapture;
            if (Player == 0)
            {
                EastCapture = (board.Pieces[0] & NotA) << 7;
                WestCapture = (board.Pieces[0] & NotH) << 9;
            } else {
                EastCapture = (board.Pieces[6] & NotA) >> 9;
                WestCapture = (board.Pieces[6] & NotH) >> 7;
            }
            ulong ForwardMoves = ((board.Pieces[ColourAdd] << 8) & ~(Player == 0 ? board.BlackPieces : board.WhitePieces));
            int PawnMoves = BitOperations.PopCount(ForwardMoves);
            AttackedSquares |= (WestCapture | EastCapture);

            int CentreControl = 2*BitOperations.PopCount(AttackedSquares & 0x0000003C3C000000);
            float MiddleGameScore = (float)(2*(KnightMoves + BishopMoves + 2*CentreControl) + PawnMoves + 0.5*(RookMoves) + QueenMoves + OpenFileScore + KingProximityAttack + KingProximityDefense/3); //weighted more towards minor piece mobility
            float EndGameScore = (float)(KnightMoves + BishopMoves + CentreControl + 2 * (PawnMoves + RookMoves + QueenMoves) + OpenFileScore/1.5f + KingProximityDefense/8 + KingProximityAttack/2);
            return (MiddleGameScore, EndGameScore, AttackedSquares, NewGamePhase, MiddleScore, EndScore); //interpolated depending on material count
        }

        public static int distance(int index1, int index2)
        {
            int differ = Math.Abs(index1 - index2);
            return (differ / 8) + (differ % 8);
        }

        public static float Material(Board board)
        {
            float score = 0;
            for (int i = 0; i < 12; i++) // for each piece excluding king
            {
                score += BitOperations.PopCount(board.Pieces[i]) * MaterialValues[i];
            }
            return score;
        }

        public static float WeightedMaterial(Board board)
        {
            float NewGamePhase = 0;
            float MiddleScoreW = 0;
            float MiddleScoreB = 0;
            float EndScoreW = 0;
            float EndScoreB = 0;
            for (int p = 0; p < 12; p++)
            {
                ulong pieces = board.Pieces[p];
                while (pieces > 0)
                {
                    int i = BitOperations.TrailingZeroCount(pieces);
                    if (p < 6) //white piece
                    {
                        NewGamePhase += Math.Abs(MaterialValues[p]);
                        MiddleScoreW += MaterialValues[p];
                        EndScoreW += MaterialValues[p];
                        MiddleScoreW += mg_tables[p][Evaluation.Flip[i]];
                        EndScoreW += eg_tables[p][Evaluation.Flip[i]];
                    } else{ //black piece
                        NewGamePhase += Math.Abs(MaterialValues[p-6]);
                        MiddleScoreB += MaterialValues[p-6];
                        EndScoreB += MaterialValues[p-6];
                        MiddleScoreB += mg_tables[p-6][i];
                        EndScoreB += eg_tables[p-6][i];
                    }
                    pieces ^= 1UL << i;
                }
            }
            board.GamePhase = (float)NewGamePhase;
            int mgPhase = (int)NewGamePhase-20000;
            int egPhase = 8000 - mgPhase;
            return ((MiddleScoreW-MiddleScoreB)*mgPhase + (EndScoreW-EndScoreB) * egPhase)/8000;
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
        
        //second set of piece square tables
        public static int[] mg_pawn_table = {
            0,   0,   0,   0,   0,   0,  0,   0,
            98, 134,  61,  95,  68, 126, 34, -11,
            -6,   7,  26,  31,  65,  56, 25, -20,
            -14,  13,   6,  21,  23,  12, 17, -23,
            -27,  -2,  -5,  12,  17,   6, 10, -25,
            -26,  -4,  -4, -10,   3,   3, 33, -12,
            -35,  -1, -20, -23, -15,  24, 38, -22,
            0,   0,   0,   0,   0,   0,  0,   0,
        };

        public static int[] eg_pawn_table = {
            0,   0,   0,   0,   0,   0,   0,   0,
            178, 173, 158, 134, 147, 132, 165, 187,
            94, 100,  85,  67,  56,  53,  82,  84,
            32,  24,  13,   5,  -2,   4,  17,  17,
            13,   9,  -3,  -7,  -7,  -8,   3,  -1,
            4,   7,  -6,   1,   0,  -5,  -1,  -8,
            13,   8,   8,  10,  13,   0,   2,  -7,
            0,   0,   0,   0,   0,   0,   0,   0,
        };

        public static int[] mg_knight_table = {
            -167, -89, -34, -49,  61, -97, -15, -107,
            -73, -41,  72,  36,  23,  62,   7,  -17,
            -47,  60,  37,  65,  84, 129,  73,   44,
            -9,  17,  19,  53,  37,  69,  18,   22,
            -13,   4,  16,  13,  28,  19,  21,   -8,
            -23,  -9,  12,  10,  19,  17,  25,  -16,
            -29, -53, -12,  -3,  -1,  18, -14,  -19,
            -105, -21, -58, -33, -17, -28, -19,  -23,
        };

        public static int[] eg_knight_table = {
            -58, -38, -13, -28, -31, -27, -63, -99,
            -25,  -8, -25,  -2,  -9, -25, -24, -52,
            -24, -20,  10,   9,  -1,  -9, -19, -41,
            -17,   3,  22,  22,  22,  11,   8, -18,
            -18,  -6,  16,  25,  16,  17,   4, -18,
            -23,  -3,  -1,  15,  10,  -3, -20, -22,
            -42, -20, -10,  -5,  -2, -20, -23, -44,
            -29, -51, -23, -15, -22, -18, -50, -64,
        };

        public static int[] mg_bishop_table = {
            -29,   4, -82, -37, -25, -42,   7,  -8,
            -26,  16, -18, -13,  30,  59,  18, -47,
            -16,  37,  43,  40,  35,  50,  37,  -2,
            -4,   5,  19,  50,  37,  37,   7,  -2,
            -6,  13,  13,  26,  34,  12,  10,   4,
            0,  15,  15,  15,  14,  27,  18,  10,
            4,  15,  16,   0,   7,  21,  33,   1,
            -33,  -3, -14, -21, -13, -12, -39, -21,
        };

        public static int[] eg_bishop_table = {
            -14, -21, -11,  -8, -7,  -9, -17, -24,
            -8,  -4,   7, -12, -3, -13,  -4, -14,
            2,  -8,   0,  -1, -2,   6,   0,   4,
            -3,   9,  12,   9, 14,  10,   3,   2,
            -6,   3,  13,  19,  7,  10,  -3,  -9,
            -12,  -3,   8,  10, 13,   3,  -7, -15,
            -14, -18,  -7,  -1,  4,  -9, -15, -27,
            -23,  -9, -23,  -5, -9, -16,  -5, -17,
        };

        public static int[] mg_rook_table = {
            32,  42,  32,  51, 63,  9,  31,  43,
            27,  32,  58,  62, 80, 67,  26,  44,
            -5,  19,  26,  36, 17, 45,  61,  16,
            -24, -11,   7,  26, 24, 35,  -8, -20,
            -36, -26, -12,  -1,  9, -7,   6, -23,
            -45, -25, -16, -17,  3,  0,  -5, -33,
            -44, -16, -20,  -9, -1, 11,  -6, -71,
            -19, -13,   1,  17, 16,  7, -37, -26,
        };

        public static int[] eg_rook_table = {
            13, 10, 18, 15, 12,  12,   8,   5,
            11, 13, 13, 11, -3,   3,   8,   3,
            7,  7,  7,  5,  4,  -3,  -5,  -3,
            4,  3, 13,  1,  2,   1,  -1,   2,
            3,  5,  8,  4, -5,  -6,  -8, -11,
            -4,  0, -5, -1, -7, -12,  -8, -16,
            -6, -6,  0,  2, -9,  -9, -11,  -3,
            -9,  2,  3, -1, -5, -13,   4, -20,
        };

        public static int[] mg_queen_table = {
            -28,   0,  29,  12,  59,  44,  43,  45,
            -24, -39,  -5,   1, -16,  57,  28,  54,
            -13, -17,   7,   8,  29,  56,  47,  57,
            -27, -27, -16, -16,  -1,  17,  -2,   1,
            -9, -26,  -9, -10,  -2,  -4,   3,  -3,
            -14,   2, -11,  -2,  -5,   2,  14,   5,
            -35,  -8,  11,   2,   8,  15,  -3,   1,
            -1, -18,  -9,  10, -15, -25, -31, -50,
        };

        public static int[] eg_queen_table = {
            -9,  22,  22,  27,  27,  19,  10,  20,
            -17,  20,  32,  41,  58,  25,  30,   0,
            -20,   6,   9,  49,  47,  35,  19,   9,
            3,  22,  24,  45,  57,  40,  57,  36,
            -18,  28,  19,  47,  31,  34,  39,  23,
            -16, -27,  15,   6,   9,  17,  10,   5,
            -22, -23, -30, -16, -16, -23, -36, -32,
            -33, -28, -22, -43,  -5, -32, -20, -41,
        };

        public static int[] mg_king_table = {
            -65,  23,  16, -15, -56, -34,   2,  13,
            29,  -1, -20,  -7,  -8,  -4, -38, -29,
            -9,  24,   2, -16, -20,   6,  22, -22,
            -17, -20, -12, -27, -30, -25, -14, -36,
            -49,  -1, -27, -39, -46, -44, -33, -51,
            -14, -14, -22, -46, -44, -30, -15, -27,
            1,   7,  -8, -64, -43, -16,   9,   8,
            -15,  36,  12, -54,   8, -28,  24,  14,
        };

        public static int[] eg_king_table = {
            -74, -35, -18, -18, -11,  15,   4, -17,
            -12,  17,  14,  17,  17,  38,  23,  11,
            10,  17,  23,  15,  20,  45,  44,  13,
            -8,  22,  24,  27,  26,  33,  26,   3,
            -18,  -4,  21,  24,  27,  23,   9, -11,
            -19,  -3,  11,  21,  23,  16,   7,  -9,
            -27, -11,   4,  13,  14,   4,  -5, -17,
            -53, -34, -21, -11, -28, -14, -24, -43
        };

        public static int[][] mg_tables = {
            mg_pawn_table,
            mg_knight_table,
            mg_bishop_table,
            mg_rook_table,
            mg_queen_table,
            mg_king_table,
        };

        public static int[][] eg_tables = {
            eg_pawn_table,
            eg_knight_table,
            eg_bishop_table,
            eg_rook_table,
            eg_queen_table,
            eg_king_table,
        };
        //original piece square tables

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