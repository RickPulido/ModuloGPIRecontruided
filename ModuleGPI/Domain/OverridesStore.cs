using System;
using System.Collections.Generic;
using System.Linq;

namespace ModuleGPI.Domain
{
    /// <summary>
    /// Contenedor en memoria de overrides por usuario/módulo.
    /// </summary>
    public sealed class OverridesStore
    {
        public List<ModuleUserOverride> Items { get; } = new List<ModuleUserOverride>();

        /// <summary>
        /// Obtiene el valor de override para (buttonName, empId).
        /// Devuelve null si no hay registro (=> heredado).
        /// </summary>
        public int? Get(string buttonName, string empId)
        {
            var ov = Items.FirstOrDefault(x =>
                string.Equals(x.ButtonName, buttonName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.EmpId, empId, StringComparison.OrdinalIgnoreCase));

            return ov == null ? (int?)null : ov.Override;
        }

        /// <summary>
        /// Establece/actualiza override. Si value==0 se elimina (heredado).
        /// </summary>
        public void Set(string buttonName, string empId, int value)
        {
            var existing = Items.FirstOrDefault(x =>
                string.Equals(x.ButtonName, buttonName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.EmpId, empId, StringComparison.OrdinalIgnoreCase));

            if (value == 0)
            {
                if (existing != null) Items.Remove(existing);
                return;
            }

            if (existing == null)
            {
                Items.Add(new ModuleUserOverride
                {
                    ButtonName = buttonName,
                    EmpId = empId,
                    Override = value
                });
            }
            else
            {
                existing.Override = value;
            }
        }

        /// <summary>
        /// Elimina cualquier override explícito para (buttonName, empId).
        /// </summary>
        public void Remove(string buttonName, string empId)
        {
            Items.RemoveAll(x =>
                string.Equals(x.ButtonName, buttonName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.EmpId, empId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
