use std::{fmt::Debug, os::raw::c_uint, sync::Arc};

use crate::sentence::Sentence;
use crate::{
    ffi::{ParagraphId, ParagraphInfo},
    wrapper::Wrapper,
};

pub struct Paragraph {
    index: ParagraphId,
    ffi: Arc<Wrapper>,
    info: ParagraphInfo,
}

impl Paragraph {
    pub(super) fn new(ffi: Arc<Wrapper>, index: ParagraphId) -> Self {
        let info = ffi.paragraph_info(index);
        Self { index, ffi, info }
    }

    pub fn info(&self) -> ParagraphInfo {
        self.info
    }

    pub fn text(&self) -> Option<String> {
        self.ffi.paragraph_text(self.index)
    }

    pub fn sentences(&self) -> impl Iterator<Item = Sentence> + '_ {
        let paragraph_info = self.info();
        let first_index: c_uint = paragraph_info.sentence_first.into();
        let range = first_index..(paragraph_info.sentences + first_index);
        range.map(move |i| Sentence::new(Arc::clone(&self.ffi), i.into()))
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
