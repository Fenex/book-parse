using System;
using System.Text;
using System.Linq;

namespace BookParse.Shell
{
    class Program
    {
        static void Parse(String str)
        {

        }

        static void Main(string[] args)
        {
            var random = new Random();
            var id = random.Next(0, TextCollection.Stories.Length - 1);

            using (var book = Book.FromUTF8(Encoding.UTF8.GetBytes(TextCollection.Stories[id])))
            {
                Console.WriteLine($"Paragraphes total: {book.Paragraphes.Count()}");
                Console.WriteLine($"Sentences total: {book.Sentences.Count()}");

                foreach (var p in book.Paragraphes)
                {
                    Console.WriteLine($"\r\nParagraph #{p.Index} (sentences: {p.Sentences.Count()} symbols: {p.Size.symbols})");
                    foreach (var s in p.Sentences)
                    {
                        Console.WriteLine($"\tSentence #{s.SentenceIndex} (symbols: {s.Size.symbols}): {s.Text}");
                    }
                }

            }

            Console.WriteLine("End.");
        }
    }
}
