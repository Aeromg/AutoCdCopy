using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace AutoCdCopy.Logics
{
    public class CdInfoRepo : List<CdInfo>
    {
        public CdInfo GetByLabelAndSerial(string label, string serial)
        {
            return
                (from info in this
                 where info.Volume == label && info.Serial == serial
                 select info).FirstOrDefault();
        }

        public void Update(CdInfo item) {
            var existingItem = GetByLabelAndSerial(item.Volume, item.Serial);
            if (existingItem != null)
                Remove(existingItem);

            base.Add(item);
        }

        public static void SaveToFile(CdInfoRepo repo, string file)
        {
            var items = repo.ToArray();
            var serializer = new XmlSerializer(items.GetType());
            var bakFileName = file + ".bak";
            if(File.Exists(file))
                File.Move(file, bakFileName);
            using (var output = File.OpenWrite(file))
            {
                serializer.Serialize(output, items);
                output.Flush();
                output.Close();
            }
            File.Delete(bakFileName);
        }
        public static CdInfoRepo ReadFromFile(string file)
        {
            if (!File.Exists(file))
                return new CdInfoRepo();

            var serializer = new XmlSerializer(typeof(CdInfo[]));
            CdInfo[] infos;

            using (var input = File.OpenRead(file))
            {
                infos = (CdInfo[])serializer.Deserialize(input);
            }

            var repo = new CdInfoRepo();
            foreach (var info in infos)
                repo.Add(info);

            return repo;
        }
        public static CdInfoRepo Demo()
        {
            var repo = new CdInfoRepo();

            repo.Add(new CdInfo()
            {
                Serial = "123",
                InitDate = DateTime.Now
            });

            repo.Add(new CdInfo()
            {
                Serial = "141423",
                InitDate = DateTime.Now
            });

            return repo;
        }
    }
}
