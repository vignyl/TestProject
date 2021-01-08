using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TestProject.Models
{
    public class QRCodeModel
    {
        [Display(Name = "QRCode Text")]
        public string QRCodeText { get; set; }

        [Display(Name = "QRCode Image")]
        public string QRCodeImagePath { get; set; }


        public static int TOKEN_LENGHT = 20;
        public static string GenerateRandomString(int RequiredLength = 8, int RequiredUniqueChars = 4,
            bool RequireDigit = true, bool RequireLowercase = true,
            bool RequireNonAlphanumeric = true, bool RequireUppercase = true)
        {
            if (RequiredLength == 0) RequiredLength = TOKEN_LENGHT;
            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < RequiredLength
                || chars.Distinct().Count() < RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
        public static String SAVE_PATH = "~/Images/";

        public static double randomDouble(double start, double end)
        {
            Random rand = new Random();
            return (rand.NextDouble() * Math.Abs(end - start)) + start;
        }
    }
}