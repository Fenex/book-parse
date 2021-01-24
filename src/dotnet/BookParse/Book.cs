using System;
using System.Collections.Generic;
using System.Linq;
using BookParse.FFI;

namespace BookParse
{
    public class Book : IDisposable
    {

        /// Storages a pointer to object that contains all data of parsed text.
        /// All
        protected readonly IntPtr _p_book = IntPtr.Zero;

        public IntPtr p_book
        {
            get
            {
                if (_p_book == IntPtr.Zero)
                {
                    throw new UninitBookException();
                }
                return _p_book;
            }
        }

        private protected readonly BookInfo Info;

        private Dictionary<uint, Paragraph> paragraphes = new Dictionary<uint, Paragraph>();
        public IEnumerable<Paragraph> Paragraphes
        {
            get
            {
                for (uint i = 0; i < Info.paragraphes; i++)
                {
                    if (!paragraphes.ContainsKey(i))
                    {
                        var pi = FFI.Binding.ParagraphInfo(p_book, i);
                        if (pi.index != i)
                            throw new BookParagraphFetchInfoException(i);

                        Func<uint, ((Func<ParagraphInfo>, Func<String>), (Func<uint, SentenceInfo>, Func<uint, String>))> callbacks =
                            (i) => (
                                (
                                    () => FFI.Binding.ParagraphInfo(p_book, i),
                                    () => FFI.Binding.ParagraphText(p_book, i)),
                                (
                                    (n) => FFI.Binding.SentenceInfo(p_book, n),
                                    (n) => FFI.Binding.SentenceText(p_book, n)));

                        paragraphes[i] = new Paragraph(callbacks(i));
                    }

                    yield return paragraphes[i];
                }
            }
        }

        public IEnumerable<Sentence> Sentences => Paragraphes.SelectMany(p => p.Sentences);

        Book(IntPtr pBook)
        {
            _p_book = pBook;

            Info = Binding.Info(p_book);
            if (Info.paragraphes == 0)
                throw new BookIsEmptyException(); // book is empty
        }

        static public Book FromUTF8(byte[] str_as_utf8)
        {
            IntPtr pBook = IntPtr.Zero;

            unsafe
            {
                fixed (byte* p = str_as_utf8)
                {
                    pBook = FFI.Binding.FromUTF8((IntPtr)p, (UInt32)str_as_utf8.LongLength);
                }
            }

            return new Book(pBook);
        }

        public void Dispose()
        {
            if (_p_book != IntPtr.Zero)
            {
                FFI.Binding.Dispose(_p_book);
            }
        }

    }

    public class BookParagraphFetchInfoException : Exception
    {
        public BookParagraphFetchInfoException(uint p) : base($"Error occurred with paragraph {p}") { }
    }

    public class UninitBookException : Exception
    {
        public UninitBookException() : base("The book is not initialized")
        {
        }
    }

    public class BookIsEmptyException : Exception
    {

    }
}
