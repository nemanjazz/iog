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
        public void TestTextTemplateGenerationLocally()
        {
            Context ctx = new Context(typeof(IDataModel));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText("templateOutputLocal.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateGenerationRemotely()
        {
            CreateDatFile();
            FileStream file = new FileStream("data.dat", FileMode.Open);
            IndexedFileStorage storage = new IndexedFileStorage(file, 256, true);
            string content = Context.GetGraphVizContentFromStorage(storage);
            System.IO.File.WriteAllText("templateOutputRemote.gv", content);
            Assert.IsTrue(true);

            storage.Dispose();
            file.Close();
        }

        [TestMethod]
        public void TestTextTemplateCollectionDataModel()
        {
            Context ctx = new Context(typeof(UseCases.CollectionDataModelTest.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText("templateOutputCollectionDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateDictionaryDataModel()
        {
            Context ctx = new Context(typeof(UseCases.DictionaryDataModelTest.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText("templateOutputDictionaryDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateIndexedCollectionDataModel()
        {
            Context ctx = new Context(typeof(UseCases.IndexedCollectionDataModelTest.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText("templateOutputIndexedCollectionDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateInheritanceDataModel()
        {
            Context ctx = new Context(typeof(UseCases.InheritanceDataModelTest.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText("templateOutputInheritanceDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplatePlainDataModel()
        {
            Context ctx = new Context(typeof(UseCases.PlainDatamodelTest.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText("templateOutputPlainDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateRecursiveDataModel()
        {
            Context ctx = new Context(typeof(UseCases.RecursiveDataModelTest.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText("templateOutputRecursiveDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateScalarCollectionDataModel()
        {
            Context ctx = new Context(typeof(UseCases.ScalarCollectionDataModel.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText("templateOutputScalarCollectionDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateScalarSetCollectionDataModel()
        {
            Context ctx = new Context(typeof(UseCases.ScalarSetCollectionDataModel.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText("templateOutputScalarSetCollectionDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestChildrenAndParentDictionaries()
        {
            Context ctx = new Context(typeof(IDataModel));
            IDictionary<String, TypeVisualUnit> units = ctx.GetTypeVisualUnits(ctx.GetRootTypeId());
            IDictionary<TypeVisualUnit, ICollection<TypeVisualUnit>> childrenDictionary, parentsDictionary;
            TypeVisualUtilities.GetChildrenAndParentsDictonaryOfTypes(units, out childrenDictionary, out parentsDictionary);
            Assert.IsTrue(childrenDictionary.Count == 4);
            Assert.IsTrue(parentsDictionary.Count == 4);

        }

        [TestMethod]
        public void TestEnumsTextTemplate()
        {
            Context ctx = new Context(typeof(IDataEnumModel));
            IDictionary<String, TypeVisualUnit> units = ctx.GetTypeVisualUnits(ctx.GetRootTypeId());
            Assert.IsTrue(units.Values.Count == 3);

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
