using System;
using TinyRadius.Net.Dictionaries;
using TinyRadius.Net.Util;

namespace TinyRadius.Net.Attributes
{
    /// <summary>
    ///  This class represents a generic Radius attribute. Subclasses implement
    ///  methods to access the fields of special attributes.
    /// </summary>
    public class RadiusAttribute
    {
        private int _attributeType = -1;

        /// <summary>
        ///  Constructs an empty Radius attribute.
        /// </summary>
        private byte[] _data;


        /// <summary>
        /// 
        /// </summary>
        public RadiusAttribute()
        {
            VendorId = -1;
            Dictionary = DefaultDictionary.GetDefaultDictionary();
        }

        /// <summary>
        ///  Constructs a Radius attribute with the specified
        ///  type and data.
        ///  @param type attribute type, see AttributeTypes./// 
        ///  @param data attribute data
        /// </summary>
        public RadiusAttribute(int type, byte[] data)
        {
            VendorId = -1;
            Type = type;
            Data = data;
        }

        /// <summary>
        ///  Returns the data for this attribute.
        ///  @return attribute data
        /// </summary>
        public byte[] Data
        {
            get { return _data; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Value", "attribute data is null");
                _data = value;
            }
        }

        /// <summary>
        ///  Returns the type of this Radius attribute.
        ///  @return type code, 0-255
        /// </summary>
        public int Type
        {
            get { return _attributeType; }
            set
            {
                if (value < 0 || value > 255)
                    throw new ArgumentException("attribute type invalid: " + value);
                _attributeType = value;
            }
        }

        /// <summary>
        ///  Sets the value of the attribute using a string.
        ///  @param value value as a string
        /// </summary>
        public virtual string Value
        {
            set { throw new NotImplementedException("cannot set the value of attribute " + _attributeType + " as a string"); }
            get { return RadiusUtil.GetHexString(Data); }
        }

        /// <summary>
        ///  Gets the Vendor-Id of the Vendor-Specific attribute this
        ///  attribute belongs to. Returns -1 if this attribute is not
        ///  a sub attribute of a Vendor-Specific attribute.
        ///  @return vendor ID
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        ///  Returns the dictionary this Radius attribute uses.
        ///  @return Hashtable instance
        /// </summary>
        public virtual IWritableDictionary Dictionary { get; set; }

        /// <summary>
        ///  Returns this attribute encoded as a byte array.
        ///  @return attribute
        /// </summary>
        public virtual byte[] WriteAttribute()
        {
            if (Type == -1)
                throw new ArgumentException("Type type not set");
            if (Data == null)
                throw new ArgumentException("attribute data not set");

            var attr = new byte[2 + _data.Length];
            attr[0] = (byte) Type;
            attr[1] = (byte) (2 + _data.Length);
            Array.Copy(_data, 0, attr, 2, _data.Length);
            return attr;
        }

        /// <summary>
        ///  Reads in this attribute from the passed byte array.
        ///  @param data
        /// </summary>
        public virtual void ReadAttribute(byte[] data, int offset, int length)
        {
            if (length < 2)
                throw new RadiusException("attribute Length too small: " + length);
            int attrType = data[offset] & 0x0ff;
            int attrLen = data[offset + 1] & 0x0ff;
            var attrData = new byte[attrLen - 2];
            Array.Copy(data, offset + 2, attrData, 0, attrLen - 2);
            Type = attrType;
            Data = attrData;
        }

        /// <summary>
        ///  String representation for debugging purposes.
        ///  @see java.lang.Object#toString()
        /// </summary>
        public override String ToString()
        {
            String name;

            // determine attribute name
            AttributeType at = GetAttributeTypeObject();
            if (at != null)
                name = at.Name;
            else if (VendorId != -1)
                name = "Unknown-Sub-Attribute-" + Type;
            else
                name = "Unknown-Attribute-" + Type;

            // indent sub attributes
            if (VendorId != -1)
                name = "  " + name;

            return name + ": " + Value;
        }

        /// <summary>
        ///  Retrieves an AttributeType object for this attribute.
        ///  @return AttributeType object for (sub-)attribute or null
        /// </summary>
        public AttributeType GetAttributeTypeObject()
        {
            return VendorId != -1
                       ? Dictionary.GetAttributeTypeByCode(VendorId, Type)
                       : Dictionary.GetAttributeTypeByCode(Type);
        }

        /// <summary>
        ///  Creates a RadiusAttribute object of the appropriate type.
        ///  @param dictionary Hashtable to use
        ///  @param vendorId vendor ID or -1
        ///  @param attributeType attribute type
        ///  @return RadiusAttribute object
        /// </summary>
        public static RadiusAttribute CreateRadiusAttribute(IWritableDictionary dictionary, int vendorId,
                                                            int attributeType)
        {
            var attribute = new RadiusAttribute();

            AttributeType at = dictionary.GetAttributeTypeByCode(vendorId, attributeType);
            if (at != null && at.Class != null)
            {
                try
                {
                    attribute = (RadiusAttribute) Activator.CreateInstance(at.Class);
                }
                catch (Exception e)
                {
                    // error instantiating class - should not occur
                }
            }

            attribute.Type = attributeType;
            attribute.Dictionary = dictionary;
            attribute.VendorId = vendorId;
            return attribute;
        }

        /// <summary>
        ///  Creates a Radius attribute, including vendor-specific
        ///  attributes. The default dictionary is used.
        ///  @param vendorId vendor ID or -1
        ///  @param attributeType attribute type
        ///  @return RadiusAttribute instance
        /// </summary>
        public static RadiusAttribute CreateRadiusAttribute(int vendorId, int attributeType)
        {
            IWritableDictionary dictionary = DefaultDictionary.GetDefaultDictionary();
            return CreateRadiusAttribute(dictionary, vendorId, attributeType);
        }

        /// <summary>
        ///  Creates a Radius attribute. The default dictionary is
        ///  used.
        ///  @param attributeType attribute type
        ///  @return RadiusAttribute instance
        /// </summary>
        public static RadiusAttribute CreateRadiusAttribute(int attributeType)
        {
            IWritableDictionary dictionary = DefaultDictionary.GetDefaultDictionary();
            return CreateRadiusAttribute(dictionary, -1, attributeType);
        }
    }
}