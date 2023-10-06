using Unity.Mathematics;

namespace Locations
{

    [System.Serializable]
    public class Transition
    {
        public int id;
        public int entranceLocationId = -1;
        public int exitLoctionId = -1;
        public bool exitOpened;
        public bool entranceOpened;
        public bool exitExposed;
        public bool entranceExposed;
        public bool entranceMaped;
        public bool exitMaped;
        public int2 exitPosition;
        public int2 entrancePosition;
        public Location EntranceLocation => Registers.GlobalMapRegister.GetLocation(entranceLocationId);
        public Location ExitLocation => Registers.GlobalMapRegister.GetLocation(exitLoctionId);
        public Location ExposedEndLocation => WeAreAtEntrance ? EntranceLocation : ExitLocation;
        public Location UnExposedEndLocation => WeAreAtEntrance ? ExitLocation : EntranceLocation;
        private bool WeAreAtEntrance => entranceLocationId == Registers.GlobalMapRegister.CurrentLocationId;
        public bool HasExposedEnd => Registers.GlobalMapRegister.CurrentLocation.TransitionsIDs.Contains(id);
        public TileData ExposedEndTile
        {
            get
            {
                if (Registers.GlobalMapRegister.CurrentLocation.TransitionsIDs.Contains(id))
                {

                    if (WeAreAtEntrance)
                    {
                        return entrancePosition.ToTileData();
                    }
                    else
                    {
                        return exitPosition.ToTileData();
                    }
                }
                else
                {
                    return TileData.Null;
                }
            }
        }
        public TileData UnExposedEndTile
        {
            get
            {
                if (Registers.GlobalMapRegister.CurrentLocation.TransitionsIDs.Contains(id))
                {

                    if (!WeAreAtEntrance)
                    {
                        return entrancePosition.ToTileData();
                    }
                    else
                    {
                        return exitPosition.ToTileData();
                    }
                }
                else
                {
                    return TileData.Null;
                }
            }
        }
        public Transition(int locationFromId, int locationToId, bool exitOpened = true, bool entranceOpened = true,
            bool entranceExposed = true, bool exitExposed = true)
        {
            this.entranceExposed = entranceExposed;
            this.exitExposed = exitExposed;
            this.exitOpened = exitOpened;
            this.entranceOpened = entranceOpened;

            entranceLocationId = locationFromId;
            exitLoctionId = locationToId;
        }
        public override string ToString()
        {
            return "Transition " + id
                    + " From " + entrancePosition + " in " + Registers.GlobalMapRegister.GetLocation(entranceLocationId).Name
                    + " to " + exitPosition + " in " + Registers.GlobalMapRegister.GetLocation(exitLoctionId).Name;
        }
    }
}

