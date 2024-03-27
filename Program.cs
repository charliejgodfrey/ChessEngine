using System;
using System.Collections.Generic;
using System.Collections;
namespace ChessEngine 
{
    public class Program 
    {
        static void Main()
        {
            Board board = new Board("r1bqkbnr/ppppp1pp/2n5/5p2/3PP3/2N5/PPP2PPP/R1BQKBNR");
            board.PrintBoard();
            PreComputeData.InitializeAttackBitboards();
            Move[] moves = MoveGenerator.GenerateMoves(board);
            Bitboard ba = MoveGenerator.GenerateBishopAttacks(board, 3);
            ba.PrintData();
            Magic.FindMagic(3, PreCompute.BishopMasks, false);
            foreach(Move move in moves)
            {
                move.PrintMove();
            }
        }
    }
}