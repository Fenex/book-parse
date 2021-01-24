using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BookParse.FFI
{
    internal static class Binding
    {

        const String LibName = "book_parse";

        static String GetLibPath()
        {
            StringBuilder target = new StringBuilder();

            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X64:
                    if (Environment.Is64BitProcess)
                        target.Append("x86_64");
                    else
                        target.Append("i686");
                    break;
                case Architecture.X86: target.Append("i686"); break;
                case Architecture.Arm64:
                    if (Environment.Is64BitProcess)
                        target.Append("aarch64");
                    else
                        target.Append("arm");
                    break;
                case Architecture.Arm: target.Append("arm"); break;
                default:
                    throw new Exception("Unknown OS Architecture");
            }

            target.Append('-');

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                target.Append(Path.Combine("pc-windows-gnu", LibName + ".dll"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                target.Append(Path.Combine("unknown-linux-gnu", LibName + ".so"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                target.Append(Path.Combine("apple-darwin", LibName + ".dylib"));
            }
            else
            {
                target.Append(Path.Combine("unknown-unknown", LibName));
            }

            return target.ToString();
        }

        static Binding()
        {
            // Linux supports loading unmanaged library only storages in PATH \ LD_LIBRARY_PATH.
            // Required from you to set LD_LIBRARY_PATH before run the application.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;

            var path = new Uri(typeof(Binding).Assembly.CodeBase).LocalPath;
            var folder = Path.GetDirectoryName(path);

            var assemblyLoadContext = new CustomAssemblyLoadContext();
            assemblyLoadContext.LoadUnmanagedLibrary(
                Path.Combine(
                    new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName,
                    "lib",
                    GetLibPath()));
        }

        // Create new Book with that UTF-8 encoded text.
        [DllImport(Binding.LibName, EntryPoint = "from_utf8")]
        public unsafe static extern IntPtr FromUTF8(IntPtr buffer, UInt32 count);

        [DllImport(Binding.LibName, EntryPoint = "dispose")]
        public unsafe static extern UInt32 Dispose(IntPtr pBook);

        [DllImport(Binding.LibName, EntryPoint = "book_info")]
        public unsafe static extern BookInfo Info(IntPtr pBook);

        [DllImport(Binding.LibName, EntryPoint = "paragraph_info")]
        public unsafe static extern ParagraphInfo ParagraphInfo(IntPtr pBook, UInt32 index);

        [DllImport(Binding.LibName, EntryPoint = "paragraph_text")]
        public unsafe static extern void ParagraphTextToBuffer(IntPtr pBook, UInt32 index, IntPtr pBuff);

        public unsafe static String ParagraphText(IntPtr pBook, UInt32 index)
        {
            var pi = ParagraphInfo(pBook, index);
            if (pi.size.bytes == 0) { return ""; }

            var buff = new byte[pi.size.bytes];
            fixed (byte* p = buff)
            {
                ParagraphTextToBuffer(pBook, index, (IntPtr)p);
            }
            return System.Text.Encoding.UTF8.GetString(buff);
        }

        [DllImport(Binding.LibName, EntryPoint = "sentence_info")]
        public unsafe static extern SentenceInfo SentenceInfo(IntPtr pBook, UInt32 index);

        [DllImport(Binding.LibName, EntryPoint = "sentence_text")]
        public unsafe static extern void SentenceTextToBuffer(IntPtr pBook, UInt32 index, IntPtr pBuff);

        public unsafe static String SentenceText(IntPtr pBook, UInt32 index)
        {
            var pi = SentenceInfo(pBook, index);
            if (pi.size.bytes == 0) { return ""; }

            var buff = new byte[pi.size.bytes];
            fixed (byte* p = buff)
            {
                SentenceTextToBuffer(pBook, index, (IntPtr)p);
            }
            return System.Text.Encoding.UTF8.GetString(buff);
        }
    }
}
