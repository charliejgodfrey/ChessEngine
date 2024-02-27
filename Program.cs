using System;
namespace ChessEngine 
{
    public class Program 
    {
        static void Main()
        {
            Board board = new Board();
            board.PrintBoard();
            PreComputeData.InitializeAttackBitboards();
            List<Move> moves = MoveGenerator.GeneratePawnMoves(board);
            Console.WriteLine(moves);
        }
    }
}