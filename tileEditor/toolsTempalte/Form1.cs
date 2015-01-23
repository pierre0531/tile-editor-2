using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//using System.Collections.Generic;

namespace toolsTempalte
{
    public partial class Form1 : Form
    {
        SGP.CSGP_Direct3D D3D = SGP.CSGP_Direct3D.GetInstance();
        SGP.CSGP_TextureManager TM = SGP.CSGP_TextureManager.GetInstance();

        int TextureID = -1;
        bool looping = true;
        public bool Looping
        {
            get { return looping; }
            set { looping = value; }
        }

        enum paintMode {full,stamp};

        paintMode m_mode;

        int mapX ;
        int mapY;

        int tileSetX ;
        int tileSetY;

        int tileWidth;
        int tileHeigth;

        //set-up the initial 
        //The current selected tile
        Tile selectedTile;
        Tile stampSelectedTile;
        //The mouse move
        Tile hoverTile;
        Tile[,] hoverTileCollection;
        BucketCollection m_bucketColletion = new BucketCollection();

        //map data
       // Size mapSizeSet ;
                        
        //tile grid     
     //   Size tileSizeSet;
                        
        //tile data     
     //   Size tileSize  ;

        //an 5x5 tile array 
        Tile[,] map ;
        Tile[,] mapFullTile;
        Tile[,] tempTile;
        //for stamp effect
        bool mouseAtTileSet = false;
        int counter = 0;
        resizeOptionsWindows tool = null;
        public Form1()
        {
            InitializeComponent();
            initializeNumber();
            m_bucketColletion.MouseX = -1;
            m_bucketColletion.MouseY = -1;

            initMap(ref map, mapX, mapY);

            initMap(ref mapFullTile, mapX, mapY);

            D3D.Initialize(panel1, true);
            D3D.AddRenderTarget(panel2);
            D3D.AddRenderTarget(panel3);
            TM.Initialize(D3D.Device, D3D.Sprite);

            m_mode = paintMode.stamp;
            buttonFull.Checked = false;
            ButtonStamp.Checked = true;

            TextureID = TM.LoadTexture("testmap3.bmp");
            panel1.AutoScrollMinSize = new Size(mapX * tileWidth, mapY * tileHeigth);
            panel2.AutoScrollMinSize = new Size(TM.GetTextureWidth(TextureID), TM.GetTextureHeight(TextureID));
        }

        private void initializeNumber()
        {
             mapX = 50;
             mapY = 50;

             tileSetX = 5;
             tileSetY = 5;

             tileWidth = 32;
             tileHeigth = 32;

            //map data
            // mapSizeSet = new Size(5, 5);

            ////tile grid
            // tileSizeSet = new Size(5, 5);

            ////tile data
            // tileSize = new Size(32, 32);
        }
        private void initMap(ref Tile[,] _map, int _mapX, int _mapY)
        {
            _map = new Tile[_mapX, _mapY];
            for (int x = 0; x < _mapX; x++)
            {
                for (int y = 0; y < _mapY; y++)
                {
                    _map[x, y].X = -1;
                    _map[x, y].Y = -1;
                    _map[x, y].CheckForBucket = false;
                    _map[x, y].PreviewOnMap = false;
                }
            }
        }

