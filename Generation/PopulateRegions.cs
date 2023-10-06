namespace GenerationModules
{
    public class PopulateRegions : GenerationModule
    {
        public override void Generate(GenerationData generationData)
        {
            foreach (var tile in GetAllMapTiles())
            {
                if (tile.Template.templateState == TemplateState.Floor)
                {
                    if (tile.Template.Biome == Biome.Cave)
                    {
                        if (tile.Template.GroundCover == SimpleObjectName.Null)
                        {
                            if (Chance(50))
                            {
                                tile.Template.GenerateObject(SimpleObjectName.Moss);
                                continue;
                            }

                            if (Chance(30) && tile.CheckStateInNeibors(TemplateState.Darkness, true))
                            {
                                int f = 0;
                                foreach (var item in tile.GetNeibors(true))
                                {
                                    if (item.Template.templateState == TemplateState.Floor) f++;
                                }
                                if (f > 4) tile.Template.GenerateObject(SimpleObjectName.Stalagmite);
                                continue;
                            }

                            if (Chance(5) && tile.CheckStateInNeibors(TemplateState.Floor, true, 4))
                            {
                                tile.Template.GenerateObject(SimpleObjectName.Mushroom);
                            }
                            if (Chance(10) && tile.CheckStateInNeibors(TemplateState.Floor, true, 4))
                            {
                                tile.Template.GenerateObject(SimpleObjectName.QuarryBush);
                            }
                        }
                    }
                    else if (tile.Template.Biome == Biome.Dungeon && !tile.CheckStateInNeibors(TemplateState.Wall, false))
                    {
                        if (Chance(2))
                        {
                            tile.Template.GenerateObject(SimpleObjectName.Vase);
                        }
                    }
                    if (tile.Template.Biome == Biome.Town)
                    {
                        if (tile.Template.GroundCover == SimpleObjectName.Null && tile.Template.FloorLayer == SimpleObjectName.RockFloor)
                        {
                            if (Chance(20))
                            {
                                tile.Template.GenerateObject(SimpleObjectName.Moss);
                                continue;
                            }
                            if (Chance(5))
                            {
                                tile.Template.GenerateObject(SimpleObjectName.QuarryBush);
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }

}

