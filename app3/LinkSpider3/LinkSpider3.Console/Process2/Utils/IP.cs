﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace LinkSpider3.Process2.Utils
{
    public class IP
    {
        // Resolve host and return only one IP
        public static string GetIPAddress(string host)
        {
            try
            {
                var addrs = Dns.GetHostEntry(host).AddressList;
                if (addrs.Length > 0)
                    return addrs[0].ToString();
            }
            catch { }

            return string.Empty;
        }

        public static string GetIPClassFamily2(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return "unknown";

            IPAddressRange range = new IPAddressRange(IPAddress.Parse("0.0.0.0"), IPAddress.Parse("127.255.255.255"));
            if (range.IsInRange(IPAddress.Parse(ipAddress)))
            {
                return "A";
            }
            else
            {
                range = new IPAddressRange(IPAddress.Parse("128.0.0.0"), IPAddress.Parse("191.255.255.255"));
                if (range.IsInRange(IPAddress.Parse(ipAddress)))
                {
                    return "B";
                }
                else
                {
                    range = new IPAddressRange(IPAddress.Parse("192.0.0.0"), IPAddress.Parse("223.255.255.255"));
                    if (range.IsInRange(IPAddress.Parse(ipAddress)))
                    {
                        return "C";
                    }
                    else
                    {
                        return "unknown";
                    }
                }
            }
        }

        public static string GetIPClassFamily(string ipAddress)
        {
            //Reference: http://en.wikipedia.org/wiki/Reserved_IP_addresses

            //if the ip address string is empty or null string, we consider it to be non-routable
            if (String.IsNullOrEmpty(ipAddress))
            {
                return "unknown";
            }

            //if we cannot parse the Ipaddress, then we consider it non-routable
            IPAddress tempIpAddress = null;
            if (!IPAddress.TryParse(ipAddress, out tempIpAddress))
            {
                return "unknown";
            }

            byte[] ipAddressBytes = tempIpAddress.GetAddressBytes();

            //if ipAddress is IPv4
            if (tempIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                if (IsIpAddressInRange(ipAddressBytes, "10.0.0.0/8")) //Class A Private network check
                {
                    return "A";
                }
                else if (IsIpAddressInRange(ipAddressBytes, "172.16.0.0/12")) //Class B private network check
                {
                    return "B";
                }
                else if (IsIpAddressInRange(ipAddressBytes, "192.168.0.0/16")) //Class C private network check
                {
                    return "C";
                }
                else if (IsIpAddressInRange(ipAddressBytes, "127.0.0.0/8")) //Loopback
                {
                    return "loopback";
                }
                else if (IsIpAddressInRange(ipAddressBytes, "0.0.0.0/8"))   //reserved for broadcast messages
                {
                    return "broadcast";
                }
            }

            return "unknown";
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <remarks>A null or empty string passed as the ipAddress will return true. An invalid ipAddress will be returned as true. </remarks>
        /// <returns></returns>
        public static bool IsNonRoutableIpAddress(string ipAddress)
        {
            //Reference: http://en.wikipedia.org/wiki/Reserved_IP_addresses

            //if the ip address string is empty or null string, we consider it to be non-routable
            if (String.IsNullOrEmpty(ipAddress))
            {
                return true;
            }

            //if we cannot parse the Ipaddress, then we consider it non-routable
            IPAddress tempIpAddress = null;
            if (!IPAddress.TryParse(ipAddress, out tempIpAddress))
            {
                return true;
            }

            byte[] ipAddressBytes = tempIpAddress.GetAddressBytes();

            //if ipAddress is IPv4
            if (tempIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                if (IsIpAddressInRange(ipAddressBytes, "10.0.0.0/8")) //Class A Private network check
                {
                    return true;
                }
                else if (IsIpAddressInRange(ipAddressBytes, "172.16.0.0/12")) //Class B private network check
                {
                    return true;
                }
                else if (IsIpAddressInRange(ipAddressBytes, "192.168.0.0/16")) //Class C private network check
                {
                    return true;
                }
                else if (IsIpAddressInRange(ipAddressBytes, "127.0.0.0/8")) //Loopback
                {
                    return true;
                }
                else if (IsIpAddressInRange(ipAddressBytes, "0.0.0.0/8"))   //reserved for broadcast messages
                {
                    return true;
                }

                //its routable if its ipv4 and meets none of the criteria
                return false;
            }
            //if ipAddress is IPv6
            else if (tempIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                //incomplete
                if (IsIpAddressInRange(ipAddressBytes, "::/128"))       //Unspecified address
                {
                    return true;
                }
                else if (IsIpAddressInRange(ipAddressBytes, "::1/128"))     //lookback address for localhost
                {
                    return true;
                }
                else if (IsIpAddressInRange(ipAddressBytes, "2001:db8::/32"))   //Addresses used in documentation
                {
                    return true;
                }

                return false;
            }
            else
            {
                //we default to non-routable if its not Ipv4 or Ipv6
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddressBytes"></param>
        /// <param name="reservedIpAddress"></param>
        /// <returns></returns>
        private static bool IsIpAddressInRange(byte[] ipAddressBytes, string reservedIpAddress)
        {
            if (String.IsNullOrEmpty(reservedIpAddress))
            {
                return false;
            }

            if (ipAddressBytes == null)
            {
                return false;
            }

            //Split the reserved ip address into a bitmask and ip address
            string[] ipAddressSplit = reservedIpAddress.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (ipAddressSplit.Length != 2)
            {
                return false;
            }

            string ipAddressRange = ipAddressSplit[0];

            IPAddress ipAddress = null;
            if (!IPAddress.TryParse(ipAddressRange, out ipAddress))
            {
                return false;
            }

            // Convert the IP address to bytes.
            byte[] ipBytes = ipAddress.GetAddressBytes();

            //parse the bits
            int bits = 0;
            if (!int.TryParse(ipAddressSplit[1], out bits))
            {
                bits = 0;
            }

            // BitConverter gives bytes in opposite order to GetAddressBytes().
            byte[] maskBytes = null;
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                uint mask = ~(uint.MaxValue >> bits);
                maskBytes = BitConverter.GetBytes(mask).Reverse().ToArray();
            }
            else if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                //128 places
                BitArray bitArray = new BitArray(128, false);

                //shift <bits> times to the right
                ShiftRight(bitArray, bits, true);

                //turn into byte array
                maskBytes = ConvertToByteArray(bitArray).Reverse().ToArray();
            }


            bool result = true;

            //Calculate
            for (int i = 0; i < ipBytes.Length; i++)
            {
                result &= (byte)(ipAddressBytes[i] & maskBytes[i]) == ipBytes[i];

            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitArray"></param>
        /// <param name="shiftN"></param>
        /// <param name="fillValue"></param>
        private static void ShiftRight(BitArray bitArray, int shiftN, bool fillValue)
        {
            for (int i = shiftN; i < bitArray.Count; i++)
            {
                bitArray[i - shiftN] = bitArray[i];
            }

            //fill the shifted bits as false
            for (int index = bitArray.Count - shiftN; index < bitArray.Count; index++)
            {
                bitArray[index] = fillValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitArray"></param>
        /// <returns></returns>
        private static byte[] ConvertToByteArray(BitArray bitArray)
        {
            // pack (in this case, using the first bool as the lsb - if you want
            // the first bool as the msb, reverse things ;-p)
            int bytes = (bitArray.Length + 7) / 8;
            byte[] arr2 = new byte[bytes];
            int bitIndex = 0;
            int byteIndex = 0;

            for (int i = 0; i < bitArray.Length; i++)
            {
                if (bitArray[i])
                {
                    arr2[byteIndex] |= (byte)(1 << bitIndex);
                }

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return arr2;
        }



        private class IPAddressRange
        {
            AddressFamily addressFamily;
            byte[] lowerBytes;
            byte[] upperBytes;

            public IPAddressRange(IPAddress lower, IPAddress upper)
            {
                // Assert that lower.AddressFamily == upper.AddressFamily

                this.addressFamily = lower.AddressFamily;
                this.lowerBytes = lower.GetAddressBytes();
                this.upperBytes = upper.GetAddressBytes();
            }

            public bool IsInRange(IPAddress address)
            {
                if (address.AddressFamily != addressFamily)
                {
                    return false;
                }

                byte[] addressBytes = address.GetAddressBytes();

                bool lowerBoundary = true, upperBoundary = true;

                for (int i = 0; i < this.lowerBytes.Length && 
                    (lowerBoundary || upperBoundary); i++)
                {
                    if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) ||
                        (upperBoundary && addressBytes[i] > upperBytes[i]))
                    {
                        return false;
                    }

                    lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
                    upperBoundary &= (addressBytes[i] == upperBytes[i]);
                }

                return true;
            }
        }
    }

}
