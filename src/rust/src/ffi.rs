use std::{
    ops::{Add, AddAssign},
    os::raw::{c_uint, c_void},
};

pub type BookRaw = *mut c_void;
pub type ParagraphId = c_uint;
pub type SentenceId = c_uint;

#[derive(Debug, Clone, Copy, Default, PartialEq)]
#[repr(C)]
pub struct StringSize {
    /// Размер UTF-8 строки в байтах
    pub bytes: c_uint,
    /// Размер UTF-8 строки в символах
    pub symbols: c_uint,
}

impl From<&str> for StringSize {
    fn from(string: &str) -> Self {
        Self {
            bytes: string.len() as c_uint,
            symbols: string.chars().count() as c_uint,
        }
    }
}

impl AddAssign for StringSize {
    fn add_assign(&mut self, rhs: Self) {
        self.symbols += rhs.symbols;
        self.bytes += rhs.bytes;
    }
}

impl Add for StringSize {
    type Output = Self;

    fn add(mut self, rhs: Self) -> Self::Output {
        self += rhs;
        self
    }
}

#[derive(Debug, Clone, Copy, Default, PartialEq)]
#[repr(C)]
pub struct BookInfo {
    /// Количество абзацев в книге
    pub paragraphes: c_uint,
    /// Количество предложений в книге
    pub sentences: c_uint,
    /// Размер книги в символах и байтах (UTF-8 строка)
    pub size: StringSize,
}

#[derive(Debug, Clone, Copy, Default, PartialEq)]
#[repr(C)]
pub struct ParagraphInfo {
    /// Порядковый номер абзаца в книге
    pub index: ParagraphId,
    /// Идентификатор (порядковый номер в книге) перевого предложения в абзаце
    pub sentence_first: SentenceId,
    /// Количество предложений в абзаце
    pub sentences: c_uint,
    /// Размер абзаца в виде UTF-8 строки
    pub size: StringSize,
}

#[derive(Debug, Clone, Copy, Default, PartialEq)]
#[repr(C)]
pub struct SentenceInfo {
    /// Порядковый номер предложения в книге
    pub index: SentenceId,
    /// Порядковый номер предложения в данном абзаце
    pub s_number: c_uint,
    /// Порядковый номер абзаца, в котором находится предложение
    pub p_index: ParagraphId,
    /// Размер предложения в виде UTF-8 строки
    pub size: StringSize,
}