        public new void Update()
        { }
        public void Render()
        {

            Render1();
            Render2();
            Render3();
        }
        public void Render1()
        {
            D3D.Present();
            D3D.Clear(panel1, Color.White);
            D3D.DeviceBegin();
            D3D.SpriteBegin();

            //   D3D.DrawText("Hello world!",20,20,Color.Beige);

            //Draw the hollow Rect 
            Point offset = panel1.AutoScrollPosition;
            for (int x = 0; x < mapX; x++)
            {
                for (int y = 0; y < mapY; y++)
                {
                    D3D.DrawHollowRect(new Rectangle(x * tileWidth + offset.X, y * tileHeigth + offset.Y, tileWidth, tileHeigth),
                        Color.FromArgb(255, 0, 0, 0), 1);
                }
            }

            //for render the map section
            Rectangle src = new Rectangle();        
            for (int x = 0; x < mapX; x++)
            {
                for (int y = 0; y < mapY; y++)
                {

                    if (map[x, y].X == -1 && map[x, y].Y == -1)
                        continue;

                    src.X = map[x, y].X * tileWidth;
                    src.Y = map[x, y].Y * tileHeigth;
                    src.Size = new Size(tileWidth, tileHeigth);
                    int locationX = x * tileWidth + offset.X;
                    int locationY = y * tileWidth + offset.Y;
                    TM.Draw(TextureID, x * tileWidth + offset.X, y * tileHeigth + offset.Y, 1, 1, src);
                 
                }
            }

            //render the preview and mouse
            switch (m_mode)
            {

                case paintMode.full:
                    renderPreviewFull();
                    break;
                case paintMode.stamp:
                    renderPreviewStamp();
                    break;
                default:
                    break;
            }
        

            D3D.SpriteEnd();
            D3D.DeviceEnd();
            D3D.Present();
        }

        private void renderPreviewFull()
        {
            Point offset = panel1.AutoScrollPosition;

            //for render the map section
            Rectangle src = new Rectangle();
            for (int x = 0; x < mapX; x++)
            {
                for (int y = 0; y < mapY; y++)
                {

                    if (mapFullTile[x, y].X == -1 && mapFullTile[x, y].Y == -1)
                        continue;

                    //only show when this value is true
                    if (mapFullTile[x, y].PreviewOnMap != true)
                        continue;

                    src.X = mapFullTile[x, y].X * tileWidth;
                    src.Y = mapFullTile[x, y].Y * tileHeigth;
                  
                    src.Size = new Size(tileWidth, tileHeigth);
                  
                    int locationX = x * tileWidth + offset.X;
                    int locationY = y * tileWidth + offset.Y;


                    TM.Draw(TextureID, locationX, locationY, 1, 1, src);

                    if(  m_bucketColletion.ShowBluePath == true)
                    D3D.DrawRect(new Rectangle(locationX, locationY,
                             tileWidth, tileHeigth), Color.FromArgb(128, 0, 0, 255));
           

                }
            }
        }
        private void renderPreviewStamp()
        {
            Point offset = panel1.AutoScrollPosition;
            Rectangle src = new Rectangle();        
            //render the mouse location
            for (int tempX = 0; tempX <= stampSelectedTile.X - selectedTile.X; tempX++)
            {
                for (int tempY = 0; tempY <= stampSelectedTile.Y - selectedTile.Y; tempY++)
                {
                    src.X = (selectedTile.X + tempX) * tileWidth;
                    src.Y = (selectedTile.Y + tempY) * tileHeigth;
                    src.Size = new Size(tileWidth, tileHeigth);
                    int locationX = hoverTile.X * tileWidth + tempX * tileWidth+ offset.X;
                    int locationY = hoverTile.Y * tileHeigth + tempY * tileHeigth+ offset.Y;
                    TM.Draw(TextureID, locationX, locationY, 1, 1, src);
                }
            }
        }
        public void Render2()
        {
            D3D.Clear(panel2, Color.WhiteSmoke);

            D3D.DeviceBegin();
            D3D.SpriteBegin();


            TM.Draw(TextureID, panel2.AutoScrollPosition.X, panel2.AutoScrollPosition.Y);

            //draw grid
            Point offset = panel2.AutoScrollPosition;
            for (int x = 0; x < tileSetX; x++)
            {
                for (int y = 0; y < tileSetY; y++)
                {
                    D3D.DrawHollowRect(new Rectangle(x * tileWidth + offset.X, y * tileHeigth + offset.Y, tileWidth, tileHeigth), Color.FromArgb(255, 0, 0, 0), 1);
                }
            }

            //draw green selection area
            for (int x = minor(selectedTile.X, stampSelectedTile.X); x <= major(selectedTile.X, stampSelectedTile.X); x++)
            {
                for (int y =  minor(selectedTile.Y, stampSelectedTile.Y); y <= major(selectedTile.Y, stampSelectedTile.Y); y++)
                {
                    D3D.DrawRect(new Rectangle(x * tileWidth + offset.X, y * tileHeigth + offset.Y,
                        tileWidth, tileHeigth), Color.FromArgb(128, 0, 255, 0));
                    //  D3D.DrawHollowRect(new Rectangle(x * tileWidth + offset.X, y * tileHeigth + offset.Y, tileWidth, tileHeigth), Color.FromArgb(255, 255, 0, 0), 3);

                }
            }
                       
            D3D.SpriteEnd();
            D3D.DeviceEnd();
            D3D.Present(); 
        }

