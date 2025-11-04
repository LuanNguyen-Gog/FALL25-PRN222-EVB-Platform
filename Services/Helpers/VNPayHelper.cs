using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Services.Helpers
{
    public static class VNPayHelper
    {
        public static string BuildPaymentUrl(
            string baseUrl,
            string tmnCode,
            string hashSecret,
            string returnUrl,
            string ipnUrl,
            long amountTimes100,
            string txnRef,
            string orderInfo,
            string clientIp,
            DateTime? expireUtc = null)
        {
            var dict = new SortedDictionary<string, string>
            {
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "pay",
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = amountTimes100.ToString(),
                ["vnp_CurrCode"] = "VND",
                ["vnp_TxnRef"] = txnRef,
                ["vnp_OrderInfo"] = orderInfo,
                ["vnp_OrderType"] = "other",
                ["vnp_Locale"] = "vn",
                ["vnp_IpAddr"] = clientIp ?? "0.0.0.0",
                ["vnp_ReturnUrl"] = returnUrl,
                ["vnp_CreateDate"] = DateTime.UtcNow.ToString("yyyyMMddHHmmss")
            };

            if (!string.IsNullOrWhiteSpace(ipnUrl))
                dict["vnp_UrlNotify"] = ipnUrl;

            if (expireUtc.HasValue)
                dict["vnp_ExpireDate"] = expireUtc.Value.ToString("yyyyMMddHHmmss");

            var query = BuildQuery(dict);
            var secureHash = HmacSHA512(hashSecret, query);
            return $"{baseUrl}?{query}&vnp_SecureHash={secureHash}";
        }

        public static bool ValidateReturn(IQueryCollection query, string hashSecret, out Dictionary<string, string> data)
        {
            data = query.ToDictionary(k => k.Key, v => v.Value.ToString());
            if (!data.TryGetValue("vnp_SecureHash", out var secureHash)) return false;

            data.Remove("vnp_SecureHash");
            var filtered = new SortedDictionary<string, string>(data
                .Where(kv => kv.Key.StartsWith("vnp_"))
                .ToDictionary(kv => kv.Key, kv => kv.Value));

            var raw = BuildQuery(filtered);
            var myHash = HmacSHA512(hashSecret, raw);
            return string.Equals(myHash, secureHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildQuery(IDictionary<string, string> dict)
            => string.Join("&", dict.OrderBy(k => k.Key)
                                    .Select(kv => $"{kv.Key}={HttpUtility.UrlEncode(kv.Value)}"));

        private static string HmacSHA512(string key, string rawData)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }
        public static string Get(Dictionary<string, string> dict, string key)
        {
            return dict.TryGetValue(key, out var value) ? value : string.Empty;
        }

        public static long GetAmountTimes100(Dictionary<string, string> dict)
        {
            if (dict.TryGetValue("vnp_Amount", out var value) && long.TryParse(value, out var amount))
                return amount;
            return 0;
        }
    }
}
