using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace toolsTempalte
{
    public struct tileMap
    {
        string pathName;

        public string PathName
        {
            get { return pathName; }
            set { pathName = value; }
        }
        int textureID;

        public int TextureID
        {
            get { return textureID; }
            set { textureID = value; }
        }

    
        
    }
    public struct BucketCollection
    {
     //   List<Tile> m_listTile;
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
    public struct Event_Collision_Object_Rect
    {
        Size size;

        public Size Size
        {
            get { return m_Rect.Size; }
            set { m_Rect.Size = value; }
        }

        Rectangle m_Rect;

        public Rectangle Rect
        {
            get { return m_Rect; }
            set { m_Rect = value; }
        }
        string name ;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public override string ToString()
        {
            return name + "{Top= " + m_Rect.Top + ", Left=" + m_Rect.Left + ", Right=" + m_Rect.Right   + ", Botton=" + m_Rect.Bottom +'}';
        }
    }

    
    public struct ObjectPt
    {
        Rectangle m_Rect;

        public Rectangle Rect
        {
            get { return m_Rect; }
            set { m_Rect = value; }
        }

        
        Point m_Pt;

        public Point Pt
        {
            get { return m_Rect.Location; }
            set { m_Rect.Location = value; }
        }
        string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public override string ToString()
        {
            return  "{name= "+name + "X= " + Pt.X + ", Y=" + Pt.Y + '}';
        }
    }
    public struct Tile
    {
        int tabIndex;

        public int TabIndex
        {
            get { return tabIndex; }
            set { tabIndex = value; }
        }

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
        //public Tile(int _x, int _y)
        //{
        //    this.x = _x;
        //    this.y = _y;
        //    checkForBucket = false;
        //    previewOnMap = false;
        //}
       
    }
}
