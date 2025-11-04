using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Services.Mapping
{
    public static class MapsterIdHelper
    {
        public static void RegisterIdMappings(TypeAdapterConfig config, Assembly modelsAssembly, Assembly dtoAssembly)
        {
            var modelTypes = modelsAssembly.GetTypes()
                .Where(t => t.IsClass && t.GetProperty("Id") != null)
                .ToList();

            foreach (var modelType in modelTypes)
            {
                var modelName = modelType.Name;

                // Tìm tất cả DTO có property trùng với ModelName + "Id"
                var dtoTypes = dtoAssembly.GetTypes()
                    .Where(t => t.IsClass && (
                        t.GetProperty($"{modelName}Id", BindingFlags.Public | BindingFlags.Instance) != null
                    ))
                    .ToList();

                foreach (var dtoType in dtoTypes)
                {
                    // Tạo mapping Model -> DTO
                    config.NewConfig(modelType, dtoType)
                          .Map($"{modelName}Id", "Id");

                    // Nếu DTO có thể map ngược (ví dụ request), tạo mapping ngược DTO -> Model
                    config.NewConfig(dtoType, modelType)
                          .Map("Id", $"{modelName}Id");
                }
            }
        }
    }
}