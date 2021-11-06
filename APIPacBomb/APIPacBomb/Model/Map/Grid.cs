using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace APIPacBomb.Model.Map
{
    public class Grid
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Heigth { get; set; }

        [JsonProperty("columns")]
        public List<List<Tile>> Columns { get; private set; }

        [JsonProperty("columnCount")]
        public int ColumnCount { get; set; }

        [JsonProperty("rowCount")]
        public int RowCount { get; set; }

        [JsonProperty("squareFactor")]
        public int SquareFactor { get; set; }

        public Grid(int width, int height, int squareFactor)
        {
            Width = width;
            Heigth = height;
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
                Type randomType = Type.FREE;

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
                                Type.FREE
                            )
                        );
                    }

                    // Einrandung Flagge
                    else if
                    (
                           ((k == (RowCount / 2) - 1 || k == (RowCount / 2) + 1) && (i == 1 || i == 2 || i == 3 || i == ColumnCount - 2 || i == ColumnCount - 3 || i == ColumnCount - 4))
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
                        Type t;

                        if (random.Next(1, 100) % 7 == 0)
                        {
                            t = Type.FREE;
                        }
                        else
                        {
                            t = Type.WALL;
                        }

                        if (t == Type.WALL)
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
