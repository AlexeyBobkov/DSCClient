using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Reflection;

namespace SettingsSupport
{
    ///////////////////////////////////////
    public enum AddType
    {
        Default,                    // default
        None,                       // no type
        Short,                      // short type
        ShortForCurrentAssembly,    // short type for current assembly only, otherwise full
        Full                        // full type
    }

    ///////////////////////////////////////
    public interface IProfile
    {
        string Name { get; set; }

        void SetValue(string section, string entry, object value);
        void SetValue(string section, string entry, object value, AddType addType);

        string GetValue(string section, string entry, string defaultValue);
        int GetValue(string section, string entry, int defaultValue);
        long GetValue(string section, string entry, long defaultValue);
        float GetValue(string section, string entry, float defaultValue);
        double GetValue(string section, string entry, double defaultValue);
        bool GetValue(string section, string entry, bool defaultValue);
        object GetValue(string section, string entry);
        object GetValue(string section, string entry, Type type);
        object GetValue(string section, string entry, Type type, object defaultValue);

        void Flush();
    }

    ///////////////////////////////////////
    public class XmlBuffer : IDisposable
    {
        private XmlProfile profile_;
        private XmlDocument doc_;
        private FileStream file_;
        internal bool needsFlushing_;

        internal XmlBuffer(XmlProfile profile, bool lockFile)
        {
            profile_ = profile;
            if (lockFile && File.Exists(profile_.Name))
                file_ = new FileStream(profile_.Name, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        }

        internal void Load(XmlTextWriter writer)
        {
            writer.Flush();
            writer.BaseStream.Position = 0;
            doc_.Load(writer.BaseStream);
            needsFlushing_ = true;
        }

        internal XmlDocument XmlDocument
        {
            get
            {
                if (doc_ == null)
                {
                    doc_ = new XmlDocument();
                    if (file_ != null)
                    {
                        file_.Position = 0;
                        doc_.Load(file_);
                    }
                    else if (File.Exists(profile_.Name))
                        doc_.Load(profile_.Name);
                }
                return doc_;
            }
        }

        internal bool IsEmpty
        {
            get { return XmlDocument.InnerXml == String.Empty; }
        }

        public bool NeedsFlushing
        {
            get { return needsFlushing_; }
        }

        public bool Locked
        {
            get { return file_ != null; }
        }

        public void Flush()
        {
            if (profile_ == null)
                throw new InvalidOperationException("Cannot flush an XmlBuffer object that has been closed.");

            if (doc_ == null)
                return;

            if (file_ == null)
                doc_.Save(profile_.Name);
            else
            {
                file_.SetLength(0);
                doc_.Save(file_);
            }

            needsFlushing_ = false;
        }

        public void Reset()
        {
            if (profile_ == null)
                throw new InvalidOperationException("Cannot reset an XmlBuffer object that has been closed.");

            doc_ = null;
            needsFlushing_ = false;
        }

        public void Close()
        {
            if (profile_ == null)
                return;

            if (needsFlushing_)
                Flush();

            doc_ = null;

            if (file_ != null)
            {
                file_.Close();
                file_ = null;
            }

            if (profile_ != null)
            {
                profile_.buffer_ = null;
                profile_ = null;
            }
        }

        public void Dispose()
        {
            Close();
        }
    }

    ///////////////////////////////////////
    public class XmlProfile : IProfile
    {
        public XmlProfile()
        {
            string fileName = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            Name = fileName.Substring(0, fileName.LastIndexOf('.')) + ".profile.xml";
        }

        public XmlProfile(string fileName)
        {
            if (fileName == null || fileName == "")
                throw new InvalidOperationException("Name is null or empty.");
            Name = fileName;
        }

        public string RootName
        {
            get { return rootName_; }
            set { rootName_ = value; }
        }

        public Encoding Encoding
        {
            get { return encoding_; }
            set { encoding_ = value; }
        }

        public bool Buffering
        {
            get { return buffer_ != null; }
        }

        public AddType AddTypes
        {
            get { return addTypes_; }
            set { addTypes_ = value; }
        }

        public XmlBuffer Buffer()
        {
            return Buffer(true);
        }

