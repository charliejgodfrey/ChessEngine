//this is where the code for precomputing certain values is
using System;
using System.Collections.Generic;
using System.Collections;

namespace ChessEngine 
{
    static class PreComputeData
    {
        public static Bitboard[] KnightAttackBitboards = new Bitboard[64];
        public static Bitboard[] KingAttackBitboards = new Bitboard[64];
        public static Bitboard[] WhitePawnAttackBitboards = new Bitboard[64];
        public static Bitboard[] BlackPawnAttackBitboards = new Bitboard[64];
        public static ulong[] RookMasks = new ulong[64];
        public static ulong[] BishopMasks = new ulong[64];
        public static ulong[] RookMagics = new ulong[64];
        public static ulong[] BishopMagics = new ulong[64];
        public static Bitboard[,] RookAttacks = new Bitboard[64,8196];
        public static Bitboard[,] BishopAttacks = new Bitboard[64,8196];
        public static ulong[] KingAdjacents = new ulong[64];
        public static ulong[,] KingRays = new ulong[64, 8];
        public static ulong[] IsolationMasks = new ulong[64];
        public static ulong[,] PasserMasks = new ulong[2,64];
        public static ulong[,] KingShieldMasks = new ulong[2,64];

        public static void InitializeAttackBitboards()
        {
            PreComputeKnightAttacks();
            PreComputePawnAttacks();
            PreComputeKingAttacks();
            PreComputeRookMasks();
            PreComputeBishopMasks();
            LoadMagics();
            PreComputeKingAdjacents();
            PreComputeKingRays();
            PreComputeIsolationMasks();
            PreComputePasserMasks();
            PreComputeKingShieldMasks();
        }

        public static void PreComputeKingShieldMasks()
        {
            for (int i  = 0; i < 64; i++)
            {
                ulong WhiteMask = 0UL;
                ulong BlackMask = 0UL;
                if (i / 8 != 7)
                {
                    WhiteMask |= 1UL << (i+8);
                    if (i % 8 != 0) WhiteMask |= 1UL << (i+7);
                    if (i % 8 != 7) WhiteMask |= 1UL << (i+9);
                }

                if (i / 8 != 0)
                {
                    BlackMask |= 1UL << (i-8);
                    if (i % 8 != 0) WhiteMask |= 1UL << (i-9);
                    if (i % 8 != 0) WhiteMask |= 1UL << (i-7);
                }

                KingShieldMasks[0,i] = WhiteMask;
                KingShieldMasks[1,i] = BlackMask;
            }
        }

        public static void PreComputePasserMasks()
        {
            for (int i = 0; i < 64; i++) 
            {
                ulong InFront = ulong.MaxValue << (8*(i / 8));
                ulong NextTo = 0x0101010101010101UL << (i%8);
                if (i % 8 != 0) NextTo |= 0x0101010101010101UL << (i%8 - 1);
                if (i % 8 != 7) NextTo |= 0x0101010101010101UL << (i%8 + 1);
                PasserMasks[0,i] = NextTo & (InFront<<8);
                PasserMasks[1,i] = NextTo & ~InFront;
            }
        }

        public static void PreComputeIsolationMasks()
        {
            for (int i = 0; i < 64; i++) 
            {
                ulong NextTo = 0UL;
                if (i % 8 != 0) NextTo |= 0x0101010101010101UL << (i%8 - 1);
                if (i % 8 != 7) NextTo |= 0x0101010101010101UL << (i%8 + 1);
                IsolationMasks[i] = NextTo;
            }
        }

        public static void PreComputeKingRays()
        {
            int[][] directions = [[0,1],[0,-1],[1,0],[-1,0],[1,1],[-1,-1],[1,-1],[-1,1]];
            for (int i = 0; i < 64; i++) 
            {
                for (int d = 0; d < 8; d++)
                {
                    Bitboard ray = new Bitboard();
                    int index = i;
                    int rank = index / 8 + directions[d][0];
                    int file = index % 8 + directions[d][1];
                    while (file >= 0 && file < 8 && rank >= 0 && rank < 8)
                    {
                        ray.SetBit(rank*8 + file);
                        rank += directions[d][0];
                        file += directions[d][1];
                    }
                    KingRays[i,d] = ray.GetData();
                }
            }
        }

