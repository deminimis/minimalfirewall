using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MinimalFirewall
{
    public static class FolderPicker
    {
        public static bool TryPickFolder(out string path, string initialDirectory = null)
        {
            var dialog = (IFileOpenDialog)new FileOpenDialog();
            try
            {
                if (!string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory))
                {
                    // Check if SHCreateItemFromParsingName is available before calling
                    SHCreateItemFromParsingName(initialDirectory, IntPtr.Zero, typeof(IShellItem).GUID, out var initialFolder);
                    if (initialFolder != null)
                    {
                        dialog.SetFolder(initialFolder);
                    }
                }

                dialog.SetOptions(FOS.FOS_PICKFOLDERS | FOS.FOS_FORCEFILESYSTEM);
                var hwnd = Application.Current.MainWindow == null ? IntPtr.Zero : new WindowInteropHelper(Application.Current.MainWindow).Handle;

                if (dialog.Show(hwnd) == 0)
                {
                    dialog.GetResult(out var result);
                    result.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out path);
                    Marshal.ReleaseComObject(result);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open folder picker.\n\nError: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Marshal.ReleaseComObject(dialog);
            }

            path = null;
            return false;
        }

        #region COM Interop Definitions

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IntPtr pbc, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItem ppv);

        [ComImport, Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
        private class FileOpenDialog { }

        [ComImport, Guid("42f85136-db7e-439c-85f1-e4075d135fc8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog
        {
            [PreserveSig]
            int Show(IntPtr parent);
            void SetFileTypes(); void SetFileTypeIndex(); void GetFileTypeIndex(); void Advise(); void Unadvise();
            void SetOptions([In] FOS fos);
            void GetOptions(); void SetDefaultFolder();
            void SetFolder(IShellItem psi);
            void GetFolder(); void GetCurrentSelection(); void SetFileName(); void GetFileName();
            void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel(); void SetFileNameLabel();
            void GetResult(out IShellItem ppsi);
            void AddPlace(); void SetDefaultExtension(); void Close(); void SetClientGuid(); void ClearClientData(); void SetFilter();
        }

        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            void BindToHandler(IntPtr pbc, [In, MarshalAs(UnmanagedType.LPStruct)] Guid bhid, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IntPtr ppv);
            void GetParent(out IShellItem ppsi);
            void GetDisplayName([In] SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            void GetAttributes([In] uint sfgaoMask, out uint psfgaoAttribs);
            void Compare(IShellItem psi, uint hint, out int piOrder);
        }

        [Flags]
        private enum FOS : uint { FOS_PICKFOLDERS = 0x00000020, FOS_FORCEFILESYSTEM = 0x00000040, }
        private enum SIGDN : uint { SIGDN_FILESYSPATH = 0x80058000, }
        #endregion
    }
}