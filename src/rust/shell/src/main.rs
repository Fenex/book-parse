use std::{
    fs::File,
    io::{self, Write},
    os::raw::c_uint,
    path::Path,
    time::Duration,
};

use clap::{crate_authors, crate_version, Clap};
use tokio::{
    io::AsyncWriteExt,
    sync::oneshot::{channel, error::TryRecvError, Sender},
    task::JoinHandle,
    time::sleep,
};

use bpw::*;

#[derive(Clap, Debug)]
#[clap(
    name = "Book Parse Shell",
    version = crate_version!(),
    author = crate_authors!()
)]
struct Opts {
    #[clap(about = "Source file that will be parsed.")]
    source: String,

    #[clap(
        long,
        short,
        about = "Path to output file to save parsed text.",
        long_about = "Path to output file to save parsed text. If this options is empty, parsed data will be written into stdout."
    )]
    output: Option<String>,

    #[clap(
        long,
        about = "Every splitted part should contains at least this count symbols.",
        default_value = "200"
    )]
    min: u32,

    #[clap(
        long,
        about = "Recommended maximum size of splitted parts.",
        default_value = "600"
    )]
    max: u32,

    #[clap(
        short,
        long,
        about = "Starts new parts if sentence is equal to this value.",
        long_about = "Parsed text will be splitted by paragraphes that contain a single sentence with given string."
    )]
    split_by_paragraph: Option<String>,

    #[clap(
        short,
        long,
        long_about = "Sets template for parts separator to inserting its instead of empty rows. Default is: `## {} ##` if this argument was passed without a value."
    )]
    parts_separator: Option<Option<String>>,

    #[clap(
        short,
        about = "Starts count a parsed part's number from 0 instead of 1."
    )]
    count_parts_from_zero: bool,

    #[clap(
        long,
        about = "Inserts sentences' index in output.",
        long_about = "Inserts sentences' index into output. Sentences' index format: (P:N:U) where P: current `part` count number, N: sentence's count number inside paragraph, U: sentence's count number inside the book (it's also an unique index)."
    )]
    view_index_sentence: bool,

    #[clap(long, about = "Show verbose info when splitting stady is active")]
    verbose_splitting: bool,
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error + 'static>> {
    let opts: Opts = Opts::parse();
    let text = ::std::fs::read_to_string(&opts.source)?;

    println!(
        "Read file `{}`, size: {} symbols, {} bytes",
        opts.source,
        text.chars().count(),
        text.as_bytes().len()
    );

    let writer = get_writer(opts.output.as_ref())?;

    parse_book(text, writer, &opts).await?;

    Ok(())
}

