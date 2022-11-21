using UnityEngine;
 
public class PrefixAttribute : PropertyAttribute
{
    public string prefixName { get ; private set; }    
    public PrefixAttribute( string name )
    {
        prefixName = name+" " ;
    }
}