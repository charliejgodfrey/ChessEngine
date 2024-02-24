//this is the file where all the bitboard management code is going to be stored
using System;
using System.Collections.Generic;
namespace ChessEngine
{
    public class Bitboard 
    {
        private ulong data; //this is the actual bitboard

        public Bitboard(ulong initialData = 0) 
        {
            data = initialData; //allows for a predefined starting point for the bitboard
        }

        public void SetBit(int square) 
        {
            ulong mask = 1UL << square; //this creates a mask of all zeros except the square
            data |= mask; //applies the bitwise OR such that all bits stay the same except the one we are changing
        }

        public void ClearBit(int square) 
        {
            ulong mask = 1UL << square; //creates an all zero except bit we're changing mask
            data &= ~mask; //the mask is inversed so the bit we want must become a 0 and everything else is unchanged
        }

        public void ToggleBit(int square)
        {
            ulong mask = 1UL << square; //same as the other two
            data ^= mask; //will switch the bit to the other value, 1 => 0, 0 => 1
        }

        public bool IsBitSet(int square)
        {
            ulong mask = 1UL << square;
            return (data & mask) == 1; // return 1 if the bit is set, 0 if the bit is not set
        }

        public ulong GetData()
        {
            return data;
        }
    }   
}