using System;
using System.Text;
using Xunit;
using BookParse;
using BookParse.FFI;

namespace Tests
{
    class ConstSentences
    {
        internal const String SENTENCE_EMPTY = "";
        internal const String SENTENCE_1 = "One.";
        internal const String SENTENCE_2 = "One two, three. Четыре-five!!!";

        internal static byte[] AsBytes(String str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        internal static (byte[], SentenceInfo) AsSentenceInfo(String str)
        {
            byte[] buff = AsBytes(str);

            SentenceInfo si = new SentenceInfo();
            si.size.bytes = (uint)buff.Length;
            si.size.symbols = (uint)str.Length;

            return (buff, si);
        }
    }

    public class UnitTestSentence
    {
        [Fact]
        public void SentenceEmpty()
        {
            var (_, si) = ConstSentences.AsSentenceInfo(ConstSentences.SENTENCE_EMPTY);

            (Func<SentenceInfo>, Func<String>) cb = (() => si, () => ConstSentences.SENTENCE_EMPTY);
            Assert.Throws<BookSentenceZeroSizeException>(() => new Sentence(cb));
        }

        [Fact]
        public void CallbackIsNull()
        {
            var (_, si) = ConstSentences.AsSentenceInfo(ConstSentences.SENTENCE_EMPTY);
            (Func<SentenceInfo>, Func<String>) cb;

            cb = (null, () => ConstSentences.SENTENCE_EMPTY);
            Assert.Throws<BookSentenceCallbackNullException>(() => new Sentence(cb));

            cb = (null, null);
            Assert.Throws<BookSentenceCallbackNullException>(() => new Sentence(cb));

            cb = (() => ConstSentences.AsSentenceInfo(ConstSentences.SENTENCE_EMPTY).Item2, null);
            Assert.Throws<BookSentenceCallbackNullException>(() => new Sentence(cb));

            cb = (() => ConstSentences.AsSentenceInfo(ConstSentences.SENTENCE_1).Item2, null);
            Assert.Throws<BookSentenceCallbackNullException>(() => new Sentence(cb));
        }

        [Fact]
        public void Sentence_1()
        {
            var testing_value = ConstSentences.SENTENCE_1;

            var (buff, si) = ConstSentences.AsSentenceInfo(testing_value);
            (Func<SentenceInfo>, Func<String>) cb;
            cb = (() => si, () => testing_value);
            var sentence = new Sentence(cb);

            Assert.Equal((uint)0, sentence.Index);
            Assert.Equal((uint)0, sentence.ParagraphIndex);
            Assert.Equal((uint)0, sentence.SentenceIndex);
            Assert.Equal(((uint)buff.Length), sentence.Size.bytes);
            Assert.Equal(((uint)testing_value.Length), sentence.Size.symbols);
            Assert.Equal(testing_value, sentence.Text);

            si.index = 1;
            si.p_number = 1;
            si.s_number = 1;

            /* No changes: */
            Assert.Equal((uint)0, sentence.Index);
            Assert.Equal((uint)0, sentence.ParagraphIndex);
            Assert.Equal((uint)0, sentence.SentenceIndex);
            Assert.Equal(((uint)buff.Length), sentence.Size.bytes);
            Assert.Equal(((uint)testing_value.Length), sentence.Size.symbols);
            Assert.Equal(testing_value, sentence.Text);

            /* Create new Sentence from updated values and check for changes: */
            sentence = new Sentence(cb);
            Assert.Equal((uint)1, sentence.Index);
            Assert.Equal((uint)1, sentence.ParagraphIndex);
            Assert.Equal((uint)1, sentence.SentenceIndex);
            Assert.Equal(((uint)buff.Length), sentence.Size.bytes);
            Assert.Equal(((uint)testing_value.Length), sentence.Size.symbols);
            Assert.Equal(testing_value, sentence.Text);
        }
        [Fact]
        public void Sentence_2()
        {
            var testing_value = ConstSentences.SENTENCE_2;

            var (buff, si) = ConstSentences.AsSentenceInfo(testing_value);
            (Func<SentenceInfo>, Func<String>) cb;
            cb = (() => si, () => testing_value);
            var sentence = new Sentence(cb);

            Assert.Equal((uint)0, sentence.Index);
            Assert.Equal((uint)0, sentence.ParagraphIndex);
            Assert.Equal((uint)0, sentence.SentenceIndex);
            Assert.Equal(((uint)buff.Length), sentence.Size.bytes);
            Assert.Equal(((uint)testing_value.Length), sentence.Size.symbols);
            Assert.Equal(testing_value, sentence.Text);

            si.index = 333;
            si.p_number = 333;
            si.s_number = 333;

            /* No changes: */
            Assert.Equal((uint)0, sentence.Index);
            Assert.Equal((uint)0, sentence.ParagraphIndex);
            Assert.Equal((uint)0, sentence.SentenceIndex);
            Assert.Equal(((uint)buff.Length), sentence.Size.bytes);
            Assert.Equal(((uint)testing_value.Length), sentence.Size.symbols);
            Assert.Equal(testing_value, sentence.Text);

            /* Create new Sentence from updated values and check for changes: */
            sentence = new Sentence(cb);
            Assert.Equal((uint)333, sentence.Index);
            Assert.Equal((uint)333, sentence.ParagraphIndex);
            Assert.Equal((uint)333, sentence.SentenceIndex);
            Assert.Equal(((uint)buff.Length), sentence.Size.bytes);
            Assert.Equal(((uint)testing_value.Length), sentence.Size.symbols);
            Assert.Equal(testing_value, sentence.Text);
        }

    }
}
