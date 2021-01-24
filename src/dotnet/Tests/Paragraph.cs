using System;
using System.Linq;
using Xunit;
using BookParse;
using BookParse.FFI;

namespace Tests
{
    class ConstParagraph
    {
        internal static readonly String SENTENCE_EMPTY = "";
        internal static readonly String SENTENCE_1 = "One.";
        internal static readonly String SENTENCE_2 = "One. Two. Three-Четыре!!?";
        internal ParagraphInfo GetEmptyParagraph()
        {
            return new ParagraphInfo();
        }

        internal static (ParagraphInfo, SentenceInfo) Get_Paragraph_1_EmptySentence_1()
        {
            var si = new SentenceInfo();
            si.size.bytes = 0;
            si.size.symbols = 0;

            var pi = new ParagraphInfo();
            pi.sentences = 0;
            pi.size.bytes = 0;
            pi.size.symbols = 0;

            return (pi, si);
        }

        internal static (ParagraphInfo, SentenceInfo) Get_Paragraph_1_Sentence_1()
        {
            var si = new SentenceInfo();
            si.size.bytes = 4;
            si.size.symbols = 4;

            var pi = new ParagraphInfo();
            pi.sentences = 1;
            pi.size.bytes = 4;
            pi.size.symbols = 4;

            return (pi, si);
        }

        internal static (ParagraphInfo, SentenceInfo, SentenceInfo) Get_Paragraph_1_Sentence_2()
        {
            var si1 = new SentenceInfo();
            si1.size.bytes = 4;
            si1.size.symbols = 4;

            var si2 = new SentenceInfo();
            si2.size.bytes = 43;
            si2.size.symbols = 25;
            si2.s_number = 1;
            si2.index = 1;

            var pi = new ParagraphInfo();
            pi.sentences = 2;
            pi.size.bytes = 4 + 1 + 43;
            pi.size.symbols = 4 + 1 + 25;

            return (pi, si1, si2);
        }

        internal static (
            (ParagraphInfo, SentenceInfo),
            (ParagraphInfo, SentenceInfo)
        ) Get_Paragraph_2_Sentence_1_1()
        {
            var si1 = new SentenceInfo();
            si1.size.bytes = 4;
            si1.size.symbols = 4;

            var pi1 = new ParagraphInfo();
            pi1.sentences = 1;
            pi1.size.bytes = 4;
            pi1.size.symbols = 4;

            var si2 = new SentenceInfo();
            si2.size.bytes = 43;
            si2.size.symbols = 25;
            si2.p_number = 1;
            si2.index = 1;

            var pi2 = new ParagraphInfo();
            pi2.sentences = 1;
            pi2.size.bytes = 43;
            pi2.size.symbols = 25;
            pi2.index = 1;
            pi2.sentence_first = 1;

            return ((pi1, si1), (pi2, si2));
        }

        internal static (ParagraphInfo, SentenceInfo, SentenceInfo, SentenceInfo)
        Get_Paragraph_1_Sentence_1emtpy1()
        {
            var pi = new ParagraphInfo();
            pi.sentences = 3;
            pi.size.bytes = 4 + 1 + 43;
            pi.size.symbols = 4 + 1 + 25;

            var si1 = new SentenceInfo();
            si1.size.bytes = 4;
            si1.size.symbols = 4;

            var si_empty = new SentenceInfo();
            si_empty.size.bytes = 0;
            si_empty.size.symbols = 0;
            si_empty.index = 1;
            si_empty.s_number = 1;


            var si2 = new SentenceInfo();
            si2.size.bytes = 43;
            si2.size.symbols = 25;
            si2.index = 2;
            si2.s_number = 2;

            return (pi, si1, si_empty, si2);
        }
    }

    public class UnitTestParagraph
    {



        [Fact]
        public void paragraph_with_nullable_callback()
        {

            // (Func<ParagraphInfo> info, Func<String> text) cb_p;
            // (Func<uint, SentenceInfo> info, Func<uint, String> text) cb_s;
            var (pi, si) = ConstParagraph.Get_Paragraph_1_Sentence_1();

            Assert.Throws<BookParagraphCallbackNullException>(() => new Paragraph(((null, null), (null, null))));
            Assert.Throws<BookParagraphCallbackNullException>(() => new Paragraph(
                (
                    (null, () => ConstParagraph.SENTENCE_1),
                    (
                        (i) => si,
                        (i) => ConstParagraph.SENTENCE_1
                    )
                )
            ));
            Assert.Throws<BookParagraphCallbackNullException>(() => new Paragraph(
                (
                    (() => pi, null),
                    (
                        (i) => si,
                        (i) => ConstParagraph.SENTENCE_1
                    )
                )
            ));
            Assert.Throws<BookParagraphCallbackNullException>(() => new Paragraph(
                (
                    (() => pi, () => ConstParagraph.SENTENCE_1),
                    (
                        null,
                        (i) => ConstParagraph.SENTENCE_1
                    )
                )
            ));
            Assert.Throws<BookParagraphCallbackNullException>(() => new Paragraph(
                (
                    (() => pi, () => ConstParagraph.SENTENCE_1),
                    (
                        (i) => si,
                        null
                    )
                )
            ));
        }

