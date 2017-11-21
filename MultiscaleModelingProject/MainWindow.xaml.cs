using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MultiscaleModelingProject
{
    public partial class MainWindow : Window
    {
        private static readonly Random random = new Random();
        private static readonly string white = "#FFFFFF";

        private BackgroundWorker bg;

        private Grid spaceGrid;
        private int GridHeight;
        private int GridWidth;

        private static Cell[,] grid;
        private static Cell[,] tmp;

        private int cellId = 0;

        public MainWindow()
        {
            InitializeComponent();
            GenerateGrid();
        }

        private void GenerateGrid()
        {
            DrawGrid.Children.Clear();

            spaceGrid = new Grid();

            //wymiary tablicy
            GridHeight = 0;
            GridWidth = 0;

            int.TryParse(HeightTB.Text, out GridHeight);
            int.TryParse(WidthTB.Text, out GridWidth);

            //tworze 2 tabele dwuwymiareowe obiektów cell o wymiarach x i y równych wimarom siatki
            grid = new Cell[GridWidth, GridHeight];
            tmp = new Cell[GridWidth, GridHeight];

            //generate rows -> robię tylę rowów jaki mam rozmiar tablicy
            for (int i = 0; i < GridHeight; i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(1, GridUnitType.Star);
                spaceGrid.RowDefinitions.Add(rd);
            }

            //generate columns -> to samo tyle kolumn jaki rozmiar tablicy
            for (int i = 0; i < GridWidth; i++)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(1, GridUnitType.Star);
                spaceGrid.ColumnDefinitions.Add(cd);
            }

            for (int i = 0; i < GridWidth; i++)
            {
                for (int j = 0; j < GridHeight; j++)
                {
                    cellId++;

                    grid[i, j] = new Cell(i, j);
                    tmp[i, j] = new Cell(i, j);

                    Rectangle sp = new Rectangle();
                    Grid.SetColumn(sp, j);
                    Grid.SetRow(sp, i);

                    sp.DataContext = new Tuple<int, int>(i, j);
                    sp.Fill = new SolidColorBrush(Colors.White);
                    sp.PreviewMouseLeftButtonUp += Cell_PreviewMouseLeftButtonUp;
                    spaceGrid.Children.Add(sp);
                    grid[i, j].rectangle = sp;

                    grid[i, j].id = cellId;
                    tmp[i, j].id = cellId;
                }
            }

            DrawGrid.Children.Add(spaceGrid);
            drawResult();
        }

        private void drawResult()
        {
            //iteruje każdy kwadracik w poziomie i w pionie
            for (int i = 0; i < GridWidth; i++)
            {
                for (int j = 0; j < GridHeight; j++)
                {
                    var fgd = grid[i, j].color;

                    var xxx = (Color)ColorConverter.ConvertFromString(grid[i, j].color);

                    //ustaw dla kwadrata o współrzędnych i,j kolor
                    grid[i, j].rectangle.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(grid[i, j].color));
                }
            }

        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            // ustaw randomowo współrzędne ziaren + kolory i przekopiuj do drugiej tabeli ustawione rzeczy
            initializeNuclei();

            bg = new BackgroundWorker();
            bg.DoWork += Bg_DoWork; // ROZROST ZIAREN
            //bg.RunWorkerCompleted += Bg_RunWorkerCompleted;
            bg.WorkerReportsProgress = true;
            bg.WorkerSupportsCancellation = true;
            bg.RunWorkerAsync();
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = true;
            GenerateGrid();
        }

        private void initializeNuclei()
        {
            //Randomowo wysyp ziarna

            int nucleiAmount = int.Parse(nucleiCountTB.Text);

            int xi, yi;
            for (int i = 0; i < nucleiAmount; i++)
            {
                xi = random.Next(GridWidth);
                yi = random.Next(GridHeight);

                //INCLUSION
                //if (grid[xi, yi].isInclusion)
                //{
                //    i--;
                //    continue;
                //}

                if (grid[xi, yi].isInclusion || grid[xi, yi].isSubstructure || grid[xi, yi].isDualPhase)
                {
                    i--;
                    continue;
                }


                Color color = Color.FromRgb((byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255));
                grid[xi, yi].color = ColorToHexStringConverter(color);
            }

            //przekopiuj z tabeli do tabeli
            for (int i = 0; i < GridWidth; i++)
            {
                for (int j = 0; j < GridHeight; j++)
                {
                    tmp[i, j].color = grid[i, j].color;
                    tmp[i, j].flag = grid[i, j].flag;
                }
            }
        }

        private void Bg_DoWork(object sender, DoWorkEventArgs e)
        {
            grainGrowth();
        }


        private void Bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //nazwy przycisków w xaml

            //pauseButton.IsEnabled = false;
            //stopButton.IsEnabled = false;
        }


        private void grainGrowth()
        {
            int iteration = 0;
            int x, x1, x2, y, y1, y2;

            do
            {
                for (int i = 0; i < GridWidth; i++)
                {
                    for (int j = 0; j < GridHeight; j++)
                    {
                        x = i;
                        x1 = i - 1;
                        x2 = i + 1;

                        y = j;
                        y1 = j - 1;
                        y2 = j + 1;

                        //rozrost do granicy
                        if (i == 0)
                            x1 = 0;

                        if (i == GridWidth - 1)
                            x2 = GridWidth - 1;

                        if (j == GridHeight - 1)
                            y2 = GridHeight - 1;

                        if (j == 0)
                            y1 = 0;



                        if (tmp[x, y].color == white)
                        {
                            //extended_moore(x, x1, x2, y, y1, y2);

                            //if (VonNeumanRB.IsChecked == true)
                            vonNeumann(x, x1, x2, y, y1, y2);

                            //if (MooreRB.IsChecked == true)
                            //    extended_moore(x, x1, x2, y, y1, y2);
                            //rozrost ziaren metoda vonNeumann

                        }

                    }
                }

                //copy to table
                for (int i = 0; i < GridWidth; i++)
                {
                    for (int j = 0; j < GridHeight; j++)
                    {
                        grid[i, j].color = tmp[i, j].color;
                        grid[i, j].flag = tmp[i, j].flag;
                    }
                }

                iteration++;

                bg.ReportProgress(0);

                Dispatcher.Invoke(() => { drawResult(); });

            } while (AnyWhiteCell(grid, GridWidth, GridHeight));
        }

        private void vonNeumann(int x, int x1, int x2, int y, int y1, int y2)
        {
            //x i y analizowana komórka

            Cell c2 = grid[x, y1];
            Cell c4 = grid[x1, y];
            Cell c6 = grid[x2, y];
            Cell c8 = grid[x, y2];

            List<string> neighborhoodColorsList = new List<string>();
            if (c2.CanGrowth)
                neighborhoodColorsList.Add(c2.color);

            if (c4.CanGrowth)
                neighborhoodColorsList.Add(c4.color);

            if (c6.CanGrowth)
                neighborhoodColorsList.Add(c6.color);

            if (c8.CanGrowth)
                neighborhoodColorsList.Add(c8.color);

            var colorCounts = neighborhoodColorsList.GroupBy(c => c)
                                .Select(group => new { color = group.Key, count = group.Count() })
                                .OrderByDescending(c => c.count);

            var mostPopularColor = colorCounts.FirstOrDefault();

            if (colorCounts != null && colorCounts.Count() > 1)
                grid[x, y].isOnBorder = true;

            if (mostPopularColor != null)
                tmp[x, y].color = mostPopularColor.color;
        }

        private void extended_moore(int x, int x1, int x2, int y, int y1, int y2)
        {
            //int probability = int.Parse(ProbabilityAmountTB.Text);
            int probability = 90;

            Cell c1 = grid[x1, y1]; Cell c2 = grid[x, y1]; Cell c3 = grid[x2, y1];
            Cell c4 = grid[x1, y]; Cell c0 = grid[x, y]; Cell c6 = grid[x2, y];
            Cell c7 = grid[x1, y2]; Cell c8 = grid[x, y2]; Cell c9 = grid[x2, y2];

            #region rule1

            //Rule 1
            //jeżeli od 5 do 8 sąsiadów komórki ma takie same id to wtedy komórce dajemy to samo Id

            List<string> neighborhoodColorsList = new List<string>();

            if (c1.CanGrowth) neighborhoodColorsList.Add(c1.color);
            if (c2.CanGrowth) neighborhoodColorsList.Add(c2.color);
            if (c3.CanGrowth) neighborhoodColorsList.Add(c3.color);
            if (c4.CanGrowth) neighborhoodColorsList.Add(c4.color);
            if (c6.CanGrowth) neighborhoodColorsList.Add(c6.color);
            if (c7.CanGrowth) neighborhoodColorsList.Add(c7.color);
            if (c8.CanGrowth) neighborhoodColorsList.Add(c8.color);
            if (c9.CanGrowth) neighborhoodColorsList.Add(c9.color);

            var colorCounts = neighborhoodColorsList.GroupBy(c => c).Select(group => new { color = group.Key, count = group.Count() }).OrderByDescending(c => c.count);

            var mostPopularColor = colorCounts.FirstOrDefault();
            if (colorCounts != null && colorCounts.Count() > 1) grid[x, y].isOnBorder = true;
            if (mostPopularColor != null && mostPopularColor.count >= 5) tmp[x, y].color = mostPopularColor.color;

            #endregion
            #region rule2

            //Rule 2
            //kolor komórki ustalany jest na podstawie najbliższych sąsiedztw. Jeżeli 3 z nabliższych sąsiednich komórek ma id równe x
            //wtedy ustawiam to id tej komórce

            neighborhoodColorsList.Clear();
            colorCounts = null;

            if (c2.CanGrowth) neighborhoodColorsList.Add(c2.color);
            if (c4.CanGrowth) neighborhoodColorsList.Add(c4.color);
            if (c6.CanGrowth) neighborhoodColorsList.Add(c6.color);
            if (c8.CanGrowth) neighborhoodColorsList.Add(c8.color);

            colorCounts = neighborhoodColorsList.GroupBy(c => c).Select(group => new { color = group.Key, count = group.Count() }).OrderByDescending(c => c.count);
            mostPopularColor = colorCounts.FirstOrDefault();
            if (colorCounts != null && colorCounts.Count() > 1) grid[x, y].isOnBorder = true;
            if (mostPopularColor != null && mostPopularColor.count >= 3) tmp[x, y].color = mostPopularColor.color;

            #endregion
            #region rule3

            //Rule 3
            //kolor komórki ustalany jest na podstawie dalszych sąsiadów. Jeżeli 3 z dalszych sąsiadów mają kolor == X to

            neighborhoodColorsList.Clear();
            colorCounts = null;

            if (c1.CanGrowth) neighborhoodColorsList.Add(c1.color);
            if (c3.CanGrowth) neighborhoodColorsList.Add(c3.color);
            if (c7.CanGrowth) neighborhoodColorsList.Add(c7.color);
            if (c9.CanGrowth) neighborhoodColorsList.Add(c9.color);

            colorCounts = neighborhoodColorsList.GroupBy(c => c).Select(group => new { color = group.Key, count = group.Count() }).OrderByDescending(c => c.count);
            mostPopularColor = colorCounts.FirstOrDefault();
            if (colorCounts != null && colorCounts.Count() > 1) grid[x, y].isOnBorder = true;
            if (mostPopularColor != null && mostPopularColor.count >= 3) tmp[x, y].color = mostPopularColor.color;
            #endregion
            #region rule4

            neighborhoodColorsList.Clear();
            colorCounts = null;

            if (c1.CanGrowth) neighborhoodColorsList.Add(c1.color);
            if (c2.CanGrowth) neighborhoodColorsList.Add(c2.color);
            if (c3.CanGrowth) neighborhoodColorsList.Add(c3.color);
            if (c4.CanGrowth) neighborhoodColorsList.Add(c4.color);
            if (c6.CanGrowth) neighborhoodColorsList.Add(c6.color);
            if (c7.CanGrowth) neighborhoodColorsList.Add(c7.color);
            if (c8.CanGrowth) neighborhoodColorsList.Add(c8.color);
            if (c9.CanGrowth) neighborhoodColorsList.Add(c9.color);

            colorCounts = neighborhoodColorsList.GroupBy(c => c).Select(group => new { color = group.Key, count = group.Count() }).OrderByDescending(c => c.count);
            mostPopularColor = colorCounts.FirstOrDefault();

            //The id of particular cell depends on its all neighbors, and has X % probability chance to change.

            if (mostPopularColor != null && random.Next(1, 101) <= probability)
                tmp[x, y].color = mostPopularColor.color;
            #endregion
        }

        private void moore(int x, int x1, int x2, int y, int y1, int y2)
        {
            Cell c1 = grid[x1, y1]; Cell c2 = grid[x, y1]; Cell c3 = grid[x2, y1];
            Cell c4 = grid[x1, y]; Cell c0 = grid[x, y]; Cell c6 = grid[x2, y];
            Cell c7 = grid[x1, y2]; Cell c8 = grid[x, y2]; Cell c9 = grid[x2, y2];

            List<string> neighborhoodColorsList = new List<string>();
            if (c1.CanGrowth) neighborhoodColorsList.Add(c1.color);
            if (c2.CanGrowth) neighborhoodColorsList.Add(c2.color);
            if (c3.CanGrowth) neighborhoodColorsList.Add(c3.color);
            if (c4.CanGrowth) neighborhoodColorsList.Add(c4.color);
            if (c6.CanGrowth) neighborhoodColorsList.Add(c6.color);
            if (c7.CanGrowth) neighborhoodColorsList.Add(c7.color);
            if (c8.CanGrowth) neighborhoodColorsList.Add(c8.color);
            if (c9.CanGrowth) neighborhoodColorsList.Add(c9.color);

            //Rule 1
            //jeżeli od 5 do 8 sąsiadów komórki ma takie same id to wtedy komórce dajemy to samo Id

            var colorCounts = neighborhoodColorsList.GroupBy(c => c).Select(group => new { color = group.Key, count = group.Count() }).OrderByDescending(c => c.count);
            var mostPopularColor = colorCounts.FirstOrDefault();

            //dodaje
            if (colorCounts != null && colorCounts.Where(hhh => hhh.color == mostPopularColor.color).Count() >= 4)
            {
                grid[x, y].isOnBorder = true;
                tmp[x, y].color = mostPopularColor.color;
            }

            //Rule 2
            //kolor komórki ustalany jest na podstawie najbliższych sąsiedztw. Jeżeli 3 z nabliższych sąsiednich komórek ma id równe x
            //wtedy ustawiam to id tej komórce

            //2, 8, 4, 6
            neighborhoodColorsList.Clear();

            if (c2.CanGrowth) neighborhoodColorsList.Add(c2.color);
            if (c4.CanGrowth) neighborhoodColorsList.Add(c4.color);
            if (c6.CanGrowth) neighborhoodColorsList.Add(c6.color);
            if (c8.CanGrowth) neighborhoodColorsList.Add(c8.color);

            if (colorCounts != null && colorCounts.Where(hhh => hhh.color == mostPopularColor.color).Count() == 3)
            {
                grid[x, y].isOnBorder = true;
                tmp[x, y].color = mostPopularColor.color;
                return;
            }

            //Rule 3
            //kolor komórki ustalany jest na podstawie dalszych sąsiadów. Jeżeli 3 z dalszych sąsiadów mają kolor == X to

            //1, 3, 7, 9
            neighborhoodColorsList.Clear();
            if (c1.CanGrowth) neighborhoodColorsList.Add(c1.color);
            if (c3.CanGrowth) neighborhoodColorsList.Add(c3.color);
            if (c7.CanGrowth) neighborhoodColorsList.Add(c7.color);
            if (c9.CanGrowth) neighborhoodColorsList.Add(c9.color);

            if (colorCounts != null && colorCounts.Where(hhh => hhh.color == mostPopularColor.color).Count() == 3)
            {
                grid[x, y].isOnBorder = true;
                tmp[x, y].color = mostPopularColor.color;
                return;
            }

            //Rule 4

            //if (colorCounts != null && colorCounts.Count() > 1) grid[x, y].isOnBorder = true;
            //if (mostPopularColor != null) tmp[x, y].color = mostPopularColor.color;
        }



        //zapis zdjęć
        #region SAVE/LOAD FILES/IMAGES 

        private void importToTextBtn_Click(object sender, RoutedEventArgs e)
        {
            List<string> myList = new List<string>();
            List<string> myList2 = new List<string>();

            for (int i = 0; i < GridWidth; i++)
            {
                for (int j = 0; j < GridHeight; j++)
                {
                    string concatedString = $"{i} {j} {0} {grid[i, j].id}";
                    string concatedString2 = $"{i} {j} {0} {grid[i, j].id} {grid[i, j].color}";

                    myList.Add(concatedString);
                    myList2.Add(concatedString2);
                }
            }

            System.IO.File.WriteAllLines(@"D:\tests\WriteLines.txt", myList.ToArray());
            System.IO.File.WriteAllLines(@"D:\tests\WriteLines2.txt", myList2.ToArray());
        }

        private void LoadFromTxtBtn_Click(object sender, RoutedEventArgs e)
        {
            string[] lines = System.IO.File.ReadAllLines(@"D:\tests\WriteLines2.txt");

            foreach (var x in lines)
            {
                var words = GetWords(x);

                int xSize = Int32.Parse(words[0]);
                int ySize = Int32.Parse(words[1]);
                string color = words[4];

                grid[xSize, ySize].color = color;
            }

            drawFromTxtResult(45, 45);
        }

        private void drawFromTxtResult(int gridWith, int gridHeight)
        {
            //iteruje każdy kwadracik w poziomie i w pionie
            for (int i = 0; i < GridWidth; i++)
            {
                for (int j = 0; j < GridHeight; j++)
                {
                    var formatedStringColor = @"#" + grid[i, j].color;

                    //ustaw dla kwadrata o współrzędnych i,j kolor
                    grid[i, j].rectangle.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(formatedStringColor));
                }
            }
        }


        static string[] GetWords(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\b[\w']*\b");

            var words = from m in matches.Cast<Match>()
                        where !string.IsNullOrEmpty(m.Value)
                        select TrimSuffix(m.Value);

            return words.ToArray();
        }

        static string TrimSuffix(string word)
        {
            int apostropheLocation = word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                word = word.Substring(0, apostropheLocation);
            }

            return word;
        }


        private void SaveFrameworkElementToPng(FrameworkElement frameworkElement, int width, int height, string filePath)
        {
            BitmapImage bitmapImage = VisualToBitmapImage(frameworkElement);
            SaveImage(bitmapImage, width, height, filePath);
        }

        public BitmapImage VisualToBitmapImage(FrameworkElement frameworkElement)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)frameworkElement.ActualWidth, (int)frameworkElement.ActualHeight, 96d, 96d, PixelFormats.Default);
            rtb.Render(frameworkElement);

            MemoryStream stream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(stream);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        public void SaveImage(BitmapImage sourceImage, int width, int height, string filePath)
        {
            TransformGroup transformGroup = new TransformGroup();
            ScaleTransform scaleTransform = new ScaleTransform();
            scaleTransform.ScaleX = (double)width / sourceImage.PixelWidth;
            scaleTransform.ScaleY = (double)height / sourceImage.PixelHeight;
            transformGroup.Children.Add(scaleTransform);

            DrawingVisual vis = new DrawingVisual();
            DrawingContext cont = vis.RenderOpen();
            cont.PushTransform(transformGroup);
            cont.DrawImage(sourceImage, new Rect(new Size(sourceImage.PixelWidth, sourceImage.PixelHeight)));
            cont.Close();

            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);
            rtb.Render(vis);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(stream);
                stream.Close();
            }
        }

        private void SaveAsImageBtn_Click(object sender, RoutedEventArgs e)
        {
            string filePath = @"D:\tests\image.jpg";

            SaveFrameworkElementToPng(spaceGrid, 400, 400, filePath);
        }

        private void LoadImageBtn_Click(object sender, RoutedEventArgs e)
        {
            DrawGrid.Children.Clear();
            spaceGrid = new Grid();

            DrawGrid.Children.Add(spaceGrid);

            string ImgNameMole = @"D:\tests\image.jpg";

            ImageBrush myBrush = new ImageBrush();
            myBrush.ImageSource = new BitmapImage
            (new Uri(ImgNameMole));
            spaceGrid.Background = myBrush;
        }

        #endregion




        // KLIKAM NA KOMÓRKĘ !!!!! KLIK !!!
        // CLASS 5 -> CA
        private void Cell_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SubstructureRB.IsChecked == true)
            {

                //SUBSTRUCTURE
                Rectangle sp = sender as Rectangle;
                Tuple<int, int> dim = sp.DataContext as Tuple<int, int>; // w dim współrzędne komórki x,y -> tej której kliknęliśmy
                Cell cell = grid[dim.Item1, dim.Item2];
                cell.isSubstructure = true;

                Color color = Color.FromRgb(204, 229, 255);
                SolidColorBrush scb = new SolidColorBrush(color);

                for (int i = 0; i < GridWidth; i++)
                    for (int j = 0; j < GridHeight; j++)
                        if (grid[i, j].color == cell.color)
                        {
                            grid[i, j].isSubstructure = true;
                            tmp[i, j].isSubstructure = true;
                            grid[i, j].rectangle.Fill = scb;
                        }
            }

            if (DualPhaseRB.IsChecked == true)
            {
                //DUAL PHASE
                Rectangle sp = sender as Rectangle;
                Tuple<int, int> dim = sp.DataContext as Tuple<int, int>;
                Cell cell = grid[dim.Item1, dim.Item2];
                cell.isDualPhase = true;
                string oldColor = cell.color;
                Color color = Color.FromRgb(255, 0, 255);
                SolidColorBrush scb = new SolidColorBrush(color);

                for (int i = 0; i < GridWidth; i++)
                    for (int j = 0; j < GridHeight; j++)
                        if (grid[i, j].color == oldColor)
                        {
                            grid[i, j].color = ColorToHexStringConverter(color);
                            tmp[i, j].color = grid[i, j].color;
                            grid[i, j].isDualPhase = true;
                            tmp[i, j].isDualPhase = true;
                            grid[i, j].rectangle.Fill = scb;
                        }
            }

            if (BordersColorRB.IsChecked == true)
            {
                //COLOR boundaries
                int x, x1, x2, y, y1, y2;

                Rectangle sp = sender as Rectangle;
                Tuple<int, int> dim = sp.DataContext as Tuple<int, int>;
                Cell cell = grid[dim.Item1, dim.Item2];
                string oldColor = cell.color;
                Color color = Color.FromRgb(0, 0, 0);
                SolidColorBrush scb = new SolidColorBrush(color);

                for (int i = 0; i < GridWidth; i++)
                    for (int j = 0; j < GridHeight; j++)
                        if (grid[i, j].color == oldColor)
                        {
                            //sprawdzam sąsiadów
                            x = i;
                            x1 = i - 1;
                            x2 = i + 1;

                            y = j;
                            y1 = j - 1;
                            y2 = j + 1;

                            //rozrost do granicy
                            if (i == 0)
                                x1 = 0;

                            if (i == GridWidth - 1)
                                x2 = GridWidth - 1;

                            if (j == GridHeight - 1)
                                y2 = GridHeight - 1;

                            if (j == 0)
                                y1 = 0;

                            Cell c2 = grid[x, y1];
                            Cell c4 = grid[x1, y];
                            Cell c6 = grid[x2, y];
                            Cell c8 = grid[x, y2];

                            List<string> neighborhoodColorsList = new List<string>();

                            neighborhoodColorsList.Add(c2.color);

                            neighborhoodColorsList.Add(c4.color);

                            neighborhoodColorsList.Add(c6.color);

                            neighborhoodColorsList.Add(c8.color);

                            var colorCounts = neighborhoodColorsList.GroupBy(c => c)
                                                .Select(group => new { color = group.Key, count = group.Count() })
                                                .OrderByDescending(c => c.count);


                            if (colorCounts != null && colorCounts.Count() > 1)
                            {
                                if (colorCounts.Count() == 2 && colorCounts.Any(b => b.color == "#000000"))
                                    continue;

                                grid[i, j].color = ColorToHexStringConverter(color);
                                tmp[i, j].color = grid[i, j].color;
                                grid[i, j].isBoundaryColored = true;
                                tmp[i, j].isBoundaryColored = true;
                                grid[i, j].rectangle.Fill = scb;
                            }
                        }
            }
        }



        //czyszczenie grida po 
        private void DP_Sub_ClearGrid_Click(object sender, RoutedEventArgs e)
        {
            //***

            if (BordersColorRB.IsChecked == true)
            {
                //CZYSZCZENIE BOUNDARIES COLORING

                spaceGrid.Children.Clear();
                for (int i = 0; i < GridWidth; i++)
                    for (int j = 0; j < GridHeight; j++)
                    {
                        if (grid[i, j].isBoundaryColored)
                        {
                            spaceGrid.Children.Add(grid[i, j].rectangle);
                        }
                        else
                        {
                            grid[i, j].color = "#FFFFFF";
                            grid[i, j].flag = 0;
                            grid[i, j].isInclusion = false;
                            grid[i, j].isOnBorder = false;
                            tmp[i, j].color = "#FFFFFF";
                            tmp[i, j].flag = 0;
                            tmp[i, j].isInclusion = false;
                            tmp[i, j].isOnBorder = false;
                            grid[i, j].isBoundaryColored = false;
                            tmp[i, j].isBoundaryColored = false;

                            Rectangle sp = new Rectangle();
                            Grid.SetColumn(sp, j);
                            Grid.SetRow(sp, i);
                            sp.DataContext = new Tuple<int, int>(i, j);
                            sp.Fill = new SolidColorBrush(Colors.White);
                            sp.PreviewMouseLeftButtonUp += Cell_PreviewMouseLeftButtonUp;
                            spaceGrid.Children.Add(sp);
                            grid[i, j].rectangle = sp;
                        }
                    }
            }
            else
            {
                //CZYSZCZENIE SUBSTRUCTURES
                startButton.IsEnabled = true;
                spaceGrid.Children.Clear();
                for (int i = 0; i < GridWidth; i++)
                    for (int j = 0; j < GridHeight; j++)
                    {
                        if (grid[i, j].isSubstructure || grid[i, j].isDualPhase)
                        {
                            spaceGrid.Children.Add(grid[i, j].rectangle);
                        }
                        else
                        {
                            grid[i, j].color = "#FFFFFF";
                            grid[i, j].flag = 0;
                            grid[i, j].isInclusion = false;
                            grid[i, j].isOnBorder = false;
                            tmp[i, j].color = "#FFFFFF";
                            tmp[i, j].flag = 0;
                            tmp[i, j].isInclusion = false;
                            tmp[i, j].isOnBorder = false;

                            Rectangle sp = new Rectangle();
                            Grid.SetColumn(sp, j);
                            Grid.SetRow(sp, i);
                            sp.DataContext = new Tuple<int, int>(i, j);
                            sp.Fill = new SolidColorBrush(Colors.White);
                            sp.PreviewMouseLeftButtonUp += Cell_PreviewMouseLeftButtonUp;
                            spaceGrid.Children.Add(sp);
                            grid[i, j].rectangle = sp;
                        }
                    }
            }
        }




        private void SetInlusionBtn_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush scb = new SolidColorBrush(Colors.Black);

            int size = 4;
            int inclusionAmount = 1;
            bool gridIsBlank = !AnyColoredCell(grid, GridWidth, GridHeight);

            string inclusionColor = "#000000";
            int xi;
            int yi;

            ////DLA TESTU !!!!

            //for (int i = 0; i < GridWidth; i++)
            //{
            //    for (int j = 0; j < GridHeight; j++)
            //    {
            //        grid[i, j].isInclusion = true;
            //        tmp[i, j].isInclusion = true;
            //    }
            //}


            for (int k = 0; k < inclusionAmount; k++)
            {
                xi = random.Next(GridWidth);
                yi = random.Next(GridHeight);

                //xi = 0;
                //yi = 0;

                //
                if (grid[xi, yi].isInclusion || (!gridIsBlank && !grid[xi, yi].isOnBorder))
                {
                    k--;
                    continue;
                }

                int ii;
                int jj;

                if (InclusionCircularRB.IsChecked == true)
                {

                    //CIRCLE
                    for (int i = xi - size; i < xi + size; i++)
                    {
                        for (int j = yi - size; j < yi + size; j++)
                        {
                            if (i >= GridWidth)
                                ii = GridWidth - 1;
                            else
                            {
                                if (i < 0)
                                    ii = 0;
                                else
                                    ii = i; //bez przeszkod
                            }

                            if (j >= GridHeight)
                                jj = GridHeight - 1;
                            else
                            {
                                if (j < 0)
                                    jj = 0;
                                else
                                    jj = j;
                            }

                            //xi - randomowa wspolrzedna

                            if (grid[ii, jj].isInclusion || Math.Sqrt(Math.Pow(xi - ii, 2) + Math.Pow(yi - jj, 2)) > size)
                                continue;

                            grid[ii, jj].color = inclusionColor;
                            tmp[ii, jj].color = inclusionColor;
                            grid[ii, jj].isInclusion = true;
                            grid[ii, jj].rectangle.Fill = scb;
                        }
                    }
                }

                if (InclusionSquareRB.IsChecked == true)
                {
                    //SQUARE
                    for (int i = xi - size / 2; i < xi + size / 2; i++)
                    {
                        for (int j = yi - size / 2; j < yi + size / 2; j++)
                        {
                            if (i >= GridWidth)
                                ii = GridWidth - 1;
                            else
                            {
                                if (i < 0)
                                    ii = 0;
                                else
                                    ii = i;
                            }

                            if (j >= GridWidth)
                                jj = GridWidth - 1;
                            else
                            {
                                if (j < 0)
                                    jj = 0;
                                else
                                    jj = j;
                            }

                            if (grid[ii, jj].isInclusion)
                                continue;

                            grid[ii, jj].color = inclusionColor;
                            tmp[ii, jj].color = inclusionColor;
                            grid[ii, jj].isInclusion = true;
                            grid[ii, jj].rectangle.Fill = scb;
                        }
                    }
                }

            }
        }


        #region HELPER METHODS

        //HELPERS METHOD
        public static bool AnyWhiteCell(Cell[,] tab1, int GridWidth, int GridHeight)
        {
            for (int i = 0; i < GridWidth; i++)
                for (int j = 0; j < GridHeight; j++)
                    if (tab1[i, j].color == "#FFFFFF")
                        return true;
            return false;
        }

        public static string ColorToHexStringConverter(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static bool AnyColoredCell(Cell[,] tab1, int GridWidth, int GridHeight)
        {
            for (int i = 0; i < GridWidth; i++)
                for (int j = 0; j < GridHeight; j++)
                    if (tab1[i, j].color != "#FFFFFF")
                        //if (tab1[i, j].color != "#FFFFFF" || (tab1[i, j].color == "#000000" && tab1[i, j].isInclusion))
                        //if (tab1[i, j].color != "#FFFFFF" && (tab1[i, j].color != "#000000" && tab1[i, j].isInclusion))
                        if (tab1[i, j].color != "#FFFFFF" && tab1[i, j].color != "#000000")
                            return true;
            return false;
        }

        private void DP_Sub_DrawBoundariesBtn_Click(object sender, RoutedEventArgs e)
        {

            Color color = Color.FromRgb(0, 0, 0);
            SolidColorBrush scb = new SolidColorBrush(color);

            int x, x1, x2, y, y1, y2;

            for (int i = 0; i < GridWidth; i++)
            {
                for (int j = 0; j < GridHeight; j++)
                {
                    x = i;
                    x1 = i - 1;
                    x2 = i + 1;

                    y = j;
                    y1 = j - 1;
                    y2 = j + 1;

                    //rozrost do granicy
                    if (i == 0)
                        x1 = 0;

                    if (i == GridWidth - 1)
                        x2 = GridWidth - 1;

                    if (j == GridHeight - 1)
                        y2 = GridHeight - 1;

                    if (j == 0)
                        y1 = 0;




                    Cell c2 = grid[x, y1];
                    Cell c4 = grid[x1, y];
                    Cell c6 = grid[x2, y];
                    Cell c8 = grid[x, y2];

                    List<string> neighborhoodColorsList = new List<string>();

                    neighborhoodColorsList.Add(c2.color);

                    neighborhoodColorsList.Add(c4.color);

                    neighborhoodColorsList.Add(c6.color);

                    neighborhoodColorsList.Add(c8.color);

                    var colorCounts = neighborhoodColorsList.GroupBy(c => c)
                                        .Select(group => new { color = group.Key, count = group.Count() })
                                        .OrderByDescending(c => c.count);


                    if (colorCounts != null && colorCounts.Count() > 1)
                    {
                        grid[i, j].rectangle.Fill = scb;
                        grid[i, j].isBoundaryColored = true;
                        tmp[i, j].isBoundaryColored = true;
                    }

                }
            }





            //Color color = Color.FromRgb(255, 0, 0);
            //SolidColorBrush scb = new SolidColorBrush(color);

            //for (int i = 0; i < GridWidth; i++)
            //{
            //    for (int j = 0; j < GridHeight; j++)
            //    {
            //        var cell = grid[i, j];
            //        var cell2 = tmp[i, j];

            //        if (cell.isOnBorder || cell2.isOnBorder)
            //        {
            //            tmp[i, j].color = "#000000";
            //            grid[i, j].color = "#000000";
            //            grid[i, j].rectangle.Fill = scb;
            //            //tmp[i, j].rectangle.Fill = scb;
            //        }
            //    }
            //}
        }

        #endregion

        //private void checkBox_Checked(object sender, RoutedEventArgs e)
        //{

        //}
    }
}
