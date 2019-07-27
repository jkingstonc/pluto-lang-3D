using System.Collections.Generic;

public class PlutoObject
{
    public PlutoClass the_class;
    public Dictionary<string, object> members;

    public PlutoObject(PlutoClass the_class)
    {
        this.members = new Dictionary<string, object>();
        this.the_class = the_class;
    }

    // Get a method or member
    public object get(Token identifier)
    {
        // Check class methods
        PlutoFunction method = the_class.find_method(this, (string)identifier.value);
        if (method != null)
        {
            return method;
        }
        // Check object members
        object member = find_member((string)identifier.value);
        if(member!=null)
        {
            return member;
        }
        throw new RuntimeException("Class member/method cannot be found '"+identifier.value+"'");
    }

    // Set a member value
    public object set(Token identifier, object value)
    {
        this.members[(string)identifier.value] = value;
        return members[(string)identifier.value];
    }

    // Get a member object
    public object find_member(string identifier)
    {
        if (this.members.ContainsKey(identifier))
        {
            return members[identifier];
        }
        return null;
    }
}
