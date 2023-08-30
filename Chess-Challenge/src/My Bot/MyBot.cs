using ChessChallenge.API;
using ChessChallenge.Application;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{

    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    bool myColor;
    public Move Think(Board board, Timer timer)
    {
        //Finds out what color I am playing as
        myColor = board.IsWhiteToMove;

        //Ensures some move will be found.
        int bestMoveFoundValue = int.MinValue;

        //Gets Move List
        Move[] allMovesList = board.GetLegalMoves();

        //Instantiates moveToPlay
        Move moveToPlay = allMovesList[0];


        foreach (Move move in allMovesList){

            board.MakeMove(move);


            
            if (ValueOfThe(board) > bestMoveFoundValue){
                moveToPlay = move;
                bestMoveFoundValue = ValueOfThe(board);
            }
            

            Move[] allOpponentMoves = board.GetLegalMoves();
            bool checkmatePreventionFound = true;
            foreach (Move opponentMove in allOpponentMoves){
                checkmatePreventionFound = true;
                board.MakeMove(opponentMove);
                Move[] allResponses = board.GetLegalMoves();
                foreach (Move response in allResponses){
                    board.MakeMove(response);
                    if (board.IsInCheckmate()){
                        checkmatePreventionFound = false;
                    }
                    board.UndoMove(response);
                }
                board.UndoMove(opponentMove);
                if (checkmatePreventionFound){
                    break;
                }  
            }
            
            if (!checkmatePreventionFound && bestMoveFoundValue<20000){
                moveToPlay = move;
                bestMoveFoundValue = 19999;
            }


            board.UndoMove(move);

        }

        return moveToPlay;
    }







    private int ValueOfThe(Board inputBoard){


        bool isMyTurn = inputBoard.IsWhiteToMove == myColor;
    
    /*Endgame Eval*/
        //Returns neutral when drawn
        if (inputBoard.IsDraw()){
            return 0;
        }

        
        if (inputBoard.IsInCheckmate()){
            //Favorable score when opponent is in checkmate
            if (!isMyTurn){
                return 20000;
            }
            //Lower score when we are in checkmate
            else {
                return -20000;
            }
        }
    
    /*Material Eval*/
        //Adds up material
        int materialValue = 0;
        PieceList[] allPieces = inputBoard.GetAllPieceLists();
        foreach (PieceList pieces in allPieces){
            int modifier = 1;
            if (pieces.IsWhitePieceList != myColor){
                modifier = -1;
            }
            materialValue+= modifier * pieces.Count * pieceValues[(int)pieces.TypeOfPieceInList];
        }

    /*Repetition Eval*/
        int repetitionValue = 0;
        ulong[] repetitionHistory = inputBoard.GameRepetitionHistory;
        ulong zobristKey = inputBoard.ZobristKey;
        foreach(ulong key in repetitionHistory){
            if (key == zobristKey){
                repetitionValue-=300;
            }
        }



    /* TODO Development Eval*/


    /*Threatenings Eval*/
        //Slightly favors checks
        int threatValue = 0;
        if (inputBoard.IsInCheck()){
            if (isMyTurn){
                threatValue-=1;
            }
            else {
                threatValue+=1;
            }
        }

    /*Endgame Eval*/
    int endgameValue = 0;
        if (inputBoard.PlyCount > 40){

            //Favors"pushing" enemy king to edge of board
            Square enemyKingSquare = inputBoard.GetKingSquare(!myColor);
            if (enemyKingSquare.Rank%7 == 0 || enemyKingSquare.File%7 == 0){
                endgameValue+=10;
            }

            //Favors pushing pawns
            PieceList myPawns = inputBoard.GetPieceList((PieceType)1, myColor);
            for (int pawnIndex = 0; pawnIndex < myPawns.Count; pawnIndex++){
                int pawnPos = myPawns.GetPiece(pawnIndex).Square.Rank;
                if (myColor /* is white */){
                    endgameValue+= pawnPos - 1;
                }
                else {
                    endgameValue+= 6 - pawnPos;
                }
            }
        }
        
        return materialValue+repetitionValue+threatValue+endgameValue;
    }
}