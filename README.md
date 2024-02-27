# ChessEngine
Trying to code a chess engine in c# 

## Current To-Do List
- create functions to precompute the attack bitboards of the different pieces on different squares
- import magics for sliding piece move generation



# Board Representation

I'm going to use bitboards to represent the board, here's a summary of how it's gonna work:
- Each piece type will have a bitboard to store the locations of all of the pieces of that type
- There will be an occupancy bitboard to store the locations of all pieces
- There will be two colour bitboards, one for white and one for black pieces
- There will be attack bitboards for each type of piece, showing the squares attacked by said piece

Most of these bitboards can be generated using bitwise operations and are therefore very time efficient. They will be used for move generation which should be essentially done using just bitwise operators. Of course there will also be variables to store the castling rights, enPassant squares and turn

# Move Generation

This will build off the bitboard representation of the board. Using bitwise operations move generation should be very fast. Precomputed data can also be used to speed things up, such as pre calculating the potential knight moves from each square and how many squares in each direction there are from each square on the board until the edge of the board

## Check Detection
This is super fast using bitboards. Bitwise OR operations can be used to easily find all the attacked squares by either side, this can then be bitwise ANDed with the bitboard for the other colours king to see if the king is under attack, this method can be used to see if any particular square is under attack