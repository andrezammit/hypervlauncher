using System;
using System.Linq;
using System.Collections.Generic;

namespace HyperVLauncher.Contracts.Models
{
    public class AppSettings
    {
        public List<Shortcut> Shortcuts { get; } = new();

        public void AddShortcut(string vmName)
        {
            var id = Guid.NewGuid().ToString();
            var shortcut = new Shortcut(id, vmName);

            Shortcuts.Add(shortcut);
        }

        public Shortcut? GetShortcut(string id)
        {
            return Shortcuts.FirstOrDefault(x => x.Id == id);
        }

        public void DeleteShortcut(string id)
        {
            var itemToRemove = GetShortcut(id);

            if (itemToRemove != null)
            {
                Shortcuts.Remove(itemToRemove);
            }
        }
    }
}
