//this is where all the move generation will be done
using System;
using System.Collections.Generic;
using System.Numerics;


namespace ChessEngine 
{
    public static class MoveGenerator
    //the move arrays are initialised with length 218 as this is the most possible moves in any chess position
    {
        public static Move[] GenerateMoves(Board board)
        {
            Move[] Moves = new Move[218];
            int MoveNumber = 0;
            MoveNumber = GenerateRookMoves(board, Moves, MoveNumber);
            MoveNumber = GenerateBishopMoves(board, Moves, MoveNumber);
            MoveNumber = GenerateQueenMoves(board, Moves, MoveNumber);
            MoveNumber = GeneratePawnMoves(board, Moves, MoveNumber);
            MoveNumber = GenerateKingMoves(board, Moves, MoveNumber); 
            MoveNumber = GenerateKnightMoves(board, Moves, MoveNumber);
            MoveNumber = CheckCastle(board, Moves, MoveNumber);
            return Moves;
        }

        public static bool CheckLegal(Board board, Move Move) 
        {
            board.MakeMove(Move);
            int ColourAdd = board.ColourToMove == 1 ? 6 : 0;
            int KingSquare = (board.ColourToMove == 1 ? board.WhiteKing.LSB() : board.BlackKing.LSB());
            if (KingSquare < 0 || KingSquare > 63){ Console.WriteLine("King Square l31: " + KingSquare);
            board.PrintBoard();board.WhiteKing.PrintData();Move.PrintMove();}
            ulong KnightAttacks = PreComputeData.KnightAttackBitboards[KingSquare].GetData(); // where a knight could attack the king from
            if (((KnightAttacks) & board.Pieces[1 + ColourAdd].GetData()) != 0)
            {
                board.UnmakeMove(Move);
                return false;
            }

            Bitboard BishopAttacks = GenerateBishopAttacks(board, KingSquare);
            if ((BishopAttacks.GetData() & board.Pieces[2 + ColourAdd].GetData()) != 0)
            {
                board.UnmakeMove(Move); 
                return false;
            }

            Bitboard RookAttacks = GenerateRookAttacks(board, KingSquare);
            if ((RookAttacks.GetData() & board.Pieces[3 + ColourAdd].GetData()) != 0) 
            {
                board.UnmakeMove(Move); 
                return false;
            }

            if (((RookAttacks.GetData() | BishopAttacks.GetData()) & board.Pieces[4 + ColourAdd].GetData()) != 0) 
            {
                board.UnmakeMove(Move); 
                return false;
            }

            if (((board.Pieces[ColourAdd].IsBitSet(KingSquare + 1 + (board.ColourToMove == 1 ? 8 : -8))) && KingSquare % 8 != 7) || ((board.Pieces[ColourAdd].IsBitSet(KingSquare - 1 + (board.ColourToMove == 1 ? 8 : -8))) && KingSquare % 8 != 0)) // checking if either of the attackable squares of the king from pawns are occupied
            {
                board.UnmakeMove(Move); 
                return false;
            }

            if ((PreComputeData.KingAttackBitboards[KingSquare].GetData() & board.Pieces[5+ ColourAdd].GetData()) != 0)
            {
                board.UnmakeMove(Move); 
                return false;
            }
            board.UnmakeMove(Move); 
            return true;
        }

        public static int CheckCastle(Board board, Move[] moves, int MoveNumber) // currently allows castling out of, and through check
        {
            if (board.ColourToMove == 0 && ((board.OccupiedSquares.GetData() & 0x60) == 0) && board.WhiteShortCastle && !MoveGenerator.UnderAttack(board, 5) && !MoveGenerator.UnderAttack(board, 6))
            {
                moves[MoveNumber] = new Move(0,0,0b0010,0,0); //white short castle
                MoveNumber++;
            }
            if (board.ColourToMove == 0 && ((board.OccupiedSquares.GetData() & 0xE) == 0) && board.WhiteLongCastle && !MoveGenerator.UnderAttack(board, 2) && !MoveGenerator.UnderAttack(board,3))
            {
                moves[MoveNumber] = new Move(0,0,0b0011,0,0); //white long castle
                MoveNumber++;
            }
            if (board.ColourToMove == 1 && ((board.OccupiedSquares.GetData() & 0x6000000000000000) == 0) && board.BlackShortCastle && !MoveGenerator.UnderAttack(board, 61) && !MoveGenerator.UnderAttack(board, 62))
            {
                moves[MoveNumber] = new Move(0,0,0b0010,0,0); //black short castle
                //Console.WriteLine("black short castle");
                MoveNumber++;
            }
            if (board.ColourToMove == 1 && ((board.OccupiedSquares.GetData() & 0x0E00000000000000) == 0) && board.BlackLongCastle && !MoveGenerator.UnderAttack(board, 58) && !MoveGenerator.UnderAttack(board,59))
            {
                moves[MoveNumber] = new Move(0,0,0b0011,0,0); //black long castle
                MoveNumber++;
                //Console.WriteLine("black long castle");
            }
            return MoveNumber;
        }

        public static bool UnderAttack(Board board, int square)
        {
            int ColourAdd = (board.ColourToMove == 0 ? 6 : 0);
            ulong KnightAttacks = PreComputeData.KnightAttackBitboards[square].GetData(); // where a knight could attack the king from
            if (((KnightAttacks) & board.Pieces[1 + ColourAdd].GetData()) != 0)
            {
                return true;
            }

            Bitboard BishopAttacks = GenerateBishopAttacks(board, square);
            if ((BishopAttacks.GetData() & board.Pieces[2 + ColourAdd].GetData()) != 0)
            {
                return true;
            }

            Bitboard RookAttacks = GenerateRookAttacks(board, square);
            if ((RookAttacks.GetData() & board.Pieces[3 + ColourAdd].GetData()) != 0) 
            { 
                return true;
            }

            if (((RookAttacks.GetData() | BishopAttacks.GetData()) & board.Pieces[4 + ColourAdd].GetData()) != 0) 
            {
                return true;
            }

            if (((board.Pieces[ColourAdd].IsBitSet(square + 1 + (board.ColourToMove == 0 ? 8 : -8))) && square % 8 != 7) || ((board.Pieces[ColourAdd].IsBitSet(square - 1 + (board.ColourToMove == 0 ? 8 : -8))) && square % 8 != 0)) // checking if either of the attackable squares of the king from pawns are occupied
            {
                return true;
            }

            if ((PreComputeData.KingAttackBitboards[square].GetData() & board.Pieces[5+ ColourAdd].GetData()) != 0)
            { 
                return true;
            }
            return false;
        }


        public static int GenerateQueenMoves(Board board, Move[] Moves, int MoveNumber)
        {
            Bitboard QueenLocations = (board.ColourToMove == 0 ? new Bitboard(board.WhiteQueens.GetData()) : new Bitboard(board.BlackQueens.GetData()));
            while (QueenLocations.GetData() > 0)
            {
                int startSquare = QueenLocations.LSB();
                Bitboard QueenAttacks = GenerateRookAttacks(board, startSquare);
                QueenAttacks.SetData(QueenAttacks.GetData() | GenerateBishopAttacks(board, startSquare).GetData()); //union of bishop moves and rook moves
                QueenAttacks.SetData(QueenAttacks.GetData() & ~(board.ColourToMove == 0? board.WhitePieces.GetData() : board.BlackPieces.GetData()));
                QueenLocations.ClearBit(startSquare);
                while (QueenAttacks.GetData() > 0) //for each target square
                {
                    int target = QueenAttacks.LSB();
                    int capture = board.GetPiece(target, (board.ColourToMove == 0 ?  1 : 0));
                    int flag = (board.ColourToMove == 0 && board.BlackPieces.IsBitSet(target)) || (board.ColourToMove == 1 && (capture != 0b111)) ? 0b0100 : 0b0000; //pretty much just checks if it's a capture or not
                    Moves[MoveNumber] = new Move(startSquare, target, flag, 0b100, capture);
                    MoveNumber++;
                    QueenAttacks.ClearBit(target);
                }
            }
            return MoveNumber;
        }

        public static int GenerateRookMoves(Board board, Move[] Moves, int MoveNumber)
        {
            Bitboard RookLocations = (board.ColourToMove == 0 ? new Bitboard(board.WhiteRooks.GetData()) : new Bitboard(board.BlackRooks.GetData()));
            while (RookLocations.GetData() > 0) //cycles through each rook on the board
            {
                int startSquare = RookLocations.LSB();
                Bitboard RookAttacks = GenerateRookAttacks(board, startSquare);
                RookAttacks.SetData(RookAttacks.GetData() & ~(board.ColourToMove == 0? board.WhitePieces.GetData() : board.BlackPieces.GetData()));
                RookLocations.ClearBit(startSquare);
                while (RookAttacks.GetData() > 0) //for each target square
                {
                    int target = RookAttacks.LSB();
                    int capture = board.GetPiece(target,(board.ColourToMove == 0 ?  1 : 0));
                    int flag = (board.ColourToMove == 0 && board.BlackPieces.IsBitSet(target)) || (board.ColourToMove == 1 && capture != 0b111) ? 0b0100 : 0b0000; //pretty much just checks if it's a capture or not
                    Moves[MoveNumber] = new Move(startSquare, target, flag, 0b011, capture);
                    MoveNumber++;
                    RookAttacks.ClearBit(target);
                }
            }
            return MoveNumber;
        }

        public static int GenerateBishopMoves(Board board, Move[] Moves, int MoveNumber)
        {
            Bitboard BishopLocations = (board.ColourToMove == 0 ? new Bitboard(board.WhiteBishops.GetData()) : new Bitboard(board.BlackBishops.GetData()));
            while (BishopLocations.GetData() > 0) //cycles through each rook on the board
            {
                int startSquare = BishopLocations.LSB();
                Bitboard BishopAttacks = GenerateBishopAttacks(board, startSquare);
                BishopAttacks.SetData(BishopAttacks.GetData() & ~(board.ColourToMove == 0? board.WhitePieces.GetData() : board.BlackPieces.GetData()));
                BishopLocations.ClearBit(startSquare);
                while (BishopAttacks.GetData() > 0) //for each target square
                {
                    int target = BishopAttacks.LSB();
                    int capture = board.GetPiece(target, (board.ColourToMove == 0 ?  1 : 0));
                    int flag = (board.ColourToMove == 0 && board.BlackPieces.IsBitSet(target)) || (board.ColourToMove == 1 && (capture != 0b111)) ? 0b0100 : 0b0000; //pretty much just checks if it's a capture or not
                    Moves[MoveNumber] = new Move(startSquare, target, flag, 0b010, capture);
                    MoveNumber++;
                    BishopAttacks.ClearBit(target);
                }
            }
            return MoveNumber;
        }

        public static Bitboard GenerateRookAttacks(Board board, int square)
        {
            ulong mask = PreComputeData.RookMasks[square];
            ulong ReleventBits = board.OccupiedSquares.GetData() & mask;
            ulong index = ReleventBits * PreComputeData.RookMagics[square]; //the generate move function in the magic file is incorrect, logic here is sound i think
            index = index >> (64 - Magic.RookBits[square]);
            Bitboard Attacks = new Bitboard(PreComputeData.RookAttacks[square, index].GetData());
            return Attacks;
        }

        public static Bitboard GenerateBishopAttacks(Board board, int square)
        {
            ulong mask = PreComputeData.BishopMasks[square];
            ulong ReleventBits = board.OccupiedSquares.GetData() & mask;
            ulong index = ReleventBits * PreComputeData.BishopMagics[square];
            index = index >> (64 - Magic.BishopBits[square]);
            //Console.WriteLine("index: " + index + " square: " + square);
            Bitboard Attacks = new Bitboard(PreComputeData.BishopAttacks[square, index].GetData());
            return Attacks;
        }

        public static int GenerateKingMoves(Board board, Move[] Moves, int MoveNumber)
        {
            int KingSquare = (board.ColourToMove == 0) ? board.WhiteKing.LSB() : board.BlackKing.LSB(); //gets the location of the king
            if (KingSquare > 63 || KingSquare < 0){ Console.WriteLine("king index is out of bounds: " + KingSquare);
            board.PrintBoard();}
            Bitboard Attacks = new Bitboard(PreComputeData.KingAttackBitboards[KingSquare].GetData() & (board.ColourToMove == 0 ? ~board.WhitePieces.GetData() : ~board.BlackPieces.GetData()));
            while (Attacks.GetData() > 0)
            {
                int target = Attacks.LSB();
                int capture = board.GetPiece(target, (board.ColourToMove == 0 ?  1 : 0));
                int flag = (board.ColourToMove == 0 && board.BlackPieces.IsBitSet(target)) || (board.ColourToMove == 1 && (target == 0b111)) ? 0b0100 : 0b0000;
                Moves[MoveNumber] = new Move(KingSquare, target, flag, 0b101, capture);
                MoveNumber++;
                Attacks.ClearBit(target);
            }
            return MoveNumber;
        }

        public static int GenerateKnightMoves(Board board, Move[] Moves, int MoveNumber)
        {
            Bitboard KnightLocations = new Bitboard(board.ColourToMove == 0 ? board.WhiteKnights.GetData() : board.BlackKnights.GetData()); //gets location of all the knights of a colour
            while (KnightLocations.GetData() > 0) //for all of the knights
            {
                int startSquare = KnightLocations.LSB();
                Bitboard Attacks = new Bitboard(PreComputeData.KnightAttackBitboards[startSquare].GetData() & (board.ColourToMove == 0 ? ~board.WhitePieces.GetData() : ~board.BlackPieces.GetData()));
                while (Attacks.GetData() > 0)
                {
                    int target = Attacks.LSB();
                    int capture = board.GetPiece(target, (board.ColourToMove == 0 ?  1 : 0));
                    int flag = (board.ColourToMove == 0 && board.BlackPieces.IsBitSet(target)) || (board.ColourToMove == 1 && (capture == 0b111)) ? 0b0100 : 0b0000;
                    Moves[MoveNumber] = new Move(startSquare, target, flag, 0b001, capture);
                    MoveNumber++;
                    Attacks.ClearBit(target);
                }
                KnightLocations.ClearBit(startSquare);
            }
            return MoveNumber;
        }

        public static int GeneratePawnMoves (Board board, Move[] Moves, int MoveNumber)
        {
            Bitboard SinglePushPawns = new Bitboard((board.ColourToMove == 0) ? (~board.OccupiedSquares.GetData() >> 8) & board.WhitePawns.GetData() : (~board.OccupiedSquares.GetData() << 8) & board.BlackPawns.GetData());
            Bitboard DoublePushPawns = new Bitboard(board.ColourToMove == 0 ? (~board.OccupiedSquares.GetData() >> 16 & board.WhitePawns.GetData() & SinglePushPawns.GetData() & 0xff00) : (~board.OccupiedSquares.GetData() << 16 & board.BlackPawns.GetData() & SinglePushPawns.GetData() & 0x00ff000000000000));
            while (SinglePushPawns.GetData() > 0) //while the bitboard isn't empty
            {
                int startSquare = SinglePushPawns.LSB();
                if ((startSquare + 8 < 56 && board.ColourToMove == 0) || (startSquare - 8 > 7 && board.ColourToMove == 1)) //checks it isn't a promotion pawn move
                {
                    Moves[MoveNumber] = (new Move(startSquare, startSquare + (board.ColourToMove == 0 ? 8 : -8), 0, 0b000, 0b111));
                    MoveNumber++;
                }
                else {AddPromotions(startSquare, startSquare + (board.ColourToMove == 0 ? 8 : -8), Moves, MoveNumber, 0b111);MoveNumber+=4;} //adds all the promo moves
                SinglePushPawns.ClearBit(startSquare); //need to change this to use the bitboard class, will make it all easer TODO
            }
            while (DoublePushPawns.GetData() > 0) //while the bitboard isn't empty
            {
                int startSquare = DoublePushPawns.LSB(); 
                //no need to check for promotions of double pawn pushes
                Moves[MoveNumber] = new Move(startSquare, startSquare + (board.ColourToMove == 0 ? 16 : -16), 0b0001, 0b000, 0b111);
                MoveNumber++;
                DoublePushPawns.ClearBit(startSquare); //need to change this to use the bitboard class, will make it all easer TODO
            }

            //this is for all the diagonal capturing moves
            
            Bitboard EastCapture;
            Bitboard WestCapture;
            if (board.ColourToMove == 0) //turn is white
            {
                EastCapture = new Bitboard((board.WhitePawns.GetData()) & ((board.BlackPieces.GetData() | 1UL<<board.EnPassantSquare) >>9) & 0x7F7F7F7F7F7F7F7F); //this essentially checks there is a piece that can be captured and accounts for overflow stuff
                WestCapture = new Bitboard((board.WhitePawns.GetData()) & ((board.BlackPieces.GetData() | 1UL<<board.EnPassantSquare) >>7) & 0xFEFEFEFEFEFEFEFE); //does the same thing for the other direction
            } else { //blacks turn
                WestCapture = new Bitboard((board.BlackPawns.GetData()) & ((board.WhitePieces.GetData() | 1UL<<board.EnPassantSquare) <<9) & 0xFEFEFEFEFEFEFEFE); //this essentially checks there is a piece that can be captured and accounts for overflow stuff
                EastCapture = new Bitboard((board.BlackPawns.GetData()) & ((board.WhitePieces.GetData() | 1UL<<board.EnPassantSquare) <<7) & 0x7F7F7F7F7F7F7F7F); //does the same thing for the other direction
            }
            //WestCapture.PrintData();

            while (EastCapture.GetData() > 0) //for all the capturing pawns to the right
            {
                int startSquare = EastCapture.LSB();
                int target = startSquare + 1 + (board.ColourToMove == 0 ? 8 : -8);
                int capture = board.GetPiece(target, (board.ColourToMove == 0 ?  1 : 0));
                if (board.ColourToMove == 0 ? startSquare < 56 : startSquare >= 8) //isn't a promotion
                {
                    Moves[MoveNumber] = new Move(startSquare, target, (target == board.EnPassantSquare ? 0b0101 : 0b0100), 0b000, capture);
                    MoveNumber++;
                } else {
                    AddPromotions(startSquare, target, Moves, MoveNumber, capture);
                    MoveNumber += 4;
                }
                EastCapture.ClearBit(startSquare);
            }
            while (WestCapture.GetData() > 0) //for all the capturing pawns to the left
            {
                int startSquare = WestCapture.LSB();
                int target = startSquare - 1 + (board.ColourToMove == 0 ? 8 : -8);
                int capture = board.GetPiece(target, (board.ColourToMove == 0 ?  1 : 0));
                if (board.ColourToMove == 0 ? startSquare < 48 : startSquare >= 16) //isn't a promotion
                {
                    Moves[MoveNumber] = new Move(startSquare, target, (target == board.EnPassantSquare ? 0b0101 : 0b0100), 0b000, capture);
                    MoveNumber++;
                } else {
                    AddPromotions(startSquare, target, Moves, MoveNumber, capture);
                    MoveNumber += 4;
                }
                WestCapture.ClearBit(startSquare);
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
        public Move(int start, int target, int flags, int piece, int capture, int nullMove = 0)
        {
            Data = (start | (target << 6) | (flags << 12) | (piece << 16) | (capture << 19) | (nullMove << 25)); //the first 6 bits are the start square, the next 6 the target square then the next 4 are for flags (castling, enpassant)
        }
        public int GetCapture() // the normal piece indicators however for no capture all bits are active (0b111)
        {
            return (Data >> 19) & 0b111;
        }
        public int GetNullMove()
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
            if (Data == 0) return;
            Console.WriteLine("data: " + Convert.ToString((long)Data, 2) + ", start: " + this.GetStart() + ", target: " + this.GetTarget() + ", flags: " + this.GetFlag() + ", piece: " + this.GetPiece() + ", capture: " + this.GetCapture());
        }
    }
    // [000] 3bits for actual piece
    //note on the use of flags:
    // there are 4 bits for flags
    // the flag will characterize the following: move type, castling type, any promotion, promotion capture
}
