using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Execom.IOG.Attributes;
using Execom.IOG.Types;
using Execom.IOG;
using Execom.IOG.TypeVisualisation;
using Execom.IOG.Storage;
using System.IO;

namespace Execom.IOG.Test
{
    [TestClass]
    public class TypesVisualisationTest
    {

        private static readonly string testResultsLocation = "d:\\TestFiles\\";

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
            ICollection<IUser> Users { get; set; }
        }

        public interface IUser
        {
            [PrimaryKey]
            [Immutable]
            Int32 Id { get; set; }
            string Username { get; set; }
            //IDictionary<Int32,IDataModel> DataModels { get; set; }
            string Password { get; set; }
            IUserInfo UserInfo { get; set; }
            IDictionary<Int32,IAdress> Adress { get; set; }
        }

        public interface IDataModel
        {
            [Immutable]
            IIndexedCollection<IUser> User { get; set; }
            IDictionary<Int32,Double> DoubleNumber { get; set; }
            //ICollection<Int32> Int32Collection { get; set; }
        }


        //[TestMethod]
        //public void TestTypeNameExtractionLocally()
        //{
        //    Context ctx = new Context(typeof(IDataModel));
        //   // List<TypeVisualisationUnit> typeUnits = ctx.GetTypeVisualisationUnitsOfContext();
        //    Assert.IsTrue(typeUnits.Count == 4);
        //    //Assert.AreEqual(propertyNames[0], "User");
        //    //Assert.AreEqual(propertyTypes[0], "IUser");
        //}

        [TestMethod]
        public void TestTextTemplateGenerationLocally()
        {
            Context ctx = new Context(typeof(IDataModel));
            string content = ctx.getGraphVizContent("IUser");
            System.IO.File.WriteAllText(testResultsLocation + "templateOutputLocal.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateGenerationRemotely()
        {
            Context ctx = null;
            FileStream file = new FileStream(testResultsLocation + "data.dat", FileMode.Open);
            IndexedFileStorage storage = new IndexedFileStorage(file, 256, true);
            string content = Context.GetGraphVizContentFromStorage(storage);
            System.IO.File.WriteAllText(testResultsLocation + "templateOutputRemote.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateCollectionDataModel()
        {
            Context ctx = new Context(typeof(UseCases.CollectionDataModelTest.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText(testResultsLocation + "templateOutputCollectionDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateDictionaryDataModel()
        {
            Context ctx = new Context(typeof(UseCases.DictionaryDataModelTest.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText(testResultsLocation + "templateOutputDictionaryDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateIndexedCollectionDataModel()
        {
            Context ctx = new Context(typeof(UseCases.IndexedCollectionDataModelTest.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText(testResultsLocation + "templateOutputIndexedCollectionDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateInheritanceDataModel()
        {
            Context ctx = new Context(typeof(UseCases.InheritanceDataModelTest.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText(testResultsLocation + "templateOutputInheritanceDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplatePlainDataModel()
        {
            Context ctx = new Context(typeof(UseCases.PlainDatamodelTest.IDatabase));
            string content = ctx.getGraphVizContent("IEvent");
            System.IO.File.WriteAllText(testResultsLocation + "templateOutputPlainDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateRecursiveDataModel()
        {
            Context ctx = new Context(typeof(UseCases.RecursiveDataModelTest.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText(testResultsLocation + "templateOutputRecursiveDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateScalarCollectionDataModel()
        {
            Context ctx = new Context(typeof(UseCases.ScalarCollectionDataModel.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText(testResultsLocation + "templateOutputScalarCollectionDataModel.gv", content);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestTextTemplateScalarSetCollectionDataModel()
        {
            Context ctx = new Context(typeof(UseCases.ScalarSetCollectionDataModel.IDatabase));
            string content = ctx.getGraphVizContent();
            System.IO.File.WriteAllText(testResultsLocation + "templateOutputScalarSetCollectionDataModel.gv", content);
            Assert.IsTrue(true);
        }
    }
}
