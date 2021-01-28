use std::{fmt::Debug, os::raw::c_uint, sync::Arc};

use crate::{
    ffi::{SentenceId, SentenceInfo},
    wrapper::Wrapper,
};

pub struct Sentence {
    index: SentenceId,
    ffi: Arc<Wrapper>,
    info: SentenceInfo,
}

impl Sentence {
    pub(super) fn new(ffi: Arc<Wrapper>, index: SentenceId) -> Self {
        let info = ffi.sentence_info(index);
        Self { index, ffi, info }
    }

    pub fn info(&self) -> SentenceInfo {
        self.info
    }

    pub fn text(&self) -> Option<String> {
        self.ffi.sentence_text(self.index)
    }

    /// Returns `true` if the sentence has the first at its paragraph
    pub fn is_first(&self) -> bool {
        self.info().s_number == 0
    }

    /// Returns `true` if the sentence has the last position its paragraph
    pub fn is_last(&self) -> bool {
        let si = self.ffi.sentence_info(self.index);
        let pi = self.ffi.paragraph_info(si.p_index);
        c_uint::from(pi.sentence_first) + si.s_number == self.index.into()
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
