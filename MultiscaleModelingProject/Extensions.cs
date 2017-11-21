using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MultiscaleModelingProject
{
    public class Extensions
    {
        public static string ColorToHexStringConverter(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static void CopyTable(Cell[,] tab1, Cell[,] tab2, int GridWidth, int GridHeight)
        {
            for (int i = 0; i < GridWidth; i++)
                for (int j = 0; j < GridHeight; j++)
                {
                    tab1[i, j].color = tab2[i, j].color;
                    tab1[i, j].flag = tab2[i, j].flag;
                }
        }

        public static bool AnyWhiteCell(Cell[,] tab1, int GridWidth, int GridHeight)
        {
            for (int i = 0; i < GridWidth; i++)
                for (int j = 0; j < GridHeight; j++)
                    if (tab1[i, j].color == "#FFFFFF")
                        return true;
            return false;
        }
    }
}
