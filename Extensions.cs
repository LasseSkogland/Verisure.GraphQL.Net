using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;

namespace Verisure.GraphQL {
    public static class Extensions {
        public static StringBuilder Indent(this StringBuilder sb, int indentation) {
            for (int i = 0; i < indentation * 2; i++) sb.Append(' ');
            return sb;
        }

        public static SecureString ToSecureString(this string value) {
            var securePassword = new SecureString();
            foreach (char c in value) securePassword.AppendChar(c);
            securePassword.MakeReadOnly();
            return securePassword;
        }

        public static string ToClearTextString(this SecureString value) {
            IntPtr valuePtr = IntPtr.Zero;
            try {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        public static dynamic ParseValue(this JsonElement el) {
            switch (el.ValueKind) {
                case JsonValueKind.Object:
                    return el.CreateDictionary();
                case JsonValueKind.Array:
                    var list = new List<dynamic>();
                    foreach (var item in el.EnumerateArray()) {
                        list.Add(item.ParseValue());
                    }
                    return list.ToArray();

                case JsonValueKind.Number:
                    return el.GetDouble();

                case JsonValueKind.String:
                    return el.GetString();

                case JsonValueKind.False:
                case JsonValueKind.True:
                    return el.GetBoolean();

                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                default:
                    return null;
            }
        }

        public static IDictionary<string, dynamic> CreateDictionary(this JsonElement el) {
            if (el.ValueKind == JsonValueKind.Object) {
                var output = new Dictionary<string, dynamic>();
                foreach (var item in el.EnumerateObject()) {
                    output.Add(item.Name, item.Value.ParseValue());
                }
                return output;
            }
            return default;
        }
    }
}
