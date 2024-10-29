//this is where all the move generation will be done
using System;
using System.Collections.Generic;
using System.Numerics;


namespace ChessEngine 
{
    public static class MoveGenerator
    //the move arrays are initialised with length 218 as this is the most possible moves in any chess position
    {
        public static int normal = 0;
        public static int fullcheck = 0;
        public static (bool, Move[]) GenerateMoves(Board board, bool onlyCaptures = false)
        {
            Move[] Moves = new Move[218];
            int MoveNumber = 0;
            //Console.WriteLine("turn: " + board.ColourToMove);
            (bool Check, bool DoubleCheck, ulong Blocks) = FindCheck(board, board.ColourToMove);
            
            if (DoubleCheck)
            {
                MoveNumber = GenerateKingMoves(board, Moves, MoveNumber, Blocks, onlyCaptures); //in double check the king has to move
            } else {
                //Blocks = ulong.MaxValue;
                //(ulong Pins, ulong[] PinMaintains) = PotentialPins(board, board.ColourToMove);
                MoveNumber = GenerateRookMoves(board, Moves, MoveNumber, Blocks, onlyCaptures);
                MoveNumber = GenerateBishopMoves(board, Moves, MoveNumber, Blocks, onlyCaptures);
                MoveNumber = GenerateQueenMoves(board, Moves, MoveNumber, Blocks, onlyCaptures);
                MoveNumber = GeneratePawnMoves(board, Moves, MoveNumber, Blocks, onlyCaptures);
                MoveNumber = GenerateKingMoves(board, Moves, MoveNumber, Blocks, onlyCaptures); 
                MoveNumber = GenerateKnightMoves(board, Moves, MoveNumber, Blocks, onlyCaptures);
            }
            //Console.WriteLine("turn: " + board.ColourToMove);

            //if (!onlyCaptures) MoveNumber = CheckCastle(board, Moves, MoveNumber);
            Moves = FilterIllegalMoves(board, Moves, Check);
            //Console.WriteLine("turn: " + board.ColourToMove);
            return (Check, Moves);
        }

        public static Move[] FilterIllegalMoves(Board board, Move[] Moves, bool Check) //assumes it is the turn of whoevers moves they are
        {
            Move[] LegalMoves = new Move[218];
            int MoveCount = 0;
            bool IsInCheck = Check;
            (ulong Pins, ulong[] PinMaintains) = PotentialPins(board, board.ColourToMove);
            for (int i = 0; i < 218; i++) //for each sudo legal move
            {
                if (Moves[i].GetData() == 0) break; //done all moves
                if (Moves[i].GetPiece() == 5 || Moves[i].GetFlag() == 0b0101)
                {
                    if (CheckLegal(board, Moves[i])) {
                        LegalMoves[MoveCount] = Moves[i];
                        MoveCount++;
                    } 
                }
                else if (((Pins & (1UL << Moves[i].GetStart())) == 0)) { //doesn't need to be checked thoroughly
                    //Moves[i].PrintMove();
                    LegalMoves[MoveCount] = Moves[i];
                    MoveCount++;
                    normal++;
                } else {
                    normal++;
                    // Console.WriteLine("1");
                    for (int p = 0; p < 8; p++) 
                    {
                        if ((PinMaintains[p] & (1Ul << Moves[i].GetStart())) > 0)
                        {
                            //Console.WriteLine("2");
                            if ((PinMaintains[p] & (1UL << Moves[i].GetTarget())) > 0)
                            {
                                LegalMoves[MoveCount] = Moves[i];
                                MoveCount++;
                            }
                            break;
                        }
                    }
                }
            }
            return LegalMoves;
        }

        public static (ulong, ulong[]) PotentialPins(Board board, int Player) //pieces blocking lines of sight to the king, player is the player who's pins are being detected
        {
            ulong Pins = 0UL;
            ulong[] StillPinnedSquares = [0Ul, 0Ul, 0Ul, 0Ul, 0UL, 0UL, 0UL, 0UL]; //places that pieces could move to which would maintain each pin (not put the king in check)
            int KingSquare = BitOperations.TrailingZeroCount(board.Pieces[Player * 6 + 5]);
            int Enemey = (Player == 0 ? 1 : 0);
            for (int d = 0; d < 4; d++) //rook and queen pins 
            {
                ulong Pinner = PreComputeData.KingRays[KingSquare,d] & (board.Pieces[Enemey * 6 + 4] | board.Pieces[Enemey * 6 + 3]);
                while (Pinner > 0)
                {
                    int index = BitOperations.TrailingZeroCount(Pinner);
                    ulong blockers = PreComputeData.KingRays[KingSquare,d] & PreComputeData.KingRays[index,d+(d%2 == 0?1:-1)] & board.OccupiedSquares; 
                    if (BitOperations.PopCount(blockers) == 1) 
                    {
                        Pins |= blockers;
                        int BlockerIndex = BitOperations.TrailingZeroCount(blockers);
                        StillPinnedSquares[d] = PreComputeData.KingRays[BlockerIndex,d] & ~(PreComputeData.KingRays[index, d]) | (1Ul << BlockerIndex);
                    }
                    Pinner &= ~(1Ul << index);
                }
            }
            for (int d = 4; d < 8; d++) //bishop and queen pins 
            {
                ulong Pinner = PreComputeData.KingRays[KingSquare,d] & (board.Pieces[Enemey * 6 + 4] | board.Pieces[Enemey * 6 + 2]);
                while (Pinner > 0)
                {
                    int index = BitOperations.TrailingZeroCount(Pinner);
                    ulong blockers = PreComputeData.KingRays[KingSquare,d] & PreComputeData.KingRays[index,d+(d%2 == 0?1:-1)] & board.OccupiedSquares; //will always be at least 2 intersected for the king and pinning piece
                    if (BitOperations.PopCount(blockers) == 1) {
                        Pins |= blockers;
                        int BlockerIndex = BitOperations.TrailingZeroCount(blockers);
                        StillPinnedSquares[d] = PreComputeData.KingRays[BlockerIndex,d] & ~(PreComputeData.KingRays[index, d]) | (1Ul << BlockerIndex);
                    }
                    Pinner &= ~(1Ul << index);
                }
            }
            Pins &= (Player == 0 ? board.WhitePieces : board.BlackPieces);//don't care about the enemeys pieces
            //Bitboard PinBitboard = new Bitboard(Pins);
            Pins &= ~(1UL << KingSquare); //king square is never pinned
            // for (int i = 0; i < 8; i++)
            // {
            //     (new Bitboard(StillPinnedSquares[i])).PrintData();
            // }
            return (Pins, StillPinnedSquares);
        }

        public static bool InCheck(Board board, int Player)
        {
            int ColourAdd = (Player == 0 ? 6 : 0);
            int KingSquare = BitOperations.TrailingZeroCount(board.Pieces[5+Player*6]);

            ulong KnightAttacks = PreComputeData.KnightAttackBitboards[KingSquare].GetData(); // where a knight could attack the king from
            if ((KnightAttacks & board.Pieces[1 + ColourAdd]) != 0)
            {
                return true;
            }

            ulong BishopAttacks = GenerateBishopAttacks(board, KingSquare);
            if ((BishopAttacks & board.Pieces[2 + ColourAdd]) != 0)
            { 
                return true;
            }

            ulong RookAttacks = GenerateRookAttacks(board, KingSquare);
            if ((RookAttacks & board.Pieces[3 + ColourAdd]) != 0) 
            {
                return true;
            }

            if (((RookAttacks | BishopAttacks) & board.Pieces[4 + ColourAdd]) != 0) 
            { 
                return true;
            }

            if ((((board.Pieces[ColourAdd] & (1UL << (KingSquare + 1 + (Player == 1 ? -8 : 8))))!=0) && KingSquare % 8 != 7) || (((board.Pieces[ColourAdd] & (1UL <<(KingSquare - 1 + (Player == 1 ? -8 : 8))))!=0) && KingSquare % 8 != 0)) // checking if either of the attackable squares of the king from pawns are occupied, don't need to worry about overflow because no pawns on the 1st and 8th rank
            {
                return true;
            }

            if ((PreComputeData.KingAttackBitboards[KingSquare].GetData() & board.Pieces[5+ ColourAdd]) != 0)
            {
                return true;
            }
            return false;
        }

        public static (bool, bool, ulong) FindCheck(Board board, int Player)
        {
            int ColourAdd = (Player == 0 ? 6 : 0);
            int KingSquare = BitOperations.TrailingZeroCount(board.Pieces[5+Player*6]);
            bool DoubleCheck = false;
            bool Check = false;
            ulong Checkers = 0UL;

            ulong KnightAttacks = PreComputeData.KnightAttackBitboards[KingSquare].GetData(); // where a knight could attack the king from
            ulong Overlap = KnightAttacks & board.Pieces[1 + ColourAdd];
            if (Overlap != 0)
            {
                Checkers |= (1UL << BitOperations.TrailingZeroCount(Overlap));
                //return (true, false, Checkers);
                //if (Check) return (true, true, Checkers);
                Check = true;
            }

            ulong BishopAttacks = GenerateBishopAttacks(board, KingSquare);
            Overlap = (BishopAttacks & (board.Pieces[2 + ColourAdd] | board.Pieces[4+ColourAdd]));
            if (Overlap != 0)
            {
                int Bishop = BitOperations.TrailingZeroCount(Overlap);
                Checkers |= ((BishopAttacks & GenerateBishopAttacks(board, Bishop)) | (1UL << Bishop));
                if (Check) return (true, true, (Checkers));
                Check = true;
            }

            ulong RookAttacks = GenerateRookAttacks(board, KingSquare);
            Overlap = (RookAttacks & (board.Pieces[3 + ColourAdd] | board.Pieces[4 + ColourAdd]));
            if (Overlap != 0) 
            {
                int Rook = BitOperations.TrailingZeroCount(Overlap); //could also be a queen
                Checkers |= (RookAttacks & GenerateRookAttacks(board, Rook)) | (1UL << Rook);
                if (Check) return (true, true, (Checkers));
                Check = true;
            }

            if ((((board.Pieces[ColourAdd] & (1UL << (KingSquare + 1 + (Player == 1 ? -8 : 8)))) != 0) && (KingSquare % 8 != 7))) 
            {
                Checkers |= 1UL << (KingSquare + 1 + (Player == 1 ? -8 : 8));
                return (true, Check, Checkers); //can't be in double check from a pawn
            }
            if ((((board.Pieces[ColourAdd] & (1UL << (KingSquare - 1 + (Player == 1 ? -8 : 8)))) != 0) && (KingSquare % 8 != 0))) 
            {
                Checkers |= 1UL << (KingSquare - 1 + (Player == 1 ? -8 : 8));
                return (true, Check, Checkers); //can't be in double check from a pawn
            }
            //shouldn't be in check from the king so this doesn't need to be identified here
            return (Check, DoubleCheck, (Checkers != 0 ? Checkers : ulong.MaxValue));
        }

        public static bool CheckLegal(Board board, Move Move) //will soon only be used for complex moves
        {
            fullcheck++;
            board.MakeMove(Move);
            bool Legal = InCheck(board, Math.Abs(board.ColourToMove - 1));
            board.UnmakeMove(Move); 
            return !Legal;
        }

        // public static int CheckCastle(Board board, Move[] moves, int MoveNumber) // currently allows castling out of, and through check
        // {
        //     if (board.ColourToMove == 0 && ((board.OccupiedSquares & 0x60) == 0) && board.WhiteShortCastle && !MoveGenerator.UnderAttack(board, 5) && !MoveGenerator.UnderAttack(board, 6))
        //     {
        //         moves[MoveNumber] = new Move(0,0,0b0010,0,0); //white short castle
        //         MoveNumber++;
        //     }
        //     if (board.ColourToMove == 0 && ((board.OccupiedSquares & 0xE) == 0) && board.WhiteLongCastle && !MoveGenerator.UnderAttack(board, 2) && !MoveGenerator.UnderAttack(board,3))
        //     {
        //         moves[MoveNumber] = new Move(0,0,0b0011,0,0); //white long castle
        //         MoveNumber++;
        //     }
        //     if (board.ColourToMove == 1 && ((board.OccupiedSquares.Data() & 0x6000000000000000) == 0) && board.BlackShortCastle && !MoveGenerator.UnderAttack(board, 61) && !MoveGenerator.UnderAttack(board, 62))
        //     {
        //         moves[MoveNumber] = new Move(0,0,0b0010,0,0); //black short castle
        //         //Console.WriteLine("black short castle");
        //         MoveNumber++;
        //     }
        //     if (board.ColourToMove == 1 && ((board.OccupiedSquares.GetData() & 0x0E00000000000000) == 0) && board.BlackLongCastle && !MoveGenerator.UnderAttack(board, 58) && !MoveGenerator.UnderAttack(board,59))
        //     {
        //         moves[MoveNumber] = new Move(0,0,0b0011,0,0); //black long castle
        //         MoveNumber++;
        //         //Console.WriteLine("black long castle");
        //     }
        //     return MoveNumber;
        // }
        public static int GenerateQueenMoves(Board board, Move[] Moves, int MoveNumber, ulong Blocks, bool onlyCaptures = false)
        {
            ulong QueenLocations = board.Pieces[4 + board.ColourToMove*6];
            while (QueenLocations > 0)
            {
                int startSquare = BitOperations.TrailingZeroCount(QueenLocations);
                ulong QueenAttacks = GenerateRookAttacks(board, startSquare);
                QueenAttacks |=GenerateBishopAttacks(board, startSquare); //union of bishop moves and rook moves
                if (onlyCaptures)
                {
                    QueenAttacks &= (board.ColourToMove == 1 ? board.WhitePieces : board.BlackPieces); //only counts the intersection with enemy pieces
                } else {
                    QueenAttacks &= ~(board.ColourToMove == 0? board.WhitePieces : board.BlackPieces);
                }
                QueenAttacks &= Blocks;
                QueenLocations &= ~(1UL << startSquare);
                while (QueenAttacks > 0) //for each target square
                {
                    int target = BitOperations.TrailingZeroCount(QueenAttacks);
                    int capture = board.GetPiece(target, (board.ColourToMove == 0 ?  1 : 0));
                    int flag = (board.ColourToMove == 0 && ((board.BlackPieces & (1UL <<(target)))!=0)) || (board.ColourToMove == 1 && (capture != 0b111)) ? 0b0100 : 0b0000; //pretty much just checks if it's a capture or not
                    Moves[MoveNumber] = new Move(startSquare, target, flag, 0b100, capture, 0);
                    MoveNumber++;
                    QueenAttacks &= ~(1UL << target);
                }
            }
            return MoveNumber;
        }

        public static int GenerateRookMoves(Board board, Move[] Moves, int MoveNumber, ulong Blocks, bool onlyCaptures = false)
        {
            ulong RookLocations = board.Pieces[3 + board.ColourToMove*6];
            while (RookLocations > 0) //cycles through each rook on the board
            {
                int startSquare = BitOperations.TrailingZeroCount(RookLocations);
                ulong RookAttacks = GenerateRookAttacks(board, startSquare);
                if (onlyCaptures)
                {
                    RookAttacks &= (board.ColourToMove == 1 ? board.WhitePieces : board.BlackPieces); //only counts the intersection with enemy pieces
                } else {
                    RookAttacks &= ~(board.ColourToMove == 0? board.WhitePieces : board.BlackPieces);
                }
                RookAttacks &= Blocks;
                RookLocations &= ~(1UL << startSquare);
                while (RookAttacks > 0) //for each target square
                {
                    int target = BitOperations.TrailingZeroCount(RookAttacks);
                    int capture = board.GetPiece(target,(board.ColourToMove == 0 ?  1 : 0));
                    int flag = (board.ColourToMove == 0 && ((board.BlackPieces & (1UL << (target)))!=0)) || (board.ColourToMove == 1 && capture != 0b111) ? 0b0100 : 0b0000; //pretty much just checks if it's a capture or not
                    Moves[MoveNumber] = new Move(startSquare, target, flag, 0b011, capture);
                    MoveNumber++;
                    RookAttacks &= ~(1UL << target);
                }
            }
            return MoveNumber;
        }

        public static int GenerateBishopMoves(Board board, Move[] Moves, int MoveNumber, ulong Blocks, bool onlyCaptures = false)
        {
            ulong BishopLocations = board.Pieces[2 + board.ColourToMove*6];
            while (BishopLocations > 0) //cycles through each bishop on the board
            {
                int startSquare = BitOperations.TrailingZeroCount(BishopLocations);
                ulong BishopAttacks = GenerateBishopAttacks(board, startSquare);
                if (onlyCaptures)
                {
                    BishopAttacks &= (board.ColourToMove == 1 ? board.WhitePieces : board.BlackPieces); //only counts the intersection with enemy pieces
                } else {
                    BishopAttacks &= ~(board.ColourToMove == 0 ? board.WhitePieces : board.BlackPieces);
                }
                BishopAttacks &= Blocks;
                BishopLocations &= ~(1UL << startSquare);
                while (BishopAttacks > 0) //for each target square
                {
                    int target = BitOperations.TrailingZeroCount(BishopAttacks);
                    int capture = board.GetPiece(target, (board.ColourToMove == 0 ?  1 : 0));
                    int flag = (board.ColourToMove == 0 && (((board.BlackPieces & (1UL << (target)))!=0))) || (board.ColourToMove == 1 && (capture != 0b111)) ? 0b0100 : 0b0000; //pretty much just checks if it's a capture or not
                    Moves[MoveNumber] = new Move(startSquare, target, flag, 0b010, capture);
                    MoveNumber++;
                    BishopAttacks &= ~(1UL << target);
                }
            }
            return MoveNumber;
        }

        public static ulong GenerateRookAttacks(Board board, int square) //this is all one line because it saves time defining variables
        {
            return PreComputeData.RookAttacks[square, ((board.OccupiedSquares & PreComputeData.RookMasks[square]) * PreComputeData.RookMagics[square]) >> (64 - Magic.RookBits[square])].GetData();
        }

        public static ulong GenerateBishopAttacks(Board board, int square) //one line to save time
        {
            return PreComputeData.BishopAttacks[square, ((board.OccupiedSquares & PreComputeData.BishopMasks[square]) * PreComputeData.BishopMagics[square]) >> (64 - Magic.BishopBits[square])].GetData();
        }

        public static int GenerateKingMoves(Board board, Move[] Moves, int MoveNumber, ulong Blocks, bool onlyCaptures = false)
        {
            int KingSquare = BitOperations.TrailingZeroCount(board.Pieces[5+board.ColourToMove*6]); //gets the location of the king
            if (KingSquare > 63 || KingSquare < 0){ Console.WriteLine("king index is out of bounds: " + KingSquare);
            board.PrintBoard();}
            ulong Attacks = (PreComputeData.KingAttackBitboards[KingSquare].GetData());
            if (onlyCaptures)
            {
                Attacks &= (board.ColourToMove == 1 ? board.WhitePieces : board.BlackPieces); //only counts the intersection with enemy pieces
            } else {
                Attacks &= ~(board.ColourToMove == 0? board.WhitePieces : board.BlackPieces);
            }
            //if (Blocks != ulong.MaxValue) Attacks &=  ~Blocks; //cannot block check with your own king
            while (Attacks > 0)
            {
                int target = BitOperations.TrailingZeroCount(Attacks);
                int capture = board.GetPiece(target, (board.ColourToMove == 0 ?  1 : 0));
                int flag = (board.ColourToMove == 0 && ((board.BlackPieces & (1UL << (target)))!=0)) || (board.ColourToMove == 1 && (target == 0b111)) ? 0b0100 : 0b0000;
                if (1==1 || !(Blocks != ulong.MaxValue && ((Blocks & (1UL << target)) != 0) && capture == 7)) 
                {
                    Moves[MoveNumber] = new Move(KingSquare, target, flag, 0b101, capture);
                    MoveNumber++;
                }
                Attacks &= ~(1UL << target);
            }
            return MoveNumber;
        }

        public static int GenerateKnightMoves(Board board, Move[] Moves, int MoveNumber, ulong Blocks, bool onlyCaptures = false)
        {
            ulong KnightLocations = board.Pieces[1 + board.ColourToMove*6]; //gets location of all the knights of a colour
            while (KnightLocations > 0) //for all of the knights
            {
                int startSquare = BitOperations.TrailingZeroCount(KnightLocations);
                ulong Attacks = PreComputeData.KnightAttackBitboards[startSquare].GetData();
                if (onlyCaptures)
                {
                    Attacks &= (board.ColourToMove == 1 ? board.WhitePieces : board.BlackPieces); //only counts the intersection with enemy pieces
                } else {
                    Attacks &= ~(board.ColourToMove == 0? board.WhitePieces : board.BlackPieces);
                }
                Attacks &= Blocks; //in case you have to block check
                while (Attacks > 0)
                {
                    int target = BitOperations.TrailingZeroCount(Attacks);
                    int capture = board.GetPiece(target, (board.ColourToMove == 0 ?  1 : 0));
                    int flag = (board.ColourToMove == 0 && ((board.BlackPieces & (1UL << (target)))!=0)) || (board.ColourToMove == 1 && (capture == 0b111)) ? 0b0100 : 0b0000;
                    Moves[MoveNumber] = new Move(startSquare, target, flag, 0b001, capture);
                    MoveNumber++;
                    Attacks &= ~(1UL << target);
                }
                KnightLocations &= ~(1UL << startSquare);
            }
            return MoveNumber;
        }

        public static int GeneratePawnMoves (Board board, Move[] Moves, int MoveNumber, ulong Blocks, bool onlyCaptures = false)
        {
            if (!onlyCaptures) //to include non capturing moves
            {
                ulong SinglePushPawns = (board.ColourToMove == 0) ? (~board.OccupiedSquares >> 8) & board.Pieces[0] : (~board.OccupiedSquares << 8) & board.Pieces[6];
                ulong DoublePushPawns = board.ColourToMove == 0 ? (~board.OccupiedSquares >> 16 & board.Pieces[0] & SinglePushPawns & 0xff00) : (~board.OccupiedSquares << 16 & board.Pieces[6] & SinglePushPawns & 0x00ff000000000000);
                while (SinglePushPawns > 0) //while the bitboard isn't empty
                {
                    int startSquare = BitOperations.TrailingZeroCount(SinglePushPawns);
                    if ((Blocks & (1UL << (startSquare + (board.ColourToMove == 0 ? 8 : -8)))) != 0)
                    {
                        if ((startSquare + 8 < 56 && board.ColourToMove == 0) || (startSquare - 8 > 7 && board.ColourToMove == 1)) //checks it isn't a promotion pawn move
                        {
                            Moves[MoveNumber] = (new Move(startSquare, startSquare + (board.ColourToMove == 0 ? 8 : -8), 0, 0b000, 0b111));
                            MoveNumber++;
                        }
                        else {AddPromotions(startSquare, startSquare + (board.ColourToMove == 0 ? 8 : -8), Moves, MoveNumber, 0b111);MoveNumber+=4;} //adds all the promo moves
                    }
                    SinglePushPawns &= ~(1UL << startSquare); //need to change this to use the bitboard class, will make it all easer TODO
                }
                DoublePushPawns &= (board.ColourToMove == 0 ? Blocks >> 16 : Blocks << 16);
                while (DoublePushPawns > 0) //while the bitboard isn't empty
                {
                    int startSquare = BitOperations.TrailingZeroCount(DoublePushPawns); 
                    //no need to check for promotions of double pawn pushes
                    Moves[MoveNumber] = new Move(startSquare, startSquare + (board.ColourToMove == 0 ? 16 : -16), 0b0001, 0b000, 0b111);
                    MoveNumber++;
                    DoublePushPawns &= ~(1UL << startSquare); 
                }
            }

            //this is for all the diagonal capturing moves
            
            ulong EastCapture;
            ulong WestCapture;
            if (board.ColourToMove == 0) //turn is white
            {
                EastCapture = (board.Pieces[0]) & ((board.BlackPieces | 1UL<<board.EnPassantSquare) >>9) & 0x7F7F7F7F7F7F7F7F; //this essentially checks there is a piece that can be captured and accounts for overflow stuff
                WestCapture = (board.Pieces[0]) & ((board.BlackPieces | 1UL<<board.EnPassantSquare) >>7) & 0xFEFEFEFEFEFEFEFE; //does the same thing for the other direction
            } else { //blacks turn
                WestCapture = (board.Pieces[6]) & ((board.WhitePieces | 1UL<<board.EnPassantSquare) <<9) & 0xFEFEFEFEFEFEFEFE; //this essentially checks there is a piece that can be captured and accounts for overflow stuff
                EastCapture = (board.Pieces[6]) & ((board.WhitePieces | 1UL<<board.EnPassantSquare) <<7) & 0x7F7F7F7F7F7F7F7F; //does the same thing for the other direction
            }

            while (EastCapture > 0) //for all the capturing pawns to the right
            {
                int startSquare = BitOperations.TrailingZeroCount(EastCapture);
                int target = startSquare + 1 + (board.ColourToMove == 0 ? 8 : -8);
                if ((Blocks & (1UL << target)) != 0)
                {
                    int capture = board.GetPiece(target, (board.ColourToMove == 0 ?  1 : 0));
                    if ((board.ColourToMove == 0 ? startSquare < 48 : startSquare > 15)) //isn't a promotion
                    {
                        Moves[MoveNumber] = new Move(startSquare, target, (target == board.EnPassantSquare ? 0b0101 : 0b0100), 0b000, capture);
                        MoveNumber++;
                    } else {
                        AddPromotions(startSquare, target, Moves, MoveNumber, capture);
                        MoveNumber += 4;
                    }
                }
                EastCapture &= ~(1UL << startSquare);
            }
            while (WestCapture > 0) //for all the capturing pawns to the left
            {
                int startSquare = BitOperations.TrailingZeroCount(WestCapture);
                int target = startSquare - 1 + (board.ColourToMove == 0 ? 8 : -8);
                if ((Blocks & (1UL << target)) != 0)
                {
                    int capture = board.GetPiece(target, (board.ColourToMove == 0 ?  1 : 0));
                    if ((board.ColourToMove == 0 ? startSquare < 48 : startSquare >= 16)) //isn't a promotion
                    {
                        Moves[MoveNumber] = new Move(startSquare, target, (target == board.EnPassantSquare ? 0b0101 : 0b0100), 0b000, capture);
                        MoveNumber++;   
                    } else {
                        AddPromotions(startSquare, target, Moves, MoveNumber, capture);
                        MoveNumber += 4;
                    }
                }
                WestCapture &= ~(1UL << startSquare);
            }
            return MoveNumber;
        }

        private static void AddPromotions(int start, int target, Move[] MoveList, int MoveNumber, int capture)
        {
            if (capture != 0b111)
            {//flags are the capture promo ones
                MoveList[MoveNumber] = new Move(start, target, 0b1100, 0b001, capture); //knight
                MoveList[MoveNumber+1] = new Move(start, target, 0b1101, 0b010, capture);//bishop
                MoveList[MoveNumber+2] = new Move(start, target, 0b1110, 0b011, capture);//rook
                MoveList[MoveNumber+3] = new Move(start, target, 0b1111, 0b100, capture);//queen
            }
            else
            {//flags are the normal promo ones
                MoveList[MoveNumber] = new Move(start, target, 0b1000, 0b001, 0b111); //same order as above
                MoveList[MoveNumber+1] = new Move(start, target, 0b1001, 0b010, 0b111);
                MoveList[MoveNumber+2] = new Move(start, target, 0b1010, 0b011, 0b111);
                MoveList[MoveNumber+3] = new Move(start, target, 0b1011, 0b100, 0b111);
            }
        }
    }

