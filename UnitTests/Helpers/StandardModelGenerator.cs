using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaikesSimplexService.DataModel;
using RaikesSimplexService.Implementation;

namespace UnitTests.Helpers
{
    public static class StandardModelGenerator
    {
        public static StandardModel GetSimpleStandardModel()
        {
            return StandardModel.FromModel(ModelGenerator.GetSimpleModel());
        }

        public static StandardModel GetImpossibleStandardModel()
        {
            return StandardModel.FromModel(ModelGenerator.GetImpossibleModel());
        }
    }
}
