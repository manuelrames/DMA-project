using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ShotsDetect
{
    class ShotsXml
    {
        #region Member variables
        public String FilePath;
        public int method;
        public int param1;
        public int param2;

        XmlDocument xmlDoc = null;
        #endregion

        /// <summary>
        /// Initialize the XML file and save the file
        /// </summary>
        /// <param name="filepath">the path use to save the XML file</param>
        /// <param name="videoname">the name of the video</param>
        public void createXML(String filepath, String videoname, int method, double p1, double p2)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null));

            XmlNode rootNode = xmlDoc.CreateElement("ShotDetection");
            XmlAttribute file = xmlDoc.CreateAttribute("file");
            file.Value = videoname;
            rootNode.Attributes.Append(file);
            xmlDoc.AppendChild(rootNode);

            XmlNode methodNode = xmlDoc.CreateElement("method");
            XmlAttribute nr = xmlDoc.CreateAttribute("nr");
            nr.Value = method.ToString();
            methodNode.Attributes.Append(nr);
            rootNode.AppendChild(methodNode);

            XmlNode param1 = xmlDoc.CreateElement("param1");
            param1.InnerText = p1.ToString();
            methodNode.AppendChild(param1);
            XmlNode param2 = xmlDoc.CreateElement("param2");
            param2.InnerText = p2.ToString();
            methodNode.AppendChild(param2);

            XmlNode shotsNode = xmlDoc.CreateElement("shots");
            rootNode.AppendChild(shotsNode);

            FilePath = filepath;
            xmlDoc.Save(@FilePath);
        }

        /// <summary>
        /// Add each shot information to the XML file
        /// </summary>
        /// <param name="shot"></param>
        /// <param name="tags"></param>
        public void addShot(Shot shot, List<String> tags)
        {
            xmlDoc.Load(@FilePath);

            /* search the shot node */
            XmlNode shotsNode = xmlDoc.SelectSingleNode("//ShotDetection//shots");
            XmlNode singleShotNode = xmlDoc.CreateElement("shot");
            XmlAttribute frame = xmlDoc.CreateAttribute("frame");
            frame.Value = shot.frame1 + "-" + shot.frame2;
            singleShotNode.Attributes.Append(frame);
            //singleShotNode.InnerText = shot.frame1 + "-" + shot.frame2;
            shotsNode.AppendChild(singleShotNode);

            /* add the tags node */
            XmlNode xmlTags = xmlDoc.CreateElement("tags");
            singleShotNode.AppendChild(xmlTags);

            if (tags != null)
            {
                for (int i = 0; i < tags.Count; i++)
                {
                    XmlNode singleTag = xmlDoc.CreateElement("tag");
                    singleTag.InnerText = tags[i];
                    xmlTags.AppendChild(singleTag);
                }
            }

            xmlDoc.Save(@FilePath);
        }

        public void searchXML()
        {
        }
    }
}
