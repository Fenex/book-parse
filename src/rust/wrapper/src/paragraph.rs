use std::{fmt::Debug, os::raw::c_uint, rc::Rc};

use crate::sentence::Sentence;
use crate::{
    ffi::{ParagraphId, ParagraphInfo},
    wrapper::Wrapper,
};

pub struct Paragraph {
    index: ParagraphId,
    ffi: Rc<Wrapper>,
}

impl Paragraph {
    pub(super) fn new(ffi: Rc<Wrapper>, index: ParagraphId) -> Self {
        Self { index, ffi }
    }

    pub fn info(&self) -> ParagraphInfo {
        self.ffi.paragraph_info(self.index)
    }

    pub fn text(&self) -> Option<String> {
        self.ffi.paragraph_text(self.index)
    }

    pub fn sentences(&self) -> impl Iterator<Item = Sentence> + '_ {
        let paragraph_info = self.info();
        let first_index: c_uint = paragraph_info.sentence_first.into();
        let range = first_index..(paragraph_info.sentences + first_index);
        range.map(move |i| Sentence::new(Rc::clone(&self.ffi), i.into()))
    }
}

impl Debug for Paragraph {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("Paragraph")
            .field("index", &self.index)
            .field("info", &self.info())
            .field("sentences", &self.sentences().collect::<Vec<_>>())
            .finish()
    }
}
