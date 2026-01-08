using System.Xml.Serialization;

namespace KeeperDomain;

public static class DeepCopyExt
{
    // T and its properties must have parameterless constructor
    public static T DeepCopyXml<T>(this T self)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            var xs = new XmlSerializer(typeof(T));
            xs.Serialize(ms, self);

            ms.Position = 0;
            return (T)xs.Deserialize(ms);
        }
    }
}
