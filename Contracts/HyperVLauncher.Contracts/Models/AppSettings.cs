using System;
using System.Linq;
using System.Collections.Generic;

namespace HyperVLauncher.Contracts.Models
{
    public class AppSettings
    {
        public List<Shortcut> Shortcuts { get; } = new();

        public static Shortcut CreateShortcut(string name, string vmId)
        {
            var id = Guid.NewGuid().ToString();
            return new Shortcut(id, vmId, name);
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
