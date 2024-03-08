//this is where all the move generation will be done
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ChessEngine 
{
    public static class MoveGenerator
    {
        public static Move[] GeneratePawnMoves (Board board)
        {
            Move[] MoveList = new Move[218];
            int MoveNumber = 0;
            Bitboard SinglePushPawns = new Bitboard((~board.OccupiedSquares.GetData() >> 8) & board.WhitePawns.GetData());
            Bitboard DoublePushPawns = new Bitboard((~board.OccupiedSquares.GetData() >> 16) & board.WhitePawns.GetData() & SinglePushPawns.GetData() & 0xff00);
            while (SinglePushPawns.GetData() > 0) //while the bitboard isn't empty
            {
                int startSquare = SinglePushPawns.LSB();
                if (startSquare + 8 < 56) //checks it isn't a promotion pawn move
                {
                    MoveList[MoveNumber] = (new Move(startSquare, startSquare + 8, 0));
                    MoveNumber++;
                }
                else {AddPromotions(startSquare, startSquare + 8, false, MoveList, MoveNumber);MoveNumber+=4;} //adds all the promo moves
                SinglePushPawns.ClearBit(startSquare); //need to change this to use the bitboard class, will make it all easer TODO
            }
            while (DoublePushPawns.GetData() > 0) //while the bitboard isn't empty
            {
                int startSquare = DoublePushPawns.LSB(); 
                //no need to check for promotions of double pawn pushes
                MoveList[MoveNumber] = new Move(startSquare, startSquare + 16, 0b0001);
                MoveNumber++;
                DoublePushPawns.ClearBit(startSquare); //need to change this to use the bitboard class, will make it all easer TODO
            }

            Bitboard PawnCaptures = new Bitboard(board.WhitePawns.GetData());
            while (PawnCaptures.GetData() > 0) //this is all about the pawn captures that are available
            {
                int startSquare = PawnCaptures.LSB();
                Bitboard Attacks = new Bitboard(PreComputeData.PawnAttackBitboards[startSquare].GetData() & board.BlackPieces.GetData());
                while (Attacks.GetData() > 0) //sequentially adds the corresponding move of each attacked square
                {
                    int target = Attacks.LSB();
                    if (target < 56) //checks it isn't a promotion pawn move
                    {   
                        MoveList[MoveNumber] = (new Move(startSquare, target, 0b0100)); //flag for capture
                        MoveNumber++;
                    }
                    else {AddPromotions(startSquare, target, true, MoveList, MoveNumber);MoveNumber+=4;} //adds all the promo moves
                    Attacks.ClearBit(target);
                }
                PawnCaptures.ClearBit(startSquare);
            }
            return MoveList;
        }

        private static void AddPromotions(int start, int target, bool capture, Move[] MoveList, int MoveNumber)
        {
            if (capture)
            {//flags are the capture promo ones
                MoveList[MoveNumber] = new Move(start, target, 0b1100); //knight
                MoveList[MoveNumber+1] = new Move(start, target, 0b1101);//bishop
                MoveList[MoveNumber+2] = new Move(start, target, 0b1110);//rook
                MoveList[MoveNumber+3] = new Move(start, target, 0b1111);//queen
            }
            else
            {//flags are the normal promo ones
                MoveList[MoveNumber] = new Move(start, target, 0b1000); //same order as above
                MoveList[MoveNumber+1] = new Move(start, target, 0b1001);
                MoveList[MoveNumber+2] = new Move(start, target, 0b1010);
                MoveList[MoveNumber+3] = new Move(start, target, 0b1011);
            }
        }
    }

    public struct Move 
    {
        int Data = 0;
        public Move(int start, int target, int flags)
        {
            Data = (start | (target << 6) | (flags << 12)); //the first 6 bits are the start square, the next 6 the target square then the next 4 are for flags (castling, enpassant)
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
        public int GetData()
        {
            return Data;
        }
        public void PrintMove()
        {
            Console.WriteLine("data: " + Convert.ToString((long)Data, 2) + ", start: " + this.GetStart() + ", target: " + this.GetTarget() + ", flags: " + this.GetFlag());
        }
    }
    //note on the use of flags:
    // there are 4 bits for flags
    // the flag will characterize the following: move type, castling type, any promotion, promotion capture
}