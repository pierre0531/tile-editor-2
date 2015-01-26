using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml;

namespace toolsTempalte
{
    struct tileCollection
    {
        tileMap m_tileMap;

        public tileMap TileMap
        {
            get { return m_tileMap; }
            set { m_tileMap = value; }
        }
        Panel m_panel;

        public Panel Panel
        {
            get { return m_panel; }
            set { m_panel = value; }
        }
    }
    public partial class Form1 : Form
    {
        SGP.CSGP_Direct3D D3D = SGP.CSGP_Direct3D.GetInstance();
        SGP.CSGP_TextureManager TM = SGP.CSGP_TextureManager.GetInstance();

        bool looping = true;
        public bool Looping
        {
            get { return looping; }
            set { looping = value; }
        }

        enum paintMode {full,stamp,collision,eventTrigger,Object};

        paintMode m_mode;

        int mapX ;
        int mapY;

        int tileSetX ;
        int tileSetY;

        int tileWidth;
        int tileHeigth;

        int m_screenSizeW; 
        int m_screenSizeH ;
        //set-up the initial 
        //The current selected tile
        Tile selectedTile;
        Tile stampSelectedTile;
        BucketCollection m_bucketColletion = new BucketCollection();
        //The mouse move
        Tile hoverTile;
        Tile[,] hoverTileCollection;
  
        //an 5x5 tile array 
        Tile[,] map ;
        Tile[,] mapFullTile;
        Tile[,] tempTile;

        //for stamp effect
        bool mouseAtTileSet = false;
        int counter = 0;
        resizeOptionsWindows tool = null;
        
        //for collision Rect
        bool makeNewRect = false;
        List<Event_Collision_Object_Rect> m_collisionRect = new List<Event_Collision_Object_Rect>();
        int[] m_tempRect = new int[4];
      
        //for eventTrigger
        List<Event_Collision_Object_Rect> m_eventRect = new List<Event_Collision_Object_Rect>();

        //for eventTrigger
        List<Event_Collision_Object_Rect> m_objectPt = new List<Event_Collision_Object_Rect>();

        bool[] m_showLayer = new bool[4];

        //for multi-layer
        List<tileCollection> m_tileMap = new List<tileCollection>();
        public Form1()
        {
            InitializeComponent();
            D3D.Initialize(panel1, true);
            D3D.AddRenderTarget(panel3);
            TM.Initialize(D3D.Device, D3D.Sprite);

            initializeNumber();
                        
        
        }

