using UnityEngine;
using System.Collections.Generic;

namespace GenerationModules
{
    public class RemoveBadRegions : GenerationModule
    {
        [SerializeField] private float _minRegionSize;

        public override void Generate(GenerationData generationData)
        {
            int r = 0;
            var badRegions = new List<Region>();
            for (int i = 0; i < generationData.Regions.Count; i++)
            {
                if (generationData.Regions[i].Size < _minRegionSize)
                {
                    badRegions.Add(generationData.Regions[i]);
                }
            }

            foreach (var region in badRegions)
            {
                r++;
                generationData.Regions.Remove(region);
                foreach (var item in region.Tiles)
                {
                    item.Template.Clear();
                }
            }

            if (Debug) UnityEngine.Debug.Log(r + " regions removed");
        }
    }
}