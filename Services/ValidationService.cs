using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Aimidge.Services
{
    public class ValidationService
    {
        public static string CheckProfanity(string prompt)
        {
            string[] prompt_parts = prompt.Split(' ');
            string[] badWords = File.ReadAllLines("Profanity.txt");
			string pattern = @"\b\p{L}*?(\p{L})\1{2,}\p{L}*\b|(?=.*[A-Za-z])(?=.*\d)";
			if (string.IsNullOrWhiteSpace(prompt)) { return prompt; }

			foreach (string word in prompt_parts)
			{
				if (Regex.Match(word, pattern).Success)
					return word;
			}


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
