typedef void *Book;
typedef unsigned int SentenceId;
typedef unsigned int ParagraphId;

// Represents size of UTF-8 strings
typedef struct
{
    unsigned int bytes;
    unsigned int symbols;
} StringSize;

typedef struct
{
    unsigned int paragraphes;
    unsigned int sentences;
    StringSize size;
} BookInfo;

typedef struct
{
    ParagraphId index;
    SentenceId sentence_first;
    unsigned int sentences;
    StringSize size;
} ParagraphInfo;

typedef struct
{
    SentenceId index;
    unsigned int s_number;
    ParagraphId p_index;
    StringSize size;
} SentenceInfo;

extern "C"
{
    // Returns a `Book` that representes parsed text.
    // * `p` - pointer to first byte of a text in UTF-8 encoding
    // * `len` - length of the text in bytes.
    Book from_utf8(unsigned char *p, unsigned int len);

    // Dispose all data. Must be called when `Book` is no longer need.
    void dispose(Book book);

    // Returns basic info of the parsed text.
    BookInfo book_info(Book book);

    // Returns basic info of the parsed paragraph by its unique `index`.
    ParagraphInfo paragraph_info(Book book, ParagraphId index);

    // Writes a text of the parsed paragraph by its unique `index` to `buff`.
    // Required `buff` size can be given by calling `paragraph_info` function.
    void paragraph_text(Book book, ParagraphId number, unsigned char *buff);

    // Returns basic info of the parsed sentence by its unique `index`.
    SentenceInfo sentence_info(Book book, SentenceId index);

    // Writes a text of the parsed sentence by its unique `index` to `buff`.
    // Required `buff` size can be given by calling `sentence_info` function.
    void sentence_text(Book book, SentenceId index, unsigned char *buff);
}