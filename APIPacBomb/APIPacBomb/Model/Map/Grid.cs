using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace APIPacBomb.Model.Map
{
    /// <summary>
    ///   Klasse zum Darstellen einer Karte
    /// </summary>
    public class Grid : IDisposable
    {
        private System.Threading.Thread _randomGenerator = null;

        /// <summary>
        ///   Wird ausgelöst, wenn ein neues Item auf der Karte erstellt wurde
        /// </summary>
        public event EventHandler<Classes.ItemGeneratedEventArgs> ItemGenerated;

        /// <summary>
        ///   Id des Spielerpaares
        /// </summary>
        [JsonIgnore]
        public Guid PlayingPairId { get; set; }

        /// <summary>
        ///   Breite
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }

        /// <summary>
        ///   Höhe
        /// </summary>
        [JsonProperty("height")]
        public int Heigth { get; set; }

        /// <summary>
        ///   Spalten mit Zeilen
        /// </summary>
        [JsonProperty("columns")]
        public List<List<Tile>> Columns { get; private set; }

        /// <summary>
        ///   Anzahl der Spalten
        /// </summary>
        [JsonProperty("columnCount")]
        public int ColumnCount { get; set; }

        /// <summary>
        ///   Anzahl der Zeilen
        /// </summary>
        [JsonProperty("rowCount")]
        public int RowCount { get; set; }

        /// <summary>
        ///   Größe der Quadrate in Pixel
        /// </summary>
        [JsonProperty("squareFactor")]
        public int SquareFactor { get; set; }

        /// <summary>
        ///   Wurde die Map bereits einmal während der Lifetime generiert
        /// </summary>
        [JsonIgnore]
        public bool IsGeneratedOnce { get; private set; }

        /// <summary>
        ///   Erstellt eine Instanz der Grid-Klasse
        /// </summary>
        /// <param name="width">Breite</param>
        /// <param name="height">Höhe</param>
        /// <param name="squareFactor">Größe der Quadrate in Pixel</param>
        public Grid(int width, int height, int squareFactor)
        {
            Width = width;
            Heigth = height;
            ColumnCount = width / squareFactor;
            RowCount = (height - 20) / squareFactor;
            SquareFactor = squareFactor;

            Columns = new List<List<Tile>>();
            IsGeneratedOnce = false;
        }

        /// <summary>
        ///   Generiert die Map
        /// </summary>
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

            IsGeneratedOnce = true;
        }

        /// <summary>
        ///   Wandelt die Instanz der Klasse in einen JSON-String
        /// </summary>
        /// <returns>JSON-String der Instanz der Klasse</returns>
        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        ///   Hintergrundprozess zum zufälligen Generieren von Items
        /// </summary>
        /// <param name="intervallMilliSec">Intervall in Millisekunden</param>
        public void StartRandomGeneration(int intervallMilliSec)
        {
            if (Columns != null && _randomGenerator == null)
            {
                _randomGenerator = new System.Threading.Thread(
                    new System.Threading.ParameterizedThreadStart(
                        (intervall) => 
                        { 
                            while (true)
                            {
                                int itemCount = 0;

                                foreach (List<Tile> column in Columns)
                                {
                                    itemCount += column.FindAll(r => r.HasItem).Count;
                                }

                                if (itemCount <= 5)
                                {
                                    Items.Gem gem = Items.Gem.GenerateRandom(this);

                                    if (gem != null)
                                    {
                                        Columns[gem.Column][gem.Row].Item = gem;
                                        ItemGenerated?.Invoke(this, new Classes.ItemGeneratedEventArgs() 
                                        { 
                                            Grid = this,
                                            NewGem = gem
                                        });
                                    }
                                }

                                System.Threading.Thread.Sleep((int)intervall);
                            }
                        }
                    )
                );

                _randomGenerator.Start(intervallMilliSec);
            }
        }

        /// <summary>
        ///   Gibt die Resourcen der Instanz wieder frei
        /// </summary>
        public void Dispose()
        {
            if (_randomGenerator != null && _randomGenerator.IsAlive)
            {
                _randomGenerator.Abort();
                _randomGenerator = null;                     
            }
        }
    }
}