        public XmlBuffer Buffer(bool lockFile)
        {
            if (buffer_ == null)
                buffer_ = new XmlBuffer(this, lockFile);
            return buffer_;
        }

        // IMyProfile methods
        public virtual string Name
        {
            get; set;
        }
        public virtual void SetValue(string section, string entry, object value)
        {
            DoSetValue(section, entry, value, AddType.Default);
        }
        public virtual void SetValue(string section, string entry, object value, AddType addType)
        {
            DoSetValue(section, entry, value, addType);
        }
        public virtual string GetValue(string section, string entry, string defaultValue)
        {
            return (string)DoGetValue(section, entry, typeof(string)) ?? defaultValue;
        }
        public virtual int GetValue(string section, string entry, int defaultValue)
        {
            return (int?)DoGetValue(section, entry, typeof(int)) ?? defaultValue;
        }
        public virtual long GetValue(string section, string entry, long defaultValue)
        {
            return (long?)DoGetValue(section, entry, typeof(long)) ?? defaultValue;
        }
        public virtual float GetValue(string section, string entry, float defaultValue)
        {
            return (float?)DoGetValue(section, entry, typeof(float)) ?? defaultValue;
        }
        public virtual double GetValue(string section, string entry, double defaultValue)
        {
            return (double?)DoGetValue(section, entry, typeof(double)) ?? defaultValue;
        }
        public virtual bool GetValue(string section, string entry, bool defaultValue)
        {
            return (bool?)DoGetValue(section, entry, typeof(bool)) ?? defaultValue;
        }
        public virtual object GetValue(string section, string entry)
        {
            return DoGetValue(section, entry, null);
        }
        public virtual object GetValue(string section, string entry, Type type)
        {
            return DoGetValue(section, entry, type);
        }
        public virtual object GetValue(string section, string entry, Type type, object defaultValue)
        {
            if (type == null && defaultValue != null)
                type = defaultValue.GetType();
            return DoGetValue(section, entry, type) ?? defaultValue;
        }
        public virtual void Flush()
        {
            if (buffer_ != null && buffer_.needsFlushing_)
                buffer_.Flush();
        }


        ///////////////////////////////////////
        // Implementation
        ///////////////////////////////////////
        private string rootName_ = "profile";
        private Encoding encoding_ = Encoding.UTF8;
        internal XmlBuffer buffer_;
        private AddType addTypes_ = AddType.Default;

        protected void DoSetValue(string section, string entry, object value, AddType addType)
        {
            if (value == null)
            {
                RemoveEntry(section, entry);
                return;
            }

            if (addType == AddType.Default)
                addType = AddTypes;

            TypeConverter converter = TypeDescriptor.GetConverter(value.GetType());
            string valueString = (converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string))) ?
                                  converter.ConvertToString(value) : null;

            if ((buffer_ == null || buffer_.IsEmpty) && !File.Exists(Name))
            {
                // The file does not exist
                using (XmlTextWriter writer = (buffer_ == null) ?
                                new XmlTextWriter(Name, Encoding) :
                                new XmlTextWriter(new MemoryStream(), Encoding))    // If there's a buffer, write to it without creating the file
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteStartElement(RootName);
                    writer.WriteStartElement("section");
                    writer.WriteAttributeString("name", null, section);
                    writer.WriteStartElement("entry");
                    writer.WriteAttributeString("name", null, entry);
                    if (addType != AddType.None)
                        writer.WriteAttributeString("type", null, TypeName(value, addType));
                    writer.WriteAttributeString("serializeAs", null, valueString != null ? "String" : "Xml");
                    if (valueString != null)
                        writer.WriteString(valueString);
                    else
                        new XmlSerializer(value.GetType()).Serialize(writer, value);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    if (buffer_ != null)
                        buffer_.Load(writer);
                }
                return;
            }

            // The file exists
            XmlDocument doc = GetXmlDocument();
            XmlElement root = doc.DocumentElement;

            // Get the section element and add it if it's not there
            XmlNode sectionNode = root.SelectSingleNode(GetSectionsPath(section));
            if (sectionNode == null)
            {
                XmlElement element = doc.CreateElement("section");
                AddAttribute(element, "name", section);
                sectionNode = root.AppendChild(element);
            }

