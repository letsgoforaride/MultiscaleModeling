using System.Windows.Shapes;

namespace MultiscaleModelingProject
{
    public class Cell
    {
        public int id;
        public int x, y;
        public string color = "#FFFFFF";
        public int flag = 0;
        public Rectangle rectangle = null;
        public bool isInclusion = false;
        public bool isOnBorder = false;
        public bool isBoundaryColored = false;

        public bool isSubstructure = false;
        public bool isDualPhase = false;

        public Cell(int xx, int yy)
        {
            x = xx + 1;
            y = yy + 1;
        }

        public bool CanGrowth
        {
            get
            {
                return color != "#FFFFFF" && !isInclusion && !isDualPhase && !isSubstructure;
            }
        }

    }
}
