 /*************************************************************************
	Date  : 1/17/2015 12:00:00 AM  
	Author: Robert A. Olliff
	Email : robert.olliff@gmail.com

	This file probably has code in it and does stuff.
 ************************************************************************ */
//END STUPID COMMENT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RagelP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Paralizer
{

    /// <summary>
    /// A Paradox file contains Object's, of which there are many types.
    /// </summary>
    public enum ObjectType
    {
        /// Unknown
        Blank,

        /// Used internally to represent the object above the topmost found within the file     
        Root,

        /// Object containing properties, start with '{' and ands with '}' on the same level.
        /// @code
        ///     im_a_property_and_my_value_is_an_associative_object = {
        ///         im_a_property_in_the_object=value,
        ///         im_another_property_in_the_object=value2
        ///     }
        /// @endcode
        Associative,

        /// Simliar to an Associative, but instead of containing properties, Arrays contain only values.
        /// The values do not have to be of the same type.
        /// @code
        /// im_an_array = {
        ///     23 something 4.878 }
        /// @endcode
        Array,

        /// The values of all three "faction" Properties become elements in an Array, which is then assigned
        /// as the Value of a single "faction" property in the parent object.
        DuplicatesArray,
        Date,
        Integer,
        Float,
        String,
        Boolean,
        Name,
        AssociativeAnonymous,
        InlineArray,
    }

    /// <summary>
    /// CSharp representation of the Paradox Interactive data format.
    /// </summary>
    /// This is my [Robby's] best effort to normalize the data read from
    /// a Pardox text file
    [System.Diagnostics.DebuggerDisplay("{ParadoxDataElement.ToString()}")]
    public class ParadoxDataElement
    {
        /// Name of thing, unless thing is a primitive array item (non key/value pair)
        /// then it will be null.
        public string Name { get; set; }

        /// Where this thing was found, for debugging
        internal TokenData Location { get; set; }

        /// Content depends on type
        public Object Content { get; set; }

        /// What this thing is
        public ObjectType Type { get; set; }

        /// For debugging
        internal string MyPath()
        {
            if (Parent == null)
                return "/";

            return Parent.MyPath() +  "/" + Name;
        }
        
        
        public ParadoxDataElement Parent { get; private set; }

        public override string ToString()
        {
            return String.Format("{0}:{1}", Name == null ? "<null>" : Name, Type);
        }

        private ParadoxDataElement inline_array;

        private List<ParadoxDataElement> children = new List<ParadoxDataElement>();

        public IEnumerable<ParadoxDataElement> Children { get { return children; } }

        public ParadoxDataElement()
        {
        }

        public void RemoveObject(ParadoxDataElement that)
        {
            this.children.Remove(that);
            that.Parent = null;
        }

        public void TransferOwnership(ParadoxDataElement to)
        {
            if (this.Parent == null)
                throw new LexerException("'this' ({0}) is a root node and has no parent", this.Name);
            this.Parent.RemoveObject(this);
            to.AddObject(this);
        }
        public void SwapParent(ParadoxDataElement that)
        {
            if (this.Parent == null)
                throw new LexerException("'this' ({0}) is a root node and has no parent", this.Name);

            if (that.Parent == null)
                throw new LexerException("'that' ({0}) is a root node and has no parent", that.Name);

            var ThisParent = this.Parent;
            ThisParent.RemoveObject(this);

            var ThatParent = that.Parent;
            ThatParent.RemoveObject(that);
            
            ThisParent.AddObject(that);
            ThatParent.AddObject(this);
        }
        public void AddObject(ParadoxDataElement id)
        {
            if (id.Parent != null)
                throw new LexerException("Can't triple-stamp a double-stamp, the object you trying to add already belongs to hierarchy '{0}'", id.Parent.Name);

            // Array elements
            if (id.Name == null)
            {
                if (inline_array != null)
                {
                    inline_array.AddElement(id);
                }
                else
                {
                    int total = this.Children.Count();
                    // First array element, mark as array
                    if (total == 0)
                    {
                        this.Type = ObjectType.Array;
                        id.Parent = this;
                        children.Add(id);
                    }
                    else
                    {
                        // not first, but no named elements
                        int with_names = this.Children.Count(n => n.Name != null);
                        if (with_names == 0)
                        {
                            id.Parent = this;
                            children.Add(id);
                        }
                        else
                        {
                            // must be first array element in thing which already had named thing
                            if (inline_array != null)
                                throw new Exception("g");
                            inline_array = new ParadoxDataElement()
                            {
                                Type = ObjectType.InlineArray,
                                Parent = this
                            };
                            inline_array.AddObject(id);

                            children.Add(inline_array);
                        }
                    }
                }


            }
            else if (Type == ObjectType.Array)
            {
                Type = ObjectType.Associative;
                if (inline_array != null)
                    throw new Exception("h");
                inline_array = new ParadoxDataElement()
                {
                    Type = ObjectType.InlineArray,
                    Parent = this
                };

                var list = children.Where(n => n.Name == null).ToList();
                children.RemoveAll(n => n.Name == null);

                foreach (var j in list)
                {
                    inline_array.children.Add(j);
                }
                children.Add(inline_array);

                AddObject(id);

            }
            else
            {
                var v = (from n in Children
                         where n.Name == id.Name
                         select n).SingleOrDefault();

                if (v != null && v.Type != ObjectType.DuplicatesArray)
                {
                    // Make a new array
                    // Add the one we found
                    // Add the one passed as arg
                    // Add the new array to the ID list
                    // remove the one we found from the ID list
                    // Make a confused face and ask, "What just happened?"

                    var arr = new ParadoxDataElement { Name = v.Name, Type = ObjectType.DuplicatesArray, children = new List<ParadoxDataElement>(), Location = v.Location };
                    children.Remove(v);
                    v.Parent = null;
                    AddObject(arr);
                    AddObject(v);
                    AddObject(id);
                }
                else if (v == null)
                {
                    children.Add(id);
                }
                else
                {
                    v.children.Add(id);
                }
                id.Parent = this;
            }
            
        }

        /// <summary>
        /// Same as AddObject, but allows for duplicates
        /// </summary>
        /// <param name="id"></param>
        internal void AddElement(ParadoxDataElement id)
        {
            AddObject(id);
        }

        internal Newtonsoft.Json.Linq.JToken Metadata()
        {
            JObject obj = new JObject();
            obj["path"] = MyPath();
            obj["type"] = Type.ToString();
            return obj;
        }
    }
}
