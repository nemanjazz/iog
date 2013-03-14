using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.ObjectModel;

namespace Execom.IOG.Test
{
    [TestClass]
    public class IndexedFileTest
    {
        private string TestFileName()
        {
            return "data.dat";
        }

        private Collection<Guid> keys = new Collection<Guid>();
        private int nrItems = 750;

        [TestMethod]
        public void TestDataConsistency()
        {
            for (int i = 0; i < nrItems; i++)
            {
                keys.Add(Guid.NewGuid());
            }           
            //Write file
            WriteTestData();
            //Read back
            ReadTestData();

            for (int iterration = 0; iterration < 10; iterration++)
            {
                //Modify a record
                ModifyTestData(iterration);
                //Test the modification
                ReadModifiedTestData(iterration);
            }
        }        

        private void WriteTestData()
        {
            FileStream fs = new FileStream(TestFileName(), FileMode.Create);
            IndexedFile.IndexedFile file = new IndexedFile.IndexedFile(fs, 256, true);

            for (int i = 0; i < nrItems; i++)
            {                
                byte[] buffer = new byte[i];
                for (int j = 0; j < buffer.Length; j++)
                    buffer[j] = (byte)(j % 256);
                file.Write(keys[i], buffer, 0, buffer.Length);
            }

            file.Dispose();
            fs.Close();
        }

        private void ModifyTestData(int iterration)
        {
            FileStream fs = new FileStream(TestFileName(), FileMode.Open);
            IndexedFile.IndexedFile file = new IndexedFile.IndexedFile(fs, 256, true);

            for (int i = 0; i < nrItems; i++)
            {
                byte[] buffer = new byte[i + iterration];
                for (int j = 0; j < buffer.Length; j++)
                    buffer[j] = (byte)(j % (1+iterration % 3));
                file.Write(keys[i], buffer, 0, buffer.Length);
            }

            file.Dispose();
            fs.Close();
        }

        private void ReadTestData()
        {
            FileStream fs = new FileStream(TestFileName(), FileMode.Open);
            IndexedFile.IndexedFile file = new IndexedFile.IndexedFile(fs, 256, true);

            for (int i = 0; i < nrItems; i++)
            {
                byte[] buffer = file.Read(keys[i]);

                Assert.IsTrue(i == buffer.Length);

                for (int j = 0; j < buffer.Length; j++)
                    Assert.IsTrue(buffer[j] == (byte)(j % 256));
            }

            file.Dispose();
            fs.Close();
        }

        private void ReadModifiedTestData(int iterration)
        {
            FileStream fs = new FileStream(TestFileName(), FileMode.Open);
            IndexedFile.IndexedFile file = new IndexedFile.IndexedFile(fs, 256, true);

            for (int i = 0; i < nrItems; i++)
            {
                byte[] buffer = file.Read(keys[i]);

                Assert.IsTrue(i + iterration == buffer.Length);

                for (int j = 0; j < buffer.Length; j++)
                    Assert.IsTrue(buffer[j] == (byte)(j % (1+iterration % 3)));
            }

            file.Dispose();
            fs.Close();
        }
    }
}
