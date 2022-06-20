using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.GameInfrastructure.DataModels
{
    public class DrawingObject
    {
        public string DrawingStr { get; }
        public Guid Id { get; } = Guid.NewGuid();
        public bool SentToClient { get; private set; } = false;
        public DrawingObject(string drawing) 
        {
            DrawingStr = drawing;
        }

        public DrawingObject (string drawing, Guid id)
        {
            DrawingStr = drawing;
            Id = id;
        }
        public void MarkSentToClient()
        {
            SentToClient = true;
        }
    }
}
