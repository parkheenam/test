using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;
namespace MicronBEAssy
{
    [FeatureBind()]
    public static partial class ErrorMessageHelper
    {
        public static string CANNOT_FIND_PROCESS_STEP = "Cannot find ProcessStep";
    }
}
