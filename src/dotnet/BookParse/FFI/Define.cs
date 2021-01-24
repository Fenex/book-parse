using System;

namespace BookParse.FFI
{
    /// Size of a UTF-8 string
    public struct StringSize
    {
        public UInt32 bytes;
        public UInt32 symbols;
    }

    public struct BookInfo
    {
        /// Количество абзацев в книге
        public UInt32 paragraphes;
        /// Количество предложений в книге
        public UInt32 sentences;
        /// Размер книги в символах и байтах (UTF-8 строка)
        public StringSize size;
    }

    public struct ParagraphInfo
    {
        /// Порядковый номер абзаца в книге
        public UInt32 index;
        /// Идентификатор (порядковый номер в книге) перевого предложения в абзаце
        public UInt32 sentence_first;
        /// Количество предложений в абзаце
        public UInt32 sentences;
        /// Размер абзаца, занимаемый в виде UTF-8 строки (байты и символы)
        public StringSize size;
    }

    public struct SentenceInfo
    {
        /// Порядковый номер предложения в книге
        public UInt32 index;
        /// Порядковый номер предложения в данном абзаце
        public UInt32 s_number;
        /// Порядковый номер абзаца, в котором находится предложение
        public UInt32 p_number;
        /// Размер предложения, занимаемый в виде UTF-8 строки (байты и символы)
        public StringSize size;
    }
}
