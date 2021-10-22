using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace APIPacBomb.Model
{
    public class User
    {
        private const string INTERNAL_SECRET = "3WTql?7~GlQha332yd3K&P$%#3aE/3EoS&qe";

        /// <summary>
        ///   Nutzer-Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///   Nutzername
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///   E-Mailadresse
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///   Passwort
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///   Secret
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        ///   Vorname
        /// </summary>
        public string Prename { get; set; }

        /// <summary>
        ///   Nachname
        /// </summary>
        public string Lastname { get; set; }

        /// <summary>
        ///   Lezter Login
        /// </summary>
        public DateTime LastLogon { get; set; }

        /// <summary>
        ///   Registrierungszeitpunkt
        /// </summary>
        public DateTime RegistrationOn { get; set; }

        /// <summary>
        ///   <code>true</code>, wenn Nutzer Adminstatus hat, sonst <code>false</code>
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        ///   Erzeugt das Passwort mit Hilfe der Rohdaten und dem zufälligen Secret
        /// </summary>        
        public void GeneratePasswordHash()
        {
            Password = GeneratePasswordHash(Password, Secret);
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
