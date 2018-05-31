using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace MicronBEAssyUserInterface.ProductRoute
{
    public class ToTextBlock
    {
        public System.Windows.Shapes.Line Line { get; private set; }
        public TextBlock LineText { get; set; }
        public TextBlock BinText { get; set; }

        public ToTextBlock(System.Windows.Shapes.Line line)
        {
            this.Line = line;
        }
    }
}
