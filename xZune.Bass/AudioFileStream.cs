// Project: xZune.Bass (https://github.com/higankanshi/xZune.Bass)
// Filename: AudioFileStream.cs
// Version: 20160217

using System;
using System.IO;
using System.Runtime.InteropServices;
using xZune.Bass.Interop;
using xZune.Bass.Interop.Core;
using xZune.Bass.Interop.Flags;
using xZune.Bass.Modules;

namespace xZune.Bass
{
    /// <summary>
    ///     A audio file sample stream.
    /// </summary>
    public class AudioFileStream : AudioStream, IFileStream
    {
        /// <summary>
        ///     Adds data to a "push buffered" user file stream's buffer.
        /// </summary>
        /// <param name="buffer">The file data. </param>
        /// <returns>The number of bytes read from buffer.</returns>
        /// <exception cref="BassErrorException">
        ///     Some error occur to call a Bass function, check the error code and error message
        ///     to get more error information.
        /// </exception>
        /// <exception cref="BassNotLoadedException">
        ///     Bass DLL not loaded, you must use <see cref="BassManager.Initialize" /> to
        ///     load Bass DLL first.
        /// </exception>
        /// <exception cref="NotAvailableException">Channel object is no longer available.</exception>
        public override int PutData(byte[] buffer)
        {
            CheckAvailable();

            GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            return AudioStreamModule.StreamPutFileDataFunction.CheckResult(
                AudioStreamModule.StreamPutFileDataFunction.Delegate(Handle, bufferHandle.AddrOfPinnedObject(),
                    buffer.Length));
        }

        #region -- Creator --

        /// <summary>
        ///     Creates a sample stream from an MP3, MP2, MP1, OGG, WAV, AIFF or plug-in supported file.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <param name="configs">Configure of <see cref="AudioFileStream" />.</param>
        /// <param name="offset">File offset to begin streaming from.</param>
        /// <param name="length">Data length... 0 = use all data up to the end of the file.</param>
        public AudioFileStream(String file, StreamCreateFileConfig configs, uint offset, uint length)
        {
            configs |= StreamCreateFileConfig.Unicode;

            using (var fileNameHandle = InteropHelper.StringToPtr(file))
            {
                Handle = AudioStreamModule.StreamCreateFileFunction.CheckResult(
                    AudioStreamModule.StreamCreateFileFunction.Delegate(false,
                        fileNameHandle.Handle, offset, length, configs));
            }
        }

        /// <summary>
        ///     Creates a sample stream from an MP3, MP2, MP1, OGG, WAV, AIFF or plug-in supported file.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <param name="configs">Configure of <see cref="AudioFileStream" />.</param>
        public AudioFileStream(String file, StreamCreateFileConfig configs) : this(file, configs, 0, 0)
        {
        }

        /// <summary>
        ///     Creates a sample stream from an MP3, MP2, MP1, OGG, WAV, AIFF or plug-in supported memory stream.
        /// </summary>
        /// <param name="stream">Memory stream.</param>
        /// <param name="configs">Configure of <see cref="AudioFileStream" />.</param>
        public AudioFileStream(MemoryStream stream, StreamCreateFileConfig configs)
        {
            ArraySegment<byte> bufferSegment;
            byte[] buffer = null;
            if (stream.TryGetBuffer(out bufferSegment))
            {
                buffer = bufferSegment.Array;
            }
            else
            {
                buffer = stream.ToArray();
            }

            GCHandle bufferHandle = GCHandle.Alloc(buffer);

            Handle = AudioStreamModule.StreamCreateFileFunction.CheckResult(
                AudioStreamModule.StreamCreateFileFunction.Delegate(true,
                    bufferHandle.AddrOfPinnedObject(), 0, (uint) stream.Length, configs));

            bufferHandle.Free();
        }

        /// <summary>
        ///     Create a sample stream from an MP3, MP2, MP1, OGG, WAV, AIFF or plug-in supported file via user callback functions.
        /// </summary>
        /// <param name="handlers">The user defined file functions. </param>
        /// <param name="configs">Configure of <see cref="AudioFileStream" />.</param>
        /// <param name="systemType">File system to use.</param>
        /// <param name="user">User instance data to pass to the callback functions. </param>
        public AudioFileStream(FileHandlers handlers, StreamCreateFileUserConfig configs,
            StreamFileSystemType systemType, IntPtr user)
        {
            Handle =
                AudioStreamModule.StreamCreateFileUserFunction.CheckResult(
                    AudioStreamModule.StreamCreateFileUserFunction.Delegate(systemType, configs,
                        ref handlers, user));
        }

        /// <summary>
        ///     Create a sample stream from an MP3, MP2, MP1, OGG, WAV, AIFF or plug-in supported Bass stream.
        /// </summary>
        /// <param name="bassStream">The Bass stream.</param>
        /// <param name="configs">Configure of <see cref="AudioFileStream" />.</param>
        /// <param name="systemType">File system to use.</param>
        public AudioFileStream(BassStream bassStream, StreamCreateFileUserConfig configs,
            StreamFileSystemType systemType) : this(bassStream.StreamHandlers, configs, systemType, IntPtr.Zero)
        {
        }

        /// <summary>
        ///     Create a sample stream from an MP3, MP2, MP1, OGG, WAV, AIFF or plug-in supported .Net stream.
        /// </summary>
        /// <param name="stream">The .Net stream.</param>
        /// <param name="configs">Configure of <see cref="AudioFileStream" />.</param>
        /// <param name="systemType">File system to use.</param>
        public AudioFileStream(Stream stream, StreamCreateFileUserConfig configs,
            StreamFileSystemType systemType) : this(stream.AsBassStream(), configs, systemType)
        {
        }

        #endregion -- Creator --
    }
}