fn process(action: &str) -> (JoinHandle<()>, Sender<&'static str>) {
    let (tx, mut rx) = channel();
    let action = action.to_owned();

    let handle = ::tokio::spawn(async move {
        let mut stdout = ::tokio::io::stdout();
        let status = [br"|", br"/", br"-", br"\"];
        let mut i = 0;
        stdout.write(action.as_bytes()).await.unwrap();

        loop {
            stdout.write(status[i]).await.unwrap();
            stdout.flush().await.unwrap();
            i = match i {
                i if i == status.len() - 1 => 0,
                i => i + 1,
            };

            match rx.try_recv() {
                Ok(msg) => {
                    stdout
                        .write(format!("\u{8} {}\r\n", msg).as_bytes())
                        .await
                        .unwrap();
                    break;
                }
                Err(TryRecvError::Closed) => {
                    stdout.write("\u{8}\r\n".as_bytes()).await.unwrap();
                    break;
                }
                _ => (),
            }

            stdout.flush().await.unwrap();
            sleep(Duration::from_millis(300)).await;
            stdout.write("\u{8}".as_bytes()).await.unwrap();
        }
    });

    (handle, tx)
}

async fn parse_book(
    text: String,
    mut writer: impl Write,
    opts: &Opts,
) -> Result<(), Box<dyn std::error::Error + 'static>> {
    let (handle, tx) = process("Parsing a book... ");
    let book = Book::from_utf8(&text)?;
    tx.send("ok").unwrap();
    handle.await.unwrap();

    println!(
        "Found: {} sentences, {} paragraphes",
        book.info().sentences,
        book.info().paragraphes
    );

    let (handle, tx) = process("Splitting parts... ");

    let mut parts: Vec<Vec<Sentence>> = vec![];
    let mut current_part = vec![];

    let symbols = |sentences: &Vec<Sentence>| {
        sentences
            .iter()
            .map(|s: &Sentence| s.info().size.symbols)
            .sum::<c_uint>()
    };

    let is_force_split = |sentence: &Sentence| match &opts.split_by_paragraph {
        None => false,
        Some(s) => match sentence.text() {
            Some(ref text) => text == s,
            None => false,
        },
    };

    let mut is_last_force_splitted = false;

    for s in book.sentences() {
        if opts.verbose_splitting {
            let s_index = s.info().index;

            if u32::from(s_index) % 100 == 0 {
                println!("\ranalyzing: {} of {}", s_index, book.info().sentences);
            }
        }

        if is_force_split(&s) {
            let current_part_len = {
                let symbols: u32 = current_part
                    .iter()
                    .map(|s: &Sentence| s.info().size.symbols)
                    .sum();
                (match current_part.len() as u32 {
                    sentence_count if sentence_count > 1 => symbols + sentence_count - 1,
                    sentence_count => sentence_count,
                }) as c_uint
            };

            match (
                current_part_len < opts.min && !is_last_force_splitted,
                parts.last_mut(),
            ) {
                (true, Some(last)) => last.append(&mut current_part),
                _ => parts.push(current_part.drain(..).collect::<Vec<_>>()),
            }

            is_last_force_splitted = true;
        } else {
            let chars_sentence = s.info().size.symbols;
            let chars_part = symbols(&current_part);

            if chars_part < opts.min {
                current_part.push(s);
            } else if chars_part + chars_sentence > opts.max {
                let tmp = current_part.drain(..).collect::<Vec<_>>();
                parts.push(tmp);
                current_part.push(s);
            } else {
                current_part.push(s);
            }

            if symbols(&current_part) > opts.max {
                let tmp = current_part.drain(..).collect::<Vec<_>>();
                parts.push(tmp);
            }

            is_last_force_splitted = false;
        }
    }

    if !current_part.is_empty() {
        parts.push(current_part);
    }

    tx.send("ok").unwrap();
    handle.await.unwrap();

    let (handle, tx) = process("Mapping into strings...");

    let parts = parts
        .iter()
        .map(|p| p.iter().filter(|&s| s.text().is_some()));

    let mut out = vec![];

    for (pi, p) in parts.enumerate() {
        let view_pi = pi + if opts.count_parts_from_zero { 1 } else { 0 };
        let replacer = |input_str: &str| input_str.replace("{}", &view_pi.to_string());

        out.push(match &opts.parts_separator {
            Some(Some(s)) => vec!["\r\n", &replacer(s), "\r\n"].join(""),
            Some(None) => format!("\r\n## {} ##\r\n", view_pi),
            None => String::from("\r\n\r\n"),
        });

        for (si, s) in p.enumerate() {
            if si > 0 {
                out.push(String::from(if s.is_first() { "\r\n" } else { " " }));
            }

            if opts.view_index_sentence {
                let info = s.info();
                out.push(format!("({}:{}:{}) ", si, info.s_number, info.index));
            }
            out.push(s.text().unwrap());
        }
    }

    tx.send("ok").unwrap();
    handle.await.unwrap();

    let mut writer_status = None;
    if let Some(ref path) = opts.output {
        writer_status = Some(process(&format!("Writing into file: {}", path)));
    }

    for data in out {
        writer.write_all(data.as_bytes())?;
    }
    writer.flush()?;

    if let Some((handle, tx)) = writer_status {
        tx.send("ok").unwrap();
        handle.await.unwrap();
    }

    Ok(())
}

fn get_writer<S: AsRef<Path>>(file: Option<S>) -> io::Result<impl Write> {
    match file {
        Some(path) => File::create(path).map(|f| Box::new(f) as Box<dyn Write>),
        None => Ok(Box::new(io::stdout()) as Box<dyn Write>),
    }
}
