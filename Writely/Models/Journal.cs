using System;
using System.Collections.Generic;
using Writely.Data;

namespace Writely.Models
{
    public class Journal : Entity
    {
        public List<Entry> Entries { get; set; } = new ();

        public Journal(string title, string userId) : base(userId)
        {
            Title = title;
        }
        
        public Journal() {}

        public void Add(Entry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }
            
            entry.JournalId = Id;
            entry.Journal = this;
            Entries.Add(entry);
            UpdateLastModified();
        }

        public bool Update(JournalUpdate model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var didUpdate = false;
            if (model.Title is null || Title == model.Title) return didUpdate;
            Title = model.Title;
            didUpdate = true;
            UpdateLastModified();

            return didUpdate;
        }

        public bool Remove(Entry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }
            
            var removedSuccessfully = Entries.Remove(entry);
            if (removedSuccessfully)
            {
                UpdateLastModified();
            }

            return removedSuccessfully;
        }
    }
}