// Imported from: https://github.com/ShareX/ShareX/blob/e91fd94d2bfcecf276893fee1dd7d115939b6c0c/ShareX.MediaLib/DirectShowDevices.cs
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

using System.Collections.Generic;

namespace ShareX.MediaLib
{
    public class DirectShowDevices
    {
        public List<string> VideoDevices = new List<string>();
        public List<string> AudioDevices = new List<string>();
    }
}
#pragma warning restore IDE0065,SA1200,SA1516,SA1201,SA1101,SA1401,SA1413,IDE0009,IDE0011,SA1503,SA1202,IDE0031,SA1208,SA1303,SA1513,RCS1059,SA1310,SA1005,SA1117,SA1515,SA1124
