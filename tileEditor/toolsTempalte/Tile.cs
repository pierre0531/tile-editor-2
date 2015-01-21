using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace toolsTempalte
{
    public struct Tile
    {
        //location x
        int x;

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        //location y
        int y;

        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        //power ctor
        public Tile(int _x, int _y)
        {
            this.x = _x;
            this.y = _y;
        }
       
    }
}
