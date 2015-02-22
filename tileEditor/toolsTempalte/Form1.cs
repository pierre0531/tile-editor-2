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
    public enum enumCollision{ water, wall,None }
    public enum enumEvent { spawn, coop, None }
    public enum enumObject { player1, player2, None }
 
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
            D3D.AddRenderTarget(panel1);
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
                    tabEditLayer.Visible = false;
                    break;
                case paintMode.stamp:
                    ButtonStamp.Checked = true;
                    tabEditLayer.Visible = false;
                    break;
                case paintMode.collision:
                    collisionButton.Checked = true;
                    tabEditLayer.Visible = true;
                    break;
                case paintMode.eventTrigger:
                    EventButton.Checked = true;
                    tabEditLayer.Visible = true;
                    break;
                case paintMode.Object:
                    ObjectButton.Checked = true;
                    tabEditLayer.Visible = true;
                    break;
                default:
                    break;
            }
        }
        private void initializeNumber()
        {

            m_bucketColletion.MouseX = -1;
            m_bucketColletion.MouseY = -1;

             mapX = 50;
             mapY = 50;

             tileSetX = 5;
             tileSetY = 5;

             tileWidth = 32;
             tileHeigth = 32;

             m_screenSizeW = 800;
             m_screenSizeH = 600;

            //draw all the layer default
             MapCheckBox.Checked = true;
             EventCheckBox.Checked = true;
             ObjectCheckBox.Checked = true;
             CollisionCheckBox.Checked = true;
             checkBoxGrid.Checked = true;
             checkBoxBlockGrid.Checked = true;
             checkBoxWeight.Checked = true;

             m_bucketColletion.MouseX = -1;
             m_bucketColletion.MouseY = -1;

             initMap(ref map, mapX, mapY);
             initMap(ref mapFullTile, mapX, mapY);
             setMode(paintMode.stamp);
             panel1.AutoScrollMinSize = new Size(mapX * tileWidth, mapY * tileHeigth);

           
             //clear all
             clearAllDataStructure();
     
            //add combo to object
             addCombox(comboBoxObject, default(enumObject));
             addCombox(comboBoxEvent, default(enumEvent));
             addCombox(comboBoxCollision, default(enumCollision));

        }

        private void addCombox(ComboBox _ComboBox, Enum _enum)
        {
            //add combo to object
            foreach (var item in Enum.GetValues(_enum.GetType()))
            {
                _ComboBox.Items.Add(item);

                //default combobox
                if (_ComboBox.Items.Count > 0)
                    _ComboBox.SelectedIndex = 0;

            }
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
                    _map[x, y].Block = false;
                    _map[x, y].Weight = 1;
                }
            }
        }

       
        public void Render()
        {

            Render1();

            int TextureCount = m_tileMap.Count;

            //no tile map, so dont need to render 2 and render3
            if (TextureCount>0)
            {
                Render2();
                Render3();
            }
            else if (TextureCount == 0)
            {
                D3D.Clear(panel3, Color.White);
            }
        }
        public void Render1()
        {
           // D3D.Present();
            D3D.Clear(panel1, Color.White);
            D3D.DeviceBegin();
            D3D.SpriteBegin();

            //get tabIndex
            int tempTabIndex = tabAsset.SelectedIndex;

            //for render the map section
            drawMap();

            //draw event, object, collision box
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

            //Draw the hollow Rect  
            drawHollowGrid();

            //draw the weight grid
            drawWeightGrid();

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

        //render the map
        private void drawMap()
        {
            //get tabIndex
            int tempTabIndex = tabAsset.SelectedIndex;
            Point offset = panel1.AutoScrollPosition;

            //safe check
            if (MapCheckBox.Checked == true)
                if (tempTabIndex != -1)
                {

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
                            int locationY = y * tileHeigth + offset.Y;

                            //safe check
                            if (map[x, y].TabIndex < m_tileMap.Count)
                                TM.Draw(map[x, y].TabIndex, locationX, locationY, 1, 1, src);
                        }
                    }
                }

        }


        //Draw the drawWeightGrid 
        private void drawWeightGrid()
        {
            Point offset = panel1.AutoScrollPosition;              
            if (checkBoxWeight.Checked)
                for (int x = 0; x < mapX; x++)
                {
                    for (int y = 0; y < mapY; y++)
                    {
                        int locationX = x * tileWidth + offset.X;
                        int locationY = y * tileHeigth + offset.Y;

                        //draw the weight
                        D3D.DrawText(map[x, y].Weight.ToString(), locationX, locationY, Color.FromArgb(255, 255, 0, 0));
                    }
                }
        }

        //Draw the hollow Rect  
        private void drawHollowGrid()
        {
            Point offset = panel1.AutoScrollPosition;
                 
            if (checkBoxGrid.Checked)
                for (int x = 0; x < mapX; x++)
                {
                    for (int y = 0; y < mapY; y++)
                    {
                        int locationX = x * tileWidth + offset.X;
                        int locationY = y * tileHeigth + offset.Y;

                        D3D.DrawHollowRect(new Rectangle(locationX, locationY, tileWidth, tileHeigth),
                            Color.FromArgb(255, 0, 0, 0), 1);
                    }
                }
        }
        private void drawBox()
        {
            if (checkBoxBlockGrid.Checked == true)
                drawCheckBoxBlockGrid();

            if (EventCheckBox.Checked == true)
            drawEventBox();

            if (CollisionCheckBox.Checked == true)
            drawCollisionBox();

            if (ObjectCheckBox.Checked == true)
            drawObjectBox();
        }

        //draw grid based block
        private void drawCheckBoxBlockGrid()
        {
            //draw Collision Box
            Point offset = panel1.AutoScrollPosition;

            for (int x = 0; x < mapX; x++)
            {
                for (int y = 0; y < mapY; y++)
                {
                    if (map[x, y].Block == false)
                        continue;

                    int locationX = x * tileWidth + offset.X;
                    int locationY = y * tileHeigth + offset.Y;

                    //draw the gray point
                    D3D.DrawRect(new Rectangle(locationX, locationY, tileWidth, tileHeigth), Color.FromArgb(128, 128, 128, 128));
                }
            }  
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
        { 
            //draw Collision Box
            Point offset = panel1.AutoScrollPosition;
            for (int i = 0; i < m_collisionRect.Count; i++)
            {
                int locationX = m_collisionRect[i].Rect.Left + offset.X;
                int locationY = m_collisionRect[i].Rect.Top + offset.Y;

                D3D.DrawText(m_collisionRect[i].Name, locationX, locationY, Color.FromArgb(255, 255, 0, 0));

                D3D.DrawHollowRect(new Rectangle(locationX, locationY,
                    m_collisionRect[i].Rect.Width, m_collisionRect[i].Rect.Height), Color.FromArgb(255, 255, 0, 0), 3);
                
                //draw X
                D3D.DrawLine(locationX, locationY,
                    m_collisionRect[i].Rect.Right + offset.X, m_collisionRect[i].Rect.Bottom + offset.Y,
                    Color.FromArgb(255, 255, 0, 0), 3);
                //draw X
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
                    int locationY = y * tileHeigth + offset.Y;


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

            D3D.Resize(panelTile, panelTile.ClientSize.Width, panelTile.ClientSize.Height, false);
           // D3D.Clear(panel2, Color.WhiteSmoke);
            D3D.Clear(panelTile, Color.WhiteSmoke);

            D3D.DeviceBegin();
            D3D.SpriteBegin();
    
            TM.Draw(tempTextureID, panelTile.AutoScrollPosition.X, panelTile.AutoScrollPosition.Y);
         
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
                for (int y = minor(selectedTile.Y, stampSelectedTile.Y); y <= major(selectedTile.Y, stampSelectedTile.Y); y++)
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
            if (x >= y)
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

                    //safe check
                    if (map[x, y].TabIndex<m_tileMap.Count)
                    TM.Draw(map[x, y].TabIndex, x * tileWidth / scaleX, y * tileHeigth / scaleY,
                        1 / (float)scaleX, 1 / (float)scaleY, src);
                 
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

    

       
    
        //safe check whether map out of range
        bool outOfRangeTile(MouseEventArgs e)
        {
            //get tabIndex
            int tempTabIndex = tabAsset.SelectedIndex;

            //using tabIndex to get which tab is selected
            int tempTextureID = m_tileMap[tempTabIndex].TileMap.TextureID;

            Panel panelTileSet = m_tileMap[tempTabIndex].Panel;

            float locationX = e.Location.X - panelTileSet.AutoScrollPosition.X;
            float locationY = e.Location.Y - panelTileSet.AutoScrollPosition.Y;

            if (locationX >= tileSetX * tileWidth || locationX <= 0 ||
                locationY >= tileSetY * tileHeigth || locationY <= 0)          
                return true;

            return false;
        }

       

        //safe check whether map out of range
        bool outOfRangeMap(MouseEventArgs e)
        {
         
            float locationX = e.Location.X - panel1.AutoScrollPosition.X;
            float locationY = e.Location.Y - panel1.AutoScrollPosition.Y;

            if (locationX > mapX * tileWidth || locationX < 0 ||
                locationY > mapY * tileHeigth || locationY < 0)
                return true;

            return false;
        }
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {

            if (outOfRangeMap(e))
                return;

            //Caculate where is the mouse
            int x = (e.Location.X - panel1.AutoScrollPosition.X) / tileWidth; //0~5(default)
            int y = (e.Location.Y - panel1.AutoScrollPosition.Y) / tileHeigth;//0~5(default)

            //select tile at map
            if (e.Button == MouseButtons.Right)
            {
            //    stampSelectedTile.X = x;
              //  stampSelectedTile.Y = y;
                return;
            }

            //debug usage
            toolStripLabel1.Text = e.Location.ToString() + x.ToString() + y.ToString();

            switch (m_mode)
            {
                case paintMode.full:
                    fullMouseMove(x, y, e);
                    break;
                case paintMode.stamp:
                    stampMouseMove(x, y, e);
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
        //for precise mouse location
        private void panel1_Resize(object sender, EventArgs e)
        {
            D3D.Resize(panel1, panel1.ClientSize.Width, panel1.ClientSize.Height, true);
        }
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            //calculate which selection area
          //  calculateRect();
        }
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (outOfRangeMap(e))
                return;

            //Caculate where is the mouse
            int x = (e.Location.X - panel1.AutoScrollPosition.X) / tileWidth; //0~5(default)
            int y = (e.Location.Y - panel1.AutoScrollPosition.Y) / tileHeigth;//0~5(default)

            //select tile on map by using right click
            if (e.Button == MouseButtons.Right)
            {
           //     selectedTile.X = x;
            //    selectedTile.Y = y;
                selectedTile.X = map[x, y].X;
                selectedTile.Y = map[x, y].Y;
               // selectedTile.TabIndex = map[x, y].TabIndex;
                stampSelectedTile.X = map[x, y].X;
                stampSelectedTile.Y = map[x, y].Y;
              //  stampSelectedTile.TabIndex = map[x, y].TabIndex;
                tabAsset.SelectedIndex =  map[x, y].TabIndex;
                return;
            }

            if (m_mode == paintMode.collision)
                storageCollisionRect(e.Location.X, e.Location.Y);
            else if (m_mode == paintMode.eventTrigger)
                storageCollisionRect(e.Location.X, e.Location.Y);
        }

        private void panel1_Scroll(object sender, ScrollEventArgs e)
        {
            Render1();
        }


        //Single click to know where is the map mouse location
        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (outOfRangeMap(e) || e.Button == MouseButtons.Right)
                return;

            //Caculate where is the mouse
            int x = (e.Location.X - panel1.AutoScrollPosition.X) / tileWidth; //0~5(default)
            int y = (e.Location.Y - panel1.AutoScrollPosition.Y) / tileHeigth;//0~5(default)
      

            if (m_mode == paintMode.stamp)
                storageTile(x, y);
            else if (m_mode == paintMode.full)
                storageFullTile();
            else if (m_mode == paintMode.Object)
                storageCollisionRect(e.Location.X, e.Location.Y);

            //calculate to data structure
            calculateRect();
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
                        _target[x, y].Weight = _src[x, y].Weight;
                    }
                        //not total copy for preview
                    else
                    {
                        if (_src[x, y].PreviewOnMap == true)
                        {
                            _target[x, y].X = _src[x, y].X;
                            _target[x, y].Y = _src[x, y].Y;
                            _target[x, y].TabIndex = _src[x, y].TabIndex;
                            _target[x, y].Weight = _src[x, y].Weight;
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

            counter = 0;
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
            //max recursive safe check
            if (counter >= 2500)
                return;

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
            //counter++;

            limitRecursiveXY(ref tempX, ref tempY);

            ////only chech while the tile is not check
           if (mapFullTile[tempX, tempY].CheckForBucket == true)
                return;

            ////error check for not out of range
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

                Panel panelTileSet = m_tileMap[tempTabIndex].Panel;
 

                //get the mouse tile location
                int x = (e.Location.X - panelTileSet.AutoScrollPosition.X) / tileWidth;//0~5
                int y = (e.Location.Y - panelTileSet.AutoScrollPosition.Y) / tileHeigth;//0~1

                //safe check
                if (x >= tileSetX )
                    x = tileSetX - 1;
                else if (x < 0)
                    x = 0;

                if (y >= tileSetY)
                    y = tileSetY - 1;
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

            //safe check
            if (endX >= tileSetX)
                endX = tileSetX;

            //safe check
            if (endY >= tileSetY)
                endY = tileSetY;

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
            hoverTileCollection = new Tile[stampSelectedTile.X - selectedTile.X, stampSelectedTile.Y -    selectedTile.Y ];
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
                        tempTile[x, y].Block = map[x, y].Block;
                        tempTile[x, y].Weight = map[x, y].Weight;
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
                    map[x, y].Block = tempTile[x, y].Block;
                    map[x, y].Weight = tempTile[x, y].Weight;
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
        }

        private void importTileHelper(string open)
        {
            //Open a stream for reading
            tileMap tempListMap = new tileMap();
            tempListMap.TextureID = TM.LoadTexture(open);
            
            //error check
            if (tempListMap.TextureID == -1)
                return;

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

        //import tile set
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

                 //D3D.Resize(tempPanel, tempPanel.ClientSize.Width, tempPanel.ClientSize.Height, true);

                 //setting the new tab
                 tempPanel.Parent = tempTabPage;
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
     
        private void collisionButton_Click(object sender, EventArgs e)
        {
            setMode(paintMode.collision);
            tabEditLayer.SelectedTab = tabPageCollision;
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

           Event_Collision_Object_Rect tempECORect = new Event_Collision_Object_Rect();
           tempECORect.Rect = tempRect;
        
           switch (m_mode)
           {         
               case paintMode.collision:
                   //add to data structure    
                   addToDataStructure(ref tempECORect);
                   addGridBlock(ref tempRect,comboBoxCollision.SelectedIndex);
                   break;
               case paintMode.eventTrigger:
                   //add to data structure    
                   addToDataStructure(ref tempECORect);      

                   break;
               case paintMode.Object:           
                   //add to data structure    
                   addToDataStructure(ref tempECORect);
                  
                   break;

               default:
                   break;
           }
         
        }

        private void addGridBlock(ref Rectangle _rectangle, int _comboIndex)
        {
           
            //get the map X , Y (0~5)
            int left = (_rectangle.Left ) / tileWidth;
            int top = (_rectangle.Top )  / tileHeigth;
            int right = (_rectangle.Right ) / tileWidth;
            int bottom = (_rectangle.Bottom ) / tileHeigth;

            if (left <= 0)
                left = 0;

            if (top <= 0)
                top = 0;

            if (right >= mapX)
                right = mapX - 1;

            if (bottom >= mapY)
                bottom = mapY - 1;
       
            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                {
                    //make the bool to true
                    map[x, y].Block = true;

                    string grabString = comboBoxCollision.Items[_comboIndex].ToString();
                    int _position = grabString.IndexOf('_');

                    //safe check
                    if (_position != -1)
                    {
                        string weight_string = grabString.Substring(_position+1);
                        int weight_int = Int32.Parse(weight_string);
                        map[x, y].Weight = weight_int; 
                    }else
                        map[x, y].Weight = 1;
                 } 
            }           
        }

        private void resetGridBlock()
        {
            for (int x = 0; x < mapX; x++)
            {
                for (int y = 0; y < mapY; y++)
                {
                    //reset false
                    map[x, y].Block = false;
                }
            }

             //get every collision data
            for (int i = 0; i < m_collisionRect.Count; i++)
            {
                int left = m_collisionRect[i].Rect.Left / tileWidth;
                int top = m_collisionRect[i].Rect.Top / tileHeigth;
                int right = m_collisionRect[i].Rect.Right / tileWidth;
                int bottom = m_collisionRect[i].Rect.Bottom / tileHeigth;

                if (left <= 0)
                    left = 0;

                if (top <= 0)
                    top = 0;

                if (right >= mapX)
                    right = mapX - 1;

                if (bottom >= mapY)
                    bottom = mapY - 1;

                //get the map X , Y EX:(0~5)
                for (int x = left; x <= right; x++)
                {
                    for (int y = top; y <= bottom; y++)
                    {
                        //make the bool to true
                        map[x, y].Block = true;
                    }
                }
            }       
        }

        private void addToDataStructure(ref Event_Collision_Object_Rect _tempEventRect)
        {
            List<Event_Collision_Object_Rect> tempCollection = new List<Event_Collision_Object_Rect>();
            ListBox tempListBox = new ListBox();
            ComboBox tempComboBox = new ComboBox();
            switch (m_mode)
            {
                case paintMode.collision:
                    tempListBox = listBoxCollision;
                    tempCollection = m_collisionRect;
                    tempComboBox = comboBoxCollision;
                    break;
                case paintMode.eventTrigger:
                    tempListBox = listBoxEvent;
                    tempCollection = m_eventRect;
                    tempComboBox = comboBoxEvent;
                    break;
                case paintMode.Object:
                    tempListBox = listBoxObject;
                    tempCollection = m_objectPt;
                    tempComboBox = comboBoxObject;
                    _tempEventRect.Size = new Size(20, 20);
                    break;
                default:
                    break;
            }

            //add to data structure                 
            int comboIndex = tempComboBox.SelectedIndex;

            //safe check
            if (comboIndex != -1)
                _tempEventRect.Name = tempComboBox.Items[comboIndex].ToString();

            //add to data collection
            tempCollection.Add(_tempEventRect);

            //add to listBox
            tempListBox.Items.Add(_tempEventRect);
            tempListBox.SelectedIndex = tempListBox.Items.Count - 1; 
          
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
        private void buttonUpdate()
        {
            //three data structure
            List<Event_Collision_Object_Rect> tempCollection = new List<Event_Collision_Object_Rect>();
            ListBox tempListBox = new ListBox();
            ComboBox tempComboBox = new ComboBox();

            //temp Event_Collision_Object_Rect
            Event_Collision_Object_Rect tempECO_Rect = new Event_Collision_Object_Rect();

            //get the value
            NumericUpDown tempBottom = new NumericUpDown();
            NumericUpDown tempTop = new NumericUpDown();
            NumericUpDown tempLeft = new NumericUpDown();
            NumericUpDown tempRight = new NumericUpDown();
            switch (m_mode)
            {
                case paintMode.collision:
                    //three data structure
                    tempListBox = listBoxCollision;
                    tempCollection = m_collisionRect;
                    tempComboBox = comboBoxCollision;

                    //get the value
                    tempBottom = numCollision_bottom;
                    tempTop = numCollision_top;
                    tempLeft = numCollision_left;
                    tempRight = numCollision_right;
                    break;
                case paintMode.eventTrigger:
                    //three data structure
                    tempListBox = listBoxEvent;
                    tempCollection = m_eventRect;
                    tempComboBox = comboBoxEvent;

                    //get the value
                    tempBottom = numEvent_bottom;
                    tempTop = numEvent_top;
                    tempLeft = numEvent_left;
                    tempRight = numEvent_right;
                    break;
                case paintMode.Object:
                    //three data structure
                    tempListBox = listBoxObject;
                    tempCollection = m_objectPt;
                    tempComboBox = comboBoxObject;

                    //get the value
                    tempTop = numObject_top;
                    tempLeft = numObject_left;
                    tempRight.Maximum = 9999999999999;
                    tempBottom.Maximum = 9999999999999;

                    tempRight.Value = numObject_left.Value + 20;
                    tempBottom.Value = numObject_top.Value + 20;
                    break;
                default:
                    break;

            }

            if (tempListBox.SelectedIndex == -1)
                return;

            //get the number info
            int bottom = (int)tempBottom.Value;
            int right = (int)tempRight.Value;
            int top = (int)tempTop.Value;
            int left = (int)tempLeft.Value;
            int width = right - left;
            int height = bottom - top;

            //safe check
            if (width <= 0)
                width = 0;
            if (height <= 0)
                height = 0;

            //name here
            int comboIndex = tempComboBox.SelectedIndex;
            if (comboIndex >= 0)
            {
                tempECO_Rect.Name = tempComboBox.Items[comboIndex].ToString();

            }
            
            //temp Rect
            Rectangle tempRect = new Rectangle();

            //copy info to the rect
            tempRect.Location = new Point(left, top);
            tempRect.Width = width;
            tempRect.Height = height;

            //copy rect back
            tempECO_Rect.Rect = tempRect;

            //old one index
            int tempIndex = tempListBox.SelectedIndex;

            //insert a new one
            tempListBox.Items.Insert(tempIndex, tempECO_Rect);
            tempCollection.Insert(tempIndex, tempECO_Rect);

            //remove old one
            tempListBox.Items.RemoveAt(tempIndex+1);
            tempCollection.RemoveAt(tempIndex+1);

            //re select list index
            tempListBox.SelectedIndex = tempIndex;
        }

        private void listBoxMouseSelectIndexChanged()
        {
            //three data structure
            List<Event_Collision_Object_Rect> tempCollection = new List<Event_Collision_Object_Rect>();
            ListBox tempListBox = new ListBox();
            ComboBox tempComboBox = new ComboBox();

            //get the value
            NumericUpDown tempBottom = new NumericUpDown();
            NumericUpDown tempTop = new NumericUpDown();
            NumericUpDown tempLeft = new NumericUpDown();
            NumericUpDown tempRight = new NumericUpDown();
            switch (m_mode)
            {
                case paintMode.collision:
                    //three data structure
                    tempListBox = listBoxCollision;
                    tempCollection = m_collisionRect;
                    tempComboBox = comboBoxCollision;

                    //get the value
                    tempBottom = numCollision_bottom;
                    tempTop = numCollision_top;
                    tempLeft = numCollision_left;
                    tempRight = numCollision_right;
                    break;
                case paintMode.eventTrigger:
                    //three data structure
                    tempListBox = listBoxEvent;
                    tempCollection = m_eventRect;
                    tempComboBox = comboBoxEvent;

                    //get the value
                    tempBottom = numEvent_bottom;
                    tempTop = numEvent_top;
                    tempLeft = numEvent_left;
                    tempRight = numEvent_right;
                    break;
                case paintMode.Object:
                    //three data structure
                    tempListBox = listBoxObject;
                    tempCollection = m_objectPt;
                    tempComboBox = comboBoxObject;

                    //get the value
                    tempTop = numObject_top;
                    tempLeft = numObject_left;
                    tempRight.Maximum = 9999999999999;
                    tempBottom.Maximum = 9999999999999;

                    tempRight.Value = numObject_left.Value + 20;
                    tempBottom.Value = numObject_top.Value + 20;
                    break;
                default:
                    break;

            }

            if (tempListBox.SelectedIndex == -1)
                return;

            Event_Collision_Object_Rect r = (Event_Collision_Object_Rect)tempListBox.Items[tempListBox.SelectedIndex];
            tempTop.Value = r.Rect.Top;
            tempLeft.Value = r.Rect.Left;
            tempRight.Value = r.Rect.Right;
            tempBottom.Value = r.Rect.Bottom;
            tempComboBox.SelectedIndex = tempComboBox.FindString(r.Name);     
        }

        private void buttonDelete()
        {
            //three data structure
            List<Event_Collision_Object_Rect> tempCollection = new List<Event_Collision_Object_Rect>();
            ListBox tempListBox = new ListBox();
            ComboBox tempComboBox = new ComboBox();
            TextBox tempTextBox = new TextBox();

            //use reference to get content
            switchHelper(ref tempCollection, ref tempListBox, ref tempComboBox, ref tempTextBox);

            if (tempListBox.SelectedIndex == -1)
                return;

            int tempIndex = tempListBox.SelectedIndex;
            tempListBox.Items.RemoveAt(tempIndex);
            tempCollection.RemoveAt(tempIndex);

            if (m_mode == paintMode.collision)
            {
                resetGridBlock();
            }
            //re select new index
            if (tempListBox.Items.Count > 0 && tempIndex < tempListBox.Items.Count)
                tempListBox.SelectedIndex = tempIndex;
            else if (tempListBox.Items.Count > 0 && tempIndex >= tempListBox.Items.Count)
                tempListBox.SelectedIndex = tempListBox.Items.Count - 1;
            else
                tempListBox.SelectedIndex = -1;
        }
        //box collision//
        //***************************************************************************************//

        private void buttonCollisionDelete_Click(object sender, EventArgs e)
        {
            buttonDelete();
          

        }

        private void buttonUpdateCollisioin_Click(object sender, EventArgs e)
        {
            buttonUpdate();
         
        }

        private void listBoxCollision_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxMouseSelectIndexChanged();
           
        }


        //box event//
        //***************************************************************************************//

      
        private void buttonUpdateEvent_Click(object sender, EventArgs e)
        {
            buttonUpdate();
          
        }

       
      //event index change
        private void listBoxEvent_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxMouseSelectIndexChanged();
            
        }

        //event delete
        private void buttonEventDelete_Click(object sender, EventArgs e)
        {
            buttonDelete();
        }

        //object event//
        //***************************************************************************************//
    

        private void buttonObjectDelete_Click(object sender, EventArgs e)
        {
            buttonDelete();
        }

        private void buttonObjectUpdate_Click(object sender, EventArgs e)
        {
            buttonUpdate();
           
        }

        private void listBoxObject_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxMouseSelectIndexChanged();
            
        }

      
        private void buttonOjectNext_Click(object sender, EventArgs e)
        {
            buttonNext();
        }

        private void buttonNext()
        {
            List<Event_Collision_Object_Rect> tempCollection = new List<Event_Collision_Object_Rect>();
            ListBox tempListBox = new ListBox();
            ComboBox tempComboBox = new ComboBox();
            TextBox tempTextBox = new TextBox();

            //use reference to get content
            switchHelper(ref tempCollection, ref tempListBox, ref tempComboBox, ref tempTextBox);

            //safe check
            if (tempCollection.Count <= 0)
                return;

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
            ComboBox tempComboBox = new ComboBox();
            TextBox tempTextBox = new TextBox();

            //use reference to get content
            switchHelper(ref tempCollection, ref tempListBox, ref tempComboBox, ref tempTextBox);

            //safe check
            if (tempCollection.Count <= 0)
                return;

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
           
            initMap(ref map, mapX, mapY);
            initMap(ref mapFullTile, mapX, mapY);

            setMode(paintMode.stamp);
            panel1.AutoScrollMinSize = new Size(mapX * tileWidth, mapY * tileHeigth);
            
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

                XAttribute totalLine = new XAttribute("totalLine", m_collisionRect.Count + m_eventRect.Count + m_objectPt.Count+mapX*mapY);
                xRoot.Add(totalLine);
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

                    //safe check
                    if (m_collisionRect[i].Name == null)
                    {
                        XAttribute collision_name = new XAttribute("collision_name","None");
                        collision.Add(collision_name);
                    }
                    else
                    {
                        XAttribute collision_name = new XAttribute("collision_name", m_collisionRect[i].Name);
                        collision.Add(collision_name);
 
                    }
                    
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

                     //safe check
                    if (m_eventRect[i].Name == null)
                    {
                        XAttribute event_name = new XAttribute("event_name", "None");
                        eventRect.Add(event_name);
                    }
                    else
                    {
                        XAttribute event_name = new XAttribute("event_name", m_eventRect[i].Name);
                        eventRect.Add(event_name);
                    }

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

                      //safe check
                    if (m_objectPt[i].Name == null)
                    {
                        XAttribute object_name = new XAttribute("object_name", "None");
                        objectRect.Add(object_name);
                    }
                    else
                    {
                        XAttribute object_name = new XAttribute("object_name", m_objectPt[i].Name);
                        objectRect.Add(object_name);
                    }

                    XAttribute object_left = new XAttribute("object_left", m_objectPt[i].Rect.Left);
                    objectRect.Add(object_left);

                    XAttribute object_top = new XAttribute("object_top", m_objectPt[i].Rect.Top);
                    objectRect.Add(object_top);               
                }
                
                //collision name
                foreach (var item in comboBoxCollision.Items)
                {
                    XElement HostName = new XElement("collision_name");
                    xRoot.Add(HostName);

                    XAttribute subName = new XAttribute("SubName",item.ToString());
                    HostName.Add(subName);                         
                }
                
                //event name
                foreach (var item in comboBoxEvent.Items)
                {
                    XElement HostName = new XElement("event_name");
                    xRoot.Add(HostName);

                    XAttribute subName = new XAttribute("SubName", item.ToString());
                    HostName.Add(subName);
                }

                
                //object name
                foreach (var item in comboBoxObject.Items)
                {
                    XElement HostName = new XElement("object_name");
                    xRoot.Add(HostName);

                    XAttribute subName = new XAttribute("SubName", item.ToString());
                    HostName.Add(subName);
                }

                //tile map
                //object left,top,name
                for (int y = 0; y < mapY; y++)
                  {
                      for (int x = 0; x < mapX; x++)
                        {
                        int indexX = map[x, y].X;
                        int indexY = map[x, y].Y;
                        int layerIndex = map[x, y].TabIndex;
                        bool block = map[x, y].Block;
                        int weight = map[x, y].Weight;

                        XElement tileMap = new XElement("tile");
                        xRoot.Add(tileMap);
                                
                        //calculate total grid
                        int gridTotal = indexX + indexY * tileSetX + tileSetX * tileSetY * layerIndex;

                        XAttribute tileMap_grid = new XAttribute("grid", gridTotal);
                        tileMap.Add(tileMap_grid);

                        XAttribute tileMap_weight = new XAttribute("tileMap_weight", weight);
                        tileMap.Add(tileMap_weight);

                        XAttribute tileMap_block = new XAttribute("tileMap_block", block);
                        tileMap.Add(tileMap_block);

                        XAttribute tileMap_indexY = new XAttribute("indexY", indexY);
                        tileMap.Add(tileMap_indexY);

                        XAttribute tileMap_indexX = new XAttribute("indexX", indexX);
                        tileMap.Add(tileMap_indexX);

                        XAttribute tileMap_subY = new XAttribute("y", y);
                        tileMap.Add(tileMap_subY);

                        XAttribute tileMap_subX = new XAttribute("x", x);
                        tileMap.Add(tileMap_subX);

                        XAttribute tileMap_layerIndex = new XAttribute("layerIndex", layerIndex);
                        tileMap.Add(tileMap_layerIndex);                  
                        }
                     }              
            
                xRoot.Save(dlg.FileName);
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|XML Files|*.xml";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                //clear all tab first
                clearAllDataStructure();

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
 
                IEnumerable<XElement> xPaths = xRoot.Elements();

               
                int totalIndex = 0;

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

                        xCollisioin = xPath.Attribute("collision_name");
                        string collision_name = xCollisioin.Value;

                        Size tempSize = new Size(collision_size_w, collision_size_h);
                        Event_Collision_Object_Rect tempCollision =
                            new Event_Collision_Object_Rect(collision_left, collision_top, tempSize, collision_name);

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

                        Size tempSize = new Size(20, 20);
                        Event_Collision_Object_Rect tempObject =
                            new Event_Collision_Object_Rect(object_left, object_top, tempSize, object_name);

                        m_objectPt.Add(tempObject);
                        listBoxObject.Items.Add(tempObject);

                    }

                    //collision_name
                    if (xPath.Name.ToString() == "collision_name")
                    {
                        //get from subName
                        XAttribute xEvent = xPath.Attribute("SubName");

                        string SubName = xEvent.Value;
                        comboBoxCollision.Items.Add(SubName);
                        comboBoxCollision.SelectedIndex = comboBoxCollision.Items.Count - 1;
                    }

                    //event_name
                    if (xPath.Name.ToString() == "event_name")
                    {
                        //get from subName
                        XAttribute xEvent = xPath.Attribute("SubName");
                        
                        string SubName = xEvent.Value;
                        comboBoxEvent.Items.Add(SubName);
                        comboBoxEvent.SelectedIndex = comboBoxEvent.Items.Count - 1;
                    }

                    //object_name
                    if (xPath.Name.ToString() == "object_name")
                    {
                        //get from subName
                        XAttribute xEvent = xPath.Attribute("SubName");
                        
                        string SubName = xEvent.Value;
                        comboBoxObject.Items.Add(SubName);
                        comboBoxObject.SelectedIndex = comboBoxObject.Items.Count - 1;
                    }

                    //tile
                    if (xPath.Name.ToString() == "tile")
                    {
                        XAttribute xEvent = xPath.Attribute("grid");
                        XAttribute xEventBlock = xPath.Attribute("tileMap_block");
                        XAttribute xEventWeight = xPath.Attribute("tileMap_weight");
                        
                        //declare 
                        int index = new int();
                        bool block = new bool();
                        int weight = new int();

                        //index & safe check***********************
                        if (xEvent != null)
                            index = Convert.ToInt32(xEvent.Value);
                        else
                            index = 0;

                        if (xEventBlock != null)
                            block = Convert.ToBoolean(xEventBlock.Value);
                        else
                            block = false;

                        if (xEventWeight != null)
                            weight = Convert.ToInt32(xEventWeight.Value);
                        else
                            weight = 1;
                        //index & safe check***********************

                        int mapIndexX = totalIndex % mapX;
                        int mapIndexY = totalIndex / mapX;
                        totalIndex++;

                        map[mapIndexX, mapIndexY].Block = block;
                        map[mapIndexX, mapIndexY].Weight = weight;

                        //if less than -1, do need to send data anymore
                        if (index <= -1)
                            continue;

                        //another layer
                        int layer = index / (tileSetX * tileSetY);
                        
                        //index adjust
                        index -= layer * (tileSetX * tileSetY);

                        int indexX = index % tileSetX;
                        int indexY = index / tileSetX;
                  
                        map[mapIndexX, mapIndexY].TabIndex = layer;
                        map[mapIndexX, mapIndexY].X = indexX;
                        map[mapIndexX, mapIndexY].Y = indexY;
                       
                      
                    }

                }
              
            }
        }

        private void    clearAllDataStructure()
        {
            //clean the texture
            for (int i = 0; i < m_tileMap.Count; i++)
                TM.UnloadTexture(i);


            tabAsset.TabPages.Clear();
          
            m_tileMap.Clear();
         
            comboBoxCollision.Items.Clear();
            comboBoxEvent.Items.Clear();
            comboBoxObject.Items.Clear();
          
            m_collisionRect.Clear();
            m_eventRect.Clear();
            m_objectPt.Clear();

            listBoxCollision.Items.Clear();
            listBoxEvent.Items.Clear();
            listBoxObject.Items.Clear();
        }
                
        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel1.Text = e.Location.ToString() ;
        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {

        }

        private void panel3_MouseMove_1(object sender, MouseEventArgs e)
        {
            toolStripLabel1.Text = e.Location.ToString() ;
        }

        private void panel2_MouseMove_1(object sender, MouseEventArgs e)
        {
            toolStripLabel1.Text = e.Location.ToString();
        }

        private void buttonNameDelete_Click(object sender, EventArgs e)
        {
            //three data structure
            List<Event_Collision_Object_Rect> tempCollection = new List<Event_Collision_Object_Rect>();
            ListBox tempListBox = new ListBox();
            ComboBox tempComboBox = new ComboBox();

            TextBox tempTextBox = new TextBox();

            //use reference to get content
            switchHelper(ref tempCollection, ref tempListBox, ref tempComboBox, ref tempTextBox);

            if (tempComboBox.SelectedIndex == -1)
                return;

            //remove from comboBox
            int tempComboIndex =  tempComboBox.SelectedIndex;

            //get compare name
            string compareName = tempComboBox.Items[tempComboIndex].ToString();
      
            //set at least none name
            if (compareName != "None")
            {
                //remove from combo box
                tempComboBox.Items.RemoveAt(tempComboIndex);

                //update name to none
                UpdateNameHelper(ref tempCollection, ref tempListBox, compareName, "None", tempComboIndex);
            }
          

            //re select new index
            tempComboBox.SelectedIndex = tempComboBox.Items.Count - 1;
        }

        private void buttonNameAdd_Click(object sender, EventArgs e)
        {
            NameAdd();
        }

        private void NameAdd()
        {

            //three data structure
            List<Event_Collision_Object_Rect> tempCollection = new List<Event_Collision_Object_Rect>();
            ListBox tempListBox = new ListBox();
            ComboBox tempComboBox = new ComboBox();
            TextBox tempTextBox = new TextBox();

            //use reference to get content
            switchHelper(ref tempCollection, ref tempListBox, ref tempComboBox, ref tempTextBox);
        
            //add to the last
            int tempComboIndex = tempComboBox.Items.Count;

            //insert a new one
            if (tempTextBox.Text != "")
            tempComboBox.Items.Insert(tempComboIndex, tempTextBox.Text);

            //clear text
            tempTextBox.Text = "";

            //re select new index
            tempComboBox.SelectedIndex = tempComboBox.Items.Count - 1;
        }

      

        private void switchHelper(ref  List<Event_Collision_Object_Rect> tempCollection, ref ListBox tempListBox, 
            ref ComboBox tempComboBox, ref TextBox tempTextBox)
        {
            switch (m_mode)
            {
                case paintMode.collision:
                    //three data structure
                    tempListBox = listBoxCollision;
                    tempCollection = m_collisionRect;
                    tempComboBox = comboBoxCollision;
                    tempTextBox = textBoxChangeCollision;
                    break;
                case paintMode.eventTrigger:
                    //three data structure
                    tempListBox = listBoxEvent;
                    tempCollection = m_eventRect;
                    tempComboBox = comboBoxEvent;
                    tempTextBox = textBoxChangeEvent;
                    break;
                case paintMode.Object:
                    //three data structure
                    tempListBox = listBoxObject;
                    tempCollection = m_objectPt;
                    tempComboBox = comboBoxObject;
                    tempTextBox = textBoxChangeObject;
                    break;
                default:
                    break;

            } 
        }
        private void buttonUpdateName()
        {  //three data structure
            List<Event_Collision_Object_Rect> tempCollection = new List<Event_Collision_Object_Rect>();
            ListBox tempListBox = new ListBox();
            ComboBox tempComboBox = new ComboBox();
            TextBox tempTextBox = new TextBox();

            //use reference to get content
            switchHelper(ref tempCollection, ref tempListBox, ref tempComboBox, ref tempTextBox);
        
            if (tempComboBox.SelectedIndex == -1)
                return;

            int tempComboIndex = tempComboBox.SelectedIndex;

            string compareName = tempComboBox.Items[tempComboIndex].ToString();
      
            //insert a new one
            if (tempTextBox.Text != "")
            {
                tempComboBox.Items.Insert(tempComboIndex, tempTextBox.Text);

                //remove the old one
                tempComboBox.Items.RemoveAt(tempComboIndex + 1);

                //update new info
                UpdateNameHelper(ref tempCollection, ref tempListBox, compareName, tempTextBox.Text, tempComboIndex);

                //clear
                tempTextBox.Text = "";
            }

            //re select combo index
            tempComboBox.SelectedIndex = tempComboIndex;         
        }

        private void buttonCollisionUpdateName_Click(object sender, EventArgs e)
        {
            buttonUpdateName();
        }

        private void UpdateNameHelper(ref List<Event_Collision_Object_Rect> _collection, ref ListBox _listBox,
            string _compareName,string _replaceName, int _comboIndex)
        {
            for (int i = 0; i < _collection.Count; i++)
                {
                    if (_collection[i].Name == _compareName)
                    {
                        //temp Event_Collision_Object_Rect
                        Event_Collision_Object_Rect tempECO_Rect = new Event_Collision_Object_Rect();
                       
                        //copy temp Rect
                        tempECO_Rect.Rect = _collection[i].Rect;
                       
                        //copy info to the rect
                        tempECO_Rect.Name = _replaceName;
                                            
                        //insert a new one
                        _listBox.Items.Insert(i, tempECO_Rect);
                        _collection.Insert(i, tempECO_Rect);

                        //remove old one
                        _listBox.Items.RemoveAt(i+1);
                        _collection.RemoveAt(i+1);

                        if (m_mode == paintMode.collision)
                        {
                            //update the weight
                            Rectangle weightRect = new Rectangle();
                            weightRect = _collection[i].Rect;
                            addGridBlock(ref  weightRect, _comboIndex);
 
                        }
                    }
               }
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            Render();
        }

        private void splitContainer3_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }


      
      

       
     

       
      
      

      

       

        //////////////////////////////////////tool windows////////////////////////////////////

    }
}
