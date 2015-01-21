using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



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


        int mapX = 5;
        int mapY = 5;

        int tileSetX = 10;
        int tileSetY = 10;

        int tileWidth = 32;
        int tileHeigth = 32;

        //set-up the initial 
        //The current selected tile
        Tile selectedTile;
        Tile stampSelectedTile;
        //The mouse move
        Tile hoverTile;
        //map data
        Size mapSizeSet = new Size (5, 5);

        //tile grid
        Size tileSizeSet = new Size(5, 5);

        //tile data
        Size tileSize = new Size(32, 32);

        //an 5x5 tile array 
        Tile[,] map = new Tile[5, 5];
      
       
        //for stamp effect
        bool mouseAtTileSet = false;


        public Form1()
        {
            InitializeComponent();

            for (int x = 0; x < mapX; x++)
            {
                for (int y = 0; y < mapY; y++)
                {
                    map[x, y].X = -1;
                    map[x, y].Y = -1;
                }
            }
            D3D.Initialize(panel1, true);
            D3D.AddRenderTarget(panel2);
            D3D.AddRenderTarget(panel3);
            TM.Initialize(D3D.Device, D3D.Sprite);

            TextureID = TM.LoadTexture("testmap3.bmp");
            panel1.AutoScrollMinSize = new Size(mapX * tileWidth, mapY * tileHeigth);
            panel2.AutoScrollMinSize = new Size(TM.GetTextureWidth(TextureID), TM.GetTextureHeight(TextureID));
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
                    TM.Draw(TextureID, x * tileWidth + offset.X, y * tileHeigth + offset.Y, 1, 1, src);

                }
            }

            //render the mouse location
            src.X = selectedTile.X * tileWidth;
            src.Y = selectedTile.Y * tileHeigth;
            src.Size = new Size(tileWidth, tileHeigth);
            TM.Draw(TextureID, hoverTile.X * tileWidth, hoverTile.Y * tileHeigth, 1, 1, src);


            D3D.SpriteEnd();
            D3D.DeviceEnd();
            D3D.Present();
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

           
            for (int x = selectedTile.X; x <= stampSelectedTile.X; x++)
            {
                for (int y = selectedTile.Y; y <= stampSelectedTile.Y; y++)
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

        private int minor()
       {}
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
                  
                    int scaleX = 2;
                    int scaleY = 2;
                    TM.Draw(TextureID, x * tileWidth / scaleX, y * tileHeigth / scaleY, 1 / (float)scaleX, 1 / (float)scaleY, src);

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

            if (e.Location.X >= tileSizeSet.Width * tileSize.Width || e.Location.X <= 0 ||
                e.Location.Y >= tileSizeSet.Height * tileSize.Height || e.Location.Y <= 0)
                return true;

            return false;
        }

       

        //safe check whether map out of range
        bool outOfRangeMap(MouseEventArgs e)
        {

            if (e.Location.X >= mapSizeSet.Width * tileSize.Width || e.Location.X <= 0 ||
                e.Location.Y >= mapSizeSet.Height * tileSize.Height || e.Location.Y <= 0)
                return true;

            return false;
        }

        //Single click to know where is the map mouse location
        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (outOfRangeMap(e))
                return;

            //Caculate where is the mouse
            int x = (e.Location.X - panel1.AutoScrollPosition.X) / tileSize.Width; //0~5(default)
            int y = (e.Location.Y - panel1.AutoScrollPosition.Y) / tileSize.Height;//0~5(default)

            //Set the selected tile in the map, put the selected result in the map array
            map[x, y] = selectedTile;

        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            //Caculate where is the mouse
            int x = (e.Location.X - panel1.AutoScrollPosition.X) / tileSize.Width; //0~5(default)
            int y = (e.Location.Y - panel1.AutoScrollPosition.Y) / tileSize.Height;//0~5(default)

            hoverTile.X = x;
            hoverTile.Y = y;

            //left click for continuous draw
            if (e.Button == MouseButtons.Left)
            {
                //safe check
                if (outOfRangeMap(e))
                    return;

                //safe check
                if (x > mapSizeSet.Width)
                    x = mapSizeSet.Width - 1;
                else if (x < 0)
                    x = 0;

                if (y > mapSizeSet.Height)
                    y = mapSizeSet.Height - 1;
                else if (y < 0)
                    y = 0;

                //Set the selected tile in the map, put the selected result in the 5x5 map array
                map[x, y] = selectedTile;

            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void resizeMapToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void panel2_MouseClick(object sender, MouseEventArgs e)
        {
           

        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            //if mouse click and move, need to do something here
            if (mouseAtTileSet == true)
            {

                //get the mouse tile location
                int x = (e.Location.X - panel2.AutoScrollPosition.X) / tileSize.Width;//0~5
                int y = (e.Location.Y - panel2.AutoScrollPosition.Y) / tileSize.Height;//0~1

                //safe check
                if (x > mapSizeSet.Width)
                    x = mapSizeSet.Width - 1;
                else if (x < 0)
                    x = 0;

                if (y > mapSizeSet.Height)
                    y = mapSizeSet.Height - 1;
                else if (y < 0)
                    y = 0;

                //mouse up tile is the same as selectedTile, so no need to stamp
                if (x == selectedTile.X && y == selectedTile.Y)
                    return;

                //send the x and y postion to stampSelectedTile
                stampSelectedTile.X = x;
                stampSelectedTile.Y = y;

              //  calculateStamp();
            }
        }

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            //no need to do anything if it's not available action
            if (mouseAtTileSet != true)
                return;
          
            mouseAtTileSet = false;
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

        }
        //select tile first
        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (outOfRangeTile(e))
                return;

            //get the mouse location at selectedTile
            selectedTile.X = (e.Location.X - panel2.AutoScrollPosition.X) / tileSize.Width;//0~5
            selectedTile.Y = (e.Location.Y - panel2.AutoScrollPosition.Y) / tileSize.Height;//0~1

            //send the mouse info to stampSelectedTile
            stampSelectedTile.X = selectedTile.X;
            stampSelectedTile.Y = selectedTile.Y;

            mouseAtTileSet = true;
        }

      
    }
}