        public static void PreComputeKingAdjacents()
        {
            for (int s = 0; s < 64; s++)
            {
                int file = s % 8;
                int rank = s / 8;
                ulong SurroundingSquares = 0UL;
                SurroundingSquares |= 1UL << s;
                for (int f = -1; f <= 1; f++)
                {
                    for (int r = -1; r <= 1; r++)
                    {
                        if (file + f >= 0 && file + f < 8 && rank + r < 8 && rank + r >= 0)
                        {
                            SurroundingSquares |= 1UL << (rank + r) * 8 + file + f;
                        }
                    }
                }
                KingAdjacents[s] = SurroundingSquares;
            }
        }

        public static void ComputeMagics() //recalculates a set of fresh magic numbers 
        {
            for (int i = 0; i < 64; i++)
            {
                (ulong RookMagic, Bitboard[] RookTable) = Magic.FindMagic(i, RookMasks, true);
                (ulong BishopMagic, Bitboard[] BishopTable) = Magic.FindMagic(i, BishopMasks, false);
                RookMagics[i] = RookMagic;
                BishopMagics[i] = BishopMagic;
                for (int count = 0; count < RookTable.Length; count++) //setting the move lookup tables
                {
                    RookAttacks[i, count] = RookTable[count];
                }
                for (int count = 0; count < BishopTable.Length; count++) //setting the move lookup tables
                {
                    BishopAttacks[i, count] = BishopTable[count];
                }
                Console.WriteLine("Bishop Magic: " + i + " " + BishopMagic);
                Console.WriteLine("Rook Magic: " + i + " " + RookMagic);
            }
        }

        private static void PreComputeBishopMasks()
        {
            for (int i = 0; i < 64; i++)
            {
                Bitboard mask = new Bitboard();
                int file = i % 8;
                int rank = i / 8;
                int[] directions = {7,-7,9,-9};
                int[] distances = {Math.Min(file, 7-rank), Math.Min(7-file, rank), Math.Min(7-file, 7-rank), Math.Min(file, rank)}; //how many squares to the edge of the board
                for (int d = 0; d < 4; d++) //for each direction
                {
                    for (int step = 1; step < distances[d]; step++) //for each square in that direction to the edge of the board except edge squares
                    {
                        mask.SetBit(i + step*directions[d]); //edge squares not considered as they are irrelevent for generating attacks as there are no squares to block
                    }
                }
                BishopMasks[i] = mask.GetData();
            }
        }

        private static void PreComputeRookMasks()
        {
            for (int i = 0; i < 64; i++)
            {
                ulong mask = 0UL;
                int file = i % 8;
                int rank = i / 8;
                mask |= 0x7eUL << (8*rank); //for the rank
                mask |= 0x0001010101010100UL << file; //for the file
                mask &= ~(1UL << i);
                RookMasks[i] =  mask;
            }
        }

        private static void PreComputeKingAttacks()
        {
            int[][] directions = [[0,1],[1,1],[1,0],[1,-1],[0,-1],[-1,-1],[-1,0],[-1,1]];
            for (int i = 0; i < 64; i++)
            {
                KingAttackBitboards[i] = new Bitboard();
                int file = i % 8;
                int rank = i / 8; //file and rank of square index
                for (int q = 0; q < 8; q++)
                {
                    int NewFile = file + directions[q][0]; //new locations
                    int NewRank = rank + directions[q][1];
                    if (NewRank >= 0 && NewRank <= 7 && NewFile >= 0 && NewFile <= 7) //make sure it's on the board
                    {
                        KingAttackBitboards[i].SetBit(NewRank*8 + NewFile); //set the bitboard to contain the square as attacked
                    }
                }
            }
        }

