using System.IO;
using System.Threading.Tasks;

namespace MAD.Project
{
    public class DataMatrixCsvParser
    {
        public async Task<DataMatrix> LoadAsync(string filename, char separator = ',')
        {
            using (var sr = new StreamReader(filename))
            {
                int id = 0;
                var result = new DataMatrix((await sr.ReadLineAsync()).Split(separator));

                DataMatrixRow ParseLine(string l)
                {
                    id++;
                    var attributes = l.Split(separator);
                    return new DataMatrixRow(id, attributes);
                }

                string line;
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    var record = ParseLine(line);
                    result.Add(record);
                }
                return result;
            }
        }
    }
}
