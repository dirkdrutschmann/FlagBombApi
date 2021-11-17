using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace APIPacBomb.Model.Map
{
    /// <summary>
    ///   Spielerfarben
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlayerColor
    {
        /// <summary>
        ///   Blau
        /// </summary>
        BLUE,

        /// <summary>
        ///   Rot
        /// </summary>
        RED,

        /// <summary>
        ///   Grün
        /// </summary>
        GREEN,

        /// <summary>
        ///   Gelb
        /// </summary>
        YELLOW
    }
}