        private int minor(int x, int y)
        {
            if (x <= y)
                return x;
            else
                return y;
        }

        private int major(int x, int y)
        {
            if (x > y)
                return x;
            else
                return y;
        }
        void Render3()
        {
            D3D.Clear(panel3, Color.WhiteSmoke);
            D3D.DeviceBegin();
            D3D.SpriteBegin();

            Rectangle src = new Rectangle();

            //for render the map section
            for (int x = 0; x < mapX; x++)
            {
                for (int y = 0; y < mapY; y++)
                {

                    if (map[x, y].X == -1 && map[x, y].Y == -1)
                        continue;

                    src.X = map[x, y].X * tileWidth;
                    src.Y = map[x, y].Y * tileHeigth;
                    src.Size = new Size(tileWidth, tileHeigth);
                  
                    //calculate scale on panel3
                    int scaleX = mapX * tileWidth /panel3.Size.Width;                    
                    int scaleY = mapY * tileHeigth/ panel3.Size.Height;

                    //safe check
                    if (scaleX <= 0)
                        scaleX = 1;
                    else
                        scaleX += 1;
                   
                    if (scaleY <= 0)
                        scaleY = 1;
                    else
                        scaleY += 1;

                    if (scaleX >= scaleY)
                        scaleY = scaleX;
                    else
                        scaleX = scaleY;
                    
                    TM.Draw(TextureID, x * tileWidth / scaleX, y * tileHeigth / scaleY,
                        1 / (float)scaleX, 1 / (float)scaleY, src);

                }
            }

            D3D.SpriteEnd();
            D3D.DeviceEnd();
            D3D.Present(); 
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Looping = false;
        }

     
        private void panel2_Scroll(object sender, ScrollEventArgs e)
        {
            Render2();
        }

        private void addTileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Scroll(object sender, ScrollEventArgs e)
        {
            Render1();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        //safe check whether map out of range
        bool outOfRangeTile(MouseEventArgs e)
        {

            if (e.Location.X >= tileWidth * tileSetX  || e.Location.X <= 0 ||
                e.Location.Y >= tileHeigth * tileSetY || e.Location.Y <= 0)
                return true;

            return false;
        }

       

        //safe check whether map out of range
        bool outOfRangeMap(MouseEventArgs e)
        {

            if (e.Location.X >= mapX * tileWidth || e.Location.X <= 0 ||
                e.Location.Y >= mapY * tileHeigth || e.Location.Y <= 0)
                return true;

            return false;
        }

        //Single click to know where is the map mouse location
        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (outOfRangeMap(e))
                return;

            //Caculate where is the mouse
            int x = (e.Location.X - panel1.AutoScrollPosition.X) / tileWidth; //0~5(default)
            int y = (e.Location.Y - panel1.AutoScrollPosition.Y) / tileHeigth;//0~5(default)


            if (m_mode == paintMode.stamp)
                storageTile(x, y);
            else if (m_mode == paintMode.full)
                storageFullTile();
            else
            {
            }
        }

       // private
    
        private void storageFullTile()
        {
            copyMapInfo(ref mapFullTile,ref map,false);
            m_bucketColletion.ShowBluePath = false;
            //map = mapFullTile;
            //for (int x = 0; x < mapX; x++)
            //{
            //    for (int y = 0; y < mapY; y++)
            //    {
            //        map = mapFullTile;
            //        map[x,y].X = mapFullTile[x,y].X;
            //        map[x,y].Y = mapFullTile[x,y].Y;
            //    }
            //}
               
        }

