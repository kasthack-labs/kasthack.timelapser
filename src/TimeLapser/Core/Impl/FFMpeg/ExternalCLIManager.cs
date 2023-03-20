// Imported from: https://github.com/ShareX/ShareX/blob/e91fd94d2bfcecf276893fee1dd7d115939b6c0c/ShareX.HelpersLib/CLI/ExternalCLIManager.cs
// changes: async
#pragma warning disable IDE0065,SA1200,SA1516,SA1201,SA1101,SA1401,SA1413,IDE0009,IDE0011,SA1503,SA1202,IDE0031,SA1208,SA1303,SA1513,RCS1059,SA1310,SA1005,SA1117,SA1515,SA1124
#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2023 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ShareX.HelpersLib
{
    public abstract class ExternalCLIManager : IDisposable
    {
        public event DataReceivedEventHandler OutputDataReceived;
        public event DataReceivedEventHandler ErrorDataReceived;

        public bool IsProcessRunning { get; private set; }

        protected Process process;

        public virtual async Task<int> Open(string path, string args = null)
        {
            if (File.Exists(path))
            {
                using (process = new Process())
                {
                    ProcessStartInfo psi = new ProcessStartInfo()
                    {
                        FileName = path,
                        WorkingDirectory = Path.GetDirectoryName(path),
                        Arguments = args,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    };

                    process.EnableRaisingEvents = true;
                    if (psi.RedirectStandardOutput) process.OutputDataReceived += cli_OutputDataReceived;
                    if (psi.RedirectStandardError) process.ErrorDataReceived += cli_ErrorDataReceived;
                    process.StartInfo = psi;

                    Debug.WriteLine($"CLI: \"{psi.FileName}\" {psi.Arguments}");
                    process.Start();

                    if (psi.RedirectStandardOutput) process.BeginOutputReadLine();
                    if (psi.RedirectStandardError) process.BeginErrorReadLine();

                    try
                    {
                        IsProcessRunning = true;
                        await process.WaitForExitAsync().ConfigureAwait(false);
                    }
                    finally
                    {
                        IsProcessRunning = false;
                    }

                    return process.ExitCode;
                }
            }

            return -1;
        }

        private void cli_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                OutputDataReceived?.Invoke(sender, e);
            }
        }

        private void cli_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                ErrorDataReceived?.Invoke(sender, e);
            }
        }

        public async Task WriteInput(string input)
        {
            if (IsProcessRunning && process != null && process.StartInfo != null && process.StartInfo.RedirectStandardInput)
            {
                await process.StandardInput.WriteLineAsync(input).ConfigureAwait(false);
            }
        }

        public virtual async Task Close()
        {
            if (IsProcessRunning && process != null)
            {
                process.CloseMainWindow();
            }
        }

        public void Dispose()
        {
            if (process != null)
            {
                process.Dispose();
            }
        }
    }
}
#pragma warning restore IDE0065,SA1200,SA1516,SA1201,SA1101,SA1401,SA1413,IDE0009,IDE0011,SA1503,SA1202,IDE0031,SA1208,SA1303,SA1513,RCS1059,SA1310,SA1005,SA1117,SA1515,SA1124

