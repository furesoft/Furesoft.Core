namespace Furesoft.Core.ExpressionEvaluator.Library
{
    public class GaußMatrix
    {
        private int _lastIndex = -1;
        private double[][] rows = new double[3][];

        public void Add(double[] line)
        {
            if (_lastIndex < rows.Length)
            {
                rows[++_lastIndex] = line;
            }
        }

        public double[] Solve()
        {
            int length = rows[0].Length;

            for (int i = 0; i < rows.Length - 1; i++)
            {
                if (rows[i][i] == 0 && !Swap(rows, i, i))
                {
                    return null;
                }

                for (int j = i; j < rows.Length; j++)
                {
                    double[] d = new double[length];
                    for (int x = 0; x < length; x++)
                    {
                        d[x] = rows[j][x];
                        if (rows[j][i] != 0)
                        {
                            d[x] = d[x] / rows[j][i];
                        }
                    }
                    rows[j] = d;
                }

                for (int y = i + 1; y < rows.Length; y++)
                {
                    double[] f = new double[length];
                    for (int g = 0; g < length; g++)
                    {
                        f[g] = rows[y][g];
                        if (rows[y][i] != 0)
                        {
                            f[g] = f[g] - rows[i][g];
                        }
                    }
                    rows[y] = f;
                }
            }

            return CalculateResult(rows);
        }

        private double[] CalculateResult(double[][] rows)
        {
            double val = 0;
            int length = rows[0].Length;
            double[] result = new double[rows.Length];
            for (int i = rows.Length - 1; i >= 0; i--)
            {
                val = rows[i][length - 1];
                for (int x = length - 2; x > i - 1; x--)
                {
                    val -= rows[i][x] * result[x];
                }
                result[i] = val / rows[i][i];

                if (!IsValidResult(result[i]))
                {
                    return null;
                }
            }
            return result;
        }

        private bool IsValidResult(double result)
        {
            return !(double.IsNaN(result) || double.IsInfinity(result));
        }

        private bool Swap(double[][] rows, int row, int column)
        {
            bool swapped = false;
            for (int z = rows.Length - 1; z > row; z--)
            {
                if (rows[z][row] != 0)
                {
                    double[] temp = new double[rows[0].Length];
                    temp = rows[z];
                    rows[z] = rows[column];
                    rows[column] = temp;
                    swapped = true;
                }
            }

            return swapped;
        }
    }
}