        private void copyMapInfo(ref Tile[,] _src, ref Tile[,] _target,bool totalCopy)
        {
            for (int x = 0; x < mapX; x++)
            {
                for (int y = 0; y < mapY; y++)
                {
                    //total copy
                    if (totalCopy == true)
                    {
                        _target[x, y].X = _src[x, y].X;
                        _target[x, y].Y = _src[x, y].Y;
                    }
                        //not total copy for preview
                    else
                    {
                        if (_src[x, y].PreviewOnMap == true)
                        {
                            _target[x, y].X = _src[x, y].X;
                            _target[x, y].Y = _src[x, y].Y;
                        }
                    }

                  
                }
            }
        }
        public void fullMap(int clickX, int clickY)
        {
            //know what is the mouse going to check
            //possible from -1 ~ 5
            
            //init map
            initMap(ref mapFullTile, mapX, mapY);

            //blue path to true
            m_bucketColletion.ShowBluePath = true;

            //copy map to mapFullTile
            copyMapInfo(ref map, ref  mapFullTile,true);

            int checkTileX = mapFullTile[clickX, clickY].X;
            int checkTileY = mapFullTile[clickX, clickY].Y;

            //recursive from the mouse location
            recursiveTile(clickX, clickY, checkTileX, checkTileY);         
        }


        private void limitRecursiveXY(ref int _x,ref int _y)
        {
            //safe check
            if (_x >= mapX)
                _x = mapX -1;
            else if (_x < 0)
                _x = 0;

            if (_y >= mapY)
                _y = mapY - 1;
            else if (_y < 0)
                _y = 0;
        }
        private void recursiveTile(int _startX, int _startY, int _checkTileX, int _checkTileY)
        {
            int tempX,tempY ;

            //center
            tempX = _startX;
            tempY = _startY;
  

            recursiveCheck(tempX, tempY, _checkTileX, _checkTileY);

            //top
            tempX = _startX;
            tempY = _startY - 1;

             recursiveCheck(tempX, tempY, _checkTileX, _checkTileY);
    

            //down
            tempX = _startX;
            tempY = _startY + 1;

            recursiveCheck(tempX, tempY, _checkTileX, _checkTileY);
       

            //left
            tempX = _startX - 1;
            tempY = _startY ;

            recursiveCheck(tempX, tempY, _checkTileX, _checkTileY);
       
            //right
            tempX = _startX + 1;
            tempY = _startY ;

            recursiveCheck(tempX, tempY, _checkTileX, _checkTileY);
   
        }

        private void recursiveCheck(int tempX, int tempY, int _checkTileX, int _checkTileY)
        {
            counter++;

            limitRecursiveXY(ref tempX, ref tempY);

            //only chech while the tile is not check
            if (mapFullTile[tempX, tempY].CheckForBucket == true)
                return;

            //error check for not out of range
            if (tempX >= mapX || tempY >= mapY || tempX <0 || tempY< 0)
                return;

            //if the same, storage selected tile to the map
            if (mapFullTile[tempX, tempY].X == _checkTileX && mapFullTile[tempX, tempY].Y == _checkTileY)
            {
             
                //storage to the mapFullTile for preview usage 
                mapFullTile[tempX, tempY].X = selectedTile.X;
                mapFullTile[tempX, tempY].Y = selectedTile.Y;
                mapFullTile[tempX, tempY].CheckForBucket = true;
                mapFullTile[tempX, tempY].PreviewOnMap = true;

                //go to next cursive point
                recursiveTile(tempX, tempY, _checkTileX, _checkTileY);
            }
        }

        private void stampMouseMove(int _x, int _y, MouseEventArgs e)
        {
            hoverTile.X = _x;
            hoverTile.Y = _y;


            //left click for continuous draw
            if (e.Button == MouseButtons.Left)
            {
                //safe check
                if (outOfRangeMap(e))
                    return;

                //safe check
                if (_x > mapX )
                    _x = mapX  - 1;
                else if (_x < 0)
                    _x = 0;

                if (_y > mapY)
                    _y = mapY - 1;
                else if (_y < 0)
                    _y = 0;

                storageTile(_x, _y);

            }
        }

