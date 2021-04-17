# book-parse
The repository includes shared library (located in [/lib](/lib) directory for various platforms) that can splitting UTF-8 text into `paragraphes` and `sentences`.
Additional, the repository includes two projects with safe FFI-wrappers for the library written in [Rust](/src/rust) and [C#](/src/dotnet) languages. There are own readme files for more information.

## book-parse-shell
There is a cli utility that can splits text into several parts about equals length. You can find source [here](src/rust/shell).
Here is a help of the utility:
```
USAGE:
    book-parse-shell.exe [FLAGS] [OPTIONS] <source>

ARGS:
    <source>
            Source file that will be parsed.

FLAGS:
    -c
            Starts count a parsed part's number from 0 instead of 1.

    -h, --help
            Prints help information

        --verbose-splitting
            Show verbose info when splitting stady is active

    -V, --version
            Prints version information

        --view-index-sentence
            Inserts sentences' index into output. Sentences' index format: (P:N:U) where P: current
            `part` count number, N: sentence's count number inside paragraph, U: sentence's count
            number inside the book (it's also an unique index).


OPTIONS:
        --max <max>
            Recommended maximum size of splitted parts. [default: 600]

        --min <min>
            Every splitted part should contains at least this count symbols. [default: 200]

    -o, --output <output>
            Path to output file to save parsed text. If this options is empty, parsed data will be
            written into stdout.

    -p, --parts-separator <parts-separator>...
            Sets template for parts separator to inserting its instead of empty rows. Default is:
            `## {} ##` if this argument was passed without a value.

    -s, --split-by-paragraph <split-by-paragraph>
            Parsed text will be splitted by paragraphes that contain a single sentence with given
            string.
```
