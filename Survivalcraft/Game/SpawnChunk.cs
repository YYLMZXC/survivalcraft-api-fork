using Engine;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
    public class SpawnChunk
    {
        public Point2 Point;

        public bool IsSpawned;

        public double? LastVisitedTime;

        public List<ValuesDictionary> SpawnsData = new List<ValuesDictionary>();


    }
}
