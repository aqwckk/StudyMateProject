using StudyMateTest.Models.Drawing.DrawingElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Services.DrawingServices
{
    public class DrawingAction // класс для хранения действий на холсте
    { 
        public enum ActionType { Add, Remove, Clear } // перечисление для типа действия - добавления, удаления, очистки
        
        public ActionType Type { get; set; } // свойство для типа действия
        public IDrawingElement Element { get; set; } // свойство для элемента, с которым связано действие
        public List<IDrawingElement> ClearedElements { get; set; } // свойство для списка очищенных эл-тов
        public int Index { get; set; } // свойство для индекса элемента
    }
}