        private void setMode(paintMode _paintMode)
        {
            buttonFull.Checked = false;
            ButtonStamp.Checked = false;
            collisionButton.Checked = false;
            EventButton.Checked = false;
            ObjectButton.Checked = false;
  
            m_mode = _paintMode;
            switch (m_mode)
            {
                case paintMode.full:
                    buttonFull.Checked = true;
                    break;
                case paintMode.stamp:
                    ButtonStamp.Checked = true;
                    break;
                case paintMode.collision:
                    collisionButton.Checked = true;
                    break;
                case paintMode.eventTrigger:
                    EventButton.Checked = true;
                    break;
                case paintMode.Object:
                    ObjectButton.Checked = true;
                    break;
                default:
                    break;
            }
        }
        private void initializeNumber()
        {
             mapX = 50;
             mapY = 50;

             tileSetX = 5;
             tileSetY = 5;

             tileWidth = 32;
             tileHeigth = 32;

            //draw all the layer default
             MapCheckBox.Checked = true;
             EventCheckBox.Checked = true;
             ObjectCheckBox.Checked = true;
             CollisionCheckBox.Checked = true;
             checkBoxGrid.Checked = true;

             m_bucketColletion.MouseX = -1;
             m_bucketColletion.MouseY = -1;

             initMap(ref map, mapX, mapY);
             initMap(ref mapFullTile, mapX, mapY);
             setMode(paintMode.stamp);
             panel1.AutoScrollMinSize = new Size(mapX * tileWidth, mapY * tileHeigth);
     
             m_screenSizeW = 800;
             m_screenSizeH = 600;

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
                    _map[x, y].TabIndex = -1;
                }
            }
        }

        public new void Update()
        { }
        public void Render()
        {

            Render1();

            int TextureCount = m_tileMap.Count;
            //no tile map, so dont need to render 2 and render3
            if (TextureCount>0)
       //     if (TextureID != -1)
            {
                Render2();
                Render3(); 
            }
        }
        public void Render1()
        {
            D3D.Present();
            D3D.Clear(panel1, Color.White);
            D3D.DeviceBegin();
            D3D.SpriteBegin();

            //get tabIndex
            int tempTabIndex = tabAsset.SelectedIndex;

            //using tabIndex to get which tab is selected
            //int tempTextureID = m_tileMap[tempTabIndex].TileMap.TextureID;

            //Draw the hollow Rect 
            Point offset = panel1.AutoScrollPosition;
            if(checkBoxGrid.Checked)
            for (int x = 0; x < mapX; x++)
            {
                for (int y = 0; y < mapY; y++)
                {
                    D3D.DrawHollowRect(new Rectangle(x * tileWidth + offset.X, y * tileHeigth + offset.Y, tileWidth, tileHeigth),
                        Color.FromArgb(255, 0, 0, 0), 1);
                }
            }

            //safe check
            if(MapCheckBox.Checked == true)
                if (tempTabIndex != -1)
            {
                //for render the map section
                Rectangle src = new Rectangle();
                for (int x = 0; x < mapX; x++)
                {
                    for (int y = 0; y < mapY; y++)
                    {

                        if (map[x, y].TabIndex == -1)
                            continue;

                        src.X = map[x, y].X * tileWidth;
                        src.Y = map[x, y].Y * tileHeigth;
                        src.Size = new Size(tileWidth, tileHeigth);
                        int locationX = x * tileWidth + offset.X;
                        int locationY = y * tileWidth + offset.Y;
                        TM.Draw(map[x, y].TabIndex, x * tileWidth + offset.X, y * tileHeigth + offset.Y, 1, 1, src);
                      //  TM.Draw(TextureID, x * tileWidth + offset.X, y * tileHeigth + offset.Y, 1, 1, src);

                    }
                }
            }

            drawBox();
            

            //render the preview and mouse
            if (tempTabIndex != -1)
            {
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
            }

            switch (m_mode)
            {
                case paintMode.eventTrigger:
                case paintMode.collision:            
                    renderPreviewCollision();
                    break;
                default:
                    break;
            }
        

            D3D.SpriteEnd();
            D3D.DeviceEnd();
            D3D.Present();
        }

        private void drawBox()
        {
            if (EventCheckBox.Checked == true)
            drawEventBox();

            if (CollisionCheckBox.Checked == true)
            drawCollisionBox();

            if (ObjectCheckBox.Checked == true)
            drawObjectBox();
        }

       
         private void drawObjectBox()
        { 
             //draw Collision Box
            Point offset = panel1.AutoScrollPosition;
            for (int i = 0; i < m_objectPt.Count; i++)
            {
                int locationX = m_objectPt[i].Rect.Left + offset.X;
                int locationY = m_objectPt[i].Rect.Top  + offset.Y;
                D3D.DrawText(m_objectPt[i].Name, locationX, locationY, Color.FromArgb(255, 128, 128, 128));
                D3D.DrawHollowRect(new Rectangle(locationX , locationY,
                    m_objectPt[i].Rect.Width, m_objectPt[i].Rect.Height), Color.FromArgb(255, 128, 128, 128), 3);
                
                //draw highlight
                if (i == listBoxObject.SelectedIndex && ObjectButton.Checked)
                    D3D.DrawRect(new Rectangle(locationX, locationY ,
                    m_objectPt[i].Rect.Width, m_objectPt[i].Rect.Height), Color.FromArgb(128, 128, 128, 128));
            }
         }
        private void drawEventBox()
        {
            //draw Event Box
            Point offset = panel1.AutoScrollPosition;
            for (int i = 0; i < m_eventRect.Count; i++)
            {
                int locationX = m_eventRect[i].Rect.Left + offset.X;
                int locationY =  m_eventRect[i].Rect.Top + offset.Y;
                D3D.DrawText(m_eventRect[i].Name, locationX, locationY, Color.FromArgb(255, 0, 128, 0));
                D3D.DrawHollowRect(new Rectangle(locationX, locationY,
                    m_eventRect[i].Rect.Width, m_eventRect[i].Rect.Height), Color.FromArgb(255, 0, 128, 0), 3);
                
                //draw highlight
                if (i == listBoxEvent.SelectedIndex && EventButton.Checked)
                    D3D.DrawRect(new Rectangle(locationX , locationY ,
                    m_eventRect[i].Rect.Width, m_eventRect[i].Rect.Height), Color.FromArgb(128, 0, 128, 0));
            }
        }
        private void drawCollisionBox()
        {  //draw Collision Box
            Point offset = panel1.AutoScrollPosition;
            for (int i = 0; i < m_collisionRect.Count; i++)
            {
                int locationX = m_collisionRect[i].Rect.Left + offset.X;
                int locationY = m_collisionRect[i].Rect.Top + offset.Y;

                D3D.DrawHollowRect(new Rectangle(locationX, locationY,
                    m_collisionRect[i].Rect.Width, m_collisionRect[i].Rect.Height), Color.FromArgb(255, 255, 0, 0), 3);

                D3D.DrawLine(locationX, locationY,
                    m_collisionRect[i].Rect.Right + offset.X, m_collisionRect[i].Rect.Bottom + offset.Y,
                    Color.FromArgb(255, 255, 0, 0), 3);

                D3D.DrawLine(m_collisionRect[i].Rect.Right + offset.X, locationY,
                      m_collisionRect[i].Rect.Left + offset.X, m_collisionRect[i].Rect.Bottom + offset.Y,
                      Color.FromArgb(255, 255, 0, 0), 3);
                
                //draw highlight
                if (i == listBoxCollision.SelectedIndex && collisionButton.Checked)
                    D3D.DrawRect(new Rectangle(locationX, locationY,
                    m_collisionRect[i].Rect.Width, m_collisionRect[i].Rect.Height), Color.FromArgb(128, 255, 0, 0));
            } 
        }
        //preview collision Zone Rect
        private void renderPreviewCollision()
        {
            if (makeNewRect)
            {
                int x1 = minor(m_tempRect[0], m_tempRect[2]);
                int y1 = minor(m_tempRect[1], m_tempRect[3]);

                int x2 = major(m_tempRect[0], m_tempRect[2]);
                int y2 = major(m_tempRect[1], m_tempRect[3]);

                switch (m_mode)
                {
                    case paintMode.eventTrigger:
                        D3D.DrawHollowRect(new Rectangle(x1, y1, x2 - x1, y2 - y1), Color.FromArgb(255, 0, 255, 0), 3);
                        break;
                    case paintMode.collision:
                          D3D.DrawHollowRect(new Rectangle(x1,y1,x2-x1,y2-y1), Color.FromArgb(255, 128, 0, 0), 3);
                          D3D.DrawLine(x1, y1,x2,y2,  Color.FromArgb(255, 128, 0, 0), 3);
                          D3D.DrawLine(x2, y1, x1, y2, Color.FromArgb(255, 128, 0, 0), 3);
                        break;
                    default:
                        break;
                }
              

            }
       
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

                    if (mapFullTile[x, y].TabIndex == -1)
                        continue;

                    //only show when this value is true
                    if (mapFullTile[x, y].PreviewOnMap != true)
                        continue;

                    src.X = mapFullTile[x, y].X * tileWidth;
                    src.Y = mapFullTile[x, y].Y * tileHeigth;
                  
                    src.Size = new Size(tileWidth, tileHeigth);
                  
                    int locationX = x * tileWidth + offset.X;
                    int locationY = y * tileWidth + offset.Y;


                    TM.Draw( mapFullTile[x, y].TabIndex, locationX, locationY, 1, 1, src);

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

            int tempTabIndex = tabAsset.SelectedIndex;
            int temptextureID = m_tileMap[tempTabIndex].TileMap.TextureID;

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
                    TM.Draw(temptextureID, locationX, locationY, 1, 1, src);
                }
            }
        }
         

        public void Render2()
        {
            //get tabIndex
            int tempTabIndex = tabAsset.SelectedIndex;

            //using tabIndex to get which tab is selected
            int tempTextureID = m_tileMap[tempTabIndex].TileMap.TextureID;

            Panel panelTile = m_tileMap[tempTabIndex].Panel;
         //   panelTile = panel2;
         

            //
          
         
           // panelTile.Parent = tabAsset.TabPages[tempTabIndex];
            //panelTile.Dock = DockStyle.Fill;
         //   panelTile.Location = new Point(panelTile.Parent.Left, panelTile.Parent.Top);
            //panelTile.AutoScrollMinSize = new Size(TM.GetTextureWidth(tempTextureID), TM.GetTextureHeight(tempTextureID));
       //


        
           // D3D.Clear(panel2, Color.WhiteSmoke);
            D3D.Clear(panelTile, Color.WhiteSmoke);

            D3D.DeviceBegin();
            D3D.SpriteBegin();
       //     panel2 = panelTile;
            TM.Draw(tempTextureID, panelTile.AutoScrollPosition.X, panelTile.AutoScrollPosition.Y);
           // TM.Draw(tempTextureID, panel2.AutoScrollPosition.X, panel2.AutoScrollPosition.Y);
  

            //draw grid
            Point offset = panelTile.AutoScrollPosition;
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

        private void miniMapDrawBox(int _scaleX,int _scaleY)
        {
            if(MapCheckBox.Checked == true)
            miniMapDrawMap(_scaleX, _scaleY);

            if (EventCheckBox.Checked == true)
            miniMapDrawEvent(_scaleX, _scaleY);

            if (ObjectCheckBox.Checked == true)
            miniMapDrawObject(_scaleX, _scaleY);

            if (CollisionCheckBox.Checked == true)
            miniMapDrawCollision(_scaleX, _scaleY);
        }

        private void miniMapDrawMap(int scaleX, int scaleY)
        {
            Rectangle src = new Rectangle();

            //for render the map section
            for (int x = 0; x < mapX; x++)
            {
                for (int y = 0; y < mapY; y++)
                {

                    if (map[x, y].TabIndex == -1)
                        continue;

                    src.X = map[x, y].X * tileWidth;
                    src.Y = map[x, y].Y * tileHeigth;
                    src.Size = new Size(tileWidth, tileHeigth);

                    TM.Draw(map[x, y].TabIndex, x * tileWidth / scaleX, y * tileHeigth / scaleY,
                        1 / (float)scaleX, 1 / (float)scaleY, src);
                    //TM.Draw(TextureID, x * tileWidth / scaleX, y * tileHeigth / scaleY,
                    //    1 / (float)scaleX, 1 / (float)scaleY, src);
                }
            } 
        }

        //draw event on mini map
        private void miniMapDrawEvent(int scaleX, int scaleY)
        {
            //draw event Box
            for (int i = 0; i < m_eventRect.Count; i++)
            {

                D3D.DrawHollowRect(new Rectangle(m_eventRect[i].Rect.Left / scaleX, m_eventRect[i].Rect.Top / scaleY,
                    m_eventRect[i].Rect.Width / scaleX, m_eventRect[i].Rect.Height / scaleY), Color.FromArgb(255, 0, 128, 0), 3);
              
                //draw highlight
                if (i == listBoxEvent.SelectedIndex && EventButton.Checked)
                    D3D.DrawRect(new Rectangle(m_eventRect[i].Rect.Left / scaleX, m_eventRect[i].Rect.Top / scaleY,
                    m_eventRect[i].Rect.Width / scaleX, m_eventRect[i].Rect.Height / scaleY), Color.FromArgb(128, 0, 128, 0));
            }
        }
        private void miniMapDrawObject(int scaleX, int scaleY)
        {
            //draw object Box
            for (int i = 0; i < m_objectPt.Count; i++)
            {

                D3D.DrawHollowRect(new Rectangle(m_objectPt[i].Rect.Left / scaleX, m_objectPt[i].Rect.Top / scaleY,
                    m_objectPt[i].Rect.Width / scaleX, m_objectPt[i].Rect.Height / scaleY), Color.FromArgb(255, 128, 128, 128), 3);
               
                //draw highlight
                if (i == listBoxObject.SelectedIndex && ObjectButton.Checked)
                    D3D.DrawRect(new Rectangle(m_objectPt[i].Rect.Left / scaleX, m_objectPt[i].Rect.Top / scaleY,
                    m_objectPt[i].Rect.Width / scaleX, m_objectPt[i].Rect.Height / scaleY), Color.FromArgb(128, 128, 128, 128));
            }

        }

        private void miniMapDrawCollision(int scaleX, int scaleY)
        {
            //draw Collision Box
            for (int i = 0; i < m_collisionRect.Count; i++)
            {

                D3D.DrawHollowRect(new Rectangle(m_collisionRect[i].Rect.Left / scaleX, m_collisionRect[i].Rect.Top / scaleY,
                    m_collisionRect[i].Rect.Width / scaleX, m_collisionRect[i].Rect.Height / scaleY), Color.FromArgb(255, 255, 0, 0), 3);

                D3D.DrawLine(m_collisionRect[i].Rect.Left / scaleX, m_collisionRect[i].Rect.Top / scaleY,
                    m_collisionRect[i].Rect.Right / scaleX, m_collisionRect[i].Rect.Bottom / scaleY,
                    Color.FromArgb(255, 255, 0, 0), 3);

                D3D.DrawLine(m_collisionRect[i].Rect.Right / scaleX, m_collisionRect[i].Rect.Top / scaleY,
                      m_collisionRect[i].Rect.Left / scaleX, m_collisionRect[i].Rect.Bottom / scaleY,
                      Color.FromArgb(255, 255, 0, 0), 3);
                
                //draw highlight
                if (i == listBoxCollision.SelectedIndex && collisionButton.Checked)
                    D3D.DrawRect(new Rectangle(m_collisionRect[i].Rect.Left / scaleX, m_collisionRect[i].Rect.Top / scaleY,
                    m_collisionRect[i].Rect.Width / scaleX, m_collisionRect[i].Rect.Height / scaleY), Color.FromArgb(128, 255, 0, 0));
            }
        }
        void Render3()
        {
            D3D.Clear(panel3, Color.WhiteSmoke);
            D3D.DeviceBegin();
            D3D.SpriteBegin();

           
            //calculate scale on panel3
            int scaleX = mapX * tileWidth / panel3.Size.Width;
            int scaleY = mapY * tileHeigth / panel3.Size.Height;

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

            //draw mini map
            miniMapDrawBox(scaleX, scaleY);
        
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
            else if (m_mode == paintMode.Object)
            { 
                storageCollisionRect(e.Location.X, e.Location.Y);
             //   calculateRect();
            }
               
        }

     
    
        private void storageFullTile()
        {
            copyMapInfo(ref mapFullTile,ref map,false);
            m_bucketColletion.ShowBluePath = false;
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
                        _target[x, y].TabIndex = _src[x, y].TabIndex;
                    }
                        //not total copy for preview
                    else
                    {
                        if (_src[x, y].PreviewOnMap == true)
                        {
                            _target[x, y].X = _src[x, y].X;
                            _target[x, y].Y = _src[x, y].Y;
                            _target[x, y].TabIndex = _src[x, y].TabIndex;
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

            limitRecursiveXY(ref tempX, ref tempY);

            ////only chech while the tile is not check
            if (mapFullTile[tempX, tempY].CheckForBucket != true)
            ////error check for not out of range
            //if (tempX >= mapX && tempY >= mapY && tempX < 0 && tempY < 0)
            recursiveCheck(tempX, tempY, _checkTileX, _checkTileY);

            //top
            tempX = _startX;
            tempY = _startY - 1;
            limitRecursiveXY(ref tempX, ref tempY);
            ////only chech while the tile is not check
             if (mapFullTile[tempX, tempY].CheckForBucket != true)
            //    //error check for not out of range
            // if (tempX >= mapX && tempY >= mapY && tempX < 0 && tempY < 0)
             recursiveCheck(tempX, tempY, _checkTileX, _checkTileY);
    

            //down
            tempX = _startX;
            tempY = _startY + 1;
            limitRecursiveXY(ref tempX, ref tempY);
            ////only chech while the tile is not check
            if (mapFullTile[tempX, tempY].CheckForBucket != true)
            //    //error check for not out of range
            //    if (tempX >= mapX && tempY >= mapY && tempX < 0 && tempY < 0)
            recursiveCheck(tempX, tempY, _checkTileX, _checkTileY);
       

            //left
            tempX = _startX - 1;
            tempY = _startY ;
            limitRecursiveXY(ref tempX, ref tempY);
            ////only chech while the tile is not check
            if (mapFullTile[tempX, tempY].CheckForBucket != true)
            //    //error check for not out of range
            //    if (tempX >= mapX && tempY >= mapY && tempX < 0 && tempY < 0)
            recursiveCheck(tempX, tempY, _checkTileX, _checkTileY);
       
            //right
            tempX = _startX + 1;
            tempY = _startY ;
            limitRecursiveXY(ref tempX, ref tempY);
            ////only chech while the tile is not check
            if (mapFullTile[tempX, tempY].CheckForBucket != true)
            //    //error check for not out of range
            //    if (tempX >= mapX && tempY >= mapY && tempX < 0 && tempY < 0)
            recursiveCheck(tempX, tempY, _checkTileX, _checkTileY);
   
        }

        private void recursiveCheck(int tempX, int tempY, int _checkTileX, int _checkTileY)
        {
            //counter++;

            //limitRecursiveXY(ref tempX, ref tempY);

            ////only chech while the tile is not check
            //if (mapFullTile[tempX, tempY].CheckForBucket == true)
            //    return;

            ////error check for not out of range
            //if (tempX >= mapX || tempY >= mapY || tempX <0 || tempY< 0)
            //    return;

            //if the same, storage selected tile to the map
            if (mapFullTile[tempX, tempY].X == _checkTileX && mapFullTile[tempX, tempY].Y == _checkTileY)
            {
             
                //storage to the mapFullTile for preview usage 
                mapFullTile[tempX, tempY].X = selectedTile.X;
                mapFullTile[tempX, tempY].Y = selectedTile.Y;
                mapFullTile[tempX, tempY].CheckForBucket = true;
                mapFullTile[tempX, tempY].PreviewOnMap = true;
                mapFullTile[tempX, tempY].TabIndex = tabAsset.SelectedIndex;
                counter++;
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
             if (outOfRangeMap(e))
                return;

            //Caculate where is the mouse
            int x = (e.Location.X - panel1.AutoScrollPosition.X) / tileWidth; //0~5(default)
            int y = (e.Location.Y - panel1.AutoScrollPosition.Y) / tileHeigth;//0~5(default)

            switch (m_mode)
            {
                case paintMode.full:
                    fullMouseMove(x, y, e);
                    break;
                case paintMode.stamp:
                    stampMouseMove(x, y,e);
                    break;
                case paintMode.collision:
                    makeCollisionRect(e.Location.X, e.Location.Y);
                    break;
                case paintMode.eventTrigger:
                    makeCollisionRect(e.Location.X, e.Location.Y);
                    break;
                case paintMode.Object:
                    break;
                default:
                    break;
            }
         
        }


     
        private void makeCollisionRect(int _x, int _y)
        {
            m_tempRect[2] = _x;
            m_tempRect[3] = _y;

        }
     

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            //if mouse click and move, need to do something here
            if (mouseAtTileSet == true)
            {
                //get tabIndex
                int tempTabIndex = tabAsset.SelectedIndex;

                //using tabIndex to get which tab is selected
                int tempTextureID = m_tileMap[tempTabIndex].TileMap.TextureID;

                Panel panel2 = m_tileMap[tempTabIndex].Panel;
 

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

            //get tabIndex
            int tempTabIndex = tabAsset.SelectedIndex;

            if (tempTabIndex == -1)
                return;

            //send the swap number back
            int startX = selectedTile.X;
            int startY = selectedTile.Y;

            int endX = stampSelectedTile.X;
            int endY = stampSelectedTile.Y;

            int relativeXLength = endX - startX;
            int relativeYLength = endY - startY;


              //using tabIndex to get which tab is selected
            int tempTextureID = m_tileMap[tempTabIndex].TileMap.TextureID;
          
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
                    map[mapLocationX, mapLocationY].TabIndex = tempTextureID;
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

            //get tabIndex
            int tempTabIndex = tabAsset.SelectedIndex;

            //using tabIndex to get which tab is selected
            int tempTextureID = m_tileMap[tempTabIndex].TileMap.TextureID;

            Panel panelTile = m_tileMap[tempTabIndex].Panel;

            //get the mouse location at selectedTile
            selectedTile.X = (e.Location.X - panelTile.AutoScrollPosition.X) / tileWidth;//0~5
            selectedTile.Y = (e.Location.Y - panelTile.AutoScrollPosition.Y) / tileHeigth;//0~1

            //send the mouse info to stampSelectedTile
            stampSelectedTile.X = selectedTile.X;
            stampSelectedTile.Y = selectedTile.Y;

            mouseAtTileSet = true;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            setMode(paintMode.full);
     
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            setMode(paintMode.stamp);
        
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
                        tempTile[x, y].TabIndex = map[x, y].TabIndex;
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
                    map[x, y].TabIndex = tempTile[x, y].TabIndex;
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

        private void ImportAsset_Click(object sender, EventArgs e)
        {
            openFile();
           // int tempTabIndex = tabAsset.SelectedIndex;
          //  int temptextureID = m_tileMap[tempTabIndex].TileMap.TextureID;
         //   panel2.AutoScrollMinSize = new Size(TM.GetTextureWidth(temptextureID), TM.GetTextureHeight(temptextureID));
            //panel2.AutoScrollMinSize = new Size(TM.GetTextureWidth(TextureID), TM.GetTextureHeight(TextureID));

        }

        private void importTileHelper(string open)
        {
            //Open a stream for reading
            tileMap tempListMap = new tileMap();
            tempListMap.TextureID = TM.LoadTexture(open);
            tempListMap.PathName = open;

            //new tab
            TabPage tempTabPage = new TabPage();
            tempTabPage.Parent = tabAsset;
            tempTabPage.Text = m_tileMap.Count.ToString();
            //new panel
            Panel tempPanel = new Panel();
            tempPanel.AutoScrollMinSize = new Size(TM.GetTextureWidth(tempListMap.TextureID), TM.GetTextureHeight(tempListMap.TextureID));
            tempPanel.BorderStyle = BorderStyle.FixedSingle;
            tempPanel.Dock = DockStyle.Fill;
            tempPanel.BackColor = Color.Transparent;

            //setting the new tab
            tempPanel.Parent = tempTabPage;
            //   tempPanel.Location = new Point(0, 0);
            D3D.AddRenderTarget(tempPanel);

            //add action to the panel
            tempPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseMove);
            tempPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseUp);
            tempPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseDown);

            //open a tile collection
            tileCollection temptileCollection = new tileCollection();
            temptileCollection.TileMap = tempListMap;
            temptileCollection.Panel = tempPanel;
            m_tileMap.Add(temptileCollection);        
        }
        private void openFile()
        {  //Create an open file
            OpenFileDialog open = new OpenFileDialog();

            //set the filter
            open.Filter = "All Files(*.*)|*.*|Tile Files(*.bmp)|*.bmp|Tile Files(*.gif)|*.gif|Tile Files(*.png)|*.png";

            if (DialogResult.OK == open.ShowDialog())
            {
                //Open a stream for reading
                tileMap tempListMap = new tileMap();
                tempListMap.TextureID = TM.LoadTexture(open.FileName);
                tempListMap.PathName = open.SafeFileName;           
           
                //new tab
                TabPage tempTabPage = new TabPage();
                tempTabPage.Parent = tabAsset;
                tempTabPage.Text = m_tileMap.Count.ToString();
                //new panel
                 Panel tempPanel = new Panel();
                 tempPanel.AutoScrollMinSize = new Size(TM.GetTextureWidth(tempListMap.TextureID), TM.GetTextureHeight(tempListMap.TextureID));
                 tempPanel.BorderStyle = BorderStyle.FixedSingle;
                 tempPanel.Dock = DockStyle.Fill;
                 tempPanel.BackColor = Color.Transparent;

                 //setting the new tab
                 tempPanel.Parent = tempTabPage;
                 //   tempPanel.Location = new Point(0, 0);
                 D3D.AddRenderTarget(tempPanel);

                //add action to the panel
                 tempPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseMove);
                 tempPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseUp);
                 tempPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseDown);

                 //open a tile collection
                 tileCollection temptileCollection = new tileCollection();
                 temptileCollection.TileMap = tempListMap;
                 temptileCollection.Panel = tempPanel;
                 m_tileMap.Add(temptileCollection);                        
            }
        }
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void collisionButton_Click(object sender, EventArgs e)
        {
            setMode(paintMode.collision);
            tabEditLayer.SelectedTab = tabPageCollision;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (outOfRangeMap(e))
                return;

            //Caculate where is the mouse
            int x = (e.Location.X - panel1.AutoScrollPosition.X) / tileWidth; //0~5(default)
            int y = (e.Location.Y - panel1.AutoScrollPosition.Y) / tileHeigth;//0~5(default)


            if (m_mode == paintMode.collision)
                storageCollisionRect(e.Location.X, e.Location.Y);
            else if (m_mode == paintMode.eventTrigger)
                storageCollisionRect(e.Location.X, e.Location.Y);
        }

       

        private void storageCollisionRect(int _x, int _y)
        {
            //it can show right now
            makeNewRect = true;
            
            //tempRect info for [0],[1],[2],[3]
            m_tempRect[0] = _x;
            m_tempRect[1] = _y;
            m_tempRect[2] = _x;
            m_tempRect[3] = _y;

        }
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            //calculate which selection area
            calculateRect();
        }

        private void calculateRect()
        {
            //it can show right now
            makeNewRect = false;

            //know the selectedTile info
            int x1 = m_tempRect[0];
            int y1 = m_tempRect[1];

            //send the x and y postion to stampSelectedTile
            int x2 = m_tempRect[2];
            int y2 = m_tempRect[3];

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
           m_tempRect[0] = x1;
           m_tempRect[1] = y1;

           m_tempRect[2] = x2;
           m_tempRect[3] = y2;

            //adjust to the real position
           m_tempRect[0] -= panel1.AutoScrollPosition.X;
           m_tempRect[1] -= panel1.AutoScrollPosition.Y;
           m_tempRect[2] -= panel1.AutoScrollPosition.X;
           m_tempRect[3] -= panel1.AutoScrollPosition.Y;

           //make Rect
            Rectangle tempRect = new Rectangle(new Point(m_tempRect[0], m_tempRect[1]), new Size(m_tempRect[2] - m_tempRect[0],
                m_tempRect[3] - m_tempRect[1]));

           Event_Collision_Object_Rect tempEventRect = new Event_Collision_Object_Rect();
           tempEventRect.Rect = tempRect;
           tempEventRect.Name = "default";
          
           switch (m_mode)
           {         
               case paintMode.collision:
                   m_collisionRect.Add(tempEventRect);
                   listBoxCollision.Items.Add(tempEventRect);
                   break;
               case paintMode.eventTrigger:
                   m_eventRect.Add(tempEventRect);
                   listBoxEvent.Items.Add(tempEventRect);
                   break;
               case paintMode.Object:                                               
                   tempEventRect.Size = new Size(20, 20);
                   m_objectPt.Add(tempEventRect);
                   listBoxObject.Items.Add(tempEventRect);
                   
                   break;
               default:
                   break;
           }
         
        }

        private void EventButton_Click(object sender, EventArgs e)
        {
            setMode(paintMode.eventTrigger);
            tabEditLayer.SelectedTab = tabPageEvent;
        }

        private void ObjectButton_Click(object sender, EventArgs e)
        {
            setMode(paintMode.Object);
            tabEditLayer.SelectedTab = tabPageObject;
        }

       
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //box collision//
        //***************************************************************************************//
        private void listBoxCollision_MouseClick(object sender, MouseEventArgs e)
        {
            if (listBoxCollision.SelectedIndex == -1)
                return;

            Event_Collision_Object_Rect r = (Event_Collision_Object_Rect)listBoxCollision.Items[listBoxCollision.SelectedIndex];
          
            numCollision_top.Value = r.Rect.Top;
            numCollision_left.Value = r.Rect.Left;
            numCollision_right.Value = r.Rect.Right;
            numCollision_bottom.Value = r.Rect.Bottom;
        }
        private void buttonCollisionDelete_Click(object sender, EventArgs e)
        {
            if (listBoxCollision.SelectedIndex == -1)
                return;

            int tempIndex = listBoxCollision.SelectedIndex;
            listBoxCollision.Items.RemoveAt(tempIndex);
            m_collisionRect.RemoveAt(tempIndex);

        }

        private void buttonUpdateCollisioin_Click(object sender, EventArgs e)
        {
            if (listBoxCollision.SelectedIndex == -1)
                return;

            Rectangle tempRectangle = new Rectangle();
            Event_Collision_Object_Rect tempEventRetangle = new Event_Collision_Object_Rect();
            //get the number info
            int bottom = (int)numCollision_bottom.Value;
            int right = (int)numCollision_right.Value;
            int top = (int)numCollision_top.Value;
            int left = (int)numCollision_left.Value;
            int width = right - left;
            int height = bottom - top;

            //safe check
            if (width <= 0)
                width = 0;
            if (height <= 0)
                height = 0;

        
            //copy info to the rect
            tempRectangle.Location = new Point(left, top);
            tempRectangle.Width = width;
            tempRectangle.Height = height;

            //copy rect back
            tempEventRetangle.Rect = tempRectangle;

            //old one index
            int tempIndex = listBoxCollision.SelectedIndex;

            //insert a new one
            listBoxCollision.Items.Insert(tempIndex, tempEventRetangle);
            m_collisionRect.Insert(tempIndex, tempEventRetangle);

            //remove the old one
            listBoxCollision.Items.RemoveAt(tempIndex + 1);
            m_collisionRect.RemoveAt(tempIndex + 1);
            
            listBoxCollision.SelectedIndex = tempIndex;
      
        }

        //box event//
        //***************************************************************************************//


        private void buttonUpdateEvent_Click(object sender, EventArgs e)
        {
            if (listBoxEvent.SelectedIndex == -1)
                return;

            Rectangle tempRectangle = new Rectangle();
            Event_Collision_Object_Rect tempEventRetangle = new Event_Collision_Object_Rect();
            //get the number info
            int bottom = (int)numEvent_bottom.Value;
            int right = (int)numEvent_right.Value;
            int top = (int)numEvent_top.Value;
            int left = (int)numEvent_left.Value;
            int width = right - left;
            int height = bottom - top;

            //safe check
            if (width <= 0)
                width = 0;
            if (height <= 0)
                height = 0;

            //name here
            tempEventRetangle.Name = textBoxEvent.Text;

            //copy info to the rect
            tempRectangle.Location = new Point(left, top);
            tempRectangle.Width = width;
            tempRectangle.Height = height;

            //copy rect back
            tempEventRetangle.Rect = tempRectangle;

            //old one index
            int tempIndex = listBoxEvent.SelectedIndex;

            //insert a new one
            listBoxEvent.Items.Insert(tempIndex, tempEventRetangle);
            m_eventRect.Insert(tempIndex, tempEventRetangle);

            //remove the old one
            listBoxEvent.Items.RemoveAt(tempIndex + 1);
            m_eventRect.RemoveAt(tempIndex + 1);

            listBoxEvent.SelectedIndex = tempIndex;
        }

       
      //event index change
        private void listBoxEvent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxEvent.SelectedIndex == -1)
                return;

            Event_Collision_Object_Rect r = (Event_Collision_Object_Rect)listBoxEvent.Items[listBoxEvent.SelectedIndex];
            numEvent_top.Value = r.Rect.Top;
            numEvent_left.Value = r.Rect.Left;
            numEvent_right.Value = r.Rect.Right;
            numEvent_bottom.Value = r.Rect.Bottom;
            textBoxEvent.Text = r.Name;
        }

        //event delete
        private void buttonEventDelete_Click(object sender, EventArgs e)
        {
            if (listBoxEvent.SelectedIndex == -1)
                return;

            int tempIndex = listBoxEvent.SelectedIndex;
            listBoxEvent.Items.RemoveAt(tempIndex);
            m_eventRect.RemoveAt(tempIndex);
         
        }

        //object event//
        //***************************************************************************************//
        private void listBoxObject_MouseClick(object sender, MouseEventArgs e)
        {
            if (listBoxObject.SelectedIndex == -1)
                return;
            
            Event_Collision_Object_Rect r = (Event_Collision_Object_Rect)listBoxObject.Items[listBoxObject.SelectedIndex];
            numObject_left.Value = r.Rect.Top;
            numObject_top.Value = r.Rect.Left;
            textBoxObject.Text = r.Name;
        }

        private void buttonObjectDelete_Click(object sender, EventArgs e)
        {
            if (listBoxObject.SelectedIndex == -1)
                return;
            
            int tempIndex = listBoxObject.SelectedIndex;
            listBoxObject.Items.RemoveAt(tempIndex);
            m_objectPt.RemoveAt(tempIndex);

        }

        private void buttonObjectUpdate_Click(object sender, EventArgs e)
        {
            if (listBoxObject.SelectedIndex == -1)
                return;
        
            Rectangle tempRectangle = new Rectangle();
            Event_Collision_Object_Rect tempEventRetangle = new Event_Collision_Object_Rect();
            //get the number info
            int top = (int)numObject_top.Value;
            int left = (int)numObject_left.Value;
            int width =20;
            int height = 20;

            //safe check
            if (width <= 0)
                width = 0;
            if (height <= 0)
                height = 0;
            
            //name here
            tempEventRetangle.Name = textBoxObject.Text;

            //copy info to the rect
            tempRectangle.Location = new Point(left, top);
            tempRectangle.Width = width;
            tempRectangle.Height = height;

            //copy rect back
            tempEventRetangle.Rect = tempRectangle;

            //old one index
            int tempIndex = listBoxObject.SelectedIndex;

            //insert a new one
            listBoxObject.Items.Insert(tempIndex, tempEventRetangle);
            m_objectPt.Insert(tempIndex, tempEventRetangle);

            //remove the old one
            listBoxObject.Items.RemoveAt(tempIndex + 1);
            m_objectPt.RemoveAt(tempIndex + 1);
            
            listBoxObject.SelectedIndex = tempIndex;
        }

        private void buttonOjectNext_Click(object sender, EventArgs e)
        {
            buttonNext();
        }

        private void buttonNext()
        {
            List<Event_Collision_Object_Rect> tempCollection = new List<Event_Collision_Object_Rect>();
            ListBox tempListBox = new ListBox();
            switch (m_mode)
            {            
                case paintMode.collision:
                    tempListBox = listBoxCollision;
                    tempCollection = m_collisionRect;
                    break;
                case paintMode.eventTrigger:
                    tempListBox = listBoxEvent;
                    tempCollection = m_eventRect;
                    break;
                case paintMode.Object:
                    tempListBox = listBoxObject;
                    tempCollection = m_objectPt;
                    break;
                default:
                    break;
            }

            int tempIndex = tempListBox.SelectedIndex;

            if (tempIndex == -1)
            {
                tempListBox.SelectedIndex = 0;
                return;
            }

            tempIndex++;

            if (tempIndex >= tempCollection.Count)
                tempListBox.SelectedIndex = 0;
            else
                tempListBox.SelectedIndex = tempIndex;
        }
        private void buttonPre_Click(object sender, EventArgs e)
        {
            buttonPre();
        }
        private void buttonPre()
        {
            List<Event_Collision_Object_Rect> tempCollection = new List<Event_Collision_Object_Rect>();
            ListBox tempListBox = new ListBox();
            switch (m_mode)
            {
                case paintMode.collision:
                    tempListBox = listBoxCollision;
                    tempCollection = m_collisionRect;
                    break;
                case paintMode.eventTrigger:
                    tempListBox = listBoxEvent;
                    tempCollection = m_eventRect;
                    break;
                case paintMode.Object:
                    tempListBox = listBoxObject;
                    tempCollection = m_objectPt;
                    break;
                default:
                    break;
            }

            int tempIndex = tempListBox.SelectedIndex;

            if (tempIndex == -1)
            {
                tempListBox.SelectedIndex = 0;
                return; 
            }

            tempIndex--;

            if (tempIndex < 0)
                tempListBox.SelectedIndex = tempCollection.Count - 1;
            else
                tempListBox.SelectedIndex = tempIndex;
        }
        //***************************************************************************************//

        private void tabEditLayer_MouseClick(object sender, MouseEventArgs e)
        {
            if (tabEditLayer.SelectedTab == tabPageEvent)
                setMode(paintMode.eventTrigger);
            else if (tabEditLayer.SelectedTab == tabPageCollision)
                setMode(paintMode.collision);
            else if (tabEditLayer.SelectedTab == tabPageObject)
                setMode(paintMode.Object);
        }


        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            initializeNumber();
            makeNewFile();
        }

        private void makeNewFile()
        {
            m_bucketColletion.MouseX = -1;
            m_bucketColletion.MouseY = -1;

            initMap(ref map, mapX, mapY);
            initMap(ref mapFullTile, mapX, mapY);

            setMode(paintMode.stamp);
            panel1.AutoScrollMinSize = new Size(mapX * tileWidth, mapY * tileHeigth);
            m_collisionRect.Clear();
            m_eventRect.Clear();
            m_objectPt.Clear();

            listBoxCollision.Items.Clear();
            listBoxEvent.Items.Clear();
            listBoxObject.Items.Clear();
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files (*.*)|*.*|XML(*.xml)|*.xml";
            dlg.FilterIndex = 2;
            dlg.DefaultExt = "xml";
          //  XmlWriter writer = XmlWriter.Create("employees.xml");

            if (DialogResult.OK == dlg.ShowDialog())
            {
                XElement xRoot = new XElement("TileSystem");


                XAttribute m_worldSizeW = new XAttribute("m_worldSizeW", mapX * tileWidth);
                xRoot.Add(m_worldSizeW);

                XAttribute m_worldSizeH = new XAttribute("m_worldSizeH", mapY * tileHeigth);
                xRoot.Add(m_worldSizeH);

                XAttribute screenSizeW = new XAttribute("m_screenSizeW", m_screenSizeW);
                xRoot.Add(screenSizeW);

                XAttribute screenSizeH = new XAttribute("m_screenSizeH", m_screenSizeH);
                xRoot.Add(screenSizeH);

                XAttribute m_tileSizeW = new XAttribute("m_tileSizeW", tileWidth);
                xRoot.Add(m_tileSizeW);

                XAttribute m_tileSizeH = new XAttribute("m_tileSizeH", tileHeigth);
                xRoot.Add(m_tileSizeH);

                XAttribute m_tileSetX = new XAttribute("m_tileSetX", tileSetX);
                xRoot.Add(m_tileSetX);

                XAttribute m_tileSetY = new XAttribute("m_tileSetY", tileSetY);
                xRoot.Add(m_tileSetY);

                XAttribute mapXgrid = new XAttribute("mapX", mapX);
                xRoot.Add(mapXgrid);

                XAttribute mapYgrid = new XAttribute("mapY", mapY);
                xRoot.Add(mapYgrid);

                //tile map path
                for (int i = 0; i < m_tileMap.Count; i++)
                {
                    XElement path = new XElement("Path");
                    xRoot.Add(path);

                    XAttribute pathName = new XAttribute("name", m_tileMap[i].TileMap.PathName);
                    path.Add(pathName);
                }       

                //collision left,top,size
                for (int i = 0; i < m_collisionRect.Count; i++)
                {
                    XElement collision = new XElement("collision");
                    xRoot.Add(collision);

                    XAttribute collision_left = new XAttribute("collision_left", m_collisionRect[i].Rect.Left);
                    collision.Add(collision_left);

                    XAttribute collision_top = new XAttribute("collision_top", m_collisionRect[i].Rect.Top);
                    collision.Add(collision_top);

                    XAttribute collision_size_w = new XAttribute("collision_size_w", m_collisionRect[i].Rect.Size.Width);
                    collision.Add(collision_size_w);

                    XAttribute collision_size_h = new XAttribute("collision_size_h", m_collisionRect[i].Rect.Size.Height);
                    collision.Add(collision_size_h);
                }
                
                //event name,left,top,size
                for (int i = 0; i < m_eventRect.Count; i++)
                {
                    XElement eventRect = new XElement("event");
                    xRoot.Add(eventRect);

                    XAttribute event_name = new XAttribute("event_name", m_eventRect[i].Name);
                    eventRect.Add(event_name);

                    XAttribute event_left = new XAttribute("event_left", m_eventRect[i].Rect.Left);
                    eventRect.Add(event_left);

                    XAttribute event_top = new XAttribute("event_top", m_eventRect[i].Rect.Top);
                    eventRect.Add(event_top);

                    XAttribute event_size_w = new XAttribute("event_size_w", m_eventRect[i].Rect.Size.Width);
                    eventRect.Add(event_size_w);

                    XAttribute event_size_h = new XAttribute("event_size_h", m_eventRect[i].Rect.Size.Height);
                    eventRect.Add(event_size_h);
                }
                
                //object left,top,name
                for (int i = 0; i < m_objectPt.Count; i++)
                {
                    XElement objectRect = new XElement("object");
                    xRoot.Add(objectRect);

                    XAttribute object_name = new XAttribute("object_name", m_objectPt[i].Name);
                    objectRect.Add(object_name);

                    XAttribute object_left = new XAttribute("object_left", m_objectPt[i].Rect.Left);
                    objectRect.Add(object_left);

                    XAttribute object_top = new XAttribute("object_top", m_objectPt[i].Rect.Top);
                    objectRect.Add(object_top);
                 
                }
               
                //tile map
                //object left,top,name
                 for (int x = 0; x < mapX; x++)
                  {
                    for (int y = 0; y < mapY; y++)
                        {
                        int indexX = map[x, y].X;
                        int indexY = map[x, y].Y;
                        int layerIndex = map[x, y].TabIndex;
                        XElement tileMap = new XElement("tile");
                        xRoot.Add(tileMap);
                                
                        //calculate total grid
                        int gridTotal = indexX+indexY*tileSetY+tileSetX*tileSetY*layerIndex;

                            XAttribute tileMap_index = new XAttribute("grid", gridTotal);
                            tileMap.Add(tileMap_index);
                        }
                     }              
            
                xRoot.Save(dlg.FileName);
            }
        }

        private void laodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|XML Files|*.xml";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                XElement xRoot = XElement.Load(dlg.FileName);

                //mapX ,mapY, tileSetX,tileSetY,tileSizeW,tileSizwH
                XAttribute xmapX = xRoot.Attribute("mapX");
                mapX = Convert.ToInt32(xmapX.Value);

                XAttribute xmapY = xRoot.Attribute("mapY");
                mapY = Convert.ToInt32(xmapY.Value);

                XAttribute m_tileSizeW = xRoot.Attribute("m_tileSizeW");
                tileWidth = Convert.ToInt32(m_tileSizeW.Value);

                XAttribute m_tileSizeH = xRoot.Attribute("m_tileSizeH");
                tileHeigth = Convert.ToInt32(m_tileSizeH.Value);

                XAttribute m_tileSetX = xRoot.Attribute("m_tileSetX");
                tileSetX = Convert.ToInt32(m_tileSetX.Value);

                XAttribute m_tileSetY = xRoot.Attribute("m_tileSetY");
                tileSetY = Convert.ToInt32(m_tileSetY.Value);

                makeNewFile();

                //clear all tab first
                tabAsset.TabPages.Clear();
                m_tileMap.Clear();

                IEnumerable<XElement> xPaths = xRoot.Elements();

                foreach (XElement xPath in xPaths)
                {
                    if (xPath.Name.ToString() == "Path" )
                    {

                        XAttribute xPathName = xPath.Attribute("name");
                        string path = xPathName.Value;
                        importTileHelper(path);
                    }

                    //collision
                    if (xPath.Name.ToString() == "collision")
                    {
                      
                        XAttribute xCollisioin = xPath.Attribute("collision_left");
                        //left
                        int collision_left = Convert.ToInt32(xCollisioin.Value);
                        //top
                        xCollisioin = xPath.Attribute("collision_top");
                        int collision_top = Convert.ToInt32(xCollisioin.Value);

                        xCollisioin = xPath.Attribute("collision_size_w");
                        int collision_size_w = Convert.ToInt32(xCollisioin.Value);

                        xCollisioin = xPath.Attribute("collision_size_h");
                        int collision_size_h = Convert.ToInt32(xCollisioin.Value);

                        Size tempSize = new Size(collision_size_w, collision_size_h);
                        Event_Collision_Object_Rect tempCollision =
                            new Event_Collision_Object_Rect(collision_left, collision_top, tempSize, "default");

                        m_collisionRect.Add(tempCollision);
                        listBoxCollision.Items.Add(tempCollision);
                    }

                    //event
                    if (xPath.Name.ToString() == "event")
                    {
                      
                        XAttribute xEvent = xPath.Attribute("event_left");
                        //left
                        int event_left = Convert.ToInt32(xEvent.Value);
                        //top
                        xEvent = xPath.Attribute("event_top");
                        int event_top = Convert.ToInt32(xEvent.Value);

                        xEvent = xPath.Attribute("event_size_w");
                        int event_size_w = Convert.ToInt32(xEvent.Value);

                        xEvent = xPath.Attribute("event_size_h");
                        int event_size_h = Convert.ToInt32(xEvent.Value);

                        xEvent = xPath.Attribute("event_name");
                        string event_name = xEvent.Value;
                   
                        Size tempSize = new Size(event_size_w, event_size_h);
                        Event_Collision_Object_Rect tempEvent =
                            new Event_Collision_Object_Rect(event_left, event_top, tempSize, event_name);

                        m_eventRect.Add(tempEvent);
                        listBoxEvent.Items.Add(tempEvent);
                    }

                    //object
                    if (xPath.Name.ToString() == "object")
                    {

                        XAttribute xEvent = xPath.Attribute("object_left");
                        
                        //left
                        int object_left = Convert.ToInt32(xEvent.Value);
                        //top
                        xEvent = xPath.Attribute("object_top");
                        int object_top = Convert.ToInt32(xEvent.Value);

                        //object name
                        xEvent = xPath.Attribute("object_name");
                        string object_name = xEvent.Value;

                        Size tempSize = new Size(1, 1);
                        Event_Collision_Object_Rect tempObject =
                            new Event_Collision_Object_Rect(object_left, object_top, tempSize, object_name);

                        m_objectPt.Add(tempObject);
                        listBoxObject.Items.Add(tempObject);
                    }
                             
                    //tile
                    if (xPath.Name.ToString() == "tile")
                    {
                        XAttribute xEvent = xPath.Attribute("grid");

                        //index
                        int index = Convert.ToInt32(xEvent.Value);

                        if (index <= -1)
                            continue;

                        //another layer
                        int layer = index / (tileSetX * tileSetY);
                        
                        //index adjust
                        index -= layer * (tileSetX * tileSetY);

                        int indexX = index % tileSetX;
                        int indexY = index / tileSetX;

                        map[indexX, indexY].TabIndex = layer;
                        map[indexX, indexY].X = indexX;
                        map[indexX, indexY].Y = indexY;

                    }

                }
              
            }
        }


       
     

       
      
      

      

       

        //////////////////////////////////////tool windows////////////////////////////////////

    }
}
