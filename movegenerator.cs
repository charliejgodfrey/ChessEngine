//this is where all the move generation will be done
using System;
using System.Collections.Generic;

namespace ChessEngine 
{
    public static class MoveGenerator
    {
        public static Bitboard PawnPushes(Board board)
        {
            ulong SinglePushPawns = ((~board.OccupiedSquares.GetData() >> 16) & board.WhitePawns.GetData());
            Bitboard DoublePushPawns = new Bitboard((~board.OccupiedSquares.GetData() >> 16) & board.WhitePawns.GetData() & SinglePushPawns);
            DoublePushPawns.PrintData();
            return new Bitboard();
        }   
    }
}