        private static void PreComputePawnAttacks()
        {
            for (int i = 0; i < 64; i++)
            {
                WhitePawnAttackBitboards[i] = new Bitboard();
                int file = i % 8;
                int rank = i / 8; 
                if (file != 0 && rank != 7) WhitePawnAttackBitboards[i].SetBit(i + 7);//makes sure there aren't any attacks that would be off the board
                if (file != 7 && rank != 7) WhitePawnAttackBitboards[i].SetBit(i + 9); //----^
            }
            for (int i = 0; i < 64; i++)
            {
                BlackPawnAttackBitboards[i] = new Bitboard();
                int file = i % 8;
                int rank = i / 8; 
                if (file != 0 && rank != 0) BlackPawnAttackBitboards[i].SetBit(i - 7);//makes sure there aren't any attacks that would be off the board
                if (file != 7 && rank != 0) BlackPawnAttackBitboards[i].SetBit(i - 9); //----^
            }
        }

        private static void PreComputeKnightAttacks() //procedure to initialise the knight attacks array
        {
            int[][] directions = [[1,2],[2,1],[2,-1],[1,-2],[-1,-2],[-2,-1],[-2,1],[-1,2]];
            for (int i = 0; i < 64; i++)
            {
                KnightAttackBitboards[i] = new Bitboard();
                for (int q = 0; q < 8; q++)
                {
                    int[] direction = directions[q];
                    int[] square = [i % 8, i / 8];
                    square[0] += direction[0];
                    square[1] += direction[1];
                    int index = square[0] + square[1]*8;
                    if (square[0] >= 0 && square[1] >= 0 && square[0] <= 7 && square[1] <= 7)
                    {
                        KnightAttackBitboards[i].SetBit(index);
                    }
                }
            }
        }

