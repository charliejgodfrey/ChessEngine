//this is the file where all the code for the board representation is going to be managed from
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace ChessEngine 
{
    public class Board
    { 
        public const string DefaultFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"; //this is the default chess starting position

        public int ColourToMove = 1; // 0 for white 1 for black
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

        public Bitboard BlackPieces = new Bitboard();
        public Bitboard WhitePieces = new Bitboard();
        public Bitboard OccupiedSquares = new Bitboard();
        public Bitboard[] Pieces = new Bitboard[12];

        public Board(string Fen = DefaultFEN, bool isCopy = false) 
        {
            if (!isCopy) 
            {
            this.UploadFEN(Fen);
            BlackPieces = new Bitboard(BlackPawns.GetData() | BlackKnights.GetData() | BlackBishops.GetData() | BlackRooks.GetData() | BlackQueens.GetData() | BlackKing.GetData());
            WhitePieces = new Bitboard(WhitePawns.GetData() | WhiteKnights.GetData() | WhiteBishops.GetData() | WhiteRooks.GetData() | WhiteQueens.GetData() | WhiteKing.GetData());
            OccupiedSquares = new Bitboard(BlackPieces.GetData() | WhitePieces.GetData());
            Pieces = [WhitePawns, WhiteKnights, WhiteBishops, WhiteRooks, WhiteQueens, WhiteKing, BlackPawns, BlackKnights, BlackBishops, BlackRooks, BlackQueens, BlackKing];
            }
        }

        public void MakeMove(Move move) 
        {
            int start = move.GetStart();
            int target = move.GetTarget();
            int piece = move.GetPiece();
            int flag = move.GetFlag();
            OccupiedSquares.ClearBit(start); //adjusts the occupied bitboard
            OccupiedSquares.SetBit(target);

            //adjusted colour specific occupancy bitboards
            if (ColourToMove == 0) //white moving
            {
                WhitePieces.ClearBit(start); //clear the start square
                WhitePieces.SetBit(target); //target square is now occupied for white
                BlackPieces.ClearBit(target); //any piece on the target square is captured
                //for piece specific bitboards
                Pieces[piece].ClearBit(start);
                Pieces[piece].SetBit(target);
                //taking into account captures
                for (int i = 6; i < 12; i++) //for every bitboard of white pieces
                {
                    Pieces[i].ClearBit(target);
                }
            } else { //black moving
                BlackPieces.ClearBit(start);
                BlackPieces.SetBit(target);
                WhitePieces.ClearBit(target);
                //for the piece specific bitboards
                Pieces[piece + 6].ClearBit(start); //the +6 is for offsetting the piece to the index for the black pieces
                Pieces[piece + 6].SetBit(target);
                //taking into account captures
                for (int i = 0; i < 6; i++) //for every bitboard of white pieces
                {
                    Pieces[i].ClearBit(target);
                }
            }

            //other checks:
            if (flag == 0b0001) //double pawn push
            {
                EnPassantSquare = target + (ColourToMove == 0 ? 8 : -8); //the offset means that the square is referring to where an enpassanting pawn would move to - not where the piece is being captured
            }
            if (flag == 0b0101) //en passant capture
            {
                //ColourToMove == 0 ? BlackPawns.ClearBit(target - 8) : WhitePawns.ClearBit(target + 8);
            }
            if (flag >= 0b1000) //promotion
            {
                Pieces[(flag & 0b0011) + (ColourToMove == 0 ? 1 : 7)].SetBit(target); //promotion piece bitboard
                Pieces[(ColourToMove == 0 ? 1 : 7)].ClearBit(target); //pawn bitboard
            }
            ColourToMove = (ColourToMove == 0 ? 1 : 0);
        }

        public void RefreshBitboardConfiguration()
        {
            BlackPieces = new Bitboard(BlackPawns.GetData() | BlackKnights.GetData() | BlackBishops.GetData() | BlackRooks.GetData() | BlackQueens.GetData() | BlackKing.GetData());
            WhitePieces = new Bitboard(WhitePawns.GetData() | WhiteKnights.GetData() | WhiteBishops.GetData() | WhiteRooks.GetData() | WhiteQueens.GetData() | WhiteKing.GetData());
            OccupiedSquares = new Bitboard(BlackPieces.GetData() | WhitePieces.GetData());
            Pieces = [WhitePawns, WhiteKnights, WhiteBishops, WhiteRooks, WhiteQueens, WhiteKing, BlackPawns, BlackKnights, BlackBishops, BlackRooks, BlackQueens, BlackKing];
        }

        public Board Copy()
        {
            Board board = new Board(DefaultFEN, true);
            board.ColourToMove = this.ColourToMove;
            board.EnPassantSquare = this.EnPassantSquare;
            board.MoveNumber = this.MoveNumber;
            board.WhiteShortCastle = this.WhiteShortCastle;
            board.WhiteLongCastle = this.WhiteLongCastle;
            board.BlackShortCastle = this.BlackShortCastle;
            board.BlackLongCastle = this.BlackLongCastle;

            board.WhitePawns.SetData(this.WhitePawns.GetData());
            board.WhiteKnights.SetData(this.WhiteKnights.GetData());
            board.WhiteBishops.SetData(this.WhiteBishops.GetData());
            board.WhiteRooks.SetData(this.WhiteRooks.GetData());
            board.WhiteQueens.SetData(this.WhiteQueens.GetData());
            board.WhiteKing.SetData(this.WhiteKing.GetData());

            board.BlackPawns.SetData(this.BlackPawns.GetData());
            board.BlackKnights.SetData(this.BlackKnights.GetData());
            board.BlackBishops.SetData(this.BlackBishops.GetData());
            board.BlackRooks.SetData(this.BlackRooks.GetData());
            board.BlackQueens.SetData(this.BlackQueens.GetData());
            board.BlackKing.SetData(this.BlackKing.GetData());
            
            board.RefreshBitboardConfiguration();

            return board;
        }

        public void UploadFEN(string FEN)
        {
            int currentSquare = 56;
            foreach (char character in FEN)
            {
                if (character == '/') //new rank
                {
                    currentSquare -= 17;
                }
                if (char.IsDigit(character)) //is it a number?
                {
                    currentSquare += character - '0'; // turns character into a number
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
                    currentSquare++;
                }
            }
        }

        public void PrintBoard()
        {
            Console.WriteLine("================");
            string BoardRepresentation = ""; //initial state to print
            for (int i = 0; i < 64; i++)
            {
                if (WhitePawns.IsBitSet(i)) {BoardRepresentation += "P ";}
                else if (WhiteKnights.IsBitSet(i)) {BoardRepresentation += "N ";}
                else if (WhiteBishops.IsBitSet(i)) {BoardRepresentation += "B ";}
                else if (WhiteRooks.IsBitSet(i)) {BoardRepresentation += "R ";}
                else if (WhiteQueens.IsBitSet(i)) {BoardRepresentation += "Q ";}
                else if (WhiteKing.IsBitSet(i)) {BoardRepresentation += "K ";}
                else if (BlackPawns.IsBitSet(i)) {BoardRepresentation += "p ";}
                else if (BlackKnights.IsBitSet(i)) {BoardRepresentation += "n ";}
                else if (BlackBishops.IsBitSet(i)) {BoardRepresentation += "b ";}
                else if (BlackRooks.IsBitSet(i)) {BoardRepresentation += "r ";}
                else if (BlackQueens.IsBitSet(i)) {BoardRepresentation += "q ";}
                else if (BlackKing.IsBitSet(i)) {BoardRepresentation += "k ";}
                else {BoardRepresentation += "- ";}
            }
            Console.WriteLine(BoardRepresentation.Substring(112,16));
            Console.WriteLine(BoardRepresentation.Substring(96,16));
            Console.WriteLine(BoardRepresentation.Substring(80,16));
            Console.WriteLine(BoardRepresentation.Substring(64,16));
            Console.WriteLine(BoardRepresentation.Substring(48,16));
            Console.WriteLine(BoardRepresentation.Substring(32,16));
            Console.WriteLine(BoardRepresentation.Substring(16, 16));
            Console.WriteLine(BoardRepresentation.Substring(0,16));
            Console.WriteLine("================");
        }
    }
}