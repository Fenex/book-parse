use libloading::{Library, Symbol};
use std::{
    error::Error,
    fmt::Display,
    os::raw::{c_uchar, c_uint},
};

use crate::ffi::*;

#[derive(Debug)]
pub struct Wrapper {
    lib: BookLibrary,
    pointer: BookRaw,
}

impl Wrapper {
    pub fn from_utf8(text: &str) -> Result<Self, Box<dyn Error>> {
        let lib = BookLibrary::new()?;
        let pointer = lib.from_utf8(text.as_ptr(), text.len() as c_uint);

        match pointer.is_null() {
            true => Err(Box::new(BookError)),
            false => Ok(Self { lib, pointer }),
        }
    }

    pub fn book_info(&self) -> BookInfo {
        self.lib.book_info(self.pointer)
    }

    pub fn paragraph_info(&self, index: ParagraphId) -> ParagraphInfo {
        self.lib.paragraph_info(self.pointer, index)
    }

    pub fn paragraph_text(&self, index: ParagraphId) -> Option<String> {
        let pi = self.paragraph_info(index);
        let mut buff: Vec<u8> = Vec::with_capacity(pi.size.bytes as usize);
        unsafe {
            let p_buff = buff.as_mut_ptr();
            self.lib.paragraph_text(self.pointer, index, p_buff);
            buff.set_len(buff.capacity());
        }
        String::from_utf8(buff).ok()
    }

    pub fn sentence_info(&self, index: SentenceId) -> SentenceInfo {
        self.lib.sentence_info(self.pointer, index)
    }

    pub fn sentence_text(&self, index: SentenceId) -> Option<String> {
        let pi = self.sentence_info(index);
        let mut buff: Vec<u8> = Vec::with_capacity(pi.size.bytes as usize);
        unsafe {
            let p_buff = buff.as_mut_ptr();
            self.lib.sentence_text(self.pointer, index, p_buff);
            buff.set_len(buff.capacity());
        }
        String::from_utf8(buff).ok()
    }
}

impl Drop for Wrapper {
    fn drop(&mut self) {
        self.lib.dispose(self.pointer)
    }
}

#[derive(Debug)]
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

    fn book_info(&self, book: BookRaw) -> BookInfo {
        unsafe {
            self.0
                .get::<Symbol<unsafe extern "C" fn(BookRaw) -> BookInfo>>(b"book_info")
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

    unsafe fn paragraph_text(&self, book: BookRaw, index: ParagraphId, buff: *mut c_uchar) {
        self.0
            .get::<Symbol<unsafe extern "C" fn(BookRaw, ParagraphId, *mut c_uchar)>>(
                b"paragraph_text",
            )
            .unwrap()(book, index, buff)
    }

    fn sentence_info(&self, book: BookRaw, index: SentenceId) -> SentenceInfo {
        unsafe {
            self.0
                .get::<Symbol<unsafe extern "C" fn(BookRaw, SentenceId) -> SentenceInfo>>(
                    b"sentence_info",
                )
                .unwrap()(book, index)
        }
    }

    fn sentence_text(&self, book: BookRaw, index: SentenceId, buff: *mut c_uchar) {
        unsafe {
            self.0
                .get::<Symbol<unsafe extern "C" fn(BookRaw, SentenceId, *mut c_uchar)>>(
                    b"sentence_text",
                )
                .unwrap()(book, index, buff)
        }
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