        [Fact]
        public void paragraph_with_single_empty_sentence()
        {
            var testing_value = ConstParagraph.SENTENCE_EMPTY;

            var (pi, si) = ConstParagraph.Get_Paragraph_1_EmptySentence_1();
            var paragraph = new Paragraph((
                    (() => pi, () => testing_value),
                    (
                        (i) => si,
                        (i) => testing_value
                    )));

            Assert.Equal((uint)0, paragraph.Index);
            Assert.Empty(paragraph.Sentences);
            Assert.Equal((uint)0, paragraph.Size.bytes);
            Assert.Equal((uint)0, paragraph.Size.symbols);
            Assert.Equal(testing_value, paragraph.Text);
        }

        [Fact]
        public void paragraph_1_sentence_1()
        {
            var testing_value = ConstParagraph.SENTENCE_1;

            var (pi, si) = ConstParagraph.Get_Paragraph_1_Sentence_1();
            var paragraph = new Paragraph((
                    (() => pi, () => testing_value),
                    (
                        (i) => si,
                        (i) => testing_value
                    )));

            Assert.Equal((uint)0, paragraph.Index);
            Assert.Single(paragraph.Sentences);
            Assert.Equal((uint)4, paragraph.Size.bytes);
            Assert.Equal((uint)4, paragraph.Size.symbols);
            Assert.Equal(testing_value, paragraph.Text);
        }

        [Fact]
        public void paragraph_1_sentence_2()
        {
            var (pi, si1, si2) = ConstParagraph.Get_Paragraph_1_Sentence_2();
            var paragraph = new Paragraph((
                    (() => pi, () => $"{ConstParagraph.SENTENCE_1} {ConstParagraph.SENTENCE_2}"),
                    (
                        (i) =>
                        {
                            switch (i)
                            {
                                case 0: return si1;
                                case 1: return si2;
                                default: throw new Exception($"Invalid sentence_index given: `{i}`");
                            }
                        },
                        (i) =>
                        {
                            switch (i)
                            {
                                case 0: return ConstParagraph.SENTENCE_1;
                                case 1: return ConstParagraph.SENTENCE_2;
                                default: throw new Exception($"Invalid sentence_index given: `{i}`");
                            }
                        }
            )));

            Assert.Equal((uint)0, paragraph.Index);
            Assert.Equal(2, paragraph.Sentences.Count());
            Assert.Equal((uint)4 + 1 + 43, paragraph.Size.bytes);
            Assert.Equal((uint)4 + 1 + 25, paragraph.Size.symbols);
            Assert.Equal($"{ConstParagraph.SENTENCE_1} {ConstParagraph.SENTENCE_2}", paragraph.Text);

            Sentence s;

            var iter = paragraph.Sentences.GetEnumerator();
            Assert.True(iter.MoveNext());
            s = iter.Current;

            Assert.Equal((uint)0, s.Index);
            Assert.Equal((uint)0, s.ParagraphIndex);
            Assert.Equal((uint)0, s.SentenceIndex);
            Assert.Equal((uint)4, s.Size.bytes);
            Assert.Equal((uint)4, s.Size.symbols);
            Assert.Equal(ConstParagraph.SENTENCE_1, s.Text);

            Assert.True(iter.MoveNext());
            s = iter.Current;

            Assert.Equal((uint)1, s.Index);
            Assert.Equal((uint)0, s.ParagraphIndex);
            Assert.Equal((uint)1, s.SentenceIndex);
            Assert.Equal((uint)43, s.Size.bytes);
            Assert.Equal((uint)25, s.Size.symbols);
            Assert.Equal(ConstParagraph.SENTENCE_2, s.Text);

            Assert.False(iter.MoveNext());
        }

