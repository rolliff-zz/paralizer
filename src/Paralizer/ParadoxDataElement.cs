 /*************************************************************************
	Author: Robert A. Olliff
	Date  : 1/16/2015 12:00:00 AM  

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

    public enum ObjectType
    {
        Blank,
        Root,
        Associative,
        Array,
        DuplicatesArray,
        Date,
        Integer,
        Float,
        String,
        Boolean,
        Name,
        AssociativeAnonymous,
    }

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

        /// <summary>
        /// Same as AddObject, but allows for duplicates
        /// </summary>
        /// <param name="id"></param>
        internal void AddElement(ParadoxDataElement id)
        {
            children.Add(id);
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
