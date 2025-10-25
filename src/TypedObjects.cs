// File: TypedObjects.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;

namespace MinimalFirewall.TypedObjects
{
    public enum Actions : byte
    {
        Block = 0,
        Allow = 1
    }

    [Flags]
    public enum Directions : byte
    {
        Incoming = 1,
        Outgoing = 2
    }

    [Flags]
    public enum InterfaceTypes : byte
    {
        All = RemoteAccess | Wireless | Lan,
        RemoteAccess = 1,
        Wireless = 2,
        Lan = 4
    }

    public enum SpecificLocalPort : byte
    {
        RPC,
        RPC_EPMap,
        IPHTTPS
    }

    public interface IFixedRange<TUnit>
    {
        TUnit Begin { get; }
        TUnit End { get; }
    }

    [Serializable]
    public class PortRange : IFixedRange<ushort>, IEquatable<PortRange>
    {
        private readonly bool _isSinglePort;
        private readonly SpecificLocalPort? _specificLocalPort;
        public ushort Begin { get; }
        public ushort End { get; }

        public PortRange(ushort singlePort) : this(singlePort, singlePort) { }

        public PortRange(ushort first, ushort second)
        {
            if (first > second) { Begin = second; End = first; }
            else { Begin = first; End = second; }
            _isSinglePort = (Begin == End);
            _specificLocalPort = null;
        }

        public PortRange(SpecificLocalPort specificLocalPort)
        {
            _specificLocalPort = specificLocalPort;
        }

        public static implicit operator PortRange(ushort port) => new PortRange(port);
        public static implicit operator PortRange(SpecificLocalPort port) => new PortRange(port);

        public static bool TryParse(string rangeString, [NotNullWhen(true)] out PortRange? range)
        {
            range = null;
            if (string.IsNullOrWhiteSpace(rangeString)) return false;

            if (Enum.TryParse<SpecificLocalPort>(rangeString.Replace("-", "_"), true, out var sp))
            {
                range = new PortRange(sp);
                return true;
            }

            var parts = rangeString.Split('-');
            if (parts.Length == 1 && ushort.TryParse(parts[0], out var port))
            {
                range = new PortRange(port);
                return true;
            }
            if (parts.Length == 2 && ushort.TryParse(parts[0], out var begin) && ushort.TryParse(parts[1], out var end))
            {
                range = new PortRange(begin, end);
                return true;
            }

            return false;
        }

        public bool Equals(PortRange? other)
        {
            if (other is null) return false;
            if (_specificLocalPort.HasValue) return _specificLocalPort.Value == other._specificLocalPort;
            return Begin == other.Begin && End == other.End;
        }

        public override int GetHashCode() => ToString().GetHashCode();
        public override string ToString()
        {
            if (_specificLocalPort.HasValue) return _specificLocalPort.Value.ToString().Replace("_", "-");
            if (_isSinglePort) return Begin.ToString();
            return $"{Begin}-{End}";
        }
    }

    [Serializable]
    public class IPAddressRange : IEnumerable<IPAddress>, IEquatable<IPAddressRange>, IFixedRange<IPAddress>
    {
        public IPAddress Begin { get; set; }
        public IPAddress End { get; set; }

        public IPAddressRange(IPAddress singleAddress)
        {
            if (singleAddress == null) throw new ArgumentNullException(nameof(singleAddress));
            Begin = End = singleAddress;
        }

        public IPAddressRange(IPAddress begin, IPAddress end)
        {
            if (begin.AddressFamily != end.AddressFamily) throw new ArgumentException("Addresses must be of the same family.");
            if (!Internal.Bits.GtECore(end.GetAddressBytes(), begin.GetAddressBytes())) throw new ArgumentException("Begin address must be smaller than End address.");
            Begin = begin;
            End = end;
        }

        public static IPAddressRange Parse(string ipRangeString)
        {
            if (!TryParse(ipRangeString, out IPAddressRange? range) || range == null)
            {
                throw new FormatException("Unknown IP range string format.");
            }
            return range;
        }

