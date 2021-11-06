using System;
using System.Collections.Generic;

namespace APIPacBomb.Model.Map
{
    public class Grid
    {
        public List<List<Tile>> Columns { get; private set; }

        public int ColumnCount { get; set; }

        public int RowCount { get; set; }

        public int SquareFactor { get; set; }

        public Grid(int width, int height, int squareFactor)
        {
            ColumnCount = width / squareFactor;
            RowCount = (height - 20) / squareFactor;
            SquareFactor = squareFactor;

            Columns = new List<List<Tile>>();
        }

        public void GenerateMap()
        {
            List<Tile> row;

            for (int i = 0; i < this.ColumnCount; i++)
            {
                Random random = new Random();
                Type randomType = Type.Free;

                row = new List<Tile>();

                for (int k = 0; k < RowCount; k++)
                {
                    // Erste und letzte Spalte und erste und letzte Zeile immer frei
                    if
                    (
                           i == 0
                        || i == ColumnCount - 1
                        || k == 0
                        || k == RowCount - 1
                        || (k == RowCount / 2 && (i == 1 || i == 2 || i == ColumnCount - 2 || i == ColumnCount - 3))
                    )
                    {
                        row.Add(
                            new Tile(
                                new Coord(i * SquareFactor, (k + 1) * SquareFactor),
                                SquareFactor,
                                Type.Free
                            )
                        );
                    }

                    // Einrandung Flagge
                    else if
                    (
                           (k == (RowCount / 2) - 1 || k == (RowCount / 2) + 1 && (i == 1 || i == 2 || i == 3 || i == ColumnCount - 2 || i == ColumnCount - 3 || i == ColumnCount - 4))
                        || (k == RowCount / 2 && (i == 3 || i == ColumnCount - 4))
                    )
                    {
                        row.Add(
                            new Wall(
                                new Coord(i * SquareFactor, (k + 1) * SquareFactor),
                                SquareFactor,
                                false
                            )
                        );
                    }

                    // Rest zufällig
                    else
                    {
                        Type t = randomType.Random();

                        if (t == Type.Wall)
                        {
                            row.Add(
                                new Wall(
                                    new Coord(i * SquareFactor, (k + 1) * SquareFactor),
                                    SquareFactor,
                                    random.Next(1, 500) % 2 == 0
                                )
                            );
                        }
                        else
                        {
                            row.Add(
                                new Tile(
                                    new Coord(i * SquareFactor, (k + 1) * SquareFactor),
                                    SquareFactor,
                                    t
                                )
                            );
                        }
                    }
                }

                Columns.Add(row);
            }
        }
    }
}