    public struct Move 
    {
        int Data = 0;
        public Move(int start, int target, int flags, int piece, int capture, int check = 0)
        {
            Data = (start | (target << 6) | (flags << 12) | (piece << 16) | (capture << 19) | (check << 25)); //the first 6 bits are the start square, the next 6 the target square then the next 4 are for flags (castling, enpassant)
        }
        public int GetCapture() // the normal piece indicators however for no capture all bits are active (0b111)
        {
            return (Data >> 19) & 0b111;
        }
        public int GetCheck()
        {
            return (Data >> 25);
        }
        public int GetStart()
        {
            return Data & 63;
        }
        public int GetTarget()
        {
            return (Data >> 6) & 63;
        }
        public int GetFlag() 
        {
            return (Data >> 12) & 15;
        }
        public int GetPiece()
        {
            return (Data >> 16) & 7;
        }
        public int GetCastle()
        {
            return (Data >> 22) & 3;
        }
        public bool IsCapture() 
        {
            return ((Data & (1 >> 15)) == 1);
        }
        public int GetData()
        {
            return Data;
        }
        public void PrintMove()
        {
            //if (Data == 0) return;
            Console.WriteLine("data: " + Convert.ToString((long)Data, 2) + ", start: " + this.GetStart() + ", target: " + this.GetTarget() + ", flags: " + this.GetFlag() + ", piece: " + this.GetPiece() + ", capture: " + this.GetCapture());
        }
    }
    // [000] 3bits for actual piece
    //note on the use of flags:
    // there are 4 bits for flags
    // the flag will characterize the following: move type, castling type, any promotion, promotion capture
}
