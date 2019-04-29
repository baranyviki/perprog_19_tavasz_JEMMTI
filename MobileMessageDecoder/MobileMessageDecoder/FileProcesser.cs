using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileMessageDecoder
{
    class FileProcesser
    {
        Dictionary<string, List<string>> wordsDict;
        Dictionary<char, int> charNumbers;
        string writeTofilePath;
        StreamWriter writer; 
        public Dictionary<string, List<string>> WordsDict { get => wordsDict; set => wordsDict = value; }
        public StreamWriter Writer { get => writer; set => writer = value; }
        public string WriteTofilePath { get => writeTofilePath; set => writeTofilePath = value; }

        public FileProcesser(string allWordsPath, string writeTofilePath)
        {
            this.writeTofilePath = writeTofilePath;
            if (File.Exists(writeTofilePath))
            {
                File.Delete(writeTofilePath);
            }
            Writer = File.AppendText(writeTofilePath);
            WordsDict = new Dictionary<string, List<string>>();
            charNumbers = NumberButtonsWithChar();

            string[] rawWords = File.ReadAllLines(allWordsPath,Encoding.UTF8);
            CleanUpWords(rawWords);
            
            foreach (var word  in rawWords)
            {
                AddToWordDict(word2Number(word), word);
            }
            Console.WriteLine("Word processing is done");
        }

        private void AddToWordDict(string key, string value)
        {
            if (WordsDict.ContainsKey(key))
            {
                WordsDict[key].Add(value);
            }
            else {
                WordsDict[key] = new List<string>();
                WordsDict[key].Add(value);
            }
        }

        private void CleanUpWords(string[] words)
        {
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = words[i].Trim().Replace("'", "");
            }
        }

        private string word2Number(string oneWord)
        {
            string key="";

            foreach (var letter in oneWord)
            {
                key += charNumbers[letter];
            }
            return key;
        }

        private Dictionary<char, int> NumberButtonsWithChar()
        {
            Dictionary<char, int> dict = new Dictionary<char, int>();
            dict.Add('a', 2);
            dict.Add('b', 2);
            dict.Add('c', 2);
            dict.Add('d', 3);
            dict.Add('e', 3);
            dict.Add('f', 3);
            dict.Add('g', 4);
            dict.Add('h', 4);
            dict.Add('i', 4);
            dict.Add('j', 5);
            dict.Add('k', 5);
            dict.Add('l', 5);
            dict.Add('m', 6);
            dict.Add('n', 6);
            dict.Add('o', 6);
            dict.Add('p', 7);
            dict.Add('q', 7);
            dict.Add('r', 7);
            dict.Add('s', 7);
            dict.Add('t', 8);
            dict.Add('u', 8);
            dict.Add('v', 8);
            dict.Add('w', 9);
            dict.Add('x', 9);
            dict.Add('y', 9);
            dict.Add('z', 9);
            return dict;
        }

        public override string ToString()
        {
            return base.ToString();
        }
        public void ToConsole()
        {
            foreach (var item in WordsDict)
            {
                Console.Write(item.Key + ": ");
                foreach (var value in item.Value)
                {
                    Console.Write(value+", ");
                }
                Console.Write("\r\n");
            }
        }
        public void WriteToFile(IEnumerable<string> message)
        {
            string s = "";
            foreach (var item in message)
            {
                s += item + "|";
            }
            s = s.TrimEnd('|');

            Writer.WriteLine(s);
        }
    }
}
