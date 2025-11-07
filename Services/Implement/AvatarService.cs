using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repositories.DBContext;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implement
{
    public class AvatarService : IAvatarService
    {
        private readonly EVBatteryTradingContext _db;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _cfg;

        public AvatarService(EVBatteryTradingContext db, IHttpClientFactory httpFactory, IConfiguration cfg)
        {
            _db = db;
            _httpFactory = httpFactory;
            _cfg = cfg;
        }

        // -------- public API --------
        public Task<string> UploadUserAvatarAsync(Guid userId, IFormFile file, CancellationToken ct = default)
            => UploadAndSaveAsync("users", userId, file,
                                  findEntity: async () => await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, ct),
                                  saveUrl: async url =>
                                  {
                                      var u = await _db.Users.FirstAsync(x => x.Id == userId, ct);
                                      u.AvatarUrl = url;
                                      await _db.SaveChangesAsync(ct);
                                  },
                                  ct);

        public Task<string> UploadVehicleAvatarAsync(Guid vehicleId, IFormFile file, CancellationToken ct = default)
            => UploadAndSaveAsync("vehicles", vehicleId, file,
                                  findEntity: async () => await _db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId, ct),
                                  saveUrl: async url =>
                                  {
                                      var v = await _db.Vehicles.FirstAsync(x => x.Id == vehicleId, ct);
                                      v.AvatarUrl = url;
                                      await _db.SaveChangesAsync(ct);
                                  },
                                  ct);

        public Task<string> UploadBatteryAvatarAsync(Guid batteryId, IFormFile file, CancellationToken ct = default)
            => UploadAndSaveAsync("batteries", batteryId, file,
                                  findEntity: async () => await _db.Batteries.FirstOrDefaultAsync(x => x.Id == batteryId, ct),
                                  saveUrl: async url =>
                                  {
                                      var b = await _db.Batteries.FirstAsync(x => x.Id == batteryId, ct);
                                      b.AvatarUrl = url;
                                      await _db.SaveChangesAsync(ct);
                                  },
                                  ct);

        // -------- core helpers --------
        private async Task<string> UploadAndSaveAsync(
            string kind, Guid id, IFormFile file,
            Func<Task<object?>> findEntity,
            Func<string, Task> saveUrl,
            CancellationToken ct)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("Empty file");
            if (await findEntity() is null) throw new InvalidOperationException($"{kind} not found");

            var (baseUrl, serviceKey, bucket) = ReadSupabaseEnv();
            var objectPath = BuildObjectPath(kind, id, file.FileName);
            var publicUrl = await UploadToSupabaseAsync(baseUrl, serviceKey, bucket, objectPath, file, ct);

            await saveUrl(publicUrl);
            return publicUrl;
        }

        private (string baseUrl, string serviceKey, string bucket) ReadSupabaseEnv()
        {
            var baseUrl = _cfg["SUPABASE_URL"] ?? throw new InvalidOperationException("Missing SUPABASE_URL");
            var service = _cfg["SUPABASE_SERVICE_KEY"] ?? throw new InvalidOperationException("Missing SUPABASE_SERVICE_KEY");
            var bucket = _cfg["SUPABASE_BUCKET"] ?? "evb";
            return (baseUrl.TrimEnd('/'), service, bucket);
        }

        private static string BuildObjectPath(string kind, Guid id, string fileName)
        {
            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".png";
            return $"avatars/{kind}/{id}{ext.ToLowerInvariant()}";
        }

        private async Task<string> UploadToSupabaseAsync(
            string baseUrl, string serviceKey, string bucket,
            string objectPath, IFormFile file, CancellationToken ct)
        {
            var client = _httpFactory.CreateClient();
            var url = $"{baseUrl}/storage/v1/object/{bucket}/{Uri.EscapeDataString(objectPath)}";

            using var form = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            form.Add(streamContent, "file", file.FileName); // field name phải là "file"

            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = form
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceKey);
            req.Headers.Add("x-upsert", "true"); // overwrite nếu đã tồn tại

            var res = await client.SendAsync(req, ct);
            res.EnsureSuccessStatusCode();

            return $"{baseUrl}/storage/v1/object/public/{bucket}/{objectPath}";
        }
    }
}
