use libloading::{Library, Symbol};
use std::{
    error::Error,
    fmt::Display,
    os::raw::{c_char, c_uchar, c_uint},
};

use crate::ffi::*;

struct BookLibrary(Library);

impl BookLibrary {
    fn new() -> Result<Self, Box<dyn Error>> {
        let this = Self(Library::new("book_parse")?); // .dll \ .so \ .dylib

        unsafe {
            this.0.get::<unsafe extern "C" fn()>(b"from_utf8")?;
            this.0.get::<unsafe extern "C" fn()>(b"dispose")?;
            this.0.get::<unsafe extern "C" fn()>(b"paragraph_info")?;
            this.0.get::<unsafe extern "C" fn()>(b"paragraph_text")?;
            this.0.get::<unsafe extern "C" fn()>(b"sentence_info")?;
            this.0.get::<unsafe extern "C" fn()>(b"sentence_text")?;
        }

        Ok(this)
    }

    fn from_utf8(&self, pointer: *const c_uchar, len: c_uint) -> BookRaw {
        unsafe {
            self.0
                .get::<Symbol<unsafe extern "C" fn(pointer: *const c_uchar, len: c_uint) -> BookRaw>>(
                    b"from_utf8",
                )
                .unwrap()(pointer, len)
        }
    }

    fn dispose(&self, book: BookRaw) {
        unsafe {
            self.0
                .get::<Symbol<unsafe extern "C" fn(BookRaw)>>(b"dispose")
                .unwrap()(book)
        }
    }

    fn paragraph_info(&self, book: BookRaw, index: ParagraphId) -> ParagraphInfo {
        unsafe {
            self.0
                .get::<Symbol<unsafe extern "C" fn(BookRaw, ParagraphId) -> ParagraphInfo>>(
                    b"paragraph_info",
                )
                .unwrap()(book, index)
        }
    }

    fn paragraph_text(&self, book: BookRaw, index: ParagraphId, buff: *mut c_char) {
        unsafe {
            self.0
                .get::<Symbol<unsafe extern "C" fn(BookRaw, ParagraphId, *mut c_char)>>(
                    b"paragraph_text",
                )
                .unwrap()(book, index, buff)
        }
    }

    fn sentence_info(&self, book: BookRaw, index: ParagraphId) -> SentenceInfo {
        unsafe {
            self.0
                .get::<Symbol<unsafe extern "C" fn(BookRaw, SentenceId) -> SentenceInfo>>(
                    b"sentence_info",
                )
                .unwrap()(book, index)
        }
    }

    fn sentence_text(&self, book: BookRaw, index: ParagraphId, buff: *mut c_char) {
        unsafe {
            self.0
                .get::<Symbol<unsafe extern "C" fn(BookRaw, SentenceId, *mut c_char)>>(
                    b"sentence_text",
                )
                .unwrap()(book, index, buff)
        }
    }
}

pub struct Book {
    lib: BookLibrary,
    pointer: BookRaw,
}

impl Book {
    pub fn from_utf8(text: &str) -> Result<Self, Box<dyn Error>> {
        let lib = BookLibrary::new()?;
        let pointer = lib.from_utf8(text.as_ptr(), text.len() as c_uint);

        match pointer.is_null() {
            true => Err(Box::new(BookError)),
            false => Ok(Self { lib, pointer }),
        }
    }

    pub fn paragraph_info(&self, book: BookRaw, index: ParagraphId) -> ParagraphInfo {
        self.lib.paragraph_info(book, index)
    }

    pub fn paragraph_text(&self, book: BookRaw, index: ParagraphId, buff: *mut c_char) {
        self.lib.paragraph_text(book, index, buff)
    }

    pub fn sentence_info(&self, book: BookRaw, index: ParagraphId) -> SentenceInfo {
        self.lib.sentence_info(book, index)
    }

    pub fn sentence_text(&self, book: BookRaw, index: ParagraphId, buff: *mut c_char) {
        self.lib.sentence_text(book, index, buff)
    }
}

impl Drop for Book {
    fn drop(&mut self) {
        self.lib.dispose(self.pointer)
    }
}

#[derive(Debug)]
pub struct BookError;

impl Display for BookError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "BookError occured")
    }
}

impl Error for BookError {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        None
    }

    fn description(&self) -> &str {
        "BookError occured"
    }

    fn cause(&self) -> Option<&dyn Error> {
        None
    }
}
