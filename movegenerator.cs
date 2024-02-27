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
            ulong SinglePushPawns = ((~board.OccupiedSquares.GetData() >> 16) & board.WhitePawns.GetData());
            ulong DoublePushPawns = (~board.OccupiedSquares.GetData() >> 16) & board.WhitePawns.GetData() & SinglePushPawns;
            while (SinglePushPawns > 0) //while the bitboard isn't empty
            {
                int startSquare = BitOperations.TrailingZeroCount(SinglePushPawns);
                MoveList.Add(new Move(startSquare, startSquare + 8, 0));
                SinglePushPawns.ClearBit(startSquare); //need to change this to use the bitboard class, will make it all easer TODO
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
            return Data & 127;
        }
        public int GetTarget()
        {
            return (Data >> 6) & 127;
        }
        public int GetFlag() 
        {
            return (Data >> 12) & 127;
        }
    }
}