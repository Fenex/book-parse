using System;
using BookParse.FFI;

namespace BookParse
{
    /// Represents parsed single Sentence
    public class Sentence
    {
        protected readonly SentenceInfo info;

        /// Callback to get text via FFI
        private readonly Func<String> cb_text;

        // Returns text of this sentence (without cache using)
        public String Text => cb_text();

        /// Returns size of this sentence in bytes and symbols (cache using)
        public StringSize Size => info.size;

        /// Returns unique index number of this sentence in a *book*
        public uint Index => info.index;

        /// Returns index number of the *parent paragraph*
        public uint ParagraphIndex => info.p_number;

        /// Returns index number of this sentence in a *paragraph*
        public uint SentenceIndex => info.s_number;

        internal static int QQ = 0;

        internal Sentence((Func<SentenceInfo> info, Func<String> text) cb)
        {
            if (cb.info == null || cb.text == null)
                throw new BookSentenceCallbackNullException();

            info = cb.info();

            if (info.size.bytes == 0)
                throw new BookSentenceZeroSizeException();

            cb_text = cb.text;
        }
    }

    public class BookSentenceCallbackNullException : Exception { }

    public class BookSentenceZeroSizeException : Exception { }
}
