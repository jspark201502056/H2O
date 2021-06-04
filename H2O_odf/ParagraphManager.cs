﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace H2O__
{
    public class ParagraphManager
    {
        private const string header_style = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
        private const string header_fo = "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0";
        private const string header_office = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
        private const string header_text = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";

        public ParagraphManager()
        {
        }

        //줄 간격
        public void SetLineSpace(string name, XmlDocument doc, int lineSpace)
        {
            XmlNodeList list = doc.GetElementsByTagName("style", header_style);
            XmlElement e = ((XmlElement)list.Item(0));

            string check_name = e.GetAttribute("name", header_style);
            if (check_name.Equals(name))
            {
                XmlElement e1 = null;
                string type = e.GetAttribute("family", header_style);

                e1 = (XmlElement)e.GetElementsByTagName(type + "-properties", header_style).Item(0);
                if (e1 == null)
                {
                    e1 = doc.CreateElement("style:" + type + "-properties", header_style);
                }
                e1.SetAttribute("line-height", header_fo, lineSpace.ToString() + "%");
                
                e.AppendChild(e1);
            }
        }

        public void SetBorderSpace(string name, XmlDocument doc, double top, double bottom, double left, double right)
        {
            XmlNodeList list = doc.GetElementsByTagName("style", header_style);
            XmlElement e = ((XmlElement)list.Item(0));

            string check_name = e.GetAttribute("name", header_style);
            if (check_name.Equals(name))
            {
                XmlElement e1 = null;
                string type = e.GetAttribute("family", header_style);

                e1 = (XmlElement)e.GetElementsByTagName(type + "-properties", header_style).Item(0);
                if (e1 == null)
                {
                    e1 = doc.CreateElement("style:" + type + "-properties", header_style);
                }

                e1.SetAttribute("margin-top", header_fo, top.ToString() + "in");
                e1.SetAttribute("margin-bottom", header_fo, bottom.ToString() + "in");
                e1.SetAttribute("margin-left", header_fo, left.ToString() + "in");
                e1.SetAttribute("margin-right", header_fo, right.ToString() + "in");

                XmlElement e2 = doc.CreateElement("style:tab-stops", header_style);
                e1.AppendChild(e2); //이건 뭔지 모르겠음 그냥 추가됨

                e.AppendChild(e1);
            }
        }
    }
}