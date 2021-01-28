use std::sync::Arc;

use crate::{
    ffi::BookInfo,
    paragraph::Paragraph,
    sentence::Sentence,
    wrapper::{BookError, Wrapper},
};

#[derive(Debug)]
pub struct Book<'a> {
    ffi: Arc<Wrapper>,
    text: &'a str,
}

impl<'a> Book<'a> {
    pub fn from_utf8(text: &'a str) -> Result<Self, Box<dyn std::error::Error>> {
        let ffi = Arc::new(Wrapper::from_utf8(text)?);
        let book_info = ffi.book_info();
        if book_info.size.bytes == 0 {
            return Err(Box::new(BookError));
        }

        Ok(Self { ffi, text })
    }

    pub fn info(&self) -> BookInfo {
        self.ffi.book_info()
    }

    pub fn paragraphes(&self) -> impl Iterator<Item = Paragraph> + '_ {
        let book_info = self.info();
        (0..book_info.paragraphes).map(move |i| Paragraph::new(Arc::clone(&self.ffi), i.into()))
    }

    pub fn sentences(&self) -> impl Iterator<Item = Sentence> + '_ {
        let book_info = self.info();
        (0..book_info.sentences).map(move |i| Sentence::new(Arc::clone(&self.ffi), i.into()))
    }
}
