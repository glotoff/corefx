// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Security;
using System.Runtime.InteropServices;

internal static partial class Interop
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Sec_Application_Protocols
    {
        public uint ProtocolListsSize;
        public ApplicationProtocolNegotiationExt ProtocolExtensionType;
        public short ProtocolListSize;

        public static unsafe byte[] ToByteArray(List<SslApplicationProtocol> applicationProtocols)
        {
            long protocolListSize = 0;
            for (int i = 0; i < applicationProtocols.Count; i++)
            {
                if (applicationProtocols[i].Protocol.Length == 0 || applicationProtocols[i].Protocol.Length > byte.MaxValue)
                {
                    throw new ArgumentException(SR.net_ssl_app_protocols_invalid, nameof(applicationProtocols));
                }

                protocolListSize += applicationProtocols[i].Protocol.Length + 1;

                if (protocolListSize > short.MaxValue)
                {
                    throw new ArgumentException(SR.net_ssl_app_protocols_invalid, nameof(applicationProtocols));
                }
            }

            Sec_Application_Protocols protocols = new Sec_Application_Protocols();

            int protocolListConstSize = sizeof(Sec_Application_Protocols) - sizeof(uint) /* offsetof(Sec_Application_Protocols, ProtocolExtensionType) */;
            protocols.ProtocolListsSize = (uint)(protocolListConstSize + protocolListSize);

            protocols.ProtocolExtensionType = ApplicationProtocolNegotiationExt.ALPN;
            protocols.ProtocolListSize = (short)protocolListSize;

            byte[] buffer = new byte[sizeof(Sec_Application_Protocols) + protocolListSize];
            int index = 0;

            MemoryMarshal.Write(buffer.AsSpan(index), ref protocols);
            index += sizeof(Sec_Application_Protocols);

            for (int i = 0; i < applicationProtocols.Count; i++)
            {
                buffer[index++] = (byte)applicationProtocols[i].Protocol.Length;
                applicationProtocols[i].Protocol.Span.CopyTo(buffer.AsSpan(index));
                index += applicationProtocols[i].Protocol.Length;
            }

            return buffer;
        }
    }
}
