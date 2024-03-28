using System;
using System.Collections.Generic;
using System.Collections;
namespace ChessEngine 
{
    public class Program 
    {
        static void Main()
        {
            Board board = new Board("rnbqkbnr/pppppppp/8/8/8/P7/1PPPPPPP/RNBQKBNR");
            board.PrintBoard();
            PreComputeData.InitializeAttackBitboards();
        }
    }
}