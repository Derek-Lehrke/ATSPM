﻿using System.Collections.Generic;
using MOE.Common.Models;
using MOE.Common.Models.Repositories;

namespace MOE.CommonTests.Models
{
    public class InMemoryDetectionTypeRepository : IDetectionTypeRepository
    {
        public List<DetectionType> GetAllDetectionTypes()
        {
            throw new System.NotImplementedException();
        }

        public List<DetectionType> GetAllDetectionTypesNoBasic()
        {
            throw new System.NotImplementedException();
        }

        public DetectionType GetDetectionTypeByDetectionTypeID(int detectionTypeID)
        {
            throw new System.NotImplementedException();
        }

        public void Update(DetectionType detectionType)
        {
            throw new System.NotImplementedException();
        }

        public void Add(DetectionType detectionType)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(DetectionType detectionType)
        {
            throw new System.NotImplementedException();
        }
    }
}