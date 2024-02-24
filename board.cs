//this is the file where all the code for the board representation is going to be managed from
using System;
using System.Collections.Generic;
namespace ChessEngine 
{
    public class Board
    { 
        public const string DefaultFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"; //this is the default chess starting position

        public int ColourToMove; // 0 for white 1 for black
        public int EnPassantSquare;
        public int MoveNumber;
        public bool WhiteShortCastle;
        public bool WhiteLongCastle;
        public bool BlackShortCastle;
        public bool BlackLongCastle;

        //bitboards
        public Bitboard WhitePawns = new Bitboard();
        public Bitboard WhiteKnights = new Bitboard();
        public Bitboard WhiteBishops = new Bitboard();
        public Bitboard WhiteRooks = new Bitboard();
        public Bitboard WhiteQueens = new Bitboard();
        public Bitboard WhiteKing = new Bitboard();

        public Bitboard BlackPawns = new Bitboard();
        public Bitboard BlackKnights = new Bitboard();
        public Bitboard BlackBishops = new Bitboard();
        public Bitboard BlackRooks = new Bitboard();
        public Bitboard BlackQueens = new Bitboard();
        public Bitboard BlackKing = new Bitboard();

        public Board() 
        {
            this.UploadFEN(DefaultFEN);
        }

        public void UploadFEN(string FEN)
        {
            int currentSquare = 63;
            foreach (char character in FEN)
            {
                if (character == '/') //new rank
                {
                    continue;
                }
                if (char.IsDigit(character)) //is it a number?
                {
                    currentSquare -= character - '0';
                }
                else 
                {
                    switch (character) //lowercase for black, uppercase for white
                    {
                        case 'p':
                            BlackPawns.SetBit(currentSquare);
                            break;
                        case 'n':
                            BlackKnights.SetBit(currentSquare);
                            break;
                        case 'b':
                            BlackBishops.SetBit(currentSquare);
                            break;
                        case 'r':
                            BlackRooks.SetBit(currentSquare);
                            break;
                        case 'q':
                            BlackQueens.SetBit(currentSquare);
                            break;
                        case 'k':
                            BlackKing.SetBit(currentSquare);
                            break;
                        case 'P':
                            WhitePawns.SetBit(currentSquare);
                            break;
                        case 'N':
                            WhiteKnights.SetBit(currentSquare);
                            break;
                        case 'B':
                            WhiteBishops.SetBit(currentSquare);
                            break;
                        case 'R':
                            WhiteRooks.SetBit(currentSquare);
                            break;
                        case 'Q':
                            WhiteQueens.SetBit(currentSquare);
                            break;
                        case 'K':
                            WhiteKing.SetBit(currentSquare);
                            break;
                        default:
                            break;
                    }
                    currentSquare--;
                }
            }
        }
    }
}