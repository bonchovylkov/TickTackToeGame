using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using TickTackToeGame.Models;


namespace TickTackToeGame.Controllers
{
    public class TickTackToeController : ApiController
    {
        private const int EMPTY_CELL_VALUE = 0;
        private const int COMPUTER_CELL_VALUE = 1;
        private const int PLAYER_CELL_VALUE = 2;
        private Player winnerPlayer = Player.None;

        /// <summary>
        /// This service Assumes that the computer is on a move
        /// </summary>
        /// <param name="field"> </param>
        /// <returns></returns>
        [HttpPost]
        // [HttpGet]
        [Route("api/ticktacktoe/move")]

        // public GameOptions MakeMove(int[,] field)//
        public GameOptions MakeMove(string mastrixAsString)//
        {
            //int[,] field = new int[3,3]
            //{
            //    {2,2,2},
            //    {0,0,0},
            //    {0,0,0},
            //};
            //  1,2,0,
            //  1,2,1,
            //  1,2,0

            int[,] field = new int[3, 3];
            mastrixAsString = mastrixAsString.TrimEnd(',');
            int[] arr = mastrixAsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToArray();
            int row = 0;
            int col = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                col = i % 3;
                if (i != 0 && col == 0)
                {
                    row++;
                }
                field[row, col] = arr[i];
            }
            GameOptions options = new GameOptions()
            {

                IsEndGame = false,
            };

            if (CheckForEndGame(field))
            {
                options.Field = field;
                options.WinnerNumber = (int)winnerPlayer;
                options.IsEndGame = true;
                return options;
            }

            bool haveMadeMove = false;

            #region check about to end e.g two player flags in a row or diagonal


            //check by rows
            for (int i = 0; i < field.GetLength(0); i++)
            {
                var isAboutToEnd = CheckTriadForAboutEnd(GetRow(field, i),
                    new List<Coordinate>() 
                    {
                        
                        new  Coordinate(i,0),
                        new  Coordinate(i,1),
                        new  Coordinate(i,2),
                    }
                    , Player.Player);

                if (isAboutToEnd != null)
                {
                    //those are the cordinates of the empty cell that needs to filled
                    field[isAboutToEnd.Row, isAboutToEnd.Col] = COMPUTER_CELL_VALUE;
                    haveMadeMove = true;

                    //check for end after move
                    if (CheckForEndGame(field))
                    {
                        options.Field = field;
                        options.WinnerNumber = (int)winnerPlayer;
                        options.IsEndGame = true;
                        return options;
                    }
                }
            }

            //if have move don't need to check
            if (haveMadeMove == false)
            {
                //check by col
                for (int i = 0; i < field.GetLength(1); i++)
                {
                    var isAboutToEnd = CheckTriadForAboutEnd(GetColumn(field, i),
                        new List<Coordinate>() 
                    {
                        new  Coordinate(0,i),
                        new  Coordinate(1,i),
                        new  Coordinate(2,i),
                    }
                        , Player.Player);

                    if (isAboutToEnd != null)
                    {
                        //those are the cordinates of the empty cell that needs to filled
                        field[isAboutToEnd.Row, isAboutToEnd.Col] = COMPUTER_CELL_VALUE;
                        haveMadeMove = true;

                        //check for end after move
                        if (CheckForEndGame(field))
                        {
                            options.Field = field;
                            options.WinnerNumber = (int)winnerPlayer;
                            options.IsEndGame = true;
                            return options;
                        }
                    }
                }

            }

            if (haveMadeMove == false)
            {
                //first diagonal
                var isAboutToEnd = CheckTriadForAboutEnd(new int[] { field[0, 0], field[1, 1], field[2, 2] },
                       new List<Coordinate>() 
                    {
                        new  Coordinate(0,0),
                        new  Coordinate(1,1),
                        new  Coordinate(2,2),
                    }
                       , Player.Player);

                if (isAboutToEnd != null)
                {
                    //those are the cordinates of the empty cell that needs to filled
                    field[isAboutToEnd.Row, isAboutToEnd.Col] = COMPUTER_CELL_VALUE;
                    haveMadeMove = true;

                    //check for end after move
                    if (CheckForEndGame(field))
                    {
                        options.Field = field;
                        options.WinnerNumber = (int)winnerPlayer;
                        options.IsEndGame = true;
                        return options;
                    }
                }
            }

