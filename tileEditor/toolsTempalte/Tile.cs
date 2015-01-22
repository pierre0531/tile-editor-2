using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace toolsTempalte
{
    public struct BucketCollection
    {
        List<Tile> m_listTile;
        int mouseX;

        public int MouseX
        {
            get { return mouseX; }
            set { mouseX = value; }
        }
        int mouseY;

        public int MouseY
        {
            get { return mouseY; }
            set { mouseY = value; }
        }

        bool showBluePath;

        public bool ShowBluePath
        {
            get { return showBluePath; }
            set { showBluePath = value; }
        }
    }
    public struct Tile
    {
        bool previewOnMap;

        public bool PreviewOnMap
        {
            get { return previewOnMap; }
            set { previewOnMap = value; }
        }

        bool checkForBucket;

        public bool CheckForBucket
        {
            get { return checkForBucket; }
            set { checkForBucket = value; }
        }
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
            checkForBucket = false;
            previewOnMap = false;
        }
       
    }
}
