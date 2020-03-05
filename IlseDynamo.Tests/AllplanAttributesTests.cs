using System;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Globalization;

using Allplan.Data;

namespace Allplan
{
    [TestClass]
    public class AllplanAttributesTests
    {
        [DeploymentItem(@"Resources\AttributeDefinitionCollectionLocal_de.xml")]
        [TestMethod]
        public void TestAttributeDefinitionCollectionLocal_de()
        {
            Assert.IsTrue(File.Exists("AttributeDefinitionCollectionLocal_de.xml"));
            var def1 = AttributeDefinitionCollection.ReadFrom("AttributeDefinitionCollectionLocal_de.xml");
            Assert.IsNotNull(def1);

            def1.SaveTo("Cloned_AttributeDefinitionCollectionLocal_de.xml");
            Assert.IsTrue(File.Exists("Cloned_AttributeDefinitionCollectionLocal_de.xml"));

            var def2 = AttributeDefinitionCollection.ReadFrom("Cloned_AttributeDefinitionCollectionLocal_de.xml");

            for (int i=0; i<def1.AttributeDefinition.Count; i++)
            {
                Assert.AreEqual(def1.AttributeDefinition[i], def2.AttributeDefinition[i], $"Definition #{i} is equal");
            }
        }

        [DeploymentItem(@"Resources\AttributeFavourite.atfanfx")]
        [TestMethod]
        public void TestAttributeFavourites()
        {
            Assert.IsTrue(File.Exists("AttributeFavourite.atfanfx"));
            var fav1 = AllplanAttributesContainer.ReadFrom("AttributeFavourite.atfanfx");
            Assert.IsNotNull(fav1);

            fav1.WriteTo("Cloned_AttributeFavourite.atfanfx");
            var fav2 = AllplanAttributesContainer.ReadFrom("AttributeFavourite.atfanfx");
            Assert.IsNotNull(fav2);

            for (int i = 0; i < fav1.AttributeSet.Attributes.Count; i++)
            {
                Assert.AreEqual(fav1.AttributeSet.Attributes[i], fav2.AttributeSet.Attributes[i], $"Attribute #{i} is equal");
            }
        }
    }
}
