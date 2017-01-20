using System;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Xml;
using System.Xml.Schema;
using System.IO;

namespace SincronizacaoMusical.Util
{
    public static class ValidacaoXML
    {
        public static bool ValidarXml(string xmlFilename, string schemaFilename)
        {

            // Create the XmlSchemaSet class.
            XmlSchemaSet sc = new XmlSchemaSet();

            // Add the schema to the collection.
            sc.Add(null, schemaFilename);

            // Set the validation settings.
            XmlReaderSettings settings = new XmlReaderSettings
                                             {
                                                 ValidationType = ValidationType.Schema, 
                                                 Schemas = sc
                                             };
            settings.ValidationEventHandler += ValidationCallBack;

            // Create the XmlReader object.
            XmlReader reader = XmlReader.Create(xmlFilename, settings);

            // Parse the file.  
            while (reader.Read())
            {
            }
            return true;

        }

        // Display any validation errors. 
        private static void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            throw new Exception("Validation Error: "+ e.Message);
        }
    }
}