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
        public Event_Collision_Object_Rect(int _left, int _top, Size _size, string _name)
        {
            m_Rect = new Rectangle(_left, _top, _size.Width, _size.Height);
            name = _name;
            size = _size;
        }
       

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