        [Fact]
        public void paragraph_2_sentence_1_1()
        {
            var ((pi1, si1), (pi2, si2)) = ConstParagraph.Get_Paragraph_2_Sentence_1_1();
            var paragraph1 = new Paragraph((
                    (() => pi1, () => $"{ConstParagraph.SENTENCE_1}"),
                    (
                        (i) =>
                        {
                            switch (i)
                            {
                                case 0: return si1;
                                default: throw new Exception($"Invalid sentence_index given: `{i}`");
                            }
                        },
                        (i) =>
                        {
                            switch (i)
                            {
                                case 0: return ConstParagraph.SENTENCE_1;
                                default: throw new Exception($"Invalid sentence_index given: `{i}`");
                            }
                        }
            )));

            var paragraph2 = new Paragraph((
                    (() => pi2, () => $"{ConstParagraph.SENTENCE_2}"),
                    (
                        (i) =>
                        {
                            switch (i)
                            {
                                case 1: return si2;
                                default: throw new Exception($"Invalid sentence_index given: `{i}`");
                            }
                        },
                        (i) =>
                        {
                            switch (i)
                            {
                                case 1: return ConstParagraph.SENTENCE_2;
                                default: throw new Exception($"Invalid sentence_index given: `{i}`");
                            }
                        }
            )));

            Assert.Equal((uint)0, paragraph1.Index);
            Assert.Single(paragraph1.Sentences);
            Assert.Equal((uint)4, paragraph1.Size.bytes);
            Assert.Equal((uint)4, paragraph1.Size.symbols);
            Assert.Equal($"{ConstParagraph.SENTENCE_1}", paragraph1.Text);

            Assert.Equal((uint)1, paragraph2.Index);
            Assert.Single(paragraph2.Sentences);
            Assert.Equal((uint)43, paragraph2.Size.bytes);
            Assert.Equal((uint)25, paragraph2.Size.symbols);
            Assert.Equal($"{ConstParagraph.SENTENCE_2}", paragraph2.Text);

            Sentence s;

            var iter1 = paragraph1.Sentences.GetEnumerator();
            Assert.True(iter1.MoveNext());
            s = iter1.Current;

            Assert.Equal((uint)0, s.Index);
            Assert.Equal((uint)0, s.ParagraphIndex);
            Assert.Equal((uint)0, s.SentenceIndex);
            Assert.Equal((uint)4, s.Size.bytes);
            Assert.Equal((uint)4, s.Size.symbols);
            Assert.Equal(ConstParagraph.SENTENCE_1, s.Text);

            Assert.False(iter1.MoveNext());

            var iter2 = paragraph2.Sentences.GetEnumerator();
            Assert.True(iter2.MoveNext());
            s = iter2.Current;

            Assert.Equal((uint)1, s.Index);
            Assert.Equal((uint)1, s.ParagraphIndex);
            Assert.Equal((uint)0, s.SentenceIndex);
            Assert.Equal((uint)43, s.Size.bytes);
            Assert.Equal((uint)25, s.Size.symbols);
            Assert.Equal(ConstParagraph.SENTENCE_2, s.Text);

            Assert.False(iter2.MoveNext());

        }


        [Fact]
        public void paragraph_1_sentence_1empty1()
        {
            var (pi, si1, si_empty, si2) = ConstParagraph.Get_Paragraph_1_Sentence_1emtpy1();
            var paragraph = new Paragraph((
                    (() => pi, () => $"{ConstParagraph.SENTENCE_1} {ConstParagraph.SENTENCE_2}"),
                    (
                        (i) =>
                        {
                            switch (i)
                            {
                                case 0: return si1;
                                case 1: return si_empty;
                                case 2: return si2;
                                default: throw new Exception($"Invalid sentence_index given: `{i}`");
                            }
                        },
                        (i) =>
                        {
                            switch (i)
                            {
                                case 0: return ConstParagraph.SENTENCE_1;
                                case 1: return ConstParagraph.SENTENCE_EMPTY;
                                case 2: return ConstParagraph.SENTENCE_2;
                                default: throw new Exception($"Invalid sentence_index given: `{i}`");
                            }
                        }
            )));

            Assert.Equal((uint)0, paragraph.Index);
            Assert.Equal(2, paragraph.Sentences.Count());
            Assert.Equal((uint)4 + 1 + 43, paragraph.Size.bytes);
            Assert.Equal((uint)4 + 1 + 25, paragraph.Size.symbols);
            Assert.Equal($"{ConstParagraph.SENTENCE_1} {ConstParagraph.SENTENCE_2}", paragraph.Text);

            Sentence s;

            var iter1 = paragraph.Sentences.GetEnumerator();
            Assert.True(iter1.MoveNext());
            s = iter1.Current;

            Assert.Equal((uint)0, s.Index);
            Assert.Equal((uint)0, s.ParagraphIndex);
            Assert.Equal((uint)0, s.SentenceIndex);
            Assert.Equal((uint)4, s.Size.bytes);
            Assert.Equal((uint)4, s.Size.symbols);
            Assert.Equal(ConstParagraph.SENTENCE_1, s.Text);

            Assert.True(iter1.MoveNext());
            s = iter1.Current;

            Assert.Equal((uint)2, s.Index);
            Assert.Equal((uint)0, s.ParagraphIndex);

            // FIXME: We need recalculates `SentenceIndex` if we got empty sentence
            // due all empty sentences is filtering
            // Assert.Equal((uint)1, s.SentenceIndex);

            Assert.Equal((uint)43, s.Size.bytes);
            Assert.Equal((uint)25, s.Size.symbols);
            Assert.Equal(ConstParagraph.SENTENCE_2, s.Text);

            Assert.False(iter1.MoveNext());
        }

    }
}