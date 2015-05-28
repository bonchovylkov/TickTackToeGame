using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TickTackToeGame.Models
{
    public class Coordinate
    {
        public int Row { get; set; }
        public int Col { get; set; }

        public Coordinate(int row,int col)
        {
            Row = row;
            Col = col;
        }
    }
}