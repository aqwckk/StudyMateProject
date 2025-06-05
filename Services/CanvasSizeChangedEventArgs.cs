using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Services
{
    public class CanvasSizeChangedEventArgs : EventArgs
    {
        public float Width { get; }
        public float Height { get; }

        public CanvasSizeChangedEventArgs(float width, float height)
        {
            Width = width;
            Height = height;
        }
    }
}
