//this is where all the move generation will be done
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ChessEngine 
{
    public static class MoveGenerator
    {
        public static List<Move> GeneratePawnMoves (Board board)
        {
            List<Move> MoveList = new List<Move>();
            Bitboard SinglePushPawns = new Bitboard((~board.OccupiedSquares.GetData() >> 8) & board.WhitePawns.GetData());
            Bitboard DoublePushPawns = new Bitboard((~board.OccupiedSquares.GetData() >> 16) & board.WhitePawns.GetData() & SinglePushPawns.GetData() & 0xff00);
            while (SinglePushPawns.GetData() > 0) //while the bitboard isn't empty
            {
                int startSquare = SinglePushPawns.LSB();
                MoveList.Add(new Move(startSquare, startSquare + 8, 0));
                SinglePushPawns.ClearBit(startSquare); //need to change this to use the bitboard class, will make it all easer TODO
            }
            while (DoublePushPawns.GetData() > 0) //while the bitboard isn't empty
            {
                int startSquare = DoublePushPawns.LSB();
                MoveList.Add(new Move(startSquare, startSquare + 16, 0));
                DoublePushPawns.ClearBit(startSquare); //need to change this to use the bitboard class, will make it all easer TODO
            }
            return MoveList;
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
        public void PrintMove()
        {
            Console.WriteLine("data: " + Convert.ToString((long)Data, 2) + ", start: " + this.GetStart() + ", target: " + this.GetTarget() + ", flags: " + this.GetFlag());
        }
    }
}