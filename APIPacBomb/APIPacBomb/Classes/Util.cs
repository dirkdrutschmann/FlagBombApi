using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace APIPacBomb.Classes
{
    /// <summary>
    ///   Helferklasse
    /// </summary>
    public class Util
    {
        /// <summary>
        ///   Bezeichnung des CUSTOM-JWT-Claim
        /// </summary>
        public const string CLAIM_TYPE = "uname";

        /// <summary>
        ///   Generiert einen SHA256 Hash
        /// </summary>
        /// <param name="rawData">Zeichenkette, die gehasht werden soll</param>
        /// <returns>Gehashte Zeichenkette</returns>
        public static string GenerateHash(string rawData)
        {
            using (SHA256 hash = SHA256.Create())
            {
                byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        ///   Ließt den Nutzernamen aus einen JWT
        /// </summary>
        /// <param name="context">HTTP-Kontext</param>
        /// <returns>Nutzername, wenn Claim im JWT gesetzt, sonst <code>string.Empty</code></returns>
        public static string GetUsernameFromToken(HttpContext context)
        {
            System.Security.Claims.Claim username = context.User.Claims.First(p => p.Type == CLAIM_TYPE);

            // Nur gesetzt wenn User eingeloggt ist
            if (username == null)
            {
                return string.Empty;
            }

            return username.Value;
        }
    }
}