            if (haveMadeMove == false)
            {
                //first diagonal
                var isAboutToEnd = CheckTriadForAboutEnd(new int[] { field[0, 2], field[1, 1], field[2, 0] },
                       new List<Coordinate>() 
                    {
                        new  Coordinate(0,2),
                        new  Coordinate(1,1),
                        new  Coordinate(2,0),
                    }
                       , Player.Player);

                if (isAboutToEnd != null)
                {
                    //those are the cordinates of the empty cell that needs to filled
                    field[isAboutToEnd.Row, isAboutToEnd.Col] = COMPUTER_CELL_VALUE;
                    haveMadeMove = true;

                    //check for end after move
                    if (CheckForEndGame(field))
                    {
                        options.Field = field;
                        options.WinnerNumber = (int)winnerPlayer;
                        options.IsEndGame = true;
                        return options;
                    }
                }
            }
            #endregion

            if (!haveMadeMove)
            {
                //put in the first that see if there isn't danger
                //might be done smarter

                for (int i = 0; i < field.GetLength(0); i++)
                {
                    for (int j = 0; j < field.GetLength(1); j++)
                    {
                        if (field[i, j] == EMPTY_CELL_VALUE)
                        {
                            field[i, j] = COMPUTER_CELL_VALUE;
                            haveMadeMove = true;
                            break;
                        }
                    }

                    if (haveMadeMove)
                    {
                        break;
                    }
                }
            }

            options.Field = field;
            return options;
        }

        #region Check for about to end
        /// <summary>
        /// Currently only checking for Human player if is about the end
        /// </summary>
        /// <param name="row"></param>
        /// <param name="cordinatesOfRow"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        private Coordinate CheckTriadForAboutEnd(int[] row, List<Coordinate> cordinatesOfRow, Player player)
        {
            //the cordinates represents the 0 place in the string
            List<KeyValuePair<string, Coordinate>> posibleEnds = new List<KeyValuePair<string, Coordinate>>()
           {
                new KeyValuePair<string,Coordinate>("022",cordinatesOfRow.FirstOrDefault()),
               new KeyValuePair<string,Coordinate>("202",cordinatesOfRow.Skip(1).Take(1).FirstOrDefault()),
                    new KeyValuePair<string,Coordinate>("220",cordinatesOfRow.LastOrDefault()),
              
           };

            Coordinate result = null;

            string rowString = string.Join("", row);

            var isThereAboutToEnd = posibleEnds.Any(s => s.Key == rowString);
            if (isThereAboutToEnd)
            {
                result = posibleEnds.FirstOrDefault(s => s.Key == rowString).Value;

            }

            return result;
        }

        //110 ||
        //000 
        //110

        #endregion

        #region check end game


        private bool CheckForEndGame(int[,] field)
        {


            if (CheckEndForPlayer(Player.Computer, field))
            {
                winnerPlayer = Player.Computer;
                return true;
            }
            else if (CheckEndForPlayer(Player.Player, field))
            {
                winnerPlayer = Player.Player;
                return true;
            }

            bool isDrow = true;
            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    if (field[i, j] == (int)Player.None)
                    {
                        isDrow = false;
                    }
                }
            }
            if (isDrow)
            {
                winnerPlayer = Player.None;
                return true;
            }

            return false;

        }

        private bool CheckEndForPlayer(Player player, int[,] field)
        {
            bool end = false;

            //check all rows
            for (int i = 0; i < field.GetLength(0); i++)
            {
                end = CheckTriadForEnd(GetRow(field, i), player);
                if (end)
                {
                    return end;
                }
            }

            //check by columns
            for (int i = 0; i < field.GetLength(1); i++)
            {
                end = CheckTriadForEnd(GetColumn(field, i), player);
                if (end)
                {
                    return end;
                }
            }

            //first diagonal
            end = CheckTriadForEnd(new int[] { field[0, 0], field[1, 1], field[2, 2] }, player);
            if (end)
            {
                return end;
            }

            //second diagonal
            end = CheckTriadForEnd(new int[] { field[0, 2], field[1, 1], field[2, 0] }, player);


            return end;
        }

        private bool CheckTriadForEnd(int[] row, Player player)
        {
            //first contion means that all elements in the array are equal and the second means that they are equal to current player number
            bool end = row.Distinct().Count() == 1 && row[0] == (int)player;
            return end;
        }

        public static T[] GetRow<T>(T[,] matrix, int row)
        {
            var columns = matrix.GetLength(1);
            var array = new T[columns];
            for (int i = 0; i < columns; ++i)
                array[i] = matrix[row, i];
            return array;
        }

        public static T[] GetColumn<T>(T[,] matrix, int column)
        {
            var row = matrix.GetLength(0);
            var array = new T[row];
            for (int i = 0; i < row; ++i)
                array[i] = matrix[i, column];
            return array;
        }
        #endregion


    }
}