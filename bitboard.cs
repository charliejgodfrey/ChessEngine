//this is the file where all the bitboard management code is going to be stored
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Collections;

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
            return (data & mask) != 0; // return 1 if the bit is set, 0 if the bit is not set
        }

        public ulong GetData()
        {
            return data;
        }

        public int LSB()
        {
            return BitOperations.TrailingZeroCount(data);
        }

        public void SetData(ulong NewData)
        {
            data = NewData;
        }

        public ulong FlipVertical(ulong x, bool setboard = false) //stole this from the internet
        {
            const ulong k1 = (0x00FF00FF00FF00FF);
            const ulong k2 = (0x0000FFFF0000FFFF);
            x = ((x >>  8) & k1) | ((x & k1) <<  8);
            x = ((x >> 16) & k2) | ((x & k2) << 16);
            x = ( x >> 32)       | ( x       << 32);
            if (setboard) {data = x;}
            return x;
        }

        public ulong FlipHorizontal(ulong x, bool setboard = false) { //stole this from the internet
            const ulong k1 = (0x5555555555555555);
            const ulong k2 = (0x3333333333333333);
            const ulong k4 = (0x0f0f0f0f0f0f0f0f);
            x = ((x >> 1) & k1) | ((x & k1) << 1);
            x = ((x >> 2) & k2) | ((x & k2) << 2);
            x = ((x >> 4) & k4) | ((x & k4) << 4);
            if (setboard) {data = x;}
            return x;
        }

        public ulong FlipDiagA8H1(ulong x) { //stole this from the internet
            ulong t;
            const ulong k1 = (0xaa00aa00aa00aa00);
            const ulong k2 = (0xcccc0000cccc0000);
            const ulong k4 = (0xf0f0f0f00f0f0f0f);
            t  =       x ^ (x << 36) ;
            x ^= k4 & (t ^ (x >> 36));
            t  = k2 & (x ^ (x << 18));
            x ^=       t ^ (t >> 18) ;
            t  = k1 & (x ^ (x <<  9));
            x ^=       t ^ (t >>  9) ;
            return x;
        }

        public ulong Rotate90() {
            return FlipDiagA8H1(FlipVertical(data));
        }

        public ulong RotateAnti90() {
            return FlipVertical(FlipDiagA8H1(data));
        }

        public int ActiveBits() //counts the number of ones in a bit mask
        {
            Bitboard DataCopy = new Bitboard(data);
            int Count = 0;
            while (DataCopy.GetData() > 0)
            {
                DataCopy.ClearBit(DataCopy.LSB());
                Count++;
            }
            return Count;
        }

        public void PrintData()
        {
            string StringOfBitboard = Convert.ToString((long)data, 2);
            while (StringOfBitboard.Length < 64)
            {
                StringOfBitboard = "0" + StringOfBitboard;
            }
            StringOfBitboard = string.Join(" ", StringOfBitboard.Select(c => c.ToString())); //adds some space so it's easier to read
            for (int i = 0; i < 8; i++)
            {
                Console.WriteLine(new string(StringOfBitboard.Substring(i*16, 15).Reverse().ToArray())); //prints it line by line (has to be reversed for making sense)
            }
        }
    }   
}