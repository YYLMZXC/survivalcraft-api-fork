﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class ChunkGenerationStep
    {
        public bool ShouldGenerate = true;
        public int GenerateOrder = 1600;

        public Action<TerrainChunk> GenerateAction;
        public ChunkGenerationStep(int generateOrder, Action<TerrainChunk> action) {
            //GenerateStep = generateStep;
            GenerateOrder = generateOrder;
            GenerateAction = action;
        }
    }
}
