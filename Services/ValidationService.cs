using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace Aimidge.Services
{
    public class ValidationService
    {
        public static string CheckProfanity(string prompt)
        {
            string[] prompt_parts = prompt.Split(' ');
            string[] badWords = File.ReadAllLines("Profanity.txt");
            foreach (string word in prompt_parts)
            {
                if (badWords.Contains(word.ToLower()))
                {
                    return word;
                }
            }
            return "ok";
        }
    }
}
