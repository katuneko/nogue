using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Nogue.Gameplay.Contracts
{
    public static class ContractsLoader
    {
        public static List<ContractDTO> Load(string path)
        {
            var list = new List<ContractDTO>();
            if (!File.Exists(path)) return list;
            try
            {
                var json = File.ReadAllText(path);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<ContractDTO>>(json, opts) ?? list;
            }
            catch { return list; }
        }
    }
}

