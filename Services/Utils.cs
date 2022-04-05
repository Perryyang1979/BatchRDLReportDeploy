using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace BatchRDLReportDeploy.Services
{
    public class Utils
    {
        /// <summary>
        ///     Serializes an object.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize</typeparam>
        /// <param name="serializableObject">The actual object to serialize</param>
        /// <param name="fileName">Where to save serialized object</param>
        public static void SerializeObject<T>(T serializableObject, string fileName)
        {
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (serializableObject == null)
                return;

            var xmlDocument = new XmlDocument();
            var serializer = new XmlSerializer(serializableObject.GetType());
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, serializableObject);
                stream.Position = 0;
                xmlDocument.Load(stream);
                xmlDocument.Save(fileName);
                stream.Flush();
                stream.Close();
            }
        }

        public static T DeSerializeObject<T>(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return default(T);

            T objectOut = default(T);

            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (var read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    var serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return objectOut;
        }
    }
}
