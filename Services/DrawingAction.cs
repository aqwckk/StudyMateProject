using StudyMateTest.Models.Drawing.DrawingElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Services
{
    public class DrawingAction 
    { 
        public enum ActionType { Add, Remove, Clear }
        
        public ActionType Type { get; set; }
        public IDrawingElement Element { get; set; }
        public List<IDrawingElement> ClearedElements { get; set; }
        public int Index { get; set; }
    }
}
