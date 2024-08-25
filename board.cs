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
        public bool WhiteShortCastle = false;
        public bool WhiteLongCastle = false;
        public bool BlackShortCastle = false;
        public bool BlackLongCastle = false;
        public int[] PieceCount = new int[10];
        public float Eval = 0;
        public ZobristHasher Hasher = new ZobristHasher();
        public ulong Zobrist;

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
            Zobrist = Hasher.Hash(this);
            }
        }

        public int GetPiece(int square, int Colour) //7 indicates that it isn't a capture
        {
            ulong mask = (1UL << square);
            if (Colour == 0) //white piece on the location
            {
                if ((WhitePawns.GetData() & mask) != 0) return 0;
                if ((WhiteKnights.GetData() & mask) != 0) return 1;
                if ((WhiteBishops.GetData() & mask) != 0) return 2;
                if ((WhiteRooks.GetData() & mask) != 0) return 3;
                if ((WhiteQueens.GetData() & mask) != 0) return 4;
                if ((WhiteKing.GetData() & mask) != 0) return 5;
                else return 7;
            }
            if (Colour == 1) //black piece on the location
            {
                if ((BlackPawns.GetData() & mask) != 0) return 0;
                if ((BlackKnights.GetData() & mask) != 0) return 1;
                if ((BlackBishops.GetData() & mask) != 0) return 2;
                if ((BlackRooks.GetData() & mask) != 0) return 3;
                if ((BlackQueens.GetData() & mask) != 0) return 4;
                if ((BlackKing.GetData() & mask) != 0) return 5;
                else return 7;
            }
            return 7;
        }

        public void MakeMove(Move move) 
        {
            if (move.GetNullMove() == 1)
            {
                ColourToMove = (ColourToMove == 0 ? 1 : 0);
                Zobrist ^= Hasher.SideToMove;
                return;
            }
            if ((move.GetFlag() & 0b1110) == 0b0010) //a castling move
            {
                this.Castle((move.GetFlag() == 0b0010) ? 1 : 2);
                UpdateEval(move);
                UpdateZobrist(move);    
                ColourToMove = (ColourToMove == 0 ? 1 : 0);
                return;
            }
            int start = move.GetStart();
            int target = move.GetTarget();
            int piece = move.GetPiece();
            int flag = move.GetFlag();
            int capture = move.GetCapture();
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
                if (capture != 0b111) Pieces[capture + 6].ClearBit(target);
                //for (int i = 6; i<12;i++) Pieces[i].ClearBit(target);
            } else { //black moving
                BlackPieces.ClearBit(start);
                BlackPieces.SetBit(target);
                WhitePieces.ClearBit(target);
                //for the piece specific bitboards
                Pieces[piece + 6].ClearBit(start); //the +6 is for offsetting the piece to the index for the black pieces
                Pieces[piece + 6].SetBit(target);
                //taking into account captures
                if (capture != 0b111) Pieces[capture].ClearBit(target);
                //for (int i = 0; i<6;i++) Pieces[i].ClearBit(target);
            }

            //other checks:
            if (flag == 0b0001) //double pawn push
            {
                EnPassantSquare = target + (ColourToMove == 0 ? -8 : 8); //the offset means that the square is referring to where an enpassanting pawn would move to - not where the piece is being captured
            } 
            else EnPassantSquare = -1;
            if (flag == 0b0101) //en passant capture
            {
                if (ColourToMove == 0) BlackPawns.ClearBit(target - 8);
                else WhitePawns.ClearBit(target + 8);
                EnPassantSquare = -1;
            }
            if (flag >= 0b1000) //promotion
            {
                Pieces[(flag & 0b0011) + (ColourToMove == 0 ? 1 : 7)].SetBit(target); //promotion piece bitboard
                Pieces[(ColourToMove == 0 ? 1 : 7)].ClearBit(target); //pawn bitboard
            }
            UpdateEval(move);
            UpdateZobrist(move);
            ColourToMove = (ColourToMove == 0 ? 1 : 0);
        }

        public void UnmakeMove(Move move) //add castling, en passant
        {
            int start = move.GetStart();
            int target = move.GetTarget();
            int piece = move.GetPiece();
            int flag = move.GetFlag();
            int capture = move.GetCapture();
            if ((move.GetFlag() & 0b1110) == 0b0010) //a castling move
            {
                this.Uncastle((move.GetFlag() == 0b0010) ? 1 : 2);
                UpdateEval(move);
                UpdateZobrist(move);
                ColourToMove = (ColourToMove == 0 ? 1 : 0); //because the zobrist is made from scratch including a move change    
                return;
            }
            if (ColourToMove == 1) //white unmaking the unmove
            {
                WhitePieces.SetBit(start);
                WhitePieces.ClearBit(target);
                OccupiedSquares.SetBit(start);
                Pieces[piece].ClearBit(target); //empty where the piece just moved back from
                if (capture != 0b111) // if it was a capturing move
                {
                    Pieces[capture + 6].SetBit(target);
                    BlackPieces.SetBit(target);
                    OccupiedSquares.SetBit(target);
                } else{
                    OccupiedSquares.ClearBit(target);
                }
                if (flag >= 0b1000) //promotion
                {
                    Pieces[0].SetBit(start); //move the piece back but into a pawn
                } else {
                    Pieces[piece].SetBit(start);
                }
                if (flag == 0b0101) { //en passant
                    BlackPawns.SetBit(target - 8);
                    EnPassantSquare = target - 8;
                }
            } else {
                BlackPieces.SetBit(start);
                BlackPieces.ClearBit(target);
                OccupiedSquares.SetBit(start);
                Pieces[piece + 6].ClearBit(target); //empty where the piece just moved back from
                if (capture != 0b111) // if it was a capturing move
                {
                    Pieces[capture].SetBit(target);
                    WhitePieces.SetBit(target);
                    OccupiedSquares.SetBit(target);
                } else{
                    OccupiedSquares.ClearBit(target);
                }
                if (flag >= 0b1000) //promotion
                {
                    Pieces[6].SetBit(start); //move the piece back
                } else {
                    Pieces[piece+6].SetBit(start);
                }
                if (flag == 0b0101) { //en passant
                    WhitePawns.SetBit(target + 8);
                    EnPassantSquare = (target + 8);
                }
            }

            UnupdateEval(move);
            if (flag == 0 || flag == 0b0100 || flag == 0b0001) {
                ColourToMove = (ColourToMove == 0 ? 1 : 0);
                UpdateZobrist(move);
            } else {
                UpdateZobrist(move);
                ColourToMove = (ColourToMove == 0 ? 1 : 0);
            }
        }

        public void UpdateEval(Move move)
        {
            if (move.GetFlag() == 0b0101 || move.GetFlag() >= 0b1000 || move.GetFlag() == 0b0010 || move.GetFlag() == 0b0011)
            {
                Eval = Evaluation.WeightedMaterial(this);
                return;
            }
            int piece = move.GetPiece();
            int target = move.GetTarget();
            int start = move.GetStart();
            int capture = move.GetCapture();
            if (ColourToMove == 0)
            {
                Eval += Evaluation.PieceSquareTable[piece][((7  - (target / 8)) * 8 + target % 8)];
                Eval -= Evaluation.PieceSquareTable[piece][((7  - (start / 8)) * 8 + start % 8)];
                if (capture != 0b111) //it is a capture
                {
                    Eval += Evaluation.MaterialValues[capture];
                    Eval += Evaluation.PieceSquareTable[capture][target];
                }
            }
            else if (ColourToMove == 1)
            {
                Eval -= Evaluation.PieceSquareTable[piece][target]; //the funky stuff is for reversing the piece square table to be from the black perspective
                Eval += Evaluation.PieceSquareTable[piece][start];
                if (capture != 0b111)
                {
                    Eval -= Evaluation.MaterialValues[capture];
                    Eval -= Evaluation.PieceSquareTable[capture][((7  - (target / 8)) * 8 + target % 8)];
                }
            }
        }

        public void UnupdateEval(Move move)
        {
            if (move.GetFlag() == 0b0101 || move.GetFlag() >= 0b1000)
            {
                Eval = Evaluation.WeightedMaterial(this);
                return;
            }
            int piece = move.GetPiece();
            int target = move.GetTarget();
            int start = move.GetStart();
            int capture = move.GetCapture();

            if (ColourToMove == 1) //white is unmaking the move
            {
                Eval -= Evaluation.PieceSquareTable[piece][((7  - (target / 8)) * 8 + target % 8)];
                Eval += Evaluation.PieceSquareTable[piece][((7  - (start / 8)) * 8 + start % 8)];
                if (capture != 0b111) //is an uncapturing unmove
                {
                    Eval -= Evaluation.MaterialValues[capture];
                    Eval -= Evaluation.PieceSquareTable[capture][target];
                }
            } else { //black unmaking the move
                Eval += Evaluation.PieceSquareTable[piece][target];
                Eval -= Evaluation.PieceSquareTable[piece][start];
                if (capture != 0b111) //is an uncapturing unmove
                {
                    Eval += Evaluation.MaterialValues[capture];
                    Eval += Evaluation.PieceSquareTable[capture][((7  - (target / 8)) * 8 + target % 8)];
                }
            }
        }

        public void UpdateZobrist(Move move) //this works for making and unmaking moves 
        {
            //Console.WriteLine("prior zobby: " + Zobrist);
            int colourAdd = (ColourToMove == 0 ? 0 : 6); // determines adding constant if it is black moving
            //if (unmove) colourAdd = (colourAdd == 0 ? 6 : 0);
            if (move.GetFlag() == 0b0101 || move.GetFlag() >= 0b1000 || move.GetFlag() == 0b0010 || move.GetFlag() == 0b0011)
            {
                Zobrist = Hasher.Hash(this);
                Zobrist ^= Hasher.SideToMove; //this is because UpdateZobrist is called before ColourToMove is updated
                // Console.WriteLine("manual update zobrist: " + Zobrist);
                // Console.WriteLine("correct zobrist: " + Hasher.Hash(this));
                return;
            }
            Zobrist ^= Hasher.ZobristTable[move.GetPiece()+colourAdd][move.GetStart()];
            Zobrist ^= Hasher.ZobristTable[move.GetPiece()+colourAdd][move.GetTarget()];
            if (move.GetCapture() != 0b111) //it is a capture
            {
                Zobrist ^= Hasher.ZobristTable[move.GetCapture() + (ColourToMove == 0 ? 6 : 0)][move.GetTarget()];
            }
            Zobrist ^= Hasher.SideToMove;
        }

        public void Uncastle(int type)
        {
            if (type == 1 && this.ColourToMove == 1) //short castle for white
            {
                this.WhiteKing.SetData(0x10UL); //king back in position
                this.OccupiedSquares.AND(~0x60UL); //clear the king and rook
                this.OccupiedSquares.OR(0x90UL); //puts king and rook back
                this.WhitePieces.AND(~0x60UL); //clear the king and rook
                this.WhitePieces.OR(0x90UL);
                this.WhiteRooks.ClearBit(5); //clear rook
                this.WhiteRooks.SetBit(7); //replace rook
                this.WhiteLongCastle = true;
                this.WhiteShortCastle = true;
                return;
            }
            if (type == 2 && this.ColourToMove == 1) //long castle for white
            {
                this.WhiteKing.SetData(0x10UL); //resets king position
                this.OccupiedSquares.AND(~0x6UL); //clears the king and rook
                this.OccupiedSquares.OR(0x11UL);
                this.WhitePieces.AND(~0x6UL); //clears the king and rook
                this.WhitePieces.OR(0x11UL);
                this.WhiteRooks.ClearBit(3); //clear rook
                this.WhiteRooks.SetBit(0);// replace rook
                this.WhiteLongCastle = true;
                this.WhiteShortCastle = true;
                return;
            }
            if (type == 1 && this.ColourToMove == 0) //short castle for black
            {
                this.BlackKing.SetData(0x1000000000000000UL); //resets king position
                this.OccupiedSquares.AND(~0x6000000000000000UL); //clears rook and king
                this.OccupiedSquares.OR(0x9000000000000000UL);
                this.BlackPieces.AND(~0x6000000000000000UL); //clears rook and king
                this.BlackPieces.OR(0x9000000000000000UL);
                this.BlackRooks.ClearBit(61);
                this.BlackRooks.SetBit(63);
                this.BlackShortCastle = true;
                this.BlackLongCastle = true;
                return;
            }
            if (type == 2 && this.ColourToMove == 0) //long castle for black
            {
                //Console.WriteLine("un long castled for black");
                this.BlackKing.SetData(0x1000000000000000UL); //resets king position
                this.OccupiedSquares.AND(~0x0C00000000000000UL); //clears rook and king
                this.OccupiedSquares.OR(0x1100000000000000UL);
                this.BlackPieces.AND(~0x0C00000000000000UL); //clears rook and king
                this.BlackPieces.OR(0x1100000000000000UL);
                this.BlackRooks.ClearBit(59); //clears rook from castled position
                this.BlackRooks.SetBit(56); //puts rook back in corner
                this.BlackShortCastle = true;
                this.BlackLongCastle = true;
                return;
            }
        }

        public void Castle(int type)
        {
            if (type == 1 && this.ColourToMove == 0) //short castle for white
            {
                this.WhiteKing.SetData(0x40UL);
                this.OccupiedSquares.AND(~0x90UL); 
                this.OccupiedSquares.OR(0x60UL);
                this.WhitePieces.AND(~0x90UL);
                this.WhitePieces.OR(0x60UL);
                this.WhiteRooks.ClearBit(7);
                this.WhiteRooks.SetBit(5);
                this.WhiteLongCastle = false;
                this.WhiteShortCastle = false;
                return;
            }
            if (type == 2 && this.ColourToMove == 0) //long castle for white
            {
                this.WhiteKing.SetData(0x4UL);
                this.OccupiedSquares.AND(~0x11UL);
                this.OccupiedSquares.OR(0xCUL);
                this.WhitePieces.AND(~0x11UL);
                this.WhitePieces.OR(0xCUL);
                this.WhiteRooks.ClearBit(0);
                this.WhiteRooks.SetBit(3);
                this.WhiteLongCastle = false;
                this.WhiteShortCastle = false;
                return;
            }
            if (type == 1 && this.ColourToMove == 1) //short castle for black
            {
                this.BlackKing.SetData(0x4000000000000000UL);
                this.OccupiedSquares.AND(~0x9000000000000000UL);
                this.OccupiedSquares.OR(0x6000000000000000UL);
                this.BlackPieces.AND(~0x9000000000000000UL);
                this.BlackPieces.OR(0x6000000000000000UL);
                this.BlackRooks.ClearBit(63);
                this.BlackRooks.SetBit(61);
                this.BlackShortCastle = false;
                this.BlackLongCastle = false;
                return;
            }
            if (type == 2 && this.ColourToMove == 1) //long castle for black
            {
                this.BlackKing.SetData(0x400000000000000UL);
                this.OccupiedSquares.AND(~0x1100000000000000UL);
                this.OccupiedSquares.OR(0x0C00000000000000UL);
                this.BlackPieces.AND(~0x1100000000000000UL);
                this.BlackPieces.OR(0x0C00000000000000UL);
                this.BlackRooks.ClearBit(56);
                this.BlackRooks.SetBit(59);
                this.BlackShortCastle = false;
                this.BlackLongCastle = false;
                return;
            }
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
            board.Hasher = this.Hasher;
            board.Zobrist = this.Zobrist;
            board.ColourToMove = this.ColourToMove;
            board.EnPassantSquare = this.EnPassantSquare;
            board.MoveNumber = this.MoveNumber;
            board.WhiteShortCastle = this.WhiteShortCastle;
            board.WhiteLongCastle = this.WhiteLongCastle;
            board.BlackShortCastle = this.BlackShortCastle;
            board.BlackLongCastle = this.BlackLongCastle;
            board.Eval = this.Eval;

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
                            PieceCount[5]++;
                            BlackPawns.SetBit(currentSquare);
                            break;
                        case 'n':
                            PieceCount[6]++;
                            BlackKnights.SetBit(currentSquare);
                            break;
                        case 'b':
                            PieceCount[7]++;
                            BlackBishops.SetBit(currentSquare);
                            break;
                        case 'r':
                            PieceCount[8]++;
                            BlackRooks.SetBit(currentSquare);
                            break;
                        case 'q':
                            PieceCount[9]++;
                            BlackQueens.SetBit(currentSquare);
                            break;
                        case 'k':
                            BlackKing.SetBit(currentSquare);
                            break;
                        case 'P':
                            PieceCount[0]++;
                            WhitePawns.SetBit(currentSquare);
                            break;
                        case 'N':
                            PieceCount[1]++;
                            WhiteKnights.SetBit(currentSquare);
                            break;
                        case 'B':
                            PieceCount[2]++;
                            WhiteBishops.SetBit(currentSquare);
                            break;
                        case 'R':
                            PieceCount[3]++;
                            WhiteRooks.SetBit(currentSquare);
                            break;
                        case 'Q':
                            PieceCount[4]++;
                            WhiteQueens.SetBit(currentSquare);
                            break;
                        case 'K':
                            WhiteKing.SetBit(currentSquare);
                            break;
                        // case ' ':
                        //     ColourToMove = (FEN[currentSquare+1] == 'w' ? 0 : 1);
                        //     break;
                        default:
                            break;
                    }
                    currentSquare++;
                }
            }
            // if (!WhiteKing.IsBitSet(3) || !WhiteRooks.IsBitSet(0)) WhiteShortCastle = false;
            // if (!WhiteKing.IsBitSet(3) || !WhiteRooks.IsBitSet(7)) WhiteLongCastle = false;
            // if (!WhiteKing.IsBitSet(59) || !WhiteRooks.IsBitSet(56)) BlackShortCastle = false;
            // if (!WhiteKing.IsBitSet(59) || !WhiteRooks.IsBitSet(63)) BlackLongCastle = false;
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

        public void PrintBitboards()
        {
            Console.WriteLine("White Pawns:");
            WhitePawns.PrintData();
            
            Console.WriteLine("White Knights:");
            WhiteKnights.PrintData();
            
            Console.WriteLine("White Bishops:");
            WhiteBishops.PrintData();
            
            Console.WriteLine("White Rooks:");
            WhiteRooks.PrintData();
            
            Console.WriteLine("White Queens:");
            WhiteQueens.PrintData();
            
            Console.WriteLine("White King:");
            WhiteKing.PrintData();
            
            Console.WriteLine("Black Pawns:");
            BlackPawns.PrintData();
            
            Console.WriteLine("Black Knights:");
            BlackKnights.PrintData();
            
            Console.WriteLine("Black Bishops:");
            BlackBishops.PrintData();
            
            Console.WriteLine("Black Rooks:");
            BlackRooks.PrintData();
            
            Console.WriteLine("Black Queens:");
            BlackQueens.PrintData();
            
            Console.WriteLine("Black King:");
            BlackKing.PrintData();
            
            Console.WriteLine("White Pieces:");
            WhitePieces.PrintData();
            
            Console.WriteLine("Black Pieces:");
            BlackPieces.PrintData();
            
            Console.WriteLine("Occupied Squares:");
            OccupiedSquares.PrintData();
        }
    }
}