        public static bool TryParse(string ipRangeString, [NotNullWhen(true)] out IPAddressRange? range)
        {
            range = null;
            if (string.IsNullOrWhiteSpace(ipRangeString)) return false;
            ipRangeString = ipRangeString.Trim();

            var cidrParts = ipRangeString.Split('/');
            if (cidrParts.Length == 2 && IPAddress.TryParse(cidrParts[0], out var baseAddress) && int.TryParse(cidrParts[1], out var maskLen))
            {
                var baseAdrBytes = baseAddress.GetAddressBytes();
                if (baseAdrBytes.Length * 8 < maskLen) return false;
                var maskBytes = Internal.Bits.GetBitMask(baseAdrBytes.Length, maskLen);
                var beginBytes = Internal.Bits.And(baseAdrBytes, maskBytes);
                var endBytes = Internal.Bits.Or(beginBytes, Internal.Bits.Not(maskBytes));
                range = new IPAddressRange(new IPAddress(beginBytes), new IPAddress(endBytes));
                return true;
            }

            var rangeParts = ipRangeString.Split('-');
            if (rangeParts.Length == 2 && IPAddress.TryParse(rangeParts[0], out var begin) && IPAddress.TryParse(rangeParts[1], out var end))
            {
                range = new IPAddressRange(begin, end);
                return true;
            }

            if (IPAddress.TryParse(ipRangeString, out var singleAddress))
            {
                range = new IPAddressRange(singleAddress);
                return true;
            }

            return false;
        }


        public bool Contains(IPAddress ipaddress)
        {
            if (ipaddress.AddressFamily != Begin.AddressFamily) return false;
            var adrBytes = ipaddress.GetAddressBytes();
            return Internal.Bits.LtECore(Begin.GetAddressBytes(), adrBytes) && Internal.Bits.GtECore(End.GetAddressBytes(), adrBytes);
        }

        public bool Equals(IPAddressRange? other)
        {
            if (other is null) return false;
            return Begin.Equals(other.Begin) && End.Equals(other.End);
        }

        public override int GetHashCode() => (Begin, End).GetHashCode();
        public override string ToString()
        {
            return Begin.Equals(End) ? Begin.ToString() : $"{Begin}-{End}";
        }

        public IEnumerator<IPAddress> GetEnumerator()
        {
            var first = Begin.GetAddressBytes();
            var last = End.GetAddressBytes();
            for (var ip = first; Internal.Bits.LtECore(ip, last); ip = Internal.Bits.Increment(ip))
                yield return new IPAddress(ip);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static class RangeExtensions
    {
        public static PortRange To(this ushort from, ushort to)
        {
            return new PortRange(from, to);
        }
        public static IPAddressRange To(this IPAddress from, IPAddress to)
        {
            return new IPAddressRange(from, to);
        }
    }
}

namespace MinimalFirewall.TypedObjects.Internal
{
    internal static class Bits
    {
        public static byte[] Not(byte[] bytes) => bytes.Select(b => (byte)~b).ToArray();
        public static byte[] And(byte[] A, byte[] B) => A.Zip(B, (a, b) => (byte)(a & b)).ToArray();
        public static byte[] Or(byte[] A, byte[] B) => A.Zip(B, (a, b) => (byte)(a | b)).ToArray();
        public static bool GtECore(byte[] A, byte[] B, int offset = 0)
        {
            int length = Math.Min(A.Length, B.Length);
            for (var i = offset; i < length; i++)
            {
                if (A[i] != B[i]) return A[i] >= B[i];
            }
            return true;
        }
        public static bool LtECore(byte[] A, byte[] B, int offset = 0)
        {
            int length = Math.Min(A.Length, B.Length);
            for (var i = offset; i < length; i++)
            {
                if (A[i] != B[i]) return A[i] <= B[i];
            }
            return true;
        }
        public static byte[] GetBitMask(int sizeOfBuff, int bitLen)
        {
            var maskBytes = new byte[sizeOfBuff];
            int bytesLen = bitLen / 8;
            int bitsLen = bitLen % 8;
            for (var i = 0; i < bytesLen; i++) maskBytes[i] = 0xff;
            if (bitsLen > 0) maskBytes[bytesLen] = (byte)~(255 >> bitsLen);
            return maskBytes;
        }
        public static byte[] Increment(byte[] bytes)
        {
            var incrementIndex = Array.FindLastIndex(bytes, x => x < byte.MaxValue);
            if (incrementIndex < 0) throw new OverflowException();
            return bytes.Take(incrementIndex)
                        .Concat(new byte[] { (byte)(bytes[incrementIndex] + 1) })
                        .Concat(new byte[bytes.Length - incrementIndex - 1])
                        .ToArray();
        }
    }
}