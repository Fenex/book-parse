use std::{fmt::Debug, rc::Rc};

use crate::{
    ffi::{SentenceId, SentenceInfo},
    wrapper::Wrapper,
};

pub struct Sentence {
    index: SentenceId,
    ffi: Rc<Wrapper>,
}

impl Sentence {
    pub(super) fn new(ffi: Rc<Wrapper>, index: SentenceId) -> Self {
        Self { index, ffi }
    }

    pub fn info(&self) -> SentenceInfo {
        self.ffi.sentence_info(self.index)
    }

    pub fn text(&self) -> Option<String> {
        self.ffi.sentence_text(self.index)
    }
}

impl Debug for Sentence {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("Paragraph")
            .field("index", &self.index)
            .field("info", &self.info())
            .field("text", &self.text())
            .finish()
    }
}