        private void fullMouseMove(int _x, int _y, MouseEventArgs e)
        {
            //safe check
            if (outOfRangeMap(e))
                return;

            //safe check
            if (_x > mapX )
                _x = mapX  - 1;
            else if (_x < 0)
                _x = 0;

            if (_y > mapY)
                _y = mapY - 1;
            else if (_y < 0)
                _y = 0;

            if (m_bucketColletion.MouseX == _x && m_bucketColletion.MouseY == _y)
                return;

            m_bucketColletion.MouseX = _x;
            m_bucketColletion.MouseY = _y;


            fullMap(_x, _y);

          
        }
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            //Caculate where is the mouse
            int x = (e.Location.X - panel1.AutoScrollPosition.X) / tileWidth; //0~5(default)
            int y = (e.Location.Y - panel1.AutoScrollPosition.Y) / tileHeigth;//0~5(default)

            switch (m_mode)
            {
                case paintMode.full:
                    fullMouseMove(x,y,e);
                    break;
                case paintMode.stamp:
                    stampMouseMove(x,y,e);
                    break;
                default:
                    break;
            }
          
        }


      

     

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            //if mouse click and move, need to do something here
            if (mouseAtTileSet == true)
            {

                //get the mouse tile location
                int x = (e.Location.X - panel2.AutoScrollPosition.X) / tileWidth;//0~5
                int y = (e.Location.Y - panel2.AutoScrollPosition.Y) / tileHeigth;//0~1

                //safe check
                if (x > mapX )
                    x = mapX  - 1;
                else if (x < 0)
                    x = 0;

                if (y > mapY)
                    y = mapY - 1;
                else if (y < 0)
                    y = 0;

                //mouse up tile is the same as selectedTile, so no need to stamp
                if (x == selectedTile.X && y == selectedTile.Y)
                    return;

                //send the x and y postion to stampSelectedTile
                stampSelectedTile.X = x;
                stampSelectedTile.Y = y;

            }
        }
       
        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            //no need to do anything if it's not available action
            if (mouseAtTileSet != true)
                return;
          
            mouseAtTileSet = false;

            //calculate which selection area
            calculateStamp();

         
        }

        private void storageTile(int _x, int _y)
        {
            //send the swap number back
            int startX = selectedTile.X;
            int startY = selectedTile.Y;

            int endX = stampSelectedTile.X;
            int endY = stampSelectedTile.Y;

            int relativeXLength = endX - startX;
            int relativeYLength = endY - startY;

          
            for (int startPtX = startX, relativeX = 0; startPtX <= endX; startPtX++, relativeX++)
            {
                for (int startPtY = startY, relativeY = 0; startPtY <= endY; startPtY++, relativeY++)
                {
                    int mapLocationX = _x + relativeX;
                    int mapLocationY = _y + relativeY;

                    //out of range here
                    if (mapLocationX >= mapX || mapLocationY >= mapY)
                        continue;

                    //store the relative tile to the map
                    map[mapLocationX, mapLocationY].X = selectedTile.X + relativeX;
                    map[mapLocationX, mapLocationY].Y = selectedTile.Y + relativeY;
                }
            }
        }
        private void calculateStamp()
        {
          //know the selectedTile info
           int x1=  selectedTile.X ;
           int y1 = selectedTile.Y;

           //send the x and y postion to stampSelectedTile
           int x2 = stampSelectedTile.X;
           int y2 = stampSelectedTile.Y;

           if (x2 < x1)
           {
               int temp = x1;
               x1 = x2;
               x2 = temp;
           }

           if (y2 < y1)
           {
               int temp = y1;
               y1 = y2;
               y2 = temp;
           }

            //send the swap number back
            selectedTile.X = x1;
            selectedTile.Y = y1;

            stampSelectedTile.X = x2;
            stampSelectedTile.Y = y2;

            //make hover
            hoverTileCollection = new Tile[x2 - x1, y2 - y1];

        }


        //select tile first
        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (outOfRangeTile(e))
                return;

            //get the mouse location at selectedTile
            selectedTile.X = (e.Location.X - panel2.AutoScrollPosition.X) / tileWidth;//0~5
            selectedTile.Y = (e.Location.Y - panel2.AutoScrollPosition.Y) / tileHeigth;//0~1

            //send the mouse info to stampSelectedTile
            stampSelectedTile.X = selectedTile.X;
            stampSelectedTile.Y = selectedTile.Y;

            mouseAtTileSet = true;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            m_mode = paintMode.full;
            buttonFull.Checked = true;
            ButtonStamp.Checked = false;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            m_mode = paintMode.stamp;
            buttonFull.Checked = false;
            ButtonStamp.Checked = true;
        }



        //////////////////////////////////////tool windows////////////////////////////////////
        void tool_FormClosed(object sender, FormClosedEventArgs e)
        {
            tool = null;
        }

        void tool_buttonAdjust(object sender, EventArgs e)
        {
            resizeOptionsWindows adjustToolsWindows = (resizeOptionsWindows)sender;

            //map size reset
            int oldX = mapX ;
            int oldY = mapY;

            int newX = adjustToolsWindows.ToolsMapX;
            int newY = adjustToolsWindows.ToolsMapY;
            resetMapSize(oldX, oldY, newX, newY);
            mapX  = newX;
            mapY = newY;


            //tile size reset
            tileSetX  = adjustToolsWindows.ToolsTileX;
            tileSetY = adjustToolsWindows.ToolsTileY;

            //tile set size reset
            tileWidth = adjustToolsWindows.ToolsTileSetW;
            tileHeigth = adjustToolsWindows.ToolsTileSetH;

            //selected Tile safe check
            if (selectedTile.X > tileWidth)
                selectedTile.X = tileWidth - 1;
            if (selectedTile.Y > tileHeigth)
                selectedTile.Y = tileHeigth - 1;

            setTheScroll();
            

        }
        void setTheScroll()
        {
            //set the scroll of the tile part 
            //panel1.AutoScrollMinSize = tilePic.Size;

            ////set the scroll of the map part
            //Size temp = new Size();
            //temp.Width = mapX  * tileSetX ;
            //temp.Height = mapY * tileSetY;
            //panel1.AutoScrollMinSize = temp;

            panel1.AutoScrollMinSize = new Size(mapX * tileWidth, mapY * tileHeigth);
        }
        void resetMapSize(int _oldX, int _oldY, int _newX, int _newY)
        {
            //init temp Tile
            initMap(ref tempTile, _newX, _newY);

            //copy the old to the temp
            for (int x = 0; x < _oldX; x++)
                for (int y = 0; y < _oldY; y++)
                {
                    //if new index > old index, then can copy
                    if ( x < _newX  && y < _newY )
                    {
                        tempTile[x, y].X = map[x, y].X;
                        tempTile[x, y].Y = map[x, y].Y;
                    }
                }

        
            initMap(ref map, _newX, _newY);
            initMap(ref mapFullTile ,_newX, _newY);
            for (int x = 0; x < _newX; x++)
                for (int y = 0; y < _newY; y++)
                {
                    //copy the temp to the map
                    map[x, y].X = tempTile[x, y].X;
                    map[x, y].Y = tempTile[x, y].Y;
                }
        }

        private void resizeMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tool == null)
            {

                tool = new resizeOptionsWindows(mapX , mapY, tileSetX , tileSetY,
                    tileWidth, tileHeigth);

                //generate the delegate of the closed
                tool.FormClosed += tool_FormClosed;

                //generate the delegate of the adjust
                tool.buttonAdjust += tool_buttonAdjust;

                //show control
                tool.Show(this);
            }
        }

        //////////////////////////////////////tool windows////////////////////////////////////

    }
}