        public static void LoadMagics() //loads the magics that i've previously computed using the magic.cs file - all magics are ones i have calculated
        {
            RookMagics[0] = 36039243991105536;RookMagics[1] = 18015085705298116;RookMagics[2] = 72068726609952784;RookMagics[3] = 180148451869329408;
            RookMagics[4] = 180146201299517824;RookMagics[5] =  72059810243215368;RookMagics[6] = 36030446588395776;RookMagics[7] = 36039936016695552;
            RookMagics[8] = 865957766395609088;RookMagics[9] = 70506204106752;RookMagics[10] = 140806216523776;RookMagics[11] = 162411199689916672;
            RookMagics[12] = 140771853598848;RookMagics[13] = 20267333270376516;RookMagics[14] = 576742235874328580;RookMagics[15] = 9147941038080896;
            RookMagics[16] = 63191682031550768;RookMagics[17] = 282575062960258;RookMagics[18] =565149253902400 ;RookMagics[19] = 2534374873497608;
            RookMagics[20] = 1226246285930015744;RookMagics[21] = 564053760541696;RookMagics[22] = 4507997690793992;RookMagics[23] = 36066180418545924;
            RookMagics[24] = 35736275927040;RookMagics[25] = 35189741846592;RookMagics[26] = 9253496302862464;RookMagics[27] = 17594334053504;
            RookMagics[28] = 281496452618240;RookMagics[29] = 381117419114988552;RookMagics[30] = 1407430727172196;RookMagics[31] = 1126183375011972;

            RookMagics[32] = 141012391428256;RookMagics[33] = 72268768994140160;RookMagics[34] = 576602726817861632;RookMagics[35] = 4556378358155264;
            RookMagics[36] = 81768635395221504;RookMagics[37] = 81205539379348480;RookMagics[38] = 2392563080235264;RookMagics[39] = 2393637920965764;
            RookMagics[40] = 9077570692808706;RookMagics[41] = 76562637311262720;RookMagics[42] = 27022014380310544;RookMagics[43] = 297246654968496160;
            RookMagics[44] = 281509338873872;RookMagics[45] = 73746461212409904;RookMagics[46] = 565458768232464;RookMagics[47] = 1267738257522692;
            RookMagics[48] = 22940211675673728;RookMagics[49] = 2357358299222144;RookMagics[50] = 140875104518272;RookMagics[51] = 71607060660736;
            RookMagics[52] = 5207321429147776;RookMagics[53] = 36169551691776128;RookMagics[54] = 2269426376905728;RookMagics[55] = 140741785436288;
            RookMagics[56] = 634426807816226;RookMagics[57] = 1689403915245602;RookMagics[58] = 2568460387238929;RookMagics[59] = 76637060236091401;
            RookMagics[60] = 586030936140030114;RookMagics[61] = 288793450793668610;RookMagics[62] = 79169812693572;RookMagics[63] = 180144676588913666;

            //bishop magics

            BishopMagics[0] = 21963878661390848;BishopMagics[1] = 571780458660864;BishopMagics[2] = 1130304429359104;BishopMagics[3] = 73187894139290112;
            BishopMagics[4] = 1129232801939456;BishopMagics[5] = 576746634990403840;BishopMagics[6] = 14641105728438272;BishopMagics[7] = 9029198363494400;
            BishopMagics[8] = 8933616002560;BishopMagics[9] = 2268850061568;BishopMagics[10] = 17609370126336;BishopMagics[11] = 6478339699113984;
            BishopMagics[12] = 71538049024000;BishopMagics[13] = 565157602264074;BishopMagics[14] =  2605844873625600;BishopMagics[15] = 15449005903872;
            BishopMagics[16] = 22518032770336768;BishopMagics[17] = 567365582979328;BishopMagics[18] = 2322237290053664;BishopMagics[19] = 2251939433693184;
            BishopMagics[20] = 140754779373592;BishopMagics[21] = 140771982379008;BishopMagics[22] = 2818615271621120;BishopMagics[23] = 281477283631104;
            BishopMagics[24] = 9308484099704832;BishopMagics[25] = 2256197894803968;BishopMagics[26] = 158330781704704;BishopMagics[27] = 39582422859784;
            BishopMagics[28] = 57243325170688;BishopMagics[29] = 2251937286324480;BishopMagics[30] = 45744081795617792;BishopMagics[31] = 281613506217984;

            BishopMagics[32] = 4508015392741376;BishopMagics[33] = 580559520794624;BishopMagics[34] = 288793764209623168;BishopMagics[35] = 1100585632768;
            BishopMagics[36] = 2251954432524544;BishopMagics[37] = 9011600533129216;BishopMagics[38] = 27327279176745280;BishopMagics[39] = 564332937412736;
            BishopMagics[40] = 10151808309674112;BishopMagics[41] = 571763259868160;BishopMagics[42] = 92359580772352;BishopMagics[43] = 283602060160;
            BishopMagics[44] = 288265569256358912;BishopMagics[45] = 567382393241632;BishopMagics[46] = 37719855096464896;BishopMagics[47] = 4510241796391168;
            BishopMagics[48] = 282643780407808;BishopMagics[49] = 576533354497835008;BishopMagics[50] = 565153280560128;BishopMagics[51] = 2251803068596224;
            BishopMagics[52] = 68753686560;BishopMagics[53] = 72075195619491904;BishopMagics[54] = 94578100456914944;BishopMagics[55] = 4505833063948320;
            BishopMagics[56] = 18730486743040;BishopMagics[57] = 324264672910901776;BishopMagics[58] = 18122159255750656;BishopMagics[59] = 18155548319023616;
            BishopMagics[60] = 289503617328218624;BishopMagics[61] = 17583571200;BishopMagics[62] = 11003777778690;BishopMagics[63] = 54046554394264096;

            for (int i = 0; i < 64; i++) //test all the magics to create the actual table of attacks
            {
                (bool r, Bitboard[] RookTable) = Magic.TestMagic(RookMagics[i], i, true, Magic.GenerateRookBlockerConfigurations(i, RookMasks));
                (bool ry, Bitboard[] BishopTable) = Magic.TestMagic(BishopMagics[i], i, false, Magic.GenerateBishopBlockerConfigurations(i, BishopMasks));
                for (int count = 0; count < RookTable.Length; count++) //setting the move lookup tables
                {
                    RookAttacks[i, count] = RookTable[count];
                }
                for (int count = 0; count < BishopTable.Length; count++) //setting the move lookup tables
                {
                    BishopAttacks[i, count] = BishopTable[count];
                }
            }
        }
    }
}