            // Get the entry element and add it if it's not there; otherwise, clean it
            XmlNode entryNode = sectionNode.SelectSingleNode(GetEntryPath(entry));
            if (entryNode == null)
                entryNode = sectionNode.AppendChild(doc.CreateElement("entry"));
            else
                entryNode.RemoveAll();

            AddAttribute(entryNode, "name", entry);
            if (addType != AddType.None)
                AddAttribute(entryNode, "type", TypeName(value, addType));
            AddAttribute(entryNode, "serializeAs", valueString != null ? "String" : "Xml");

            if (valueString != null)
                entryNode.InnerText = valueString;
            else
            {
                using (XmlWriter writer = entryNode.CreateNavigator().AppendChild())
                {
                    writer.WriteWhitespace(""); // hack to avoid exception
                    new XmlSerializer(value.GetType()).Serialize(writer, value);
                }
            }

            Save(doc);
        }

        protected object DoGetValue(string section, string entry, Type type)
        {
            XmlDocument doc = GetXmlDocument();
            XmlElement root = doc != null ? doc.DocumentElement : null;
            XmlNode entryNode = root != null ? root.SelectSingleNode(GetSectionsPath(section) + "/" + GetEntryPath(entry)) : null;
            if (entryNode == null)
                return null;

            XmlAttribute attribute = entryNode.Attributes["type"];
            if (attribute != null)
            {
                Type t = Type.GetType(attribute.Value);
                if(t != null)
                    type = t;   // override type
            }
            if (type == null)
                throw new InvalidOperationException("Type not specified or invalid.");

            attribute = entryNode.Attributes["serializeAs"];
            if (attribute == null)
                throw new InvalidOperationException("Serialization not specified.");
            switch (attribute.Value)
            {
                case "Xml":
                    using (StringReader sb = new StringReader(entryNode.InnerXml))
                        return new XmlSerializer(type).Deserialize(sb);

                case "String":
                    return entryNode.InnerText != null ? TypeDescriptor.GetConverter(type).ConvertFromString(entryNode.InnerText) : null;

                default:
                    throw new InvalidOperationException("Unknown serialization.");
            }
        }

        protected void RemoveEntry(string section, string entry)
        {
            XmlDocument doc = GetXmlDocument();
            XmlElement root = doc != null ? doc.DocumentElement : null;
            XmlNode entryNode = root != null ? root.SelectSingleNode(GetSectionsPath(section) + "/" + GetEntryPath(entry)) : null;
            if (entryNode == null)
                return;

            entryNode.ParentNode.RemoveChild(entryNode);
            Save(doc);
        }

        private string GetSectionsPath(string section) { return "section[@name=\"" + section + "\"]"; }
        private string GetEntryPath(string entry) { return "entry[@name=\"" + entry + "\"]"; }

        protected XmlDocument GetXmlDocument()
        {
            if (buffer_ != null)
                return buffer_.XmlDocument;

            if (!File.Exists(Name))
                return null;

            XmlDocument doc = new XmlDocument();
            doc.Load(Name);
            return doc;
        }

        protected void AddAttribute(XmlNode node, string name, string value)
        {
            XmlAttribute attribute = node.OwnerDocument.CreateAttribute(name);
            attribute.Value = value;
            node.Attributes.Append(attribute);
        }
        
        protected void Save(XmlDocument doc)
        {
            if (buffer_ != null)
                buffer_.needsFlushing_ = true;
            else
                doc.Save(Name);
        }

        protected string TypeName(object value, AddType addType)
        {
            Type t = value.GetType();
            string assemblyName = t.AssemblyQualifiedName;
            switch (addType)
            {
                case AddType.Short:
                    return assemblyName.Substring(0, assemblyName.IndexOf(',')) + ", " + t.Assembly.GetName().Name;

                case AddType.ShortForCurrentAssembly:
                    return (!t.Assembly.Equals(Assembly.GetExecutingAssembly())) ?
                            assemblyName :
                            assemblyName.Substring(0, assemblyName.IndexOf(',')) + ", " + t.Assembly.GetName().Name;

                default:
                case AddType.Default:
                case AddType.Full:
                    return assemblyName;
            }
        }
    }
}
