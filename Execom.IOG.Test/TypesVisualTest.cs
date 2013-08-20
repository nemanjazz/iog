using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;
using Execom.IOG.Types;
using Execom.IOG;
using Execom.IOG.TypeVisual;
using Execom.IOG.Storage;
using System.IO;
using Execom.IOG.Services.Data;

namespace Execom.IOG.Test
{
    [TestClass]
    public class TypesVisualTest
    {

        public interface IAdress
        {
            string Adress { get; set; }
            Int32 ZipCode { get; set; }
            string City { get; set; }
            string Country { get; set; }
        }

        public interface IUserInfo
        {
            string FirstName { get; set; }
            [Immutable]
            string LastName { get; set; }
            Int64 JMBG { get; set; }
            IUser Users { get; set; }
        }

        public interface IUser
        {
            [PrimaryKey]
            [Immutable]
            Int32 Id { get; set; }
            string Username { get; set; }
            string Password { get; set; }
            IUserInfo UserInfo { get; set; }
            IAdress Adress { get; set; }
        }

        public interface IDataModel
        {
            [Immutable]
            ICollection<IUser> User { get; set; }
            IDictionary<Int32,Double> DoubleNumber { get; set; }
        }

        public interface IDataEnumModel
        {
            EnumsEnglish firstEnum { get; set; }
            EnumsSrpski secondEnum { get; set; }
            Int32 intNumber { get; set; }
        }

        public enum EnumsEnglish
        {
            FirstValue,
            SecondValue
        }

        public enum EnumsSrpski
        {
            PrvaVrednost,
            DrugaVrednost

        }

        [TestMethod]
        public void TestTextTemplateGenerationRemotely()
        {
            CreateDatFile();
            FileStream file = new FileStream("data.dat", FileMode.Open);
            IndexedFileStorage storage = new IndexedFileStorage(file, 256, true);
            TypesVisualisationService service = new TypesVisualisationService(storage);
            string content = service.GetGraphVizContentFromStorage(storage);
            System.IO.File.WriteAllText("templateOutputRemote.gv", content);
            Assert.IsTrue(true);

            storage.Dispose();
            file.Close();
        }

        private void CreateDatFile()
        {
            FileStream file = new FileStream("data.dat", FileMode.Create);
            IndexedFileStorage storage = new IndexedFileStorage(file, 256, true);
            Context ctx = new Context(typeof(IDataModel), null, storage);

            using (var ws = ctx.OpenWorkspace<IDataModel>(IsolationLevel.Exclusive))
            {
                IDataModel database = ws.Data;
                ws.Commit();
            }

            ctx.Dispose();
            storage.Dispose();
            file.Close();
        }
    }
}
