use clap::{crate_authors, crate_version, Clap};
use std::fs::File;
use std::io::{self, Write};
use std::path::Path;

use bpw::*;

#[derive(Clap, Debug)]
#[clap(
    name = "Book Parse Shell",
    version = crate_version!(),
    author = crate_authors!()
)]
struct Opts {
    #[clap(about = "Source file that will be parsed")]
    source: String,

    #[clap(
        long,
        short,
        about = "Path to output file to save parsed text",
        long_about = "Path to output file to save parsed text. If this options is empty, parsed data will be written into stdout."
    )]
    output: Option<String>,

    #[clap(long, about = "Pretty print output")]
    pretty_print: bool,
    // #[clap(long, about = "Output in RON format")]
    // ron: bool
}

fn main() -> Result<(), Box<dyn std::error::Error + 'static>> {
    let opts: Opts = Opts::parse();
    let text = ::std::fs::read_to_string(&opts.source)?;

    println!(
        "Read file `{}`, size: {} symbols, {} bytes",
        opts.source,
        text.chars().count(),
        text.as_bytes().len()
    );

    let writer = get_writer(opts.output.as_ref())?;

    parse_book(text, writer, opts.pretty_print)?;

    Ok(())
}

fn parse_book(
    text: String,
    mut writer: impl Write,
    is_pretty_print: bool,
) -> Result<(), Box<dyn std::error::Error + 'static>> {
    let book = Book::from_utf8(&text)?;
    for p in book.paragraphes() {
        writer
            .write(
                if is_pretty_print {
                    format!("\t{:#?}\r\n", p)
                } else {
                    format!("\t{:?}\r\n", p)
                }
                .as_bytes(),
            )
            .ok();
    }
    Ok(())
}

fn get_writer<S: AsRef<Path>>(file: Option<S>) -> io::Result<impl Write> {
    match file {
        Some(path) => File::create(path).map(|f| Box::new(f) as Box<dyn Write>),
        None => Ok(Box::new(io::stdout()) as Box<dyn Write>),
    }
}
