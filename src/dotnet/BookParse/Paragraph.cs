using System;
using System.Collections.Generic;
using BookParse.FFI;

namespace BookParse
{
    /// Represents parsed single paragraph
    public class Paragraph
    {
        /// Storages native info about this parsed paragraph (given from FFI)
        protected readonly ParagraphInfo info;

        /// Callback to get text via FFI
        private readonly Func<String> cb_text;

        /// Callbacks to get sentence info & text via FFI
        private readonly (Func<uint, SentenceInfo> info, Func<uint, String> text) cb_sentences;

        /// Returns text of the paragraph (without cache using)
        public String Text => cb_text();

        /// Returns size of the paragraph in bytes and symbols (cache using)
        public StringSize Size => info.size;

        /// Returns unique index of the paragraph
        public uint Index => info.index;

        // /// Returns count of sentences inside this paragraph
        // public uint sentences_count => info.sentences;

        private Dictionary<uint, Sentence> sentences = new Dictionary<uint, Sentence>();
        public IEnumerable<Sentence> Sentences
        {
            get
            {

                for (uint i = 0; i < info.sentences; i++)
                {
                    uint index = i + info.sentence_first;
                    if (!sentences.ContainsKey(index))
                    {
                        Func<uint, (Func<SentenceInfo>, Func<String>)> callback =
                            (i) => (
                                () => cb_sentences.info(i),
                                () => cb_sentences.text(i));

                        try
                        {
                            sentences[index] = new Sentence(callback(index));
                        }
                        catch (BookSentenceZeroSizeException)
                        {
                            Console.Error.WriteLine($"sentence with index `{index}` has a zero size");
                            continue;
                        }
                    }

                    yield return sentences[index];
                }
            }
        }

        internal Paragraph(
            (
                (
                    Func<ParagraphInfo> info,
                    Func<String> text
                ) paragraph,
                (
                    Func<uint, SentenceInfo> info,
                    Func<uint, String> text
                ) sentences
            ) cb
        )
        {
            if (cb.paragraph.info == null
             || cb.paragraph.text == null
             || cb.sentences.text == null
             || cb.sentences.info == null)
                throw new BookParagraphCallbackNullException();

            info = cb.paragraph.info();
            cb_text = cb.paragraph.text;
            cb_sentences = cb.sentences;
        }
    }

    public class BookParagraphCallbackNullException : Exception { }
}
