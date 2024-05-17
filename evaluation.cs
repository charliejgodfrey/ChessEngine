using System;
using System.Collections.Generic;
using System.Numerics;

namespace ChessEngine 
{
    public static class Evaluation 
    {
        public static float[] MaterialValues = [1,3,3,5,9,0,-1,-3,-3,-5,-9,0];
        public static float Material(Board board)
        {
            float score = 0;
            for (int i = 0; i < 12; i++) // for each piece excluding king
            {
                score += board.Pieces[i].ActiveBits() * MaterialValues[i];
            }
            return score;
        }
    }
}