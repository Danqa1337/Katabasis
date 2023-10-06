using System.Collections.Generic;

public enum HyperstuctureName
{
    Null,
    Any,
    Town,
}
public class Hyperstructure
{
    public List<StructureData> generatedStrctures = new List<StructureData>();
    public virtual void Generate(TileData center)
    {

    }



}
