using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APIPacBomb.Model
{
    /// <summary>
    ///   User-Klasse
    /// </summary>
    public class User
    {
        private const string INTERNAL_SECRET = "3WTql?7~GlQha332yd3K&P$%#3aE/3EoS&qe";

        /// <summary>
        ///   Nutzer-Id
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        ///   Nutzername
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        ///   E-Mailadresse
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        ///   Passwort
        /// </summary>        
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        ///   Secret
        /// </summary>
        [JsonIgnore]
        public string Secret { get; set; }

        /// <summary>
        ///   Vorname
        /// </summary>
        [JsonProperty("prename")]
        public string Prename { get; set; }

        /// <summary>
        ///   Nachname
        /// </summary>
        [JsonProperty("lastname")]
        public string Lastname { get; set; }

        /// <summary>
        ///   Lezter Login
        /// </summary>
        [JsonProperty("lastLogon")]
        public DateTime LastLogon { get; set; }

        /// <summary>
        ///   Registrierungszeitpunkt
        /// </summary>
        [JsonProperty("registrationOn")]
        public DateTime RegistrationOn { get; set; }

        /// <summary>
        ///   <code>true</code>, wenn Nutzer Adminstatus hat, sonst <code>false</code>
        /// </summary>
        [JsonProperty("isAdmin")]
        public bool IsAdmin { get; set; }

        /// <summary>
        ///   Base64-kodierter String des Nuzterbildes
        /// </summary>
        [JsonProperty("userImageBase64")]
        public string UserImageBase64 { get; set; }

        /// <summary>
        ///   WebSocket-Instanz
        /// </summary>
        [JsonIgnore]
        public System.Net.WebSockets.WebSocket WebSocket { get; set; }

        /// <summary>
        ///   Spielermodel
        /// </summary>
        [JsonIgnore]
        public Map.Bomberman Bomberman { get; set; }

        /// <summary>
        ///   Erzeugt das Passwort mit Hilfe der Rohdaten und dem zufälligen Secret
        /// </summary>        
        public void GeneratePasswordHash()
        {
            Password = GeneratePasswordHash(Password, Secret);
        }

        /// <summary>
        ///   Prüft, ob die E-Mail des Nutzers valide ist
        /// </summary>
        /// <returns>true, wenn ja, sonst false</returns>
        public bool IsMailValid()
        {
            if (Email.Trim().EndsWith("."))
            {
                return false;
            }

            if (!Regex.IsMatch(Email, @"[A-Za-z1-9\.-_]{1,}@[A-Za-z1-9\.-_]{2,}\.[A-Za-z]{2,}"))
            {
                return false;
            }

            try
            {
                System.Net.Mail.MailAddress mail = new System.Net.Mail.MailAddress(Email);
                return mail.Address == Email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///   Erzeugt das Passwort mit Hilfe der Rohdaten und dem zufälligen Secret
        /// </summary>
        /// <param name="password">Passwort in Klartext</param>
        /// <param name="secret">Zufälliges Secret</param>
        /// <returns>Gehashted Passwort</returns>
        public static string GeneratePasswordHash(string password, string secret)
        {
            return Classes.Util.GenerateHash(Classes.Util.GenerateHash(password) + secret + INTERNAL_SECRET);
        }

